using System;
using System.IO;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal sealed class RebaseOperation : Operation
    {
        public RebaseOperation(ExecutionContext context, OperationProgressDelegate progressCallback)
            : base(context, progressCallback)
        { }

        protected override void ParseStdErr(Stream readableStream)
            => ParseInput(readableStream);

        protected override void ParseStdOut(Stream readableStream)
            => ParseInput(readableStream);

        private void ParseInput(Stream readableStream)
        {
            const string AutoMergingPrefix = "Auto-merging ";
            const string FallingBackPrefix = "Falling back ";
            const string UsingIndexPrefix = "Using index ";
            const string UnstagedChanges = "error: cannot rebase: You have unstaged changes.";

            if (ReferenceEquals(readableStream, null))
                throw new ArgumentNullException(nameof(readableStream));
            if (!readableStream.CanRead)
                throw new ArgumentException(nameof(readableStream));

            var parsers = new OperationParser[]
            {
                new ApplyingMessageParser(this),
                new WarningAndErrorParser(this),
                new HintMessageParser(this),
            };

            using (var reader = new StreamReader(readableStream, System.Text.Encoding.UTF8, false, 4 * 1024, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    OperationProgress progress;

                    if (line.StartsWith(AutoMergingPrefix, StringComparison.Ordinal)
                        || line.StartsWith(FallingBackPrefix, StringComparison.Ordinal)
                        || line.StartsWith(UsingIndexPrefix, StringComparison.Ordinal))
                    {
                        var status = new GenericOperationMessage(this, line);

                        Update(status);
                    }
                    else if (line.StartsWith(UnstagedChanges, StringComparison.Ordinal))
                    {
                        string message = ReadToEnd(line, reader);
                        progress = new WarningMessage(this, message, OperationErrorType.Error);
                        Update(progress);

                        throw new WorkingDirectoryUncommittedException();
                    }
                    else if (TryParse(line, parsers, out progress))
                    {
                        Update(progress);
                    }
                    else if (IsMessageFatal(line, reader))
                    {
                        break;
                    }
                    else if ((line = CleanLine(line)) != null)
                    {
                        progress = new GenericOperationMessage(this, line);

                        Update(progress);
                    }
                }
            }
        }
    }
}
