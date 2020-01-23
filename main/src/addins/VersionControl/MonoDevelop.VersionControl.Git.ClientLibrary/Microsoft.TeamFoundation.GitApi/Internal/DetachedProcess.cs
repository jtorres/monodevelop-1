//*************************************************************************************************
// DetachedProcess.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    [DebuggerDisplay ("Process = {Command}")]
    internal abstract class DetachedProcess : Base, IProcess, IDisposable
    {
        protected readonly object _syncpoint = new object ();
        protected bool _started;

        /// <inheritdoc/>
        public bool AllowElevated {
            get { return Volatile.Read (ref _allowElevated); }
            set {
                lock (_syncpoint) {
                    if (_started)
                        throw new InvalidOperationException ("Cannot set the ability to run elevated on a process which has already started.");

                    _allowElevated = value;
                }
            }
        }
        private bool _allowElevated;

        /// <inheritdoc/>
        public string Command {
            get { return Volatile.Read (ref _command); }
            set {
                lock (_syncpoint) {
                    if (_started)
                        throw new InvalidOperationException ("Cannot set the command line of a process which has already started.");

                    _command = value;
                }
            }
        }
        private string _command;

        /// <inheritdoc/>
        public Environment Environment {
            get { return Volatile.Read (ref _environment); }
            set {
                lock (_syncpoint) {
                    if (_started)
                        throw new InvalidOperationException ("Cannot set the environment of a process which has already started.");

                    _environment = value ?? Context.EnvironmentCreate ();
                    _workingDirectory = _environment.WorkingDirectory;
                }
            }
        }
        private Environment _environment;

        /// <inheritdoc/>
        public string WorkingDirectory {
            get { return Volatile.Read (ref _workingDirectory); }
            set {
                if (value == null)
                    throw new ArgumentNullException (nameof (WorkingDirectory));
                if (!FileSystem.DirectoryExists (value))
                    throw new DirectoryNotFoundException (value);

                lock (_syncpoint) {
                    if (_started)
                        throw new InvalidOperationException ("Cannot set the working directory of a process which has already started.");

                    _workingDirectory = value;
                }
            }
        }
        private string _workingDirectory;

        /// <inheritdoc/>
        public Encoding Encoding {
            get { return Volatile.Read (ref _encoding); }
            set {
                lock (_syncpoint) {
                    if (_started)
                        throw new InvalidOperationException ("Cannot set the encoding of a process which has already started.");

                    _encoding = value;
                }
            }
        }
        protected Encoding _encoding;

        /// <inheritdoc/>
        internal CreateApplicationNameDelegate CreateApplicationNameCallback {
            get { return Volatile.Read (ref _createApplicationNameCallback); }
            set {
                if (value == null)
                    throw new ArgumentNullException (nameof (CreateApplicationNameCallback));

                lock (_syncpoint) {
                    if (_started)
                        throw new InvalidOperationException ("Cannot set the command line creator of a process which has already started.");

                    _createApplicationNameCallback = value;
                }
            }
        }
        protected CreateApplicationNameDelegate _createApplicationNameCallback;

        /// <inheritdoc/>
        internal CreateProcessCommandLineDelegate CreateProcessCommandLineCallback {
            get { return Volatile.Read (ref _createProcessCommandLineCallback); }
            set {
                if (value == null)
                    throw new ArgumentNullException (nameof (CreateProcessCommandLineCallback));

                lock (_syncpoint) {
                    if (_started)
                        throw new InvalidOperationException ("Cannot set the command line creator of a process which has already started.");

                    _createProcessCommandLineCallback = value;
                }
            }
        }
        protected CreateProcessCommandLineDelegate _createProcessCommandLineCallback;

        /// <inheritdoc/>
        internal TerminateProcessDelegate TerminateProcessCallback {
            get { return Volatile.Read (ref _terminateProcessCallback); }
            set {
                if (value == null)
                    throw new ArgumentNullException (nameof (TerminateProcessCallback));

                lock (_syncpoint) {
                    if (_started)
                        throw new InvalidOperationException ("Cannot set the terminate process callback of a process which has already started.");

                    _terminateProcessCallback = value;
                }
            }
        }
        protected TerminateProcessDelegate _terminateProcessCallback;

        /// <inheritdoc/>
        public abstract int ExitCode { get; }

        /// <inheritdoc/>
        public abstract bool HasExited { get; }

        /// <inheritdoc/>
        public abstract bool IsWow64 { get; }

        /// <inheritdoc/>
        public abstract int ProcessId { get; }

        /// <inheritdoc/>
        public abstract bool RedirectStandardPipes { get; set; }

        /// <inheritdoc/>
        public abstract StreamReader StandardError { get; }

        /// <inheritdoc/>
        public abstract StreamWriter StandardInput { get; }

        /// <inheritdoc/>
        public abstract StreamReader StandardOutput { get; }

        /// <inheritdoc/>
        public abstract Stream StdErr { get; }

        /// <inheritdoc/>
        public abstract Stream StdIn { get; }

        /// <inheritdoc/>
        public abstract Stream StdOut { get; }

        /// <inheritdoc/>
        public event EventHandler Exited;

        /// <inheritdoc/>
        public event EventHandler<OperationOutput> ProcessOutput;

        /// <inheritdoc/>
        public abstract Task<bool> Exit (TimeSpan timeout);

        /// <inheritdoc/>
        public abstract Task<bool> Exit ();

        /// <inheritdoc/>
        public abstract Task Kill ();

        /// <inheritdoc/>
        public abstract void Start ();

        /// <inheritdoc/>
        public abstract bool WaitForExit (TimeSpan timeout);

        /// <inheritdoc/>
        public abstract void WaitForExit ();

        public DetachedProcess ()
        {
            _command = null;
            _workingDirectory = System.Environment.CurrentDirectory;
            _allowElevated = true;
            _createProcessCommandLineCallback = (string command, Environment environment) => { return command; };
        }

        protected bool _exiting;

        protected void OnExited ()
        {
            Exited?.Invoke (this, EventArgs.Empty);
        }

        internal void ExitNotify ()
        {
            Debug.Assert (_started, "The process has not yet started.");

            // use interlocked to read/write the handler atomically
            // guarantees it is only every called once
            EventHandler exited = null;
            if ((exited = Interlocked.Exchange (ref Exited, null)) != null) {
                // set the exiting value so other operations are aware
                Volatile.Write (ref _exiting, true);

                // run the exited event handler in a separate thread the listeners need to know
                // about the event, but they do not need to block the caller
                Task.Run (() => exited (this, new EventArgs ()));
            }
        }

        internal bool CreateStandardPipes {
            get { return Volatile.Read (ref _createStandardPipes); }
            set {
                lock (_syncpoint) {
                    if (_started)
                        throw new InvalidOperationException ("Cannot set standard pipe creation of a process which has already started.");

                    _createStandardPipes = value;
                }
            }
        }

        protected bool _createStandardPipes;

        protected virtual void OnProcessOutput (OperationOutput output) => ProcessOutput?.Invoke (this, output);

        #region IDisposable Support
        protected bool _disposed = false; // To detect redundant calls

        protected virtual void Dispose (bool disposing)
        {
            _disposed = true;
        }

        public void Dispose ()
        {
            Dispose (true);
        }
        #endregion
    }

}
