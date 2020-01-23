//*************************************************************************************************
// StashApplyOperation.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    [GitErrorMapping(typeof(StashRestoreIndexFailedException), Prefix = "error: patch failed:")]
    [GitErrorMapping(typeof(WorkingDirectoryUnmergedException), Suffix = "needs merge")]
    [GitErrorMapping(typeof(WorkingDirectoryUncommittedException), Prefix = "error: Your local changes to the following files would be overwritten by merge:")]
    internal class StashApplyOperation : Operation
    {
        const string StagedChangesHeader = "Changes to be committed:";
        const string UnstagedChangesHeader = "Changes not staged for commit:";
        const string UntrackedChangesHeader = "Untracked files:";

        const string FilePrefix = "\t";
        const string DeletedPrefix = FilePrefix + "deleted:    ";
        const string ModifiedPrefix = FilePrefix + "modified:   ";
        const string NewFilePrefix = FilePrefix + "new file:   ";

        static readonly OperationParser[] parsers = {
            new WarningParser()
        };

        public StashApplyOperation(ExecutionContext context, OperationCallback progressCallback)
            : base(context, progressCallback)
        {
            _updated = null;
        }

        private List<StashUpdatedFile> _updated;
        private int _numConflicts;

        public IReadOnlyList<StashUpdatedFile> UpdatedFiles
        {
            get { return Volatile.Read(ref _updated); }
        }

        public int NumberOfConflicts
        {
            get { return Volatile.Read(ref _numConflicts); }
        }

        bool isStagedSection = false;
        bool isUntrackedSection = false;

        List<StashUpdatedFile> updates = new List<StashUpdatedFile>();
        int numConflicts = 0;

        protected override bool ParseOutput (OperationOutput output)
        {
            if (output == OperationOutput.OutputClosed) {
                // Since this parsing method is shared by both stdin and stderr parsers, and that the 'conflict'
                // file update messages are written to stderr means that we don't want the stdin parser to replace
                // the collection of update files (list of conflicts) from stderr with an empty one.
                if (updates.Count > 0)
                {
                    Volatile.Write(ref _updated, updates);
                }

                if (numConflicts > 0)
                {
                    Volatile.Write(ref _numConflicts, numConflicts);
                }

                return false;
            }

            string line = output.Message;

            if (TryParse(line, parsers, out OperationProgress progress))
            {
                Update(progress);
            }
            else if (line.StartsWith(FilePrefix, StringComparison.Ordinal))
            {
                if (line.StartsWith(ModifiedPrefix, StringComparison.Ordinal))
                {
                    var fileName = line.Substring(ModifiedPrefix.Length, line.Length - ModifiedPrefix.Length);
                    var fileType = isStagedSection ? StashUpdatedFileType.StagedModified : StashUpdatedFileType.UnstagedModified;

                    var update = new StashUpdatedFile(fileName, fileType);

                    updates.Add(update);
                }
                else if (line.StartsWith(DeletedPrefix, StringComparison.Ordinal))
                {
                    var fileName = line.Substring(DeletedPrefix.Length, line.Length - DeletedPrefix.Length);
                    var fileType = isStagedSection ? StashUpdatedFileType.StagedDeleted : StashUpdatedFileType.UnstagedDeleted;

                    var update = new StashUpdatedFile(fileName, fileType);

                    updates.Add(update);
                }
                else if (line.StartsWith(NewFilePrefix, StringComparison.Ordinal))
                {
                    var fileName = line.Substring(NewFilePrefix.Length, line.Length - NewFilePrefix.Length);
                    var fileType = StashUpdatedFileType.StagedAddition;

                    var update = new StashUpdatedFile(fileName, fileType);

                    updates.Add(update);
                }
                else if (isUntrackedSection)
                {
                    var fileName = line.Substring(FilePrefix.Length, line.Length - FilePrefix.Length);

                    var update = new StashUpdatedFile(fileName, StashUpdatedFileType.Untracked);

                    updates.Add(update);
                }
                else
                {
                    throw new InvalidOperationException();
                }

                Update(new WorkingDirectoryUpdatedMessage(line));
            }
            else if (IsMessageFatal(line))
            {
                return false;
            }
            else if (!string.IsNullOrEmpty(line = CleanLine(line)))
            {
                // Avoid sending the command line usage tips as progress updates.
                if (line.StartsWith("  (use ", StringComparison.Ordinal))
                    return false;

                // Look for conflicts during apply
                if (TryParseMergeConflict(line, out string conflictFile))
                {
                    var update = new StashUpdatedFile(conflictFile, StashUpdatedFileType.Conflict);
                    updates.Add(update);
                    numConflicts++;

                    Update(new WorkingDirectoryUpdatedMessage(line));

                    return false;
                }

                if (line.StartsWith(StagedChangesHeader, StringComparison.Ordinal))
                {
                    isStagedSection = true;
                    isUntrackedSection = false;
                }
                else if (line.StartsWith(UnstagedChangesHeader, StringComparison.Ordinal))
                {
                    isStagedSection = false;
                    isUntrackedSection = false;
                }
                else if (line.StartsWith(UntrackedChangesHeader, StringComparison.Ordinal))
                {
                    isStagedSection = false;
                    isUntrackedSection = true;
                }

                Update(new GenericOperationMessage(line));
            }
            else
            {
                /* git-stash is currently implemented as a shell script which eventually calls into
                 * builtin/merge-recursive.c and merge-recursive.c.
                 * Because we set GIT_MERGE_VERBOSITY >= 4 we get a bunch of extra gibberish messages
                 * sent to us over the stdout pipe.
                 *
                 * This is not user visible output (say had the command been invoked from cmd.exe),
                 * and does not contain any useful information; just noise for tests.
                 *
                 * Examples include:
                 *   Merging Version stash was based on with Stashed changes
                 *   Merging:
                 *   virtual Version stash was based on
                 *   virtual Stashed changes
                 *
                 * Yes those lines are not badly copied into this comment.. they are formed by string
                 * concatenation in Git, leading to bad grammar.
                 *
                 * Do NOT call `Update(new GenericOperationMessage(this, line))` here otherwise
                 * we end up returning these useless messages to consumers.
                 */
            }
            return false;
        }

        private bool TryParseMergeConflict(string line, out string conflictFile)
        {
            foreach (Regex regex in MergeConflictPatterns.AllRegexs)
            {
                Match fileMatch;
                if ((fileMatch = regex.Match(line)).Success)
                {
                    conflictFile = fileMatch.Groups["file"].Value;
                    return true;
                }
            }

            // Used to capture all lines where we can't extract the file path, or for conflicts with
            // directories (we don't bother trying to parse those).
            if (Regex.IsMatch(line, "CONFLICT"))
            {
                // Return some fake file for the unknown conflict
                conflictFile = "(unknown)";
                return true;
            }

            conflictFile = null;
            return false;
        }
    }
}
