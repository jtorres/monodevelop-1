﻿//*************************************************************************************************
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

            using (var command = new StringBuffer(Command))
            {
                // Make sure the directory exists or the process class
                // will fail when validating the environment
                if (!FileSystem.DirectoryExists(_environment.WorkingDirectory))
                {
                    FileSystem.CreateDirectory(_environment.WorkingDirectory);
                }

                ApplyOptions(command, options);

                command.Append(' ')
                       .Append(PathHelper.EscapePath(_environment.WorkingDirectory));

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        string standardError;
                        string standardOutput;

                        int exitcode = Execute(command, out standardError, out standardOutput);

                        TestExitCode(exitcode, command, standardError);
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

            using (var command = new StringBuffer(Command))
            {
                ApplyOptions(command, options);

                command.Append(PathHelper.EscapePath(_environment.WorkingDirectory));

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        string standardError;
                        string standardOutput;

                        int exitcode = Execute(command, out standardError, out standardOutput);

                        TestExitCode(exitcode, command, standardError);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(InitCommand)}.{nameof(UpdateRepository)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        private void ApplyOptions(StringBuffer command, InitializationOptions options)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.Bare)
            {
                command.Append(" --bare");
            }

            if (options.SeparateGitDirectory != null)
            {
                command.Append(" \"--separate-git-dir=")
                       .Append(PathHelper.EscapePath(options.SeparateGitDirectory))
                       .Append("\"");
            }

            if (options.TemplateDirectory != null)
            {
                command.Append(" \"--template=")
                       .Append(PathHelper.EscapePath(options.TemplateDirectory))
                       .Append("\"");
            }
        }
    }
}
