//*************************************************************************************************
// SubmoduleUpdateCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class SubmoduleUpdateCommand : GitCommand
    {
        public const string Command = "submodule update";

        public SubmoduleUpdateCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public void Update(SubmoduleUpdateOptions options)
        {
            using (var command = new ArgumentList(Command))
            {
                if ((options.Flags & SubmoduleUpdateOptionsFlags.Init) != 0)
                {
                    command.AddOption("--init");
                }
                if ((options.Flags & SubmoduleUpdateOptionsFlags.Remote) != 0)
                {
                    command.AddOption("--remote");
                }
                if ((options.Flags & SubmoduleUpdateOptionsFlags.NoFetch) != 0)
                {
                    command.AddOption("--no-fetch");
                }
                if ((options.Flags & SubmoduleUpdateOptionsFlags.Force) != 0)
                {
                    command.AddOption("--force");
                }
#if false
                if ((options.Flags & SubmoduleUpdateOptionsFlags.Recursive) != 0)
                {
                    // We disallow this and force the caller do it manually so that they
                    // get progress reporting properly. command.Append(" --recursive");
                }
#endif
                switch (options.Method)
                {
                    default:
                    case SubmoduleUpdateOptionsMethods.Unspecified:
                        break;
                    case SubmoduleUpdateOptionsMethods.Checkout:
                        command.AddOption("--checkout");
                        break;
                    case SubmoduleUpdateOptionsMethods.Merge:
                        command.AddOption("--merge");
                        break;
                    case SubmoduleUpdateOptionsMethods.Rebase:
                        command.AddOption("--rebase");
                        break;
                }
                if (options.Reference != null)
                {
                    command.AddOption("--reference");
                    command.Add(options.Reference.WorkingDirectory);
                }
                if (options.Depth != null)
                {
                    command.AddOption("--depth");
                    command.Add(options.Depth.ToString());
                }

                if (options.Paths != null)
                {
                    command.EndOptions();
                    foreach (var path in options.Paths)
                    {
                        if (path == null)
                            throw new NullReferenceException(nameof(path));

                        command.Add(path);
                    }
                }

                ExecuteUpdate(command, options.ProgressCallback);
            }
        }

        private void ExecuteUpdate(string command, OperationCallback progressCallback)
        {
            try
            {
                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    var progress = new SubmoduleUpdateOperation(Context, progressCallback);
                    int exitCode = ExecuteProgress(command, progress);

                    TestExitCode(exitCode, command);
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(SubmoduleUpdateCommand)}.{nameof(ExecuteUpdate)}", exception, command))
            {
                // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy.
                throw;
            }
        }
    }
}
