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

            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, options);

                command.AddOption("--delete");
                command.Add(remote.Name);
                command.Add(remoteBranch.RevisionText);

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

            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, options);

                command.Add(remote.Name);
                command.Add(localBranch.RevisionText, ":", localBranch.LocalName);

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

            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, options);

                command.Add(remoteBranch.RemoteName);
                command.Add(localBranch.RevisionText, ":", remoteBranch.LocalName);

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

            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, options);

                command.Add(remote.Name);
                command.Add(localBranch.RevisionText, ":", remoteBranch.LocalName);
                ExecutePushCommand(command, options.ProgressCallback);
            }
        }

        public void UpdateReference(ITagName localTag, IRemote remote, PushOptions options)
        {
            if (ReferenceEquals(localTag, null))
                throw new ArgumentNullException(nameof(localTag));
            if (ReferenceEquals(remote, null))
                throw new ArgumentNullException(nameof(remote));

            using (var command = new ArgumentList(Command))
            {
                // Don't add "--tags" option here as this pushes *all* tags,
                // and is not required when specifying the tag name explicity:
                // git push [remote] [tag]
                ApplyOptions(command, options);

                command.Add(remote.Name);
                command.Add(localTag.RevisionText);

                ExecutePushCommand(command, options.ProgressCallback);
            }
        }

        public void UpdateAllTags(IRemote remote, PushOptions options)
        {
            if (ReferenceEquals(remote, null))
                throw new ArgumentNullException(nameof(remote));

            using (var command = new ArgumentList(Command))
            {
                // Ensure we're setting the "--tags" option to push all tags:
                // git push [remote] --tags
                options.Flags |= PushOptionsFlags.Tags;
                ApplyOptions(command, options);

                command.Add(remote.Name);

                ExecutePushCommand(command, options.ProgressCallback);
            }
        }

        private void ApplyOptions(ArgumentList command, PushOptions options)
        {
            Debug.Assert(command != null, $"The `{nameof(command)}` parameter is null");

            if ((options.Flags & PushOptionsFlags.ForceWithLease) > 0)
            {
                command.AddOption("--force-with-lease");
            }
            else if ((options.Flags & PushOptionsFlags.Force) > 0)
            {
                command.AddOption("--force");
            }

            if ((options.Flags & PushOptionsFlags.SetUpstream) > 0)
            {
                command.AddOption("--set-upstream");
            }

            if ((options.Flags & PushOptionsFlags.Tags) > 0)
            {
                command.AddOption("--tags");
            }

            command.AddOption("--verbose --progress");
        }

        private void ExecutePushCommand(string command, OperationCallback progressCallback)
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
