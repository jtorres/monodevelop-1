//*************************************************************************************************
// PullCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    [GitErrorMapping(typeof(PullFailedException))]
    internal class PullCommand : GitCommand
    {
        public const string Command = "pull";

        public PullCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public PullCommandResult FromRemote(IRemote remote, PullOptions options)
        {
            if (remote == null)
                throw new ArgumentNullException(nameof(remote));

            // TODO.GitApi: Should we have a refspec argument ?

            using (var command = new StringBuffer(Command))
            {
                ApplyOptions(command, options);

                command.Append(' ')
                       .Append(remote.Name);

                try
                {
                    return ExecutePull(command, options.ProgressCallback);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(PullCommand)}.{nameof(FromRemote)}", exception))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        private void ApplyOptions(StringBuffer command, PullOptions options)
        {
            command.Append(" --verbose");
            command.Append(" --progress");
            command.Append(" --no-edit");
            command.Append(" --no-stat");

            switch (options.CommitFlags)
            {
                case PullOptionCommitFlags.CommitAfterMerge:
                    command.Append(" --commit");
                    break;

                case PullOptionCommitFlags.NoCommitAfterMerge:
                    command.Append(" --no-commit");
                    break;

                case PullOptionCommitFlags.Default:
                    break;

                default:
                    Debug.Fail("Unexpected case reached");
                    break;
            }

            switch (options.FastForwardFlags)
            {
                case PullOptionFastForwardFlags.FastForwardOnly:
                    command.Append(" --ff-only");
                    break;

                case PullOptionFastForwardFlags.FastForwardOrMerge:
                    command.Append(" --ff");
                    break;

                case PullOptionFastForwardFlags.NoFastForward:
                    command.Append(" --no-ff");
                    break;

                case PullOptionFastForwardFlags.Default:
                    break;

                default:
                    Debug.Fail("Unexpected case reached");
                    break;
            }

            switch (options.SquashFlags)
            {
                case PullOptionSquashFlags.Squash:
                    command.Append(" --squash");
                    break;

                case PullOptionSquashFlags.NoSquash:
                    command.Append(" --no-squash");
                    break;

                case PullOptionSquashFlags.Default:
                    break;

                default:
                    Debug.Fail("Unexpected case reached");
                    break;
            }

            switch (options.SubmoduleRecursion)
            {
                case PullSubmoduleOptions.No:
                    command.Append(" --recurse-submodules=no");
                    break;

                case PullSubmoduleOptions.OnDemand:
                    command.Append(" --recurse-submodules=on-demand");
                    break;

                case PullSubmoduleOptions.Yes:
                    command.Append(" --recurse-submodules=yes");
                    break;

                case PullSubmoduleOptions.Default:
                    break;

                default:
                    Debug.Fail("Unexpected case reached");
                    break;
            }

            if ((options.Flags & PullOptionFlags.Force) != 0)
            {
                command.Append(" --force");
            }

            // TODO.GitApi: --strategy
            // TODO.GitApi: --strategy-option
            // TODO.GitApi: ? --verify-signatures
            // TODO.GitApi: --rebase
            // TODO.GitApi: --all
            // TODO.GitApi: ? --append
            // TODO.GitApi: ? --depth
            // TODO.GitApi: ? --unshallow
            // TODO.GitApi: ? --update-shallow
            // TODO.GitApi: ? --keep
            // TODO.GitApi: ? --no-tags
            // TODO.GitApi: ? --update-head-ok
            // TODO.GitApi: ? --upload-pack
        }

        private PullCommandResult ExecutePull(StringBuffer command, OperationProgressDelegate progressCallback)
        {
            Debug.Assert(command != null, $"The `{nameof(command)}` parameter is null");

            var progress = new PullOperation(Context, progressCallback);

            try
            {
                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    int exitCode = ExecuteProgress(command, progress);

                    PullCommandResult result = progress.Result;

                    if (exitCode == 0
                        || (exitCode == GitErrorExitCode && result == PullCommandResult.Conflict)
                        || (exitCode == GitErrorExitCode && result == PullCommandResult.RebaseConflicts))
                        return result;

                    TestExitCode(exitCode, command);
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(PullCommand)}.{nameof(ExecutePull)}", exception, command))
            {
                // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                throw;
            }

            throw new GitException("Unexpected condition, control should not have escaped the try-catch.");
        }
    }
}
