//*************************************************************************************************
// Command.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Represents a command which results in process creation.
    /// </summary>
    public abstract class Command : Base
    {
        internal const CreateApplicationNameDelegate DefaultCreateApplicationNameCallback = null;
        internal const CreateProcessCommandLineDelegate DefaultCreateProcessCommandLineCallback = null;
        internal const TerminateProcessDelegate DefaultTerminateProcessCallback = null;

        internal Command(IExecutionContext context, Environment environment, object userData)
            : base()
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            SetContext(context);

            _environment = environment;
            _createApplicationNameCallback = DefaultCreateApplicationNameCallback;
            _createProcessCommandLineCallback = DefaultCreateProcessCommandLineCallback;
            _userData = userData;
            _startProcessCallback = CreateProcessImpl;
            _terminateProcessCallback = Context.TerminateProcessCallback;
        }

        protected Environment _environment;
        protected object _userData;
        private CreateApplicationNameDelegate _createApplicationNameCallback;
        private CreateProcessCommandLineDelegate _createProcessCommandLineCallback;
        private StartProcessDelegate _startProcessCallback;
        private TerminateProcessDelegate _terminateProcessCallback;

        /// <summary>
        /// <para>Gets the <see cref="Environment"/> value for this instance of <see cref="Command"/>.</para>
        /// <para>Derived types can set the <see cref="Environment"/> value for this instance.</para>
        /// </summary>
        public Environment Environment
        {
            get { return _environment; }
            protected set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Environment));

                _environment = value;
            }
        }

        /// <summary>
        /// Gets data attached to any trace messages sent from this command to listeners.
        /// </summary>
        public object Metadata
        {
            get { return _userData; }
        }

        internal CreateApplicationNameDelegate CreateApplicationNameCallback
        {
            get { return _createApplicationNameCallback; }
            set { _createApplicationNameCallback = value; }
        }

        internal CreateProcessCommandLineDelegate CreateProcessCommandLineCallback
        {
            get { return _createProcessCommandLineCallback; }
            set { _createProcessCommandLineCallback = value; }
        }

        internal StartProcessDelegate StartProcessCallback
        {
            get { return _startProcessCallback; }
            set { _startProcessCallback = value; }
        }

        internal TerminateProcessDelegate TerminateProcessCallback
        {
            get { return _terminateProcessCallback; }
            set { _terminateProcessCallback = value; }
        }

        /// <summary>
        /// Creates a new instance of <see cref="IProcess"/> using <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command to use to create <see cref="IProcess"/>.</param>
        /// <returns>New instance of <see cref="IProcess"/>.</returns>
        protected IProcess CreateProcess(string command, bool redirect = false)
            => _startProcessCallback(command, redirect);

        /// <summary>
        /// <para>Executes a command.</para>
        /// <para>Uses <see cref="CreateProcess(string)"/> as the underlying mechanism.</para>
        /// <para>Assumes text string output from both stderr and stdout.</para>
        /// <para>Blocks the current thread until the process completes.</para>
        /// </summary>
        /// <param name="command">The command to use to create <see cref="IProcess"/>.</param>
        /// <param name="standardError">If successful, the Utf-16 encoded output of standard error; otherwise <see langword="null"/>.</param>
        /// <param name="standardOutput">If successful, the Utf-16 encoded output of standard output; otherwise <see langword="null"/>.</param>
        /// <returns>The exit code of the process.</returns>
        /// <exception cref="System.TimeoutException">When one, or more, read operations cannot complete after the process has exited.</exception>
        protected int Execute(string command, out string standardError, out string standardOutput)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            StringUtf8 utf8Error;
            StringUtf8 utf8Output;

            int exitCode = Execute(command, out utf8Error, out utf8Output);

            standardError = (string)utf8Error;
            standardOutput = (string)utf8Output;

            return exitCode;
        }

        /// <summary>
        /// <para>Executes a command.</para>
        /// <para>Uses <see cref="CreateProcess(string)"/> as the underlying mechanism.</para>
        /// <para>Assumes text string output from both stderr and stdout.</para>
        /// <para>Blocks the current thread until the process completes.</para>
        /// </summary>
        /// <param name="command">The command to use to create <see cref="IProcess"/>.</param>
        /// <param name="standardError">If successful, the Utf-8 encoded output of standard error; otherwise <see langword="null"/>.</param>
        /// <param name="standardOutput">If successful, the Utf-8 encoded output of standard output; otherwise <see langword="null"/>.</param>
        /// <returns>The exit code of the process.</returns>
        /// <exception cref="System.TimeoutException">When one, or more, read operations cannot complete after the process has exited.</exception>
        protected int Execute(string command, out StringUtf8 standardError, out StringUtf8 standardOutput)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            using (IProcess process = CreateProcess(command, true))
            {
                var sbOut = new StringBuilder ();
                var sbErr = new StringBuilder ();
                process.ProcessOutput += (sender, output) => {
                    var sb = output.Source == OutputSource.Out ? sbOut : sbErr;
                    sb.Append (output.Message);
                    sb.Append ('\n');
                };

                process.WaitForExit();
                standardError = new StringUtf8 (sbErr.ToString ());
                standardOutput = new StringUtf8 (sbOut.ToString ());
                return process.ExitCode;
            }
        }

        internal int ExecuteProgress(string command, IOperation progress, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (progress == null)
                throw new ArgumentNullException(nameof(progress));
            using (IProcess process = CreateProcess (command, true))
            {
                try
                {
                    cancellationToken.Register (async () => {
                        await process.Kill ();
                    });

                    progress.Start(process);
                }
                catch (Exception e)
                {
                    // signal the process to exit, hopefully cleanly
                    progress.Stop();

                    // re-throw the exception
                    throw;
                }
                finally
                {
                    // even though the process should have exited by this point,
                    // we need to wait for exit before asking for the exit code
                    try { process.WaitForExit(); } catch { /* squelch */ }
                }
                if (cancellationToken.IsCancellationRequested)
                    return -1;
                return process.ExitCode;
            }
        }

        private IProcess CreateProcessImpl(string command, bool redirect)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            using (Tracer.TraceProcess("Process startup.", command, userData: _userData))
            {
                var environment = _environment;
                if (environment == null)
                    throw new NullReferenceException(nameof(Environment));

                var process = Context.CreateProcess(userData: _userData);
                process.Command = command;
                process.Environment = environment;
                process.RedirectStandardPipes = redirect;

                if (_createApplicationNameCallback != null)
                {
                    process.CreateApplicationNameCallback = _createApplicationNameCallback;
                }

                if (_createProcessCommandLineCallback != null)
                {
                    process.CreateProcessCommandLineCallback = _createProcessCommandLineCallback;
                }

                if (_terminateProcessCallback != null)
                {
                    process.TerminateProcessCallback = _terminateProcessCallback;
                }

                return process;
            }
        }
    }
}
