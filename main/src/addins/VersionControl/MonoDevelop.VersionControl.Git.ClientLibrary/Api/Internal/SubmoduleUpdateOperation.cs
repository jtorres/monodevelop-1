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
        internal SubmoduleUpdateOperation(ExecutionContext context, OperationProgressDelegate progressCallback)
            : base(context, progressCallback)
        { }

        protected override void ParseStdErr(Stream readableStream)
            => ParseInput(readableStream);

        protected override void ParseStdOut(Stream readableStream)
            => ParseInput(readableStream);

        private void ParseInput(Stream readableStream)
        {
            if (ReferenceEquals(readableStream, null))
                throw new ArgumentNullException(nameof(readableStream));
            if (!readableStream.CanRead)
                throw new ArgumentException(nameof(readableStream));

            var parsers = new OperationParser[]
            {
                new SubmoduleUpdateModeCompletedParser(this),
                new SubmoduleUpdateCloningIntoParser(this),
                new SubmoduleUpdateStepDoneParser(this),
                new SubmoduleUpdateRegistrationCompletedParser(this),

                new SubmoduleUpdateNotInitializedParser(this),
                new SubmoduleUpdateRevisionNotFound(this),
                new SubmoduleUpdateUnableToCompleteParser(this),
                new SubmoduleUpdateUnableToFetchParser(this),
                new SubmoduleUpdateUnmergedParser(this),
                new SubmoduleUpdateUnableToRecurseParser(this),
            };

            using (var reader = new StreamReader(readableStream, System.Text.Encoding.UTF8, false, 4 * 1024, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    OperationProgress progress;

                    if (TryParse(line, parsers, out progress))
                    {
                        Update(progress);
                        continue;
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
