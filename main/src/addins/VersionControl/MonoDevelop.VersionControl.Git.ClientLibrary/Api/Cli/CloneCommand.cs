//*************************************************************************************************
// CloneCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.IO;
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
            using (var command = new StringBuffer(Command))
            {
                command.Append(" --bare --progress --verbose");

                if (options.RemoteName != null)
                {
                    command.Append(" --origin ")
                           .Append(options.RemoteName);
                }

                if (options.SingleBranch)
                {
                    command.Append(" --single-branch");
                }

                if (mirror)
                {
                    command.Append(" --mirror");
                }

                if (options.Reference != null)
                {
                    // TODO.GitApi: path quoting/escaping?
                    command.Append(" --reference ")
                           .Append(PathHelper.EscapePath(options.Reference.WorkingDirectory));
                }

                // TODO.GitApi: path quoting/escaping?
                command.Append(" \"")
                       .Append(remoteUrl)
                       .Append("\"");

                // TODO.GitApi: path quoting/escaping?
                command.Append(" \"")
                           .Append(localDirectory)
                           .Append("\"");

                // execute the command and return the reference to still running process
                ExecuteClone(command, options.ProgressCallback);
            }
        }

        public void NormalFromRemote(string remoteUrl, string localDirectory, CloneOptions options)
        {
            if (string.IsNullOrWhiteSpace(remoteUrl))
                throw new ArgumentNullException(nameof(remoteUrl));
            if (localDirectory == null)
                throw new ArgumentNullException(nameof(localDirectory));

            // build up a command
            using (var command = new StringBuffer(Command))
            {
                command.Append(" --progress --verbose");

                if (options.RemoteName != null)
                {
                    command.Append(" --origin ")
                           .Append(options.RemoteName);
                }

                if (options.BranchName != null)
                {
                    command.Append(" --branch ")
                           .Append(options.BranchName);
                }

                if (options.SingleBranch)
                {
                    command.Append(" --single-branch");
                }

                if (options.Reference != null)
                {
                    command.Append(" --reference \"")
                           .Append(options.Reference.WorkingDirectory)
                           .Append("\"");
                }

                if (options.SeperateGitDir != null)
                {
                    command.Append(" --separate-git-dir=\"")
                           .Append(options.SeperateGitDir)
                           .Append("\"");
                }

                if (!options.Checkout)
                {
                    command.Append(" --no-checkout");
                }

                if (options.RecurseSubmodules)
                {
                    command.Append(" --recurse-submodules");
                }

                command.Append(' ')
                       .Append(PathHelper.EscapePath(remoteUrl));

                command.Append(' ')
                       .Append(PathHelper.EscapePath(localDirectory));

                // execute the command and return the reference to still running process
                ExecuteClone(command, options.ProgressCallback);
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

        private void ExecuteClone(StringBuffer command, OperationProgressDelegate progressCallback)
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
                    int exitCode = ExecuteProgress(command, progress);

                    TestExitCode(exitCode, $"{nameof(CloneCommand)}.{nameof(ExecuteClone)}");
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CloneCommand)}.{nameof(ExecuteClone)}", exception))
            {
                // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                throw;
            }
        }
    }
}
