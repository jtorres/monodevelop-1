//*************************************************************************************************
// DiffIndexCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class DiffIndexCommand : DiffCommand
    {
        public const string Command = "diff-index";

        public DiffIndexCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public ITreeDifference ReadStatusTreeDifference(DifferenceOptions options)
        {
            ITreeDifference difference = null;

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --no-ext-diff --raw --full-index --abbrev=40 -r");

                ApplyOptions(command, options);

                command.Append(" HEAD");

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    using (IProcess process = CreateProcess(command))
                    {
                        difference = ParseDiffOutput(process.StdOut, options);

                        TestExitCode(process, $"{nameof(DiffIndexCommand)}.{nameof(ReadStatusTreeDifference)}");
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(DiffIndexCommand)}.{nameof(ReadStatusTreeDifference)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }

            return difference;
        }
    }
}
