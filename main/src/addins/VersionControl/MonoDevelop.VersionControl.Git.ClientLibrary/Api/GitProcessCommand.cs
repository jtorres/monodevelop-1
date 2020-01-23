//*************************************************************************************************
// GitProcessCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class GitProcessCommand : Command
    {
        public GitProcessCommand(IExecutionContext context, Environment environment, object userData)
            : base(context, environment, userData)
        {
            _environment = new GitProcessEnvironment(environment, Context.Git.ConfigurationArguments);

            CreateApplicationNameCallback = Context.Git.GetApplicationName;
            CreateProcessCommandLineCallback = Context.Git.GetCommandLine;
            StartProcessCallback = StartProcessImpl;
        }

        public GitProcessCommand(IExecutionContext context, Environment environment)
            : this(context, environment, null)
        { }

        public GitProcessCommand(Environment environment, object userData)
		    : this(ExecutionContext.Current, environment, userData)
		{ }

        public GitProcessCommand(Environment environment)
            : this(ExecutionContext.Current, environment, null)
        { }

        new public IProcess CreateProcess(string command)
            => base.CreateProcess(command);

        new public int Execute(string command, out string standardError, out string standardOutput)
            => base.Execute(command, out standardError, out standardOutput);

        new internal void ExecuteProgress(string command, IOperation progress)
            => base.ExecuteProgress(command, progress);

        private IProcess StartProcessImpl(string command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            using (Tracer.TraceProcess("Process startup.", command, userData: _userData))
            {
                Environment environment = _environment;
                if (environment == null)
                    throw new NullReferenceException(nameof(Environment));

                var process = new DetachedProcess(Context, userData: _userData)
                {
                    Command = command,
                    Encoding = new System.Text.UTF8Encoding(false, true),
                    Environment = environment,
                };

                if (CreateApplicationNameCallback != null)
                {
                    process.CreateApplicationNameCallback = CreateApplicationNameCallback;
                }

                if (CreateProcessCommandLineCallback != null)
                {
                    process.CreateProcessCommandLineCallback = CreateProcessCommandLineCallback;
                }

                if (TerminateProcessCallback != null)
                {
                    process.TerminateProcessCallback = TerminateProcessCallback;
                }

                process.Start();

                return process;
            }
        }
    }
}
