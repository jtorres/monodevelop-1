//*************************************************************************************************
// FetchOperation.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.IO;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal sealed class FetchOperation: Operation
    {
        static readonly OperationParser[] parsers = {
            new ReceivingObjectsParser(),
            new ResolvingDeltasParser(),
            new RemoteMessageParser(),
            new WarningAndErrorParser(),
            new HintMessageParser(),
        };

        internal FetchOperation(ExecutionContext context, OperationCallback progressCallback)
            : base(context, progressCallback)
        {
        }

        int parseState = 0;

        protected override bool ParseOutput (OperationOutput output)
        {
            string line = output.Message;
            switch (parseState)
            {
                case 0:
                    if (output == OperationOutput.OutputClosed)
                        break;
                    if (TryParse(line, parsers, out var progress))
                    {
                        Update(progress);
                        return true;
                    }
                    else if (IsMessageFatal(line))
                    {
                        parseState = 1;
                    }
                    else if ((line = CleanLine(line)) != null)
                    {
                        Update(new GenericOperationMessage(line));
                    }
                    break;
                case 1:
                    ReadFault(output);
                    break;
            }
            return false;
        }
    }
}
