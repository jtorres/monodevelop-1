//*************************************************************************************************
// PushOperation.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal sealed partial class PushOperation : Operation
    {
        private const RegexOptions CommonRegexOptions = RegexOptions.CultureInvariant | RegexOptions.Singleline;

        // matches strings line: +	refs/heads/users/jwyman/git-api:refs/heads/users/jwyman/git-api	6a55dea...552b376 (forced update)
        private const string PatternCompleted = @"^\s*\+\t(\S+):(\S+)\t[0-9a-f]+\.{3}[0-9a-f]+\s\(([^\)]+)\)\s*$";
        // matches strings like: !	refs/heads/users/jwyman/git-api:refs/heads/users/jwyman/git-api	[rejected] (non-fast-forward)
        private const string PatternRejected = @"^\s*\!\t([^\s]+)\:([^\s]+)\t\[rejected\]\s+\(([^\(]+)\)\s*$";
        // matches strings like:  = [up to date]      features/ms.tf.git -> originfeatures/gitshell
        private const string PatternUptodate = @"^\s*\=\s+\[up to date\]\s+(\S+) \-\> (\S+)";
        // matches strings like: src refepsc origin/master matches more than one.
        private const string PatternRefspecAmbiguous = @"^\S+ refspec (\S+) matches more than one";
        // matches strings like: src refspec features/foo does not match any.
        private const string PatternRefspecNotMatch = @"^\s*src refspec (\S+) does not match any$";

        internal PushOperation(ExecutionContext context, OperationCallback progressCallback)
            : base(context, progressCallback)
        { }

        //private readonly Lazy<Regex> CompletedRegex = new Lazy<Regex>(() => new Regex(PatternCompleted, CommonRegexOptions));
        private readonly Lazy<Regex> RejectedRegex = new Lazy<Regex>(() => new Regex(PatternRejected, CommonRegexOptions));
        //private readonly Lazy<Regex> UptodateRegex = new Lazy<Regex>(() => new Regex(PatternUptodate, CommonRegexOptions));
        private readonly Lazy<Regex> RefspecAmbiguousRegex = new Lazy<Regex>(() => new Regex(PatternRefspecAmbiguous, CommonRegexOptions));
        private readonly Lazy<Regex> RefspecNotMatchRegex = new Lazy<Regex>(() => new Regex(PatternRefspecNotMatch, CommonRegexOptions));

        static readonly OperationParser[] parsers = {
            new CompressingObjectsParser(),
            new WritingObjectsParser(),
            new WarningAndErrorParser(),
            new HintMessageParser(),
            new RemoteMessageParser(),
        };

        const string HintCheckoutPullPushMatch = "Updates were rejected because a pushed branch tip is behind its remote";
        const string HintRefFetchFirstMatch = "Updates were rejected because the remote contains work that you do";
        const string HintRefNeedsForceMatch = "You cannot update a remote ref that points at a non-commit object,";
        const string HintCurrentBehindRemoteMatch = "Updates were rejected because the tip of your current branch is behind";
        const string HintRefAlreadyExistsMatch = "Updates were rejected because the tag already exists in the remote.";
        const string PrefixRejected = " ! [rejected] ";
        const string PrefixRemoteRejected = " ! [remote rejected] ";

        int parseState = 0;
        ExceptionBase exception = null;

        protected override bool ParseOutput (OperationOutput output)
        {
            string line = output.Message;
            switch (parseState)
            {
                case 0:
                    if (TryParse(line, parsers, out OperationProgress progress))
                    {
                        string hintMessage;
                        if ((hintMessage = (progress as HintOperationMessage)?.Message) != null)
                        {
                            if (hintMessage.StartsWith(HintRefNeedsForceMatch, StringComparison.Ordinal))
                            {
                                exception = new PushErrorException(PushErrorType.RefNeedsForce);
                            }
                            else if (hintMessage.StartsWith(HintRefAlreadyExistsMatch, StringComparison.OrdinalIgnoreCase))
                            {
                                exception = new PushErrorException(PushErrorType.RefAlreadyExists);
                            }
                            else if (hintMessage.StartsWith(HintRefFetchFirstMatch, StringComparison.OrdinalIgnoreCase))
                            {
                                exception = new PushErrorException(PushErrorType.RefFetchFirst);
                            }
                            else if (hintMessage.StartsWith(HintCheckoutPullPushMatch, StringComparison.OrdinalIgnoreCase))
                            {
                                exception = new PushErrorException(PushErrorType.CheckoutPullPush);
                            }
                            else if (hintMessage.StartsWith(HintCurrentBehindRemoteMatch, StringComparison.OrdinalIgnoreCase))
                            {
                                exception = new PushErrorException(PushErrorType.CurrentBehindRemote);
                            }
                        }
                        else if (progress is WarningMessage warnMessage && warnMessage.Error.Type == OperationErrorType.Error)
                        {
                            string message = warnMessage.Error.Message;

                            Match match;
                            if ((match = RefspecNotMatchRegex.Value.Match(message)).Success)
                            {
                                // string refName = match.Groups[1].Value;
                                exception = new ReferenceNotFoundException(message);
                            }
                            else if ((match = RefspecAmbiguousRegex.Value.Match(message)).Success)
                            {
                                exception = new AmbiguousReferenceException(message);
                            }
                        }

                        if (exception != null)
                        {
                            AddToMessage(line);
                            parseState = 1;
                            return false;
                        }

                        Update(progress);
                        return true;
                    }
                    else if (line.StartsWith(PrefixRejected, StringComparison.Ordinal))
                    {
                        // looks like the push was rejected, fault.
                        Match match;
                        if ((match = RejectedRegex.Value.Match(line)).Success)
                        {
                            string localRefName = match.Groups[1].Value;
                            string remoteRefName = match.Groups[2].Value;
                            string reason = match.Groups[3].Value;

                            exception = new PushRejectedException(reason, localRefName, remoteRefName);
                            AddToMessage(line);
                            parseState = 1;
                            return false;
                        }
                    }
                    else if (line.StartsWith(PrefixRemoteRejected, StringComparison.Ordinal))
                    {
                        line = line.Remove(0, PrefixRemoteRejected.Length);

                        exception = new PushErrorException(line, PushErrorType.Rejected);
                        AddToMessage(line);
                        parseState = 1;
                        return false;
                    }
                    else if (IsMessageFatal(line))
                    {
                        break;
                    }
                    else if ((line = CleanLine(line)) != null)
                    {
                        Update(new GenericOperationMessage(line));
                    }
                    break;
                case 1:
                    if (output == OperationOutput.OutputClosed)
                    {
                        Update(new WarningMessage(message.ToString(), OperationErrorType.Error));
                        throw exception;
                    }
                    AddToMessage(line);
                    break;
            }
            return false;
        }

        StringBuilder message = new StringBuilder();
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
