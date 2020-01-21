//*************************************************************************************************
// PullOperation.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

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
        static readonly OperationParser[] parsers = {
            new ReceivingObjectsParser(),
            new ResolvingDeltasParser(),
            new RemoteMessageParser(),
            new AmbiguousReferenceWarningParser(),
            new WarningParser(),
            new HintMessageParser(),
            new RewindingHeadMessageParser(),
            new ApplyingMessageParser()
        };

        internal PullOperation(ExecutionContext context, OperationCallback progressCallback)
            : base(context, progressCallback)
        { }

        public PullCommandResult Result
        {
            get { return _result; }
        }
        private PullCommandResult _result;
        const string PrefixMerging = "Merging";
        const string PrefixAutoMergingFile = "Auto-merging ";
        const string PrefixUpdating = "Updating ";

        int parseState = 0;
        bool isRebase = false;
        StringBuilder message = new StringBuilder();

        protected override bool ParseOutput (OperationOutput output)
        {
            string line = output.Message;
            switch (parseState)
            {
                case 0:
                    if (string.IsNullOrWhiteSpace(line))
                        break;

                    if (line[0] == ' ' || line[0] == '=' || line[0] == '?' || line[0] == '!' || line[0] == '+' || line[0] == '*')
                    {
                        Update(new GenericOperationMessage(line));
                    }
                    else if (TryParse(line, parsers, out var progress))
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
                        return true;
                    }
                    else if (line.StartsWith("CONFLICT", StringComparison.Ordinal)
                         || line.StartsWith("Automatic merge failed", StringComparison.Ordinal))
                    {
                        // Collect any remaining lines and display them to the user
                        AddToMessage(line);
                        _result = PullCommandResult.Conflict;
                        parseState = 1;
                    }
                    else if (line.StartsWith(PrefixMerging)
                         || line.StartsWith(PrefixAutoMergingFile))
                    {
                        var status = new MergeOperationMessage(line);
                        Update(status);
                    }
                    else if (line.StartsWith(PrefixUpdating, StringComparison.Ordinal))
                    {
                        // For example: "Updating 5ef3ff1..7072358" is printed
                        // after git detects the merge is a fast-forward and
                        // before it starts the checkout.  The actual "Fast-forward"
                        // message is printed afterwards.  Use the whole line in the
                        // notification message.
                        var status = new GenericOperationMessage(line);
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
                        AddToMessage(line);
                        _result = PullCommandResult.AlreadyUpToDate;
                        parseState = 2;
                    }
                    else if (line.StartsWith("Fast-forward"))
                    {
                        // Collect any remaining lines and display them to the user
                        AddToMessage(line);
                        _result = PullCommandResult.FastForwardMerge;
                        parseState = 2;

                    }
                    else if (line.StartsWith("Merge made by the"))
                    {
                        // Collect any remaining lines and display them to the user
                        AddToMessage(line);
                        _result = PullCommandResult.NonFastForwardMerge;
                        parseState = 2;
                    }
                    else if (IsMessageFatal(line))
                    {
                        // Operation has been canceled, drop out
                        return false;
                    }
                    else if ((line = CleanLine(line)) != null)
                    {
                        Update(new GenericOperationMessage(line));
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
                    break;
                case 1:
                    if (output == OperationOutput.OutputClosed)
                    {
                        Update(new WarningMessage(message.ToString(), OperationErrorType.Error));
                        break;
                    }
                    AddToMessage(line);
                    break;
                case 2:
                    if (output == OperationOutput.OutputClosed)
                    {
                        Update(new MergeOperationMessage(message.ToString()));
                        break;
                    }
                    AddToMessage(line);
                    break;
            }
            return false;
        }

        private void AddToMessage(string line)
        {
            line = CleanLine(line);
            if (line != null)
            {
                message.AppendLine(line);
            }
        }
    }
}
