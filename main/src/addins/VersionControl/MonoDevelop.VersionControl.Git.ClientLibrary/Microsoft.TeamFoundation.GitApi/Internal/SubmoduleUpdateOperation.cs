//*************************************************************************************************
// SubmoduleUpdateOperation.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.IO;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal sealed class SubmoduleUpdateOperation : Operation
    {
        static readonly OperationParser[] parsers = {
            new SubmoduleUpdateModeCompletedParser(),
            new SubmoduleUpdateCloningIntoParser(),
            new SubmoduleUpdateStepDoneParser(),
            new SubmoduleUpdateRegistrationCompletedParser(),

            new SubmoduleUpdateNotInitializedParser(),
            new SubmoduleUpdateRevisionNotFound(),
            new SubmoduleUpdateUnableToCompleteParser(),
            new SubmoduleUpdateUnableToFetchParser(),
            new SubmoduleUpdateUnmergedParser(),
            new SubmoduleUpdateUnableToRecurseParser(),
        };

        internal SubmoduleUpdateOperation(ExecutionContext context, OperationCallback progressCallback)
            : base(context, progressCallback)
        { }

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
            else if ((line = CleanLine(line)) != null)
            {
                Update(new GenericOperationMessage(line));
            }
            return false;
        }
    }
}
