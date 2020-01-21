//*************************************************************************************************
// RevertCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    /// <summary>
    /// Object model wrapper for "git-revert".
    /// </summary>
    internal class RevertCommand : GitCommand
    {
        public const string Command = "revert";

        /// <summary>
        /// Constructor.
        /// </summary>
        public RevertCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        /// <summary>
        /// <para>Revert a commit.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-revert.html</para>
        /// </summary>
        public RevertResult Begin(IRevision revision, RevertOptions options)
        {
            if (revision == null)
                throw new ArgumentNullException(nameof(revision));
            if (options.ProgressCallback == null)
                throw new ArgumentNullException(nameof(options.ProgressCallback));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --no-edit");

                command.Append(" ")
                       .Append(revision.RevisionText);

                try
                {
                    return ExecuteRevert(command, options.ProgressCallback);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RevertCommand)}.{nameof(Begin)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        /// <summary>
        /// <para>Abort the revert a commit.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-revert.html</para>
        /// </summary>
        public RevertResult Abort(OperationProgressDelegate progressCallback)
        {
            if (ReferenceEquals(progressCallback, null))
                throw new ArgumentNullException(nameof(progressCallback));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --abort");

                try
                {
                    ExecuteRevert(command, progressCallback);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RevertCommand)}.{nameof(Abort)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }

                return RevertResult.Aborted;
            }
        }

        /// <summary>
        /// <para>Continue the revert a commit.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-revert.html</para>
        /// </summary>
        public RevertResult Continue(OperationProgressDelegate progressCallback)
        {
            if (ReferenceEquals(progressCallback, null))
                throw new ArgumentNullException(nameof(progressCallback));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --continue");

                try
                {
                    return ExecuteRevert(command, progressCallback);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RevertCommand)}.{nameof(Continue)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        /// <summary>
        /// Execute the revert command.
        /// </summary>
        private RevertResult ExecuteRevert(string command, OperationProgressDelegate progressCallback)
        {
            Debug.Assert(!ReferenceEquals(command, null), $"The `{nameof(command)}` parameter is null.");

            var revertOperation = new RevertOperation(Context, progressCallback);

            try
            {
                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    int exitCode = ExecuteProgress(command, revertOperation);

                    switch (exitCode)
                    {
                        case GitCleanExitCode:
                            return RevertResult.Completed;

                        case GitErrorExitCode:
                            return RevertResult.Conflicts;
                    }

                    TestExitCode(exitCode, $"{nameof(RevertCommand)}.{nameof(ExecuteRevert)}");
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RevertCommand)}.{nameof(ExecuteRevert)}", exception, command))
            {
                // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                throw;
            }

            // will never reach this line as the above `TestExitCode` will have thrown
            return RevertResult.Aborted;
        }
    }
}
