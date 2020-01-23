//*************************************************************************************************
// ShProcessCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class ShProcessCommand : Command
    {
        public ShProcessCommand(IExecutionContext context, Environment environment, object userData)
            : base(context, environment, userData)
        {
            if (Git.Installation.Sh == null)
                throw new InvalidOperationException("Bash is unavailable.");

            CreateApplicationNameCallback = GetApplicationName;
            CreateProcessCommandLineCallback = GetCommandLine;
            TerminateProcessCallback = context.TerminateProcessCallback;
        }

        public ShProcessCommand(IExecutionContext context, Environment environment)
            : this(context, environment, null)
        { }

        public ShProcessCommand(Environment environment, object userData)
		    : this(ExecutionContext.Current, environment, userData)
		{ }

        public ShProcessCommand(Environment environment)
            : this(ExecutionContext.Current, environment, null)
        { }

        public new IProcess CreateProcess(string command)
            => base.CreateProcess(command);

        new public ExecuteResult Execute(string command, out string standardOutput)
            => base.Execute(command, out standardOutput);

        new internal ExecuteResult ExecuteProgress(string command, Internal.IOperation progress)
            => base.ExecuteProgress(command, progress);

        private static string GetApplicationName(Environment environment)
        {
            return null;
        }

        private string GetCommandLine(string command, Environment environment)
        {
            const string CommandLineFormat = @"""{0}"" -c ""{1}""";

            System.Diagnostics.Debug.Assert(command != null, $"The `{nameof(command)}` parameter is null");

            if (Git.Installation == null)
                throw new InvalidOperationException();

            string content = string.Format(System.Globalization.CultureInfo.InvariantCulture, CommandLineFormat, Git.Installation.Sh, command);

            return new Internal.StringBuffer(content);
        }
    }
}
