//*************************************************************************************************
// ResetCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    /// <summary>
    /// Reset <see cref="Revision.HeadRevision"/> to the specified <see cref="IRevision"/>.
    /// </summary>
    internal class ResetCommand : GitCommand
    {
        public const string Command = "reset";

        public ResetCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        /// <summary>
        /// <para>Reset current <see cref="Revision.HeadRevision"/> to <paramref name="revision"/>.</para>
        /// <para>Resets the index and working tree. Any changes to tracked files in the working
        /// tree since <paramref name="revision"/> are discarded.</para>
        /// </summary>
        /// <param name="revision">The revision to reset HEAD to.</param>
        public void ResetHard(IRevision revision, OperationProgressDelegate progressCallback)
        {
            if (revision == null)
                throw new ArgumentNullException(nameof(revision));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --hard ")
                       .Append(revision.RevisionText);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        CheckoutOperation progress = new CheckoutOperation(Context, progressCallback);

                        int exitCode = ExecuteProgress(command, progress);

                        TestExitCode(exitCode, command);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(ResetCommand)}.{nameof(ResetHard)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        /// <summary>
        /// <para>Reset current <see cref="Repository.Head"/> to  <paramref name="revision"/>.</para>
        /// <para>Resets index entries and updates files in the working tree that are different
        /// between <paramref name="revision"/> and HEAD. If a file that is different between <commit>
        /// and HEAD has local changes, reset is aborted.</para>
        /// </summary>
        /// <param name="revision">The revision to reset HEAD to.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool ResetKeep(IRevision revision)
        {
            if (revision == null)
                throw new ArgumentNullException(nameof(revision));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --keep ")
                       .Append(revision.RevisionText);

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    string standardError;
                    string standardOutput;

                    int exitcode = Execute(command, out standardError, out standardOutput);

                    switch (exitcode)
                    {
                        case GitCleanExitCode:
                            {
                                ResetRepositoryDetails();

                                return true;
                            }

                        case GitFatalExitCode:
                            throw new GitFatalException(standardError);

                        case GitUsageExitCode:
                            throw new GitUsageException(standardError);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// <para>Reset current <see cref="Revision.HeadRevision"/> to <paramref name="revision"/>.</para>
        /// <para>Resets the index and updates the files in the working tree that are different
        /// between <paramref name="revision"/> and <see cref="Revision.HeadRevision"/>, but keeps those
        /// which are different between the index and working tree (i.e. which have changes which
        /// have not been added).</para>
        /// <para>If a file that is different between <paramref name="revision"/> and the index has
        /// unstaged changes, <see cref="ResetMerge(IRevision)"/> is aborted.</para>
        /// <para>In other words, <see cref="ResetMerge(IRevision)"/> does something like a
        /// `git-read-tree -u -m {commit}`, but carries forward unmerged index entries.</para>
        /// </summary>
        /// <param name="revision">The revision to reset HEAD to.</param>
        public bool ResetMerge(IRevision revision)
        {
            if (revision == null)
                throw new ArgumentNullException(nameof(revision));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --merge ")
                       .Append(revision.RevisionText);

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    string standardError;
                    string standardOutput;

                    int exitcode = Execute(command, out standardError, out standardOutput);

                    TestExitCode(exitcode, Command, standardError);
                }

                ResetRepositoryDetails();
            }

            return false;
        }

        /// <summary>
        /// <para>Reset current <see cref="Revision.HeadRevision"/> to the <paramref name="revision"/>.</para>
        /// <para>Resets the index but not the working tree (i.e., the changed files are preserved but
        /// not marked for commit) and reports what has not been updated.</para>
        /// </summary>
        /// <param name="revision">The revision to reset HEAD to.</param>
        public void ResetMixed(IRevision revision)
        {
            if (revision == null)
                throw new ArgumentNullException(nameof(revision));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --mixed ")
                       .Append(revision.RevisionText);

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    string standardError;
                    string standardOutput;

                    int exitcode = Execute(command, out standardError, out standardOutput);

                    TestExitCode(exitcode, Command, standardError);
                }

                ResetRepositoryDetails();
            }
        }

        /// <summary>
        /// <para>resets the index entries for all <paramref name="paths"/> to their state at
        /// <see cref="Revision.HeadRevision"/>. (It does not affect the working tree or the current
        /// branch.)</para>
        /// <para>This means that <see cref="ResetPaths(ICollection{string})"/> is the opposite
        /// of git-add <paramref name="paths"/>.</para>
        /// <para>After running git reset <paramref name="paths"/> to update the index entry,
        /// you can use <see cref="CheckoutCommand.Revision(IRevision, CheckoutOptions)"/> to check
        /// the contents out of the index to the working tree. Alternatively, using
        /// <see cref="CheckoutCommand.Revision(IRevision, CheckoutOptions)"/> and specifying a commit,
        /// you can copy the contents of a path out of a commit to the index and to the working
        /// tree in one go.</para>
        /// </summary>
        /// <param name="paths">The paths to reset.</param>
        public void ResetPaths(ICollection<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException(nameof(paths));

            Debug.Assert(_repository != null, $"The `{nameof(Repository)}` member is null.");

            if (paths.Count == 0)
            {
                ResetMixed(Revision.HeadRevision);
                return;
            }

            using (var command = new StringBuffer(Command))
            {
                // Examine HEAD
                switch(Repository.ReadHead()?.HeadType)
                {
                    // In the 'normal' case, specify HEAD to remove an ambiguity on which
                    // commit to reset file value from.
                    case HeadType.Detached:
                    case HeadType.Normal:
                        command.Append(" --stdin HEAD");
                        break;

                    // In the 'unborn' case, HEAD doesn't exist and specifying it would be
                    // invalid. Therefore do not specify "HEAD".
                    case HeadType.Unborn:
                        command.Append(" --stdin");
                        break;

                    // In the 'invalid' case, there is no reason to continue, we know Git will
                    // fail to reset. Throw an informative error here.
                    case HeadType.Malformed:
                    case HeadType.Unknown:
                        throw new GitException("Invalid repository state, unable to unstage changes.");
                }

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                using (IProcess process = CreateProcess(command))
                {
                    var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });
                    var stdoutTask = Task.Run(() => { process.StdOut.CopyTo(System.IO.Stream.Null); });

                    // Grab a shared buffer for writing the paths to stdin
                    using (var buffer = new ByteBuffer())
                    {
                        // Write the file list to stdin
                        foreach (var path in paths)
                        {
                            // Convert to UTF-8 explicitly, we've had bugs as a result of
                            // relying on ConHost to provide the correct encoding
                            int len = Encoding.UTF8.GetBytes(path, 0, path.Length, buffer, 0);
                            // Append a new line character
                            buffer[len] = (byte)'\n';

                            process.StdIn.Write(buffer, 0, len + 1);
                        }
                    }

                    // `TestExitCode` will close stdin and await process exit
                    TestExitCode(process, command, stderrTask);

                    ResetRepositoryDetails();
                }
            }
        }

        /// <summary>
        /// <para>Reset current <see cref="Revision.HeadRevision"/> to the <paramref name="revision"/>.</para>
        /// <para>Does not touch the index file or the working tree at all (but resets the HEAD to
        /// <paramref name="revision"/>, just like all modes do).</para>
        /// <para>This leaves all your changed files "Changes to be committed", as git-status would
        /// put it.</para>
        /// </summary>
        /// <param name="revision">The revision to reset HEAD to.</param>
        public void ResetSoft(IRevision revision)
        {
            if (revision == null)
                throw new ArgumentNullException(nameof(revision));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --soft ")
                       .Append(revision.RevisionText);

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    string standardError;
                    string standardOutput;

                    int exitcode = Execute(command, out standardError, out standardOutput);

                    switch (exitcode)
                    {
                        case GitCleanExitCode:
                            {
                                ResetRepositoryDetails();
                            }
                            break;

                        case GitFatalExitCode:
                            throw new GitFatalException(standardError);

                        case GitUsageExitCode:
                            throw new GitUsageException(standardError);
                    }
                }
            }
        }

        private void ResetRepositoryDetails()
        {
            var repository = _repository as Repository;
            if (repository != null)
            {
                repository.ResetDetails();
            }
        }
    }
}
