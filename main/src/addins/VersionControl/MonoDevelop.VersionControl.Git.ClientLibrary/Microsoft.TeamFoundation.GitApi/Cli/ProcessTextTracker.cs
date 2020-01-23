//
//*************************************************************************************************
// ProcessTextTracker.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Text;
using System.Threading;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    sealed class ProcessTextTracker : StringBuilderCacheable
    {
        private StringBuilder outBuilder;
        private StringBuilder errorBuilder;
        private IProcess process;

        public string Output => outBuilder.ToString();

        public string Error => errorBuilder.ToString();

        public ProcessTextTracker(bool trackOutput = true, bool trackError = true)
        {
            if (trackOutput)
            {
                outBuilder = GetStringBuilder();
            }

            if (trackError)
            {
                errorBuilder = GetStringBuilder();
            }
        }

        internal void Track(IProcess process)
        {
            this.process = process;
            process.ProcessOutput += ProcessOutput;
        }

        private void ProcessOutput(object sender, OperationOutput output)
        {
            var sb = output.Source == OutputSource.Out ? outBuilder : errorBuilder;
            sb?.Append (output.Message);
            sb?.Append ('\n');
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && process != null)
                {
                    if (errorBuilder != null)
                    {
                        PutBackStringBuilder(errorBuilder);
                        errorBuilder = null;
                    }
                    if (outBuilder != null)
                    {
                        PutBackStringBuilder(outBuilder);
                        outBuilder = null;
                    }
                    process.ProcessOutput -= ProcessOutput;
                    process = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
