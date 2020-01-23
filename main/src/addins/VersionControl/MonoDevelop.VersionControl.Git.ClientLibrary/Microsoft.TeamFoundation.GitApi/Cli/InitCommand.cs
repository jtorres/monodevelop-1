//*************************************************************************************************
// InitCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class InitCommand : GitCommand
    {
        public const string Command = "init";

        public InitCommand(ExecutionContext context, Environment environment, object userData)
            : base(context, environment, userData)
        { }

        public void CreateRepository(InitializationOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            // validate `GitCommand` internal state
            if (_repository != null)
                throw new InvalidOperationException($"{nameof(Repository)}");
            if (_environment == null || _environment.WorkingDirectory == null)
                throw new InvalidOperationException($"{nameof(Environment)}");

            using (var command = new ArgumentList(Command))
            {
                // Make sure the directory exists or the process class
                // will fail when validating the environment
                if (!FileSystem.DirectoryExists(_environment.WorkingDirectory))
                {
                    FileSystem.CreateDirectory(_environment.WorkingDirectory);
                }

                ApplyOptions(command, options);

                command.Add(_environment.WorkingDirectory);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        var executeResult = Execute(command, out string standardOutput);

                        TestExitCode(executeResult, command);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(InitCommand)}.{nameof(CreateRepository)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public void UpdateRepository(InitializationOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (_repository == null)
                throw new NullReferenceException(nameof(Repository));

            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, options);

                command.Add(_environment.WorkingDirectory);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        var executeResult = Execute(command, out string standardOutput);

                        TestExitCode(executeResult, command);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(InitCommand)}.{nameof(UpdateRepository)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        private void ApplyOptions(ArgumentList command, InitializationOptions options)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.Bare)
            {
                command.AddOption("--bare");
            }

            if (options.SeparateGitDirectory != null)
            {
                command.AddOption("--separate-git-dir", options.SeparateGitDirectory);
            }

            if (options.TemplateDirectory != null)
            {
                command.AddOption("--template", options.TemplateDirectory);
            }
        }
    }
}
