//*************************************************************************************************
// RebaseCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class RebaseCommand : GitCommand
    {
        public const string Command = "rebase";

        public RebaseCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public RebaseResult BeginRebase(IRevision upstream, RebaseOptions options)
        {
            if (ReferenceEquals(upstream, null))
                throw new ArgumentNullException(nameof(upstream));
            if (!options.IsValid)
                throw new ArgumentException(nameof(options));

            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, options);

                command.Add(upstream.RevisionText);

                try
                {
                    return ExecuteRebase(command, options.ProgressCallback);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RebaseCommand)}.{nameof(BeginRebase)}", exception))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public RebaseResult BeginInteractiveRebase(IRevision upstream, RebaseOptions options)
        {
            throw new NotSupportedException();

            //if (ReferenceEquals(upstream, null))
            //    throw new ArgumentNullException(nameof(upstream));
            //if (!options.IsValid)
            //    throw new ArgumentException(nameof(options));

            //using (Internal.StringBuffer command = new Internal.StringBuffer(Command))
            //{
            //    command.Append(" --interative");

            //    ApplyOptions(command, options);

            //    command.Append(' ')
            //           .Append(upstream.RevisionText);

            //    return ExecuteRebase(command, options.OperationHandler);
            //}
        }

        public RebaseResult AbortRebase(OperationCallback progressCallback)
        {
            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--abort");

                try
                {
                    ExecuteRebase(command, progressCallback);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RebaseCommand)}.{nameof(AbortRebase)}", exception))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }

                return RebaseResult.Aborted;
            }
        }

        public RebaseResult ContinueRebase(OperationCallback progressCallback)
        {
            if (ReferenceEquals(progressCallback, null))
                throw new ArgumentNullException(nameof(progressCallback));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--continue");

                try
                {
                    return ExecuteRebase(command, progressCallback);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RebaseCommand)}.{nameof(ContinueRebase)}", exception))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public RebaseResult SkipRebase(OperationCallback progressCallback)
        {
            if (ReferenceEquals(progressCallback, null))
                throw new ArgumentNullException(nameof(progressCallback));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--skip");

                try
                {
                    return ExecuteRebase(command, progressCallback);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RebaseCommand)}.{nameof(ContinueRebase)}", exception))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        private void ApplyOptions(ArgumentList command, RebaseOptions options)
        {
            Debug.Assert(command != null, $"The `{nameof(command)}` parameter is null.");
            Debug.Assert(options.IsValid, $"The `{nameof(options)}` parameter is invalid.");

            command.AddOption("--verbose --no-stat");

            if ((options.Flags & RebaseOptionFlags.KeepEmpty) > 0)
            {
                command.AddOption("--keep-empty");
            }

            if ((options.Flags & RebaseOptionFlags.PreserveMerges) > 0)
            {
                command.AddOption("--preserve-merges");
            }

            if (!ReferenceEquals(options.NewBase, null))
            {
                command.AddOption("--onto");
                command.Add(options.NewBase.RevisionText);
            }

            if (!ReferenceEquals(options.Root, null))
            {
                command.AddOption("--root");
                command.Add(options.Root.RevisionText);
            }

            if ((options.Flags & RebaseOptionFlags.Merge) > 0)
            {
                command.AddOption("--merge");

                options.MergeStrategy.ApplyOptions(command);
            }
        }

        private RebaseResult ExecuteRebase(string command, OperationCallback progressCallback)
        {
            Debug.Assert(!ReferenceEquals(command, null), $"The `{nameof(command)}` parameter is null.");

            var rebaseOperation = new RebaseOperation(Context, progressCallback);

            try
            {
                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    var executeResult = ExecuteProgress(command, rebaseOperation);

                    switch (executeResult.ExitCode)
                    {
                        case GitCleanExitCode:
                            return RebaseResult.Completed;

                        case GitErrorExitCode:
                        case GitFatalExitCode:
                            return RebaseResult.Conflicts;

                        default:
                            TestExitCode(executeResult, command);
                            // above method will throw, this is here to make the compiler happy
                            throw new GitException(command, executeResult);
                    }
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RebaseCommand)}.{nameof(ExecuteRebase)}", exception, command))
            {
                // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                throw;
            }
        }
    }
}
