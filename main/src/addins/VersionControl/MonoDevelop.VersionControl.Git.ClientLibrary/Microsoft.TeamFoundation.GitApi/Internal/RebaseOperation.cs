using System;
using System.IO;
using System.Text;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal sealed class RebaseOperation : Operation
    {
        const string AutoMergingPrefix = "Auto-merging ";
        const string FallingBackPrefix = "Falling back ";
        const string UsingIndexPrefix = "Using index ";
        const string UnstagedChanges = "error: cannot rebase: You have unstaged changes.";

        static readonly OperationParser[] parsers = {
            new ApplyingMessageParser(),
            new WarningAndErrorParser(),
            new HintMessageParser(),
        };

        public RebaseOperation(ExecutionContext context, OperationCallback progressCallback)
            : base(context, progressCallback)
        { }

        int parseState = 0;

        protected override bool ParseOutput (OperationOutput output)
        {
            string line = output.Message;
            switch (parseState)
            {
                case 0:
                    if (line.StartsWith(AutoMergingPrefix, StringComparison.Ordinal)
                      || line.StartsWith(FallingBackPrefix, StringComparison.Ordinal)
                      || line.StartsWith(UsingIndexPrefix, StringComparison.Ordinal))
                    {
                        var status = new GenericOperationMessage(line);
                        Update(status);
                        return true;
                    }
                    else if (line.StartsWith(UnstagedChanges, StringComparison.Ordinal))
                    {
                        AddToMessage(line);
                        parseState = 1;
                        return false;
                    }
                    else if (TryParse(line, parsers, out var progress))
                    {
                        Update(progress);
                    }
                    else if (IsMessageFatal(line))
                    {
                        return false;
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
                        throw new WorkingDirectoryUncommittedException();
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
