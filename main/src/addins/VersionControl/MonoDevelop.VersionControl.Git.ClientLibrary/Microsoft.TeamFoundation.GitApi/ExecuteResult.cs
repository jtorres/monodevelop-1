//*************************************************************************************************
// Command.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public struct ExecuteResult
    {
        public static readonly ExecuteResult Canceled = new ExecuteResult(-1, "task canceled.");

        public int ExitCode { get; }
        public string ErrorText { get; }

        public ExecuteResult(int exitCode, string errorText)
        {
            this.ExitCode = exitCode;
            this.ErrorText = errorText;
        }

        public override string ToString()
        {
            return $"ExitCode : {ExitCode}{System.Environment.NewLine}{ErrorText}";
        }
    }
}
