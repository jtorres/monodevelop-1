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
        static readonly OperationParser[] parsers = {
            new WarningParser()
        };

        public StashPushOperation(ExecutionContext context, OperationCallback progressCallback)
            : base(context, progressCallback)
        {
        }

        public readonly StringBuilder stdErr = new StringBuilder();
        public readonly StringBuilder stdOut = new StringBuilder();

        protected override bool ParseOutput (OperationOutput output)
        {
            if (output == OperationOutput.OutputClosed)
                return false;
            string line = output.Message;
            if (TryParse(line, parsers, out var progress))
            {
                Update(progress);
                return true;
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
                Update(new GenericOperationMessage(line));
            }

            return false;
        }
    }
}
