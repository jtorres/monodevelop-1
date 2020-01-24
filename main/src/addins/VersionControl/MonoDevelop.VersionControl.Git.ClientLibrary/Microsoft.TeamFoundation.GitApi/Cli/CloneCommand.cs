//*************************************************************************************************
// CloneCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    /// <summary>
    /// Object model wrapper for "git-clone.exe".
    /// </summary>
    internal class CloneCommand : GitCommand
    {
        public const string Command = "clone";

        public CloneCommand(ExecutionContext context, Environment environment, object userData)
            : base(context, environment, userData)
        { }

        public void BareFromRemote(string remoteUrl, string localDirectory, bool mirror, CloneOptions options)
        {
            if (string.IsNullOrWhiteSpace(remoteUrl))
                throw new ArgumentNullException(nameof(remoteUrl));
            if (localDirectory == null)
                throw new ArgumentNullException(nameof(localDirectory));

            // build up a command
            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--bare");
                command.AddOption("--progress");
                command.AddOption("--verbose");

                if (options.RemoteName != null)
                {
                    command.AddOption("--origin");
                    command.Add(options.RemoteName);
                }

                if (options.SingleBranch)
                {
                    command.AddOption("--single-branch");
                }

                if (mirror)
                {
                    command.AddOption("--mirror");
                }

                if (options.Reference != null)
                {
                    command.AddOption("--reference");
                    command.Add(options.Reference.WorkingDirectory);
                }

                command.Add(remoteUrl);
                command.Add(localDirectory);

                // execute the command and return the reference to still running process
                ExecuteClone(command, options.ProgressCallback, default).Wait ();
            }
        }
        public void NormalFromRemote (string remoteUrl, string localDirectory, CloneOptions options)
        {
            NormalFromRemoteAsync (remoteUrl, localDirectory, options).Wait ();
        }

        public async Task NormalFromRemoteAsync (string remoteUrl, string localDirectory, CloneOptions options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(remoteUrl))
                throw new ArgumentNullException(nameof(remoteUrl));
            if (localDirectory == null)
                throw new ArgumentNullException(nameof(localDirectory));

            // build up a command
            using (var command = new ArgumentList(Command))
            {
                command.Prepend("\"credential.helper=\"");
                command.Prepend("-c");
                command.Prepend("\"core.quotepath=false\"");
                command.Prepend("-c");
                command.Prepend("\"log.showSignature=false\"");
                command.Prepend("-c");
                command.AddOption("--progress");
                command.AddOption("--verbose");

                if (options.RemoteName != null)
                {
                    command.AddOption("--origin");
                    command.Add(options.RemoteName);
                }

                if (options.BranchName != null)
                {
                    command.AddOption("--branch");
                    command.Add(options.BranchName);
                }

                if (options.SingleBranch)
                {
                    command.AddOption("--single-branch");
                }

                if (options.Reference != null)
                {
                    command.AddOption("--reference");
                    command.Add(options.Reference.WorkingDirectory);
                }

                if (options.SeperateGitDir != null)
                {
                    command.AddOption("--separate-git-dir", options.SeperateGitDir);
                }

                if (!options.Checkout)
                {
                    command.AddOption("--no-checkout");
                }

                if (options.RecurseSubmodules)
                {
                    command.AddOption("--recurse-submodules");
                }

                command.Add(remoteUrl);

                command.Add(localDirectory);

                // execute the command and return the reference to still running process
                await ExecuteClone(command, options.ProgressCallback, cancellationToken);
            }
        }

        public void ShallowFromRemote(string remoteUrl, string localDirectory, int depth, CloneOptions options)
        {
            throw new NotImplementedException();
        }

        public void BareFromLocal(IRepository localRepository, string localDirectory, CloneOptions options)
        {
            throw new NotImplementedException();
        }

        public void NormalFromLocal(IRepository localRepository, string localDirectory, CloneOptions options)
        {
            throw new NotImplementedException();
        }

        public void ShallowFromLocal(IRepository localRepository, string localDirectory, int depth, CloneOptions options)
        {
            throw new NotImplementedException();
        }

        private Task ExecuteClone(string command, OperationCallback progressCallback, CancellationToken cancellationToken = default)
        {
            // Make sure the directory exists or the process class
            // will fail when validating the environment
            if (!FileSystem.DirectoryExists(_environment.WorkingDirectory))
            {
                FileSystem.CreateDirectory(_environment.WorkingDirectory);
            }

            try
            {
                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    var progress = new CloneOperation(Context, progressCallback);
                    var executeResult = ExecuteProgress(command, progress, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                        return Task.CompletedTask;
                    TestExitCode(executeResult, $"{nameof(CloneCommand)}.{nameof(ExecuteClone)}");
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CloneCommand)}.{nameof(ExecuteClone)}", exception))
            {
                // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                throw;
            }
            return Task.CompletedTask;
        }
    }
}
