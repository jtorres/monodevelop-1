//*************************************************************************************************
// PullOperation.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    [GitErrorMapping(typeof(WorkingDirectoryLocalChangesException), Prefix = WorkingDirectoryLocalChangesException.ErrorPrefix)]
    [GitErrorMapping(typeof(PullRebaseStagedChangesException), Prefix = PullRebaseStagedChangesException.StagedChangesPrefix)]
    [GitErrorMapping(typeof(PullRebaseUnstagedChangesException), Prefix = PullRebaseUnstagedChangesException.UnstagedChangesPrefix)]
    [GitErrorMapping(typeof(WorkingDirectoryUnstagedException), Prefix = WorkingDirectoryUnstagedException.UntrackedPrefix)]
    [GitErrorMapping(typeof(DetachedHeadException), Prefix = DetachedHeadException.ErrorPrefix)]
    [GitErrorMapping(typeof(NoMergeCandidatesException), Prefix = "There are no candidates for merging among the refs that you just fetched.")]
    [GitErrorMapping(typeof(NoMergeCandidatesException), Prefix = "You asked to pull from the remote '")]
    [GitErrorMapping(typeof(NoMergeCandidatesException), Prefix = "There is no tracking information for the current branch.")]
    [GitErrorMapping(typeof(RemoteRefNotFoundException), Prefix = "Your configuration specifies to merge with the ref '")]
    [GitErrorMapping(typeof(MergeFailedException), Prefix = "No merge strategy handled the merge")]
    [GitErrorMapping(typeof(MergeFailedException), Prefix = "Merge with strategy " /*%s failed*/)]
    internal sealed class PullOperation: Operation
    {
        internal PullOperation(ExecutionContext context, OperationProgressDelegate progressCallback)
            : base(context, progressCallback)
        { }

        public PullCommandResult Result
        {
            get { return _result; }
        }
        private PullCommandResult _result;

        protected override void ParseStdErr(Stream readableStream)
            => ParseOutput(readableStream);

        protected override void ParseStdOut(Stream readableStream)
            => ParseOutput(readableStream);

        private void ParseOutput(Stream readableStream)
        {
            if (ReferenceEquals(readableStream, null))
                throw new ArgumentNullException(nameof(readableStream));
            if (!readableStream.CanRead)
                throw new ArgumentException(nameof(readableStream));

            var parsers = new OperationParser[]
            {
                new ReceivingObjectsParser(this),
                new ResolvingDeltasParser(this),
                new RemoteMessageParser(this),
                new AmbiguousReferenceWarningParser(this),
                new WarningParser(this),
                new HintMessageParser(this),
                new RewindingHeadMessageParser(this),
                new ApplyingMessageParser(this),
            };

            bool isRebase = false;

            using (var reader = new StreamReader(readableStream, System.Text.Encoding.UTF8, false, 4 * 1024, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    const string PrefixMerging = "Merging";
                    const string PrefixAutoMergingFile = "Auto-merging ";
                    const string PrefixUpdating = "Updating ";

                    OperationProgress progress;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line[0] == ' ' || line[0] == '=' || line[0] == '?' || line[0] == '!' || line[0] == '+' || line[0] == '*')
                    {
                        progress = new GenericOperationMessage(this, line);

                        Update(progress);
                    }
                    else if (TryParse(line, parsers, out progress))
                    {
                        // If this is a "Rewind Head" message, then
                        // we know that pull is performing a rebase operation
                        // (as opposed to a merge operation) to update the local
                        // branch.
                        if (progress is RewindingHeadMessage)
                        {
                            isRebase = true;
                        }
                        Update(progress);
                    }
                    else if (line.StartsWith("CONFLICT", StringComparison.Ordinal)
                         || line.StartsWith("Automatic merge failed", StringComparison.Ordinal))
                    {
                        // Collect any remaining lines and display them to the user
                        string message = ReadToEnd(line, reader);

                        progress = new WarningMessage(this, message, OperationErrorType.Error);

                        Update(progress);

                        _result = PullCommandResult.Conflict;
                    }
                    else if (line.StartsWith(PrefixMerging)
                         || line.StartsWith(PrefixAutoMergingFile))
                    {
                        var status = new MergeOperationMessage(this, line);
                        Update(status);
                    }
                    else if (line.StartsWith(PrefixUpdating, StringComparison.Ordinal))
                    {
                        // For example: "Updating 5ef3ff1..7072358" is printed
                        // after git detects the merge is a fast-forward and
                        // before it starts the checkout.  The actual "Fast-forward"
                        // message is printed afterwards.  Use the whole line in the
                        // notification message.
                        var status = new GenericOperationMessage(this, line);
                        Update(status);

                        // Go ahead and assume successful fast-forward result
                        // because sometimes we lose the last line of output
                        // with the fast-forward message.
                        _result = PullCommandResult.FastForwardMerge;
                    }
                    // Post-merge messages are printed immediately before exiting,
                    // so no need for progress update.  Just remember the final
                    // resolution.
                    else if (line.StartsWith("Already up") // Match both "Already up-to-date." and "Already up to date" (note the hyphen vs space).
                         || (line.StartsWith("Current branch ", StringComparison.Ordinal)
                            && line.EndsWith(" is up to date.", StringComparison.Ordinal)))
                    {
                        // Collect any remaining lines and display them to the user
                        string message = ReadToEnd(line, reader);

                        progress = new MergeOperationMessage(this, message);

                        Update(progress);

                        _result = PullCommandResult.AlreadyUpToDate;
                    }
                    else if (line.StartsWith("Fast-forward"))
                    {
                        // Collect any remaining lines and display them to the user
                        string message = ReadToEnd(line, reader);

                        progress = new MergeOperationMessage(this, message);

                        Update(progress);

                        _result = PullCommandResult.FastForwardMerge;
                    }
                    else if (line.StartsWith("Merge made by the"))
                    {
                        // Collect any remaining lines and display them to the user
                        string message = ReadToEnd(line, reader);

                        progress = new MergeOperationMessage(this, message);

                        Update(progress);

                        _result = PullCommandResult.NonFastForwardMerge;
                    }
                    else if (IsMessageFatal(line, reader))
                    {
                        // Operation has been canceled, drop out
                        break;
                    }
                    else if ((line = CleanLine(line)) != null)
                    {
                        progress = new GenericOperationMessage(this, line);

                        Update(progress);
                    }
                }

                // If this was a rebase operation, then tweak the result for
                // a "rebase" status.
                if (isRebase)
                {
                    if (_result == PullCommandResult.Undefined)
                    {
                        _result = PullCommandResult.Rebase;
                    }
                    else if (_result == PullCommandResult.Conflict)
                    {
                        _result = PullCommandResult.RebaseConflicts;
                    }
                    else
                    {
                        Debug.Fail(string.Format("_result value ({0}) was unexpected for a rebase operation", _result));
                    }
                }
            }
        }
    }
}
