//*************************************************************************************************
// PushCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class PushCommand : GitCommand
    {
        public const string Command = "push";

        public PushCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public void DeleteReference(IRevision remoteBranch, IRemote remote, PushOptions options)
        {
            if (ReferenceEquals(remoteBranch, null))
                throw new ArgumentNullException(nameof(remoteBranch));
            if (ReferenceEquals(remote, null))
                throw new ArgumentNullException(nameof(remote));

            using (var command = new StringBuffer(Command))
            {
                ApplyOptions(command, options);

                command.Append(" --delete ")
                       .Append(remote.Name)
                       .Append(" ")
                       .Append(remoteBranch.RevisionText);

                ExecutePushCommand(command, options.ProgressCallback);
            }
        }

        public void UpdateReference(IBranch localBranch, IRemote remote, PushOptions options)
        {
            if (ReferenceEquals(localBranch, null))
                throw new ArgumentNullException(nameof(localBranch));
            if (ReferenceEquals(remote, null))
                throw new ArgumentNullException(nameof(remote));
            if (!localBranch.IsLocal)
                throw new ArgumentException(nameof(localBranch));

            using (var command = new StringBuffer(Command))
            {
                ApplyOptions(command, options);

                command.Append(" ")
                       .Append(remote.Name)
                       .Append(" ")
                       .Append(localBranch.RevisionText)
                       .Append(':')
                       .Append(localBranch.LocalName);

                ExecutePushCommand(command, options.ProgressCallback);
            }
        }

        public void UpdateReference(IBranch localBranch, IBranch remoteBranch, PushOptions options)
        {
            if (ReferenceEquals(localBranch, null))
                throw new ArgumentNullException(nameof(localBranch));
            if (ReferenceEquals(remoteBranch, null))
                throw new ArgumentNullException(nameof(remoteBranch));
            if (!localBranch.IsLocal)
                throw new ArgumentException(nameof(localBranch));
            if (remoteBranch.IsLocal)
                throw new ArgumentException(nameof(remoteBranch));

            using (var command = new StringBuffer(Command))
            {
                ApplyOptions(command, options);

                command.Append(" ")
                       .Append(remoteBranch.RemoteName)
                       .Append(" ")
                       .Append(localBranch.RevisionText)
                       .Append(":")
                       .Append(remoteBranch.LocalName);

                ExecutePushCommand(command, options.ProgressCallback);
            }
        }

        public void UpdateReference(IBranchName localBranch, IRemoteName remote, IBranchName remoteBranch, PushOptions options)
        {
            if (ReferenceEquals(localBranch, null))
                throw new ArgumentNullException(nameof(localBranch));
            if (ReferenceEquals(remote, null))
                throw new ArgumentNullException(nameof(remote));
            if (ReferenceEquals(remoteBranch, null))
                throw new ArgumentNullException(nameof(remoteBranch));

            using (var command = new StringBuffer(Command))
            {
                ApplyOptions(command, options);

                command.Append(" ")
                       .Append(remote.Name)
                       .Append(" ")
                       .Append(localBranch.RevisionText)
                       .Append(':')
                       .Append(remoteBranch.LocalName);

                ExecutePushCommand(command, options.ProgressCallback);
            }
        }

        public void UpdateReference(ITagName localTag, IRemote remote, PushOptions options)
        {
            if (ReferenceEquals(localTag, null))
                throw new ArgumentNullException(nameof(localTag));
            if (ReferenceEquals(remote, null))
                throw new ArgumentNullException(nameof(remote));

            using (var command = new StringBuffer(Command))
            {
                // Don't add "--tags" option here as this pushes *all* tags,
                // and is not required when specifying the tag name explicity:
                // git push [remote] [tag]
                ApplyOptions(command, options);

                command.Append(" ")
                    .Append(remote.Name)
                    .Append(" ")
                    .Append(localTag.RevisionText);

                ExecutePushCommand(command, options.ProgressCallback);
            }
        }

        public void UpdateAllTags(IRemote remote, PushOptions options)
        {
            if (ReferenceEquals(remote, null))
                throw new ArgumentNullException(nameof(remote));

            using (var command = new StringBuffer(Command))
            {
                // Ensure we're setting the "--tags" option to push all tags:
                // git push [remote] --tags
                options.Flags |= PushOptionsFlags.Tags;
                ApplyOptions(command, options);

                command.Append(" ")
                    .Append(remote.Name);

                ExecutePushCommand(command, options.ProgressCallback);
            }
        }

        private void ApplyOptions(StringBuffer command, PushOptions options)
        {
            Debug.Assert(command != null, $"The `{nameof(command)}` parameter is null");

            if ((options.Flags & PushOptionsFlags.ForceWithLease) > 0)
            {
                command.Append(" --force-with-lease");
            }
            else if ((options.Flags & PushOptionsFlags.Force) > 0)
            {
                command.Append(" --force");
            }

            if ((options.Flags & PushOptionsFlags.SetUpstream) > 0)
            {
                command.Append(" --set-upstream");
            }

            if ((options.Flags & PushOptionsFlags.Tags) > 0)
            {
                command.Append(" --tags");
            }

            command.Append(" --verbose --progress");
        }

        private void ExecutePushCommand(StringBuffer command, OperationProgressDelegate progressCallback)
        {
            var progress = new PushOperation(Context, progressCallback);

            using (Tracer.TraceCommand(Command, command, userData: _userData))
            {
                try
                {
                    int exitCode = ExecuteProgress(command, progress);

                    TestExitCode(exitCode, $"{nameof(PushCommand)}.{nameof(ExecutePushCommand)}");
                }
                catch (ParseException exception) when (ParseHelper.AddContext("command", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }
    }
}
