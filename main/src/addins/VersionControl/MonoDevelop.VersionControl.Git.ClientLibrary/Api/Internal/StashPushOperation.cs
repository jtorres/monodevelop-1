//*************************************************************************************************
// StashPushOperation.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.IO;
using System.Text;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    [GitErrorMapping(typeof(WorkingDirectoryUnmergedException), Suffix = "needs merge")]
    [GitErrorMapping(typeof(WorkingDirectoryUnmergedException), Suffix = "fatal: git-write-tree: error building trees")]
    internal class StashPushOperation : Operation
    {
        public StashPushOperation(ExecutionContext context, OperationProgressDelegate progressCallback)
            : base(context, progressCallback)
        {
        }

        public readonly StringBuilder stdErr = new StringBuilder();
        public readonly StringBuilder stdOut = new StringBuilder();

        protected sealed override void ParseStdErr(Stream readableStream)
            => ParseInput(readableStream, stdErr);

        protected sealed override void ParseStdOut(Stream readableStream)
            => ParseInput(readableStream, stdOut);

        private void ParseInput(Stream readableStream, StringBuilder sb)
        {
            if (readableStream is null)
                throw new ArgumentNullException(nameof(readableStream));
            if (!readableStream.CanRead)
                throw new ArgumentException(nameof(readableStream));

            var parsers = new OperationParser[]
            {
                new WarningParser(this),
            };

            using (var reader = new StreamReader(readableStream, System.Text.Encoding.UTF8, false, 4 * 1024, true))
            {
                string line;
                while (!((line = reader.ReadLine()) is null))
                {
                    sb.AppendLine(line);
                    if (TryParse(line, parsers, out OperationProgress progress))
                    {
                        Update(progress);
                    }
                    else if (IsMessageFatal(line, reader))
                    {
                        break;
                    }
                    else if (!string.IsNullOrEmpty(line = CleanLine(line)))
                    {
                        // Avoid sending the command line usage tips as progress updates.
                        if (line.StartsWith("  (use ", StringComparison.Ordinal))
                            continue;

                        progress = new GenericOperationMessage(this, line);

                        Update(progress);
                    }
                }
            }
        }
    }
}
