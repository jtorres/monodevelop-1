//*************************************************************************************************
// SymbolicRefCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class SymbolicRefCommand : GitCommand
    {
        public const string Command = "symbolic-ref";

        public SymbolicRefCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public string ReadCurrentBranchName(bool shortName, out HeadType headType)
        {
            using (var command = new ArgumentList(Command))
            {
                if (shortName)
                {
                    command.AddOption("--short");
                }

                command.Add(Head.CanonicalLabel);

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    var executeResult = Execute(command, out string standardOutput);
                     
                    switch (executeResult.ExitCode)
                    {
                        case GitCleanExitCode:
                            {
                                // We have the current (short or long) branch name.
                                headType = HeadType.Normal;
                                return standardOutput.Split('\n')[0];
                            }

                        case GitFatalExitCode:
                            {
                                string[] lines = executeResult.ErrorText.Split('\n');

                                if (lines[0].StartsWith("fatal: ref HEAD is not a symbolic ref", System.StringComparison.Ordinal))
                                {
                                    // A normal detached head. substitute our "(detached)" label.
                                    headType = HeadType.Detached;
                                    return Head.DetachedLabel;
                                }

                                if (lines[0].StartsWith("fatal: No such ref: HEAD", System.StringComparison.Ordinal))
                                {
                                    // HEAD points to a broken (but not missing) ref of some kind.
                                    //    $ echo "broken" > .git/refs/heads/master
                                    // substitute "(unknown)" label, to match "status --porcelain=v2".
                                    headType = HeadType.Malformed;
                                    return Head.MalformedLabel;
                                }
                            }
                            break;

                        case GitUsageExitCode:
                            throw new GitUsageException(executeResult);
                    }

                    // We've gone this far, then something went wrong...
                    throw new GitException(executeResult);
                }
            }
        }

        /// <summary>
        /// <para>Get the current value of head using symbolic-ref.</para>
        /// <para>
        /// You may want to use <see cref="Repository.ReadCurrentHeadValue()"/>
        /// wrapper instead, since it will handle some common error cases.
        /// </para>
        /// </summary>
        public string ReadCurrentHeadValue()
        {
            using (var command = new ArgumentList(Command))
            {
                command.Add(Head.CanonicalLabel);

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    var executeResult = Execute(command, out string standardOutput);

                    switch (executeResult.ExitCode)
                    {
                        case GitCleanExitCode:
                            {
                                // return the first line of output
                                return standardOutput.Split('\n')[0];
                            }

                        default:
                            TestExitCode(executeResult, command);
                            break;
                    }

                    throw new GitException(executeResult);
                }
            }
        }
    }
}
