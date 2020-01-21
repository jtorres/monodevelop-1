//*************************************************************************************************
// ShowCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class ShowCommand : GitCommand
    {
        public const string Command = "show";

        public ShowCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public ICommit ReadRevision(IRevision revision)
        {
            if (revision == null)
                throw new ArgumentNullException(nameof(revision));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--format=raw");
                command.AddOption("--no-patch");
                command.Add(revision.RevisionText);

                Commit commit = null;

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    try
                    {
                        StringUtf8 stderr;
                        StringUtf8 stdout;

                        int exitCode = Execute(command, out stderr, out stdout);

                        if (exitCode == GitCommand.GitCleanExitCode)
                        {
                            const string Prefix = "commit ";

                            ObjectId commitId = new ObjectId { };
                            ObjectHeader header = new ObjectHeader { };

                            commitId = stdout.ToObjectId(Prefix.Length);
                            header = new ObjectHeader(commitId, ObjectType.Commit, 0);
                            commit = new Commit(header);
                            commit.SetContextAndCache(Context, _repository as IStringCache);

                            int idx = Prefix.Length + ObjectId.Length + 1;
                            int len = stdout.Length - idx;

                            using (var buffer = new ByteBuffer())
                            {
                                stdout.CopyTo(buffer);

                                commit.ParseData(buffer, ref idx, len, 4, null);
                            }
                        }

                        TestExitCode(exitCode, command, stderr.ToString());
                    }
                    catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(ShowCommand)}.{nameof(ReadRevision)}", exception, command))
                    {
                        // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy.
                        throw;
                    }
                }

                return commit;
            }
        }
    }
}
