//*************************************************************************************************
// GitCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    [GitErrorMapping(typeof(GitHookConfigurationException), Prefix = "error: cannot spawn", Suffix = "No such file or directory")]
    [GitErrorMapping(typeof(GitHookInteractivityException), Suffix = "/dev/tty: No such device or address")]
    [GitErrorMapping(typeof(BadRevisionException), Prefix = "fatal: bad revision '")]
    [GitErrorMapping(typeof(MissingObjectException), Prefix = "fatal: bad object ")]
    [GitErrorMapping(typeof(GitFatalException), Prefix = Operation.PrefixFatal)]
    [GitErrorMapping(typeof(GitUsageException), Prefix = Operation.PrefixUsage)]
    internal abstract class GitCommand : Command
    {
        public const int GitCleanExitCode = 0;
        public const int GitErrorExitCode = 1;
        public const int GitFatalExitCode = 128;
        public const int GitUsageExitCode = 129;
        public const char Eol = '\n';
        public const char Nul = '\0';

        public static readonly System.Text.Encoding GitEncoding = new System.Text.UTF8Encoding(false, true);

        protected GitCommand(ExecutionContext context, IRepository repository)
            : this(context, context?.Git.GetProcessEnvironment(repository), repository?.UserData)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            _repository = repository;
        }

        protected GitCommand(ExecutionContext context, Environment environment, object userData)
            : base(context, environment, userData)
        {
            CreateApplicationNameCallback = context.CreateApplicationNameCallback;
            CreateProcessCommandLineCallback = context.CreateProcessCommandLineCallback;
            StartProcessCallback = StartProcessImpl;
        }

        private static IReadOnlyDictionary<Type, GitErrorMappingAttributeBase[]> _errorToExceptionMappings;
        protected IRepository _repository;

        public IRepository Repository
        {
            get { return _repository; }
        }

        protected ICollection<ConfigurationEntry> ConfigurationArguments
        {
            get { return (_environment as GitProcessEnvironment)?.ConfigurationArguments; }
        }

        protected void ExecuteCommand(string commandName, StringBuffer command)
        {
            using (var errorBuffer = new StringBuffer())
            using (var outputBuffer = new StringBuffer())
            using (Tracer.TraceCommand(commandName, command, userData: _userData))
            using (IProcess process = CreateProcess(command))
            {
                var stderrTask = Task.Run(() =>
                {
                    errorBuffer.ReadStreamReader(process.StandardError);
                    return errorBuffer.ToString();
                });
                var stdoutTask = Task.Run(() => { process.StdOut.CopyTo(System.IO.Stream.Null); });

                TestExitCode(process, command, stderrTask);
            }
        }

        protected IEnumerable<GitErrorMappingAttributeBase> GetErrorToExceptionMappings()
        {
            if (_errorToExceptionMappings == null)
            {
                // There is a potential race here, but it doesn't matter if multiple threads attempt to initialize the mappings collection. The last will win.
                _errorToExceptionMappings = BuildErrorToExceptionMappings();
            }
            GitErrorMappingAttributeBase[] mappingsForCommand;
            return _errorToExceptionMappings.TryGetValue(GetType(), out mappingsForCommand) ? mappingsForCommand : Enumerable.Empty<GitErrorMappingAttributeBase>();
        }

        internal static void MakeSpace(ByteBuffer buffer, ref int index, ref int count)
        {
            if (index == 0)
            {
                // If the index is already at zero, we cannot shift the buffer to make space instead,
                // we need to grow the buffer.
                buffer.Grow();
            }
            else
            {
                int copyStart = index;
                int copyLength = count - index;

                if (copyLength > 0)
                {
                    // Copy start->start+len to 0->len yes this leaves garbage in the buffer, we do
                    // not care.
                    buffer.Shift(copyStart, copyLength);
                }

                // Reset the read counter to the copy length (start filling at the first garbage byte).
                count = copyLength;
                // Reset the read index to the beginning of the buffer.
                index = 0;
            }
        }

        internal void TestExitCode(int exitCode, string command, string errorText, params int[] safeCodes)
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException(nameof(command));

            if (exitCode != GitCleanExitCode && !safeCodes.Contains(exitCode))
            {
                // Trim off any extra white space picked up from the stderr pipe.
                errorText = errorText?.Trim();
                // Extract the relevant part of the error message (the first line).
                string errorMessage = SplitErrorMessageFromText(errorText ?? "");

                foreach (GitErrorMappingAttributeBase attr in GitErrorMappingAttributeBase.GetMappings(this.GetType()))
                {
                    if (attr.IsMatchingError(errorMessage))
                    {
                        throw attr.CreateException(exitCode, errorText);
                    }
                }

                // If no more specific error->exception mapping was found, raise one of the general exceptions
                // based on the exit code.
                switch (exitCode)
                {
                    case GitFatalExitCode:
                        throw new GitFatalException(errorText ?? command);
                    case GitUsageExitCode:
                        throw new GitUsageException(errorText ?? command);
                    default:
                        throw new GitException(errorText ?? command, exitCode);
                }
            }
        }

        internal void TestExitCode(int exitCode, string command, params int[] safeCodes)
            => TestExitCode(exitCode, command, null as string, safeCodes);

        internal void TestExitCode(IProcess process, string command, Task<string> stderrReadTask, params int[] safeCodes)
        {
            const int WaitForStderrResultSeconds = 1;

            if (ReferenceEquals(process, null))
                throw new ArgumentNullException(nameof(process));
            if (ReferenceEquals(command, null))
                throw new ArgumentNullException(nameof(command));

            try
            {
                // Close the process' standard input handle, this prevents the it from get stuck
                // attempting to read data that is never coming. Since it may have previously been
                // closed and NetFx, for reasons unknown, throws if that's the case we need to guard
                // against that.
                process.StdIn.Close();
            }
            catch { /* squelch */ }

            // Wait for the process to exit before attempting to test its exit code.
            process.WaitForExit();

            /* Explainer for the following:
             *
             * Occasionally, users have seen the process exit without closing the standard error pipe
             * correctly. This has resulted in deadlocked reads from the pipe, with the extended result
             * being that the call to the `GitCommand` deadlocks waiting for `TestExitCode` to exit.
             *
             * Since the root cause has not been determined, this workaround has been put into place.
             * The concept being, if the read from standard error task doesn't complete with in one
             * second of the process, we can assume that the task will never complete (typical is less
             * than 5ms wait), and therefore we should explicitly fail to read the standard error output.
             */

            string standardError = null;

            if (stderrReadTask != null)
            {
                if (stderrReadTask.Wait(TimeSpan.FromSeconds(WaitForStderrResultSeconds)))
                {
                    standardError = stderrReadTask.Result;
                }
                else
                {
                    standardError = "Failed to read error message from process";

                    Tracer.TraceWarning($"Wait for {nameof(stderrReadTask)} ({stderrReadTask.Id}) timed out.", $"command = \"{command}\"", userData: _userData);
                }
            }

            TestExitCode(process.ExitCode, command, standardError, safeCodes);
        }

        internal void TestExitCode(IProcess process, string command, params int[] safeCodes)
            => TestExitCode(process, command, null, safeCodes);

        private static IReadOnlyDictionary<Type, GitErrorMappingAttributeBase[]> BuildErrorToExceptionMappings()
        {
            var mappings = new Dictionary<Type, GitErrorMappingAttributeBase[]>();

            foreach (Type commandType in typeof(GitCommand).GetTypeInfo().Assembly.GetTypes().Where(t => typeof(GitCommand).GetTypeInfo().IsAssignableFrom(t)))
            {
                var attrs = commandType.GetCustomAttributes<GitErrorMappingAttributeBase>(true);
                mappings[commandType] = attrs.ToArray();
            }

            return mappings;
        }

        private static string SplitErrorMessageFromText(string errorText)
        {
            int firstNewline = errorText.IndexOf(Eol);
            return firstNewline < 0 ? errorText : errorText.Substring(0, firstNewline);
        }

        private IProcess StartProcessImpl(string command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var environment = _environment;
            if (environment == null)
                throw new NullReferenceException(nameof(Environment));

            // Ensure the environment passed to the Git process start is a GitProcessEnvironment
            // otherwise the process won't take advantage of the named-pipe solution for Git which
            // avoids the handle inheritance problem / hostage pipe problem.
            if (environment is GitProcessEnvironment gitProcessEnvironment)
            {
                environment = new GitProcessEnvironment(gitProcessEnvironment);
            }
            else
            {
                environment = new GitProcessEnvironment(environment, Context.Git.ConfigurationArguments);
            }

            // Notify the debugger that this operation will require multiple threads to complete.
            // This helps protect the debugger from attempting to evaluate a property or value which
            // will require git.exe to be executed to do so.
            System.Diagnostics.Debugger.NotifyOfCrossThreadDependency();

            // Setting `CreateStandardPipes = true` forces the creation of pipes regardless.
            // Setting `RedirectStandardPipes = false` prevents inheritable handle creation
            // handle redirection via `USE_STANDARD_HANDLES` in `CreateProcess`.

            // Setting `CreateStandardPipes = true` and `RedirectStandardPipes = false` tells
            // the `DetachedProcess` to create pipes and wire them up as if redirection will
            // occur, but to not actually pass the pipe handles for redirection. The idea
            // being that the child process will know how to open the pipe handles and use them.

            var process = new DetachedProcess(Context, userData: _userData)
            {
                Command = command,
                CreateStandardPipes = true,
                Encoding = new System.Text.UTF8Encoding(false, true),
                Environment = environment,
                RedirectStandardPipes = false,
            };

            if (CreateApplicationNameCallback != null)
            {
                process.CreateApplicationNameCallback = CreateApplicationNameCallback;
            }

            if (CreateProcessCommandLineCallback != null)
            {
                process.CreateProcessCommandLineCallback = CreateProcessCommandLineCallback;
            }

            if (TerminateProcessCallback != null)
            {
                process.TerminateProcessCallback = TerminateProcessCallback;
            }

            process.Start();

            return process;
        }
    }
}
