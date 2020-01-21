//*************************************************************************************************
// BranchCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class BranchCommand : GitCommand
    {
        public const string Command = "branch";

        public BranchCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public IBranch Create(string branchName, IRevision createAt, bool returnNewBranch)
        {
            if (branchName == null)
                throw new ArgumentNullException(nameof(branchName));
            if (createAt == null)
                throw new ArgumentNullException(nameof(createAt));
            if (!BranchName.IsLegalName(branchName))
                throw BranchNameException.FromName(branchName);

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --force --verbose ")
                       .Append(branchName)
                       .Append(' ')
                       .Append(createAt.RevisionText);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        string standardError;
                        string standardOutput;

                        int exitCode = Execute(command, out standardError, out standardOutput);

                        TestExitCode(exitCode, command, standardError);
                    }
                }
                catch (Exception exception) when (!(exception is BranchCreationException))
                {
                    throw new BranchCreationException(branchName, exception);
                }
            }

            if (returnNewBranch)
            {
                var options = new ReferenceOptions()
                {
                    Flags = ReferenceOptionFlags.RefsHeads
                };
                var collection = new ForEachRefCommand(Context, _repository).ReadCollection(options);

                return collection[branchName] as IBranch;
            }

            return null;
        }
        public void Create(string branchName, IRevision CreateAt)
            => Create(branchName, CreateAt, false);

        public IBranch Create(string branchName, IBranch upstream, bool returnNewBranch)
        {
            if (branchName == null)
                throw new ArgumentNullException(nameof(branchName));
            if (upstream == null)
                throw new ArgumentNullException(nameof(upstream));
            if (!Branch.IsLegalName(branchName))
                throw BranchNameException.FromName(branchName);

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --force --verbose ")
                       .Append(branchName)
                       .Append(" --set-upstream ")
                       .Append(upstream.CanonicalName);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        string standardError;
                        string standardOutput;

                        int exitCode = Execute(command, out standardError, out standardOutput);

                        TestExitCode(exitCode, command, standardError);
                    }
                }
                catch (Exception exception) when (!(exception is BranchCreationException))
                {
                    throw new BranchCreationException(branchName, exception);
                }
            }

            if (returnNewBranch)
            {
                var options = new ReferenceOptions()
                {
                    Flags = ReferenceOptionFlags.RefsHeads
                };
                var collection = new ForEachRefCommand(Context, _repository).ReadCollection(options);

                return collection[branchName] as IBranch;
            }

            return null;
        }
        public void Create(string branchName, IBranch upstream)
            => Create(branchName, upstream, false);

        public void Delete(IBranch branch, DeleteBranchOptions options)
        {
            if (branch == null)
                throw new ArgumentNullException(nameof(branch));

            Delete(branch.FriendlyName, options);
        }

        public void Delete(string branchName, DeleteBranchOptions options)
        {
            if (String.IsNullOrEmpty(branchName))
                throw new ArgumentNullException(nameof(branchName));

            using (var command = new StringBuffer(Command))
            {
                if (options.Force)
                {
                    command.Append(" --force");
                }
                command.Append(" --delete --verbose ");
                command.Append(branchName);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        string standardError;
                        string standardOutput;

                        int exitCode = Execute(command, out standardError, out standardOutput);

                        if (exitCode != GitCleanExitCode)
                        {
                            ParseDeleteErrorText(standardError);
                        }

                        TestExitCode(exitCode, command, standardError);
                    }
                }
                catch (Exception exception) when (!(exception is ExceptionBase))
                {
                    throw new BranchDeletionException(branchName, exception);
                }
            }
        }

        public void SetUpstream(IBranchName branch, IBranchName upstreamBranch)
        {
            if (branch == null)
                throw new ArgumentNullException(nameof(branch));
            if (upstreamBranch == null)
                throw new ArgumentNullException(nameof(upstreamBranch));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --set-upstream-to=")
                       .Append(upstreamBranch.FriendlyName)
                       .Append(" --verbose ")
                       .Append(branch.FriendlyName);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        string standardError;
                        string standardOutput;

                        int exitCode = Execute(command, out standardError, out standardOutput);

                        TestExitCode(exitCode, command, standardError);
                    }
                }
                catch (Exception exception) when (!(exception is BranchDeletionException))
                {
                    throw new BranchDeletionException(branch.FriendlyName, exception);
                }
            }
        }

        public void SetUpstream(IBranchName branch, IRemoteName upstreamRemote, IBranchName upstreamBranch)
        {
            if (branch == null)
                throw new ArgumentNullException(nameof(branch));
            if (upstreamBranch == null)
                throw new ArgumentNullException(nameof(upstreamBranch));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --set-upstream-to=")
                           .Append(upstreamRemote.Name)
                           .Append('/')
                           .Append(upstreamBranch.FriendlyName)
                           .Append(" --verbose ")
                           .Append(branch.FriendlyName);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        string standardError;
                        string standardOutput;

                        int exitCode = Execute(command, out standardError, out standardOutput);

                        TestExitCode(exitCode, command, standardError);
                    }
                }
                catch (Exception exception) when (!(exception is BranchDeletionException))
                {
                    throw new BranchDeletionException(branch.FriendlyName, exception);
                }
            }
        }

        public void RemoveUpstream(IBranchName branch)
        {
            if (branch == null)
                throw new ArgumentNullException(nameof(branch));

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --unset-upstream --verbose ")
                       .Append(branch.FriendlyName);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        string standardError;
                        string standardOutput;

                        int exitCode = Execute(command, out standardError, out standardOutput);

                        if (exitCode == GitFatalExitCode)
                        {
                            if (standardError.EndsWith("has no upstream information\n", StringComparison.Ordinal))
                                throw new BranchModificationException(FormattableString.Invariant($"'{branch.FriendlyName}' has no upstream to remove."));
                        }

                        TestExitCode(exitCode, command, standardError);
                    }
                }
                catch (Exception exception) when (!(exception is BranchModificationException))
                {
                    throw new BranchModificationException(branch.FriendlyName, exception);
                }
            }
        }

        public IBranch Rename(IBranchName branch, string newName, bool returnNewBranch)
        {
            if (branch == null)
                throw new ArgumentNullException(nameof(branch));
            if (newName == null)
                throw new ArgumentNullException(nameof(newName));
            if (!BranchName.IsLegalName(newName))
                throw BranchNameException.FromName(newName);

            using (var command = new StringBuffer(Command))
            {
                command.Append(" --force --move --verbose ")
                       .Append(branch.FriendlyName)
                       .Append(' ')
                       .Append(newName);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        string standardError;
                        string standardOutput;

                        int exitCode = Execute(command, out standardError, out standardOutput);

                        TestExitCode(exitCode, command, standardError);
                    }
                }
                catch (Exception exception) when (!(exception is BranchModificationException))
                {
                    throw new BranchModificationException(branch.FriendlyName, exception);
                }
            }

            if (returnNewBranch)
            {
                var options = new ReferenceOptions()
                {
                    Flags = ReferenceOptionFlags.RefsHeads
                };
                var collection = new ForEachRefCommand(Context, _repository).ReadCollection(options);

                return collection[newName] as IBranch;
            }

            return null;
        }

        public void Rename(IBranchName branch, string newName)
            => Rename(branch, newName, false);

        /// <summary>
        /// Parse the standard error text from Delete Branch command to throw
        /// command specific errors.
        /// </summary>
        /// <param name="text"></param>
        internal static void ParseDeleteErrorText(string text)
        {
            Regex changesNotMergedIntoRemotePattern = new Regex(@"^warning: not deleting branch (\S+) that is not yet merged to$");
            Regex remoteBranchNamePattern = new Regex(@"^ +'(\S+)', even though it is merged to HEAD.$");
            Regex unmergedChangesPattern = new Regex(@"^error: The branch '(\S+)' is not fully merged.$");

            // Split text into lines
            string[] lines = text.Split('\n');
            string remoteBranchName = null;

            for (int i = 0; i < lines.Length; i++)
            {
                if (changesNotMergedIntoRemotePattern.IsMatch(lines[i]))
                {
                    // attempt to read the remote branch name from the next line
                    if (lines.Length > i + 1)
                    {
                        var match = remoteBranchNamePattern.Match(lines[i + 1]);
                        if (match.Success && match.Groups.Count == 2)
                        {
                            remoteBranchName = match.Groups[1].Value;
                        }
                    }
                }
                else if (unmergedChangesPattern.IsMatch(lines[i]))
                {
                    if (remoteBranchName != null)
                    {
                        throw new WorkingDirectoryUnmergedException(text, remoteBranchName);
                    }
                    else
                    {
                        throw new WorkingDirectoryUnmergedException(text);
                    }
                }
            }
        }
    }
}
