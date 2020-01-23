//*************************************************************************************************
// DiffCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.TeamFoundation.GitApi.Internal;
using System.Text;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal abstract class DiffCommand : GitCommand
    {
        public const char Break = '*';
        public const char Separator = ' ';

        protected DiffCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        internal static char GetDifferenceTypeChar(TreeDifferenceType type)
        {
            switch (type)
            {
                case TreeDifferenceType.Added: return 'A';
                case TreeDifferenceType.Copied:
                case TreeDifferenceType.CopiedInParent: return 'C';
                case TreeDifferenceType.Deleted: return 'D';
                case TreeDifferenceType.Ignored: return '!';
                case TreeDifferenceType.Modified:
                case TreeDifferenceType.Merged: return 'M';
                case TreeDifferenceType.Renamed:
                case TreeDifferenceType.RenamedInParent: return 'R';
                case TreeDifferenceType.TypeChange: return 'T';
                case TreeDifferenceType.Unmerged: return 'U';
                case TreeDifferenceType.Unmodified: return ' ';
                case TreeDifferenceType.Untracked: return '?';
            }

            return '\0';
        }

        internal static TreeDifferenceType GetDifferenceType(char symbol)
        {
            switch (symbol)
            {
                case ' ': return TreeDifferenceType.Unmodified;
                case '!': return TreeDifferenceType.Ignored;
                case '?': return TreeDifferenceType.Untracked;
                case 'A': return TreeDifferenceType.Added;
                case 'C': return TreeDifferenceType.Copied;
                case 'D': return TreeDifferenceType.Deleted;
                case 'M': return TreeDifferenceType.Modified;
                case 'R': return TreeDifferenceType.Renamed;
                case 'T': return TreeDifferenceType.TypeChange;
                case 'U': return TreeDifferenceType.Unmerged;
            }

            return TreeDifferenceType.Invalid;
        }

        internal ITreeDifference ParseDiffOutput(Stream outputStream, DifferenceOptions options)
        {
            TreeDifference treeDifference = null;
            try
            {
                using (var buffer = new ByteBuffer())
                {
                    treeDifference = new TreeDifference();
                    treeDifference.SetContext(Context);
                    ParseResult result = ParseResult.MatchIncomplete;

                    int count = 0; // The count of bytes read into the buffer
                    int index = 0; // The first unparsed byte in the buffer

                    int read = 0;
                    while (count < buffer.Length && (read = outputStream.Read(buffer, count, buffer.Length - count)) > 0)
                    {
                        // Compute total read length
                        count += read;

                        // Determine the parser to use based on the diff format option
                        if (options.Format == DifferenceFormat.Default || options.Format == DifferenceFormat.Raw)
                        {
                            result = treeDifference.ParseRaw(buffer, ref index, count, maxEntries: 0);
                        }
                        else if (options.Format == DifferenceFormat.NameStatus)
                        {
                            result = treeDifference.ParseNameStatus(buffer, ref index, count, maxEntries: 0);
                        }
                        else
                        {
                            throw new ArgumentException("Unrecognized difference format", $"{nameof(options)}.{nameof(options.Format)}");
                        }

                        switch (result)
                        {
                            case ParseResult.MatchComplete:
                                return treeDifference;

                            case ParseResult.MatchIncomplete:
                            case ParseResult.MatchMaybeComplete:
                                {
                                    // When we've moved beyond what we can reliably read we need to shift the bytes
                                    // left to make room in the buffer for new data
                                    MakeSpace(buffer, ref index, ref count);
                                    continue;
                                }
                        }
                    }

                    // If the loop breaks because there is no more data to be read, return the result
                    // if it was flagged as maybe complete or if there was zero data to be read.  It
                    // is valid for there to be no diff between two trees.
                    if (result == ParseResult.MatchMaybeComplete
                        || (count == 0
                            && index == 0
                            && read == 0
                            && treeDifference.Entries.Count == 0))
                    {
                        return treeDifference;
                    }
                }

                throw new DifferenceParseException("parse loop exited", null, 0);
            }
            catch (ParseException exception) when (ParseHelper.AddContext(nameof(treeDifference), exception, treeDifference))
            {
                // Used for logging parse failures via exception filter.  This code will
                // not be reached but the throw is required to keep the compiler happy.
                throw;
            }
        }

        internal void ApplyOptions(ArgumentList command, DifferenceOptions options)
        {
            const string DiffAlgorithmPrefix = "--diff-algorithm=";
            const string IgnoreSubmodulePrefix = "--ignore-submodules=";

            switch (options.Format)
            {
                case DifferenceFormat.Default:
                case DifferenceFormat.Raw:
                    command.AddOption("--raw");
                    break;

                case DifferenceFormat.NameStatus:
                    command.AddOption("--name-status");
                    break;
            }

            switch (options.Algorithm)
            {
                case DifferenceAlgorithm.Histogram:
                    command.AddOption(DiffAlgorithmPrefix + "histogram");
                    break;

                case DifferenceAlgorithm.Minimal:
                    command.AddOption(DiffAlgorithmPrefix + "minimal");
                    break;

                case DifferenceAlgorithm.Myers:
                    command.AddOption(DiffAlgorithmPrefix + "myers");
                    break;

                case DifferenceAlgorithm.Patience:
                    command.AddOption(DiffAlgorithmPrefix + "parience");
                    break;
            }

            switch (options.IgnoreSubmodules)
            {
                case StatusIgnoreSubmodule.All:
                    command.AddOption(IgnoreSubmodulePrefix + "all");
                    break;

                case StatusIgnoreSubmodule.Dirty:
                    command.AddOption(IgnoreSubmodulePrefix + "dirty");
                    break;

                case StatusIgnoreSubmodule.Untracked:
                    command.AddOption(IgnoreSubmodulePrefix + "untracked");
                    break;
            }

            switch (options.RenameDetection)
            {
                case RenameDetection.FollowRenames:
                    command.AddOption("--find-renames");
                    break;

                case RenameDetection.NoRenameDetection:
                    command.AddOption("--no-renames");
                    break;
            }

            if (options.Whitespace != DifferenceWhitespace.Default)
            {
                if ((options.Whitespace & DifferenceWhitespace.IgnoreAllSpaces) != 0)
                {
                    command.AddOption("--ignore-all-space");
                }

                if ((options.Whitespace & DifferenceWhitespace.IgnoreBlankLines) != 0)
                {
                    command.AddOption("--ignore-blank-lines");
                }

                if ((options.Whitespace & DifferenceWhitespace.IgnoreSpaceAtEndOfLine) != 0)
                {
                    command.AddOption("--ignore-space-at-eol");
                }

                if ((options.Whitespace & DifferenceWhitespace.IgnoreWhitespaceChanges) != 0)
                {
                    command.AddOption("--ignore-space-change");
                }
            }

            if (options.Filters != 0)
            {
                var filter = new StringBuilder();
                if ((options.Filters & DifferenceFilterFlags.IncludeAdded) != 0)
                {
                    filter.Append('A');
                }
                if ((options.Filters & DifferenceFilterFlags.IncludeCopied) != 0)
                {
                    filter.Append('C');
                }
                if ((options.Filters & DifferenceFilterFlags.IncludeDeleted) != 0)
                {
                    filter.Append('D');
                }
                if ((options.Filters & DifferenceFilterFlags.IncludeModeChanges) != 0)
                {
                    filter.Append('T');
                }
                if ((options.Filters & DifferenceFilterFlags.IncludeModified) != 0)
                {
                    filter.Append('M');
                }
                if ((options.Filters & DifferenceFilterFlags.IncludePairBroken) != 0)
                {
                    filter.Append('B');
                }
                if ((options.Filters & DifferenceFilterFlags.IncludeRenamed) != 0)
                {
                    filter.Append('R');
                }
                if ((options.Filters & DifferenceFilterFlags.IncludeUnknown) != 0)
                {
                    filter.Append('X');
                }
                if ((options.Filters & DifferenceFilterFlags.IncludeUnmerged) != 0)
                {
                    filter.Append('U');
                }
                command.AddOption("--diff-filter", filter.ToString ());
            }

            if (!string.IsNullOrEmpty(options.HintPath))
            {
                command.EndOptions();
                command.Add(options.HintPath);
            }
        }
    }
}
