//*************************************************************************************************
// StashCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class StashCommand : GitCommand
    {
        public const string Command = "stash";

        public StashCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public void Clear()
        {
            using (var command = new StringBuffer(Command))
            {
                command.Append(" clear");

                using (Tracer.TraceCommand(Command, details: command, userData: _userData))
                {
                    int exitCode = Execute(command, out string standardError, out string standardOutput);

                    TestExitCode(exitCode, command, standardError);
                }
            }
        }

        public IReadOnlyList<StashUpdatedFile> Apply(int stashRevision, StashApplyOptions options)
        {
            if (stashRevision < 0)
                throw new ArgumentOutOfRangeException(nameof(stashRevision));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" apply");

                ApplyOptions(command, stashRevision, options);

                using (Tracer.TraceCommand(Command, details: command, userData: _userData))
                {
                    // Ensure there is always a progress callback (if no progress callback was specified
                    // by the user) to ensure the StashApplyOperation always gets called to parse output.
                    OperationProgressDelegate progressCallback = options.ProgressCallback ?? NullProgressCallback;

                    var operation = new StashApplyOperation(Context, progressCallback);

                    int exitCode = ExecuteProgress(command, operation);

                    // When stash apply results in conflicts the error code (exitCode == 1) is expected 
                    // and should not result in an general exception being thrown by TestExitCode.
                    if (operation.NumberOfConflicts == 0)
                    {
                        TestExitCode(exitCode, command);
                    }

                    return operation.UpdatedFiles;
                }
            }
        }

        public void Drop(int stashRevision)
        {
            if (stashRevision < 0)
                throw new ArgumentOutOfRangeException(nameof(stashRevision));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" drop stash@{")
                    .Append(stashRevision)
                    .Append("}");

                using (Tracer.TraceCommand(Command, details: command, userData: _userData))
                {
                    int exitCode = Execute(command, out string standardError, out string standardOutput);

                    TestExitCode(exitCode, command, standardError);
                }
            }
        }

        public IReadOnlyList<StashUpdatedFile> Pop(int stashRevision, StashApplyOptions options)
        {
            if (stashRevision < 0)
                throw new ArgumentOutOfRangeException(nameof(stashRevision));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" pop");

                ApplyOptions(command, stashRevision, options);

                using (Tracer.TraceCommand(Command, details: command, userData: _userData))
                {
                    // Ensure there is always a progress callback (if no progress callback was specified
                    // by the user) to ensure the StashApplyOperation always gets called to parse output.
                    OperationProgressDelegate progressCallback = options.ProgressCallback ?? NullProgressCallback;

                    var operation = new StashApplyOperation(Context, progressCallback);

                    int exitCode = ExecuteProgress(command, operation);

                    // When stash pop results in conflicts the error code (exitCode == 1) is expected 
                    // and should not result in an general exception being thrown by TestExitCode.
                    if (operation.NumberOfConflicts == 0)
                    {
                        TestExitCode(exitCode, command);
                    }

                    return operation.UpdatedFiles;
                }
            }
        }

        public void Push(StashPushOptions options)
        {
            using (var command = new StringBuffer(Command))
            {
                ApplyOptions(command, options);

                using (Tracer.TraceCommand(Command, details: command, userData: _userData))
                {
                    // Ensure there is always a progress callback to ensure the
                    // StashPushOperation always gets called to parse (specific error) output.
                    var operation = new StashPushOperation(Context, NullProgressCallback);

                    int exitCode = ExecuteProgress(command, operation);

                    TestExitCode(exitCode, "stash push");
                }
            }
        }

        public IReadOnlyList<IStash> List(int maxCount)
        {
            const string SafeFatalMessage = "fatal: bad revision '" + ReferenceName.PatternRefStash + "'";

            if (maxCount < -1) // We assume `0` (default(int)) or `-1` here to mean "unlimited"
                throw new ArgumentException(nameof(maxCount));

            var historyOptions = new HistoryOptions
            {
                Flags = HistoryOptionFlags.FirstParent
                      | HistoryOptionFlags.WalkReflogs,
                MaxCount = maxCount,
            };
            var revision = new Revision(ReferenceName.PatternRefStash);

            using (Tracer.TraceCommand(Command, details: "list", userData: _userData))
            {
                try
                {
                    // "git-stash list" is really a call to "git rev-list" with the right options, so let's just do that.
                    var revlist = new RevListCommand(Context, Repository);
                    var list = new List<IStash>();

                    foreach (var commit in revlist.EnumerateCommits(revision, historyOptions))
                    {
                        list.Add((IStash)commit);
                    }

                    return list;
                }
                catch (ExceptionBase exception)
                {
                    if (exception.Message.StartsWith(SafeFatalMessage, StringComparison.Ordinal))
                        return Array.Empty<IStash>();

                    throw;
                }
            }
        }

        internal void ApplyOptions(StringBuffer command, int stashRevision, StashApplyOptions options)
        {
            if ((options.Flags & StashApplyFlags.ApplyIndex) != 0)
            {
                command.Append(" --index");
            }

            if (stashRevision > 0)
            {
                command.Append(" stash@{")
                    .Append(stashRevision)
                    .Append("}");
            }
        }

        internal void ApplyOptions(StringBuffer command, StashPushOptions options)
        {
            command.Append(" push");

            if ((options.Flags & StashPushFlags.KeepIndex) != 0)
            {
                command.Append(" --keep-index");
            }

            if (options.Flags == StashPushFlags.IncludeAll)
            {
                command.Append(" --all");
            }

            if ((options.Flags & StashPushFlags.IncludeUntracked) != 0)
            {
                command.Append(" --include-untracked");
            }

            if (!string.IsNullOrEmpty(options.Message))
            {
                command.Append(" --message ");

                var escapedMessage = PathHelper.EscapePath(options.Message);

                command.Append(escapedMessage);
            }
        }

        private bool NullProgressCallback(OperationProgress progress)
        {
            // do nothing
            return true;
        }
    }
}
