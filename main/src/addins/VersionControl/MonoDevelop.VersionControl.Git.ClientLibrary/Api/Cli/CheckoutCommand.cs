﻿//*************************************************************************************************
// CheckoutCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class CheckoutCommand : GitCommand
    {
        public const string Command = "checkout";

        public CheckoutCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        /// <summary>
        /// <para>Updates the working directory and HEAD to <paramref name="revision"/>.</para>
        /// <para>If <paramref name="revision"/> is not a <see cref="BranchCommand"/> and <paramref name="options"/>
        /// does not contain limiting paths, the repository will be put into "Detached HEAD State".</para>
        /// </summary>
        /// <param name="revision">The branch to update to.</param>
        /// <param name="options">Options for the checkout operation.</param>
        public void Revision(IRevision revision, CheckoutOptions options)
        {
            bool allowNullRevision = false;

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --progress");

                if ((options.Flags & CheckoutOptionFlags.Force) != 0)
                {
                    command.Append(" --force");
                }

                if ((options.Flags & CheckoutOptionFlags.IgnoreSparse) != 0)
                {
                    command.Append(" --ignore-skip-worktree-bits");
                }

                if ((options.Flags & CheckoutOptionFlags.IgnoreWorktrees) != 0)
                {
                    command.Append(" --ignore-other-worktrees");
                }

                if ((options.Flags & CheckoutOptionFlags.Merge) != 0)
                {
                    command.Append(" --merge");
                }

                if ((options.Flags & CheckoutOptionFlags.Ours) != 0)
                {
                    command.Append(" --ours");
                    allowNullRevision = true;
                }
                else if ((options.Flags & CheckoutOptionFlags.Theirs) != 0)
                {
                    command.Append(" --theirs");
                    allowNullRevision = true;
                }

                if (revision == null)
                {
                    if (!allowNullRevision)
                        throw new ArgumentNullException(nameof(revision));
                }
                else
                {
                    // git-checkout will detach HEAD if a canonical branch name is used,
                    // therefore we need to use the branch's "friendly name" to switch branches.
                    if (revision is IBranchName branchName)
                    {
                        command.Append(' ')
                               .Append(branchName.FriendlyName);
                    }
                    else
                    {
                        command.Append(' ')
                               .Append(revision.RevisionText);
                    }
                }

                if (options.Paths != null)
                {
                    // Always precede individual paths with "--" to guard against them
                    // being misinterpreted.
                    command.Append(" --");

                    foreach (var path in options.Paths)
                    {
                        if (path == null)
                            throw new NullReferenceException(nameof(path));

                        // TODO.GitApi: This quoting is not sufficient.
                        command.Append(" \"")
                               .Append(path)
                               .Append("\"");
                    }
                }

                ExecuteCheckout(command, options.ProgressCallback);
            }
        }

        private void ExecuteCheckout(StringBuffer command, OperationProgressDelegate progressCallback)
        {
            try
            {
                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    CheckoutOperation progress = new CheckoutOperation(Context, progressCallback);
                    int exitCode = ExecuteProgress(command, progress);

                    // only throw if the process exited with an actual fatal code
                    TestExitCode(exitCode, command);
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CheckoutCommand)}.{nameof(ExecuteCheckout)}", exception))
            {
                // should not be reached, but we'll re-throw just-in-case
                throw new GitException($"{nameof(CheckoutCommand)}.{nameof(ExecuteCheckout)}", exception);
            }
            catch (Exception exception) when (!(exception is GitException || exception is CheckoutException))
            {
                // wrap any non-`CliException` in a `CliException`
                throw new GitException($"{nameof(CheckoutCommand)}.{nameof(ExecuteCheckout)}", exception);
            }
        }
    }
}
