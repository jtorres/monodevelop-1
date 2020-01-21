//*************************************************************************************************
// FetchCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class FetchCommand : GitCommand
    {
        public const string Command = "fetch";

        public FetchCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public void FromRemote(IRemote remote, FetchOptions options)
        {
            if (remote == null)
                throw new ArgumentNullException(nameof(remote));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --progress --verbose");

                if ((options.Flags & FetchOptionsFlags.AllTags) != 0)
                {
                    if ((options.Flags & FetchOptionsFlags.NoTags) != 0)
                        throw new InvalidOperationException($"{nameof(FetchOptionsFlags.AllTags)} & {nameof(FetchOptionsFlags.NoTags)}");

                    command.Append(" --tags");
                }

                if ((options.Flags & FetchOptionsFlags.Force) != 0)
                {
                    command.Append(" --force");
                }

                if ((options.Flags & FetchOptionsFlags.NoTags) != 0)
                {
                    command.Append(" --no-tags");
                }

                if ((options.Flags & FetchOptionsFlags.Prune) != 0)
                {
                    command.Append(" --prune");
                }

                switch (options.SubmoduleRecusion)
                {
                    case FetchSubmoduleOptions.No:
                        command.Append(" --recurse-submodules-default=no");
                        break;

                    case FetchSubmoduleOptions.OnDemand:
                        command.Append(" --recurse-submodules-default=on-demand");
                        break;

                    case FetchSubmoduleOptions.Yes:
                        command.Append(" --recurse-submodules-default=yes");
                        break;

                    case FetchSubmoduleOptions.Default:
                        break;

                    default:
                        Debug.Fail("Unexpected case reached");
                        break;
                }

                command.Append(' ')
                       .Append(remote.Name);

                if (options.RemoteRevision != null)
                {
                    // TODO: How exactly to handle the case where the branch name on the server and client are different?
                    command.Append($" {options.RemoteRevision.RevisionText}:{options.RemoteRevision.RevisionText}");
                }

                try
                {
                    ExecuteFetch(command, options.ProgressCallback);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(FetchCommand)}.{nameof(FromRemote)}", exception))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        private void ExecuteFetch(StringBuffer command, OperationProgressDelegate progressCallback)
        {
            try
            {
                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    var progress = new FetchOperation(Context, progressCallback);

                    int exitCode = ExecuteProgress(command, progress);

                    TestExitCode(exitCode, $"{nameof(FetchCommand)}.{nameof(ExecuteFetch)}");
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(FetchCommand)}.{nameof(ExecuteFetch)}", exception, command))
            {
                // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                throw;
            }
        }
    }
}
