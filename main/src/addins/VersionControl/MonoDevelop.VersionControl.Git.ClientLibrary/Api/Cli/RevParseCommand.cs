//*************************************************************************************************
// RevParseCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.IO;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class RevParseCommand : GitCommand
    {
        public const string Command = "rev-parse";

        public RevParseCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public RepositoryDetails GetRepositoryDetails()
        {
            using (var command = new StringBuffer(Command))
            {
                try
                {
                    command.Append(" --git-dir --git-common-dir --is-bare-repository --git-path index --git-path objects/ --git-path hooks/ --git-path info/ --git-path logs/ --git-path HEAD --git-path description --git-path refs/ --show-toplevel --shared-index-path");

                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        int exitCode = Execute(command, out string standardError, out string standardOutput);

                        TestExitCode(exitCode, command, standardError);

                        string[] lines = standardOutput.Split('\n');

                        // If rev-parse doesn't return enough data throw.
                        if (lines.Length < 3)
                        {
                            // If standard error contains a message from git, display it otherwise use the default message.
                            throw string.IsNullOrWhiteSpace(standardError)
                                ? new RevParseParseException("split", new StringUtf8(standardOutput), 0)
                                : new RevParseParseException(standardError, "split", new StringUtf8(standardOutput), 0);
                        }

                        string gitDirectory = lines[0];
                        string commonDirectory = lines[1];
                        string isBare = lines[2];
                        string indexFile = lines[3];
                        string objectsDirectory = lines[4];
                        string hooksDirectory = lines[5];
                        string infoDirectory = lines[6];
                        string logsDirectory = lines[7];
                        string headFile = lines[8];
                        string descriptionFile = lines[9];
                        string refsDirectory = lines[10];
                        string workingDirectory = lines[11];
                        string sharedIndexFile = (lines.Length > 12)
                            ? lines[12]
                            : null;

                        if (!Path.IsPathRooted(gitDirectory))
                        {
                            gitDirectory = Path.Combine(workingDirectory, gitDirectory);
                        }

                        bool isBareRepository = bool.Parse(isBare);

                        return new RepositoryDetails(commonDirectory,
                                                     descriptionFile,
                                                     gitDirectory,
                                                     hooksDirectory,
                                                     infoDirectory,
                                                     indexFile,
                                                     logsDirectory,
                                                     objectsDirectory,
                                                     sharedIndexFile,
                                                     workingDirectory,
                                                     isBareRepository);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RevParseCommand)}.{nameof(GetRepositoryDetails)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public string ReadCurrentBranchName(bool shortName, out HeadType headType)
        {
            using (var command = new StringBuffer(Command))
            {
                command.Append(" --verify ");
                command.Append(Head.CanonicalLabel);
                if (shortName)
                {
                    command.Append(" --abbrev-ref");
                }
                else
                {
                    command.Append(" --symbolic-full-name");
                }

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    int exitCode = Execute(command, out string standardError, out string standardOutput);

                    switch (exitCode)
                    {
                        case GitCleanExitCode:
                            {
                                string[] lines = standardOutput.Split('\n');

                                if (string.Equals(lines[0], Head.CanonicalLabel, StringComparison.Ordinal))
                                {
                                    // rev-parse returns "HEAD" when detached.
                                    // substitute our "(detached)" label.
                                    headType = HeadType.Detached;
                                    return Head.DetachedLabel;
                                }

                                headType = HeadType.Normal;
                                return lines[0];
                            }

                        case GitFatalExitCode:
                            {
                                if (standardError.StartsWith("fatal: Needed a single revision", StringComparison.Ordinal))
                                {
                                    // When prior to the initial commit, rev-parse fails to
                                    // lookup HEAD.  There may be a valid ref in .git/HEAD,
                                    // but it cannot dereference it, so we get an error.
                                    // Substitute our "(initial)" label rather than throwing.
                                    headType = HeadType.Unborn;
                                    return Head.UnbornLabel;
                                }
                            }
                            break;

                        default:
                            TestExitCode(exitCode, command, standardError);
                            break;
                    }

                    throw new GitException(standardError, exitCode);
                }
            }
        }

        public string ReadCurrentHeadValue()
        {
            using (var command = new StringBuffer(Command))
            {
                command.Append(" --verify ");
                command.Append(Head.CanonicalLabel);

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    int exitCode = Execute(command, out string standardError, out string standardOutput);

                    switch (exitCode)
                    {
                        case GitCleanExitCode:
                            {
                                string[] lines = standardOutput.Split('\n');
                                return lines[0];
                            }

                        default:
                            TestExitCode(exitCode, command, standardError);
                            break;
                    }

                    throw new GitException(standardError, exitCode);
                }
            }
        }
    }
}