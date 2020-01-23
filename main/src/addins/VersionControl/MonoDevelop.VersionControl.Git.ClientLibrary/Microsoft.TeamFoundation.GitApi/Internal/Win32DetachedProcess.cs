//*************************************************************************************************
// DetachedProcess.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    /// <summary>
    /// <para>Represents a process or process group, which is created and detached from the current process.</para>
    /// <para>The standard pipe handles for the process are owned by the creating process, without granting
    /// TTY status to them.</para>
    /// <para>The child process is created with attached JobObjects and the options to enforce SaferLevels
    /// (forcing the process to operate non-elevated regardless of the parent process' elevation status).</para>
    /// <para>Note that new process groups are created via `CreateProcess` and not via `ShellEx` meaning that
    /// a non-elevated parent cannot spawn an elevated child process via this class. For creation of elevated
    /// child process' see <see cref="System.Diagnostics.Process"/>.</para>
    /// </summary>
    [DebuggerDisplay ("Process = {Command}")]
    internal sealed class Win32DetachedProcess : DetachedProcess
    {
        // Use a relatively large buffer size because we're intending on moving binary streams via
        // stdin/out and not text values. The value is capped at 16KB for Win7 32-bit compatibility.
        private const int BufferSize = 4 * 1024;
        private const bool DefaultRedirectStandardPipes = true;

        internal Win32DetachedProcess (IExecutionContext context, object userData = null)
            : base ()
        {
            SetContext (context);

            _pipeNamespace = DefaultPipeNamespace;

            _disposed = false;
            _exiting = false;
            _userData = userData;
            _redirectStandardPipes = true;
            _started = false;

        }

        ~Win32DetachedProcess ()
        {
            Dispose ();
        }


        /// <summary>
        /// Gets the value that the associated process specified when it terminated.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        /// <exception cref="InvalidOperationException">Before the process has exited.</exception>
        /// <exception cref="ApplicationException">If the process failed to start correctly.</exception>
        public override int ExitCode {
            get {
                lock (_syncpoint) {
                    if (!_started)
                        throw new InvalidOperationException ("Cannot get the exit code of a process which has not yet started.");

                    TestProcessState ();

                    if (!_hasExited)
                        throw new InvalidOperationException ("Cannot get the exit code of a process which has not exited yet.");

                    return _exitCode;
                }
            }
        }
        private int _exitCode;

        /// <summary>
        /// Gets a value indicating whether the associated process has been terminated.
        /// </summary>
        public override bool HasExited {
            get {
                lock (_syncpoint) {
                    if (!_started)
                        return false;
                    if (_hasExited)
                        return _hasExited;

                    try {
                        TestProcessState ();
                    } catch (ExceptionBase exception) {
                        Tracer.TraceException (exception, userData: _userData);
                        return false;
                    }

                    return _hasExited;
                }
            }
        }
        private bool _hasExited;

        /// <summary>
        /// Gets if the process is relying on the Windows-on-Windows-64 subsystem for
        /// 32-bit application on 64-bit Windows support.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        public override bool IsWow64 {
            get {
                lock (_syncpoint) {
                    if (!_started)
                        throw new InvalidOperationException ("Cannot determine use of Windows-on-Windows-65 of a process which has not yet started.");

                    return _isWow;
                }
            }
        }
        private bool _isWow;

        /// <summary>
        /// Gets the operating system assigned identity of the <see cref="DetachedProcess"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        public override int ProcessId {
            get {
                lock (_syncpoint) {
                    if (!_started)
                        throw new InvalidOperationException ("Cannot open standard output reader of a process which has not yet started.");

                    // CLS compliance demands we return a signed value, despite the
                    // operating system returns an unsigned value; so unchecked cast.
                    return unchecked((int)_processId);
                }
            }
        }
        private uint _processId;

        /// <summary>
        /// Gets or sets if the process' standard pipes are redirected or not.
        /// <para>When <see langword="true"/> pipes given to the new process during creation; otherwise <see langword="false"/>.</para>
        /// <para>Does not affect creation of named pipes for <see cref="StdIn"/>, <see cref="StdOut"/>, or <see cref="StdErr"/>.</para>
        /// </summary>
        public override bool RedirectStandardPipes {
            get { return Volatile.Read (ref _redirectStandardPipes); }
            set {
                lock (_syncpoint) {
                    if (_started)
                        throw new InvalidOperationException ("Cannot set standard pipe redirection of a process which has already started.");

                    _redirectStandardPipes = value;
                }
            }
        }
        private bool _redirectStandardPipes;

        /// <summary>
        /// Gets a <see cref="System.IO.Stream"/> wrapper around the stderr handle, for reading
        /// binary output from.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        /// <exception cref="InvalidOperationException">After process has exited.</exception>
        public override Stream StdErr {
            get {
                lock (_syncpoint) {
                    if (!_started)
                        throw new InvalidOperationException ("Cannot open standard error of a process which has not yet started.");
                    if (!_redirectStandardPipes && !_createStandardPipes)
                        throw new InvalidOperationException ("Cannot open standard error of a process which has not been redirected.");

                    return _stderrStream;
                }
            }
        }
        private FileStream _stderrStream;

        /// <summary>
        /// Gets a <see cref="System.IO.StreamReader"/> wrapper around <see cref="StdErr"/>, with
        /// the correct encoding, for reading textual content from.
        /// </summary>
        /// <remarks>Unused by actual Git filters - exists for debugging primarily</remarks>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        /// <exception cref="InvalidOperationException">After process has exited.</exception>
        public override StreamReader StandardError {
            get {
                lock (_syncpoint) {
                    if (!_started)
                        throw new InvalidOperationException ("Cannot get error reader of a process which has not yet started.");

                    // lazy load the reader if hasn't already been allocated
                    if (_stderrReader == null) {
                        try {
                            // Create a reader using the provided encoding, or attempt to detect the output encoding if not provided.
                            Encoding encoding = _encoding ?? GetEncoding (Win32.Kernel32.GetConsoleOutputCP ());
                            _stderrReader = new StreamReader (StdErr, encoding, false, BufferSize, true);
                        } catch {
                            Tracer.TraceWarning ($"Failed to create `{nameof (DetachedProcess)}.{nameof (StandardError)}`", $"-> Command = \"{Command}\".", TracerLevel.Diagnostic);

                            _stderrReader = StreamReader.Null;
                        }
                    }

                    return _stderrReader;
                }
            }
        }
        private StreamReader _stderrReader;

        /// <summary>
        /// Gets a <see cref="System.IO.Stream"/> wrapper around the stdin handle, for writing
        /// binary input to.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        /// <exception cref="InvalidOperationException">After process has exited.</exception>
        public override Stream StdIn {
            get {
                lock (_syncpoint) {
                    if (!_started)
                        throw new InvalidOperationException ("Cannot open standard input of a process which has not yet started.");
                    if (!_redirectStandardPipes && !_createStandardPipes)
                        throw new InvalidOperationException ("Cannot open standard input of a process which has not been redirected.");

                    return HasExited
                        ? Stream.Null
                        : _stdinStream;
                }
            }
        }
        private FileStream _stdinStream;

        /// <summary>
        /// Gets a <see cref="System.IO.StreamWriter"/> wrapper around <see cref="StdIn"/>, with
        /// the correct encoding, for writing textual content to.
        /// </summary>
        /// <remarks>Unused by actual Git filters - exists for debugging primarily</remarks>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        /// <exception cref="InvalidOperationException">After process has exited.</exception>
        public override StreamWriter StandardInput {
            get {
                lock (_syncpoint) {
                    if (!_started)
                        throw new InvalidOperationException ("Cannot get input writer of a process which has not yet started.");

                    // lazy load the writer if hasn't already been allocated
                    if (_stdinWriter == null) {
                        try {
                            // Create a writer using the provided encoding, or attempt to detect the input encoding if not provided.
                            Encoding encoding = _encoding ?? GetEncoding (Win32.Kernel32.GetConsoleInputCP ());
                            _stdinWriter = new StreamWriter (StdIn, encoding, BufferSize, true) {
                                AutoFlush = true,
                            };
                        } catch {
                            Tracer.TraceWarning ($"Failed to create `{nameof (DetachedProcess)}.{nameof (StandardInput)}`.", $"-> Command = \"{Command}\".", TracerLevel.Diagnostic);

                            _stdinWriter = StreamWriter.Null;
                        }
                    }

                    return _stdinWriter;
                }
            }
        }
        private StreamWriter _stdinWriter;

        /// <summary>
        /// Gets a <see cref="System.IO.Stream"/> wrapper around the stdout handle, for reading
        /// binary output from.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        public override Stream StdOut {
            get {
                lock (_syncpoint) {
                    if (!_started)
                        throw new InvalidOperationException ("Cannot open standard output reader of a process which has not yet started.");
                    if (!_redirectStandardPipes && !_createStandardPipes)
                        throw new InvalidOperationException ("Cannot open standard output of a process which has not been redirected.");

                    return _stdoutStream;
                }
            }
        }
        private FileStream _stdoutStream;

        /// <summary>
        /// Gets a <see cref="System.IO.StreamReader"/> wrapper around <see cref="StdOut"/>, with
        /// the correct encoding, for reading textual content from.
        /// </summary>
        /// <remarks>Unused by actual Git filters - exists for debugging primarily</remarks>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        public override StreamReader StandardOutput {
            get {
                lock (_syncpoint) {
                    if (!_started)
                        throw new InvalidOperationException ("Cannot get output reader of a process which has not yet started.");

                    // lazy load the reader if hasn't already been allocated
                    if (_stdoutReader == null) {
                        try {
                            // Create a reader using the provided encoding, or attempt to detect the output encoding if not provided.
                            Encoding encoding = _encoding ?? GetEncoding (Win32.Kernel32.GetConsoleOutputCP ());
                            _stdoutReader = new StreamReader (StdOut, encoding, false, BufferSize, true);
                        } catch {
                            Tracer.TraceWarning ($"Failed to create `{nameof (DetachedProcess)}.{nameof (StandardOutput)}`.", "-> Command = \"{Command}\".", TracerLevel.Diagnostic);

                            _stdoutReader = StreamReader.Null;
                        }
                    }

                    return _stdoutReader;
                }
            }
        }
        private StreamReader _stdoutReader;

        internal string DefaultPipeNamespace {
            get { return Guid.NewGuid ().ToString ("N"); }
        }

        internal string PipeNamespace {
            get { return Volatile.Read (ref _pipeNamespace); }
            set {
                if (ReferenceEquals (value, null))
                    throw new ArgumentNullException (nameof (PipeNamespace));

                lock (_syncpoint) {
                    if (_started)
                        throw new InvalidOperationException ("Cannot set the pipe namespace of a process which has already started.");

                    _pipeNamespace = value;
                }
            }
        }
        private string _pipeNamespace;



        internal event EventHandler Disposed;

        private Win32.SafeJobObjectHandle _jobObjectHandle;
        private readonly object _userData;
        private Win32.SafeProcessHandle _processHandle;
        private IDisposable _timer;

        /// <summary>
        /// Releases all resources used by the <see cref="DetachedProcess"/>, and terminates any
        /// running associated child processes.
        /// </summary>
        protected override void Dispose (bool disposing)
        {
            if (Volatile.Read (ref _started)) {
                try {
                    // close / release all handles
                    Close ();

                    // kill the process synchronously
                    Kill ()?.Wait ();
                } catch (ExceptionBase exception) {
                    Tracer.TraceException (exception, userData: _userData);
                }
            }

            // dispose and null all handle references
            lock (_syncpoint) {
                try {
                    // if not already disposed, dispose all native handles
                    // do so in try -> catch -> squelch blocks because the netfx dispose methods needlessly throw
                    if (!_disposed) {
                        try {
                            _timer?.Dispose ();
                            _timer = null;
                        } catch { /* squelch */ }

                        try {
                            _stderrReader?.Dispose ();
                            _stderrReader = null;
                        } catch { /* squelch */ }

                        try {
                            _stdinWriter?.Dispose ();
                            _stdinWriter = null;
                        } catch { /* squelch */ }

                        try {
                            _stdoutReader?.Dispose ();
                            _stdoutReader = null;
                        } catch { /* squelch */ }

                        try {
                            _stderrStream?.Dispose ();
                            _stderrStream = null;
                        } catch { /* squelch */ }

                        try {
                            _stdinStream?.Dispose ();
                            _stdinStream = null;
                        } catch { /* squelch */ }

                        try {
                            _stdoutStream?.Dispose ();
                            _stdoutStream = null;
                        } catch { /* squelch */ }

                        try {
                            _processHandle?.Dispose ();
                        } catch { /* squelch */ }

                        try {
                            _jobObjectHandle?.Dispose ();
                        } catch { /* squelch */ }
                    }
                } catch (ExceptionBase exception) {
                    Tracer.TraceException (exception, userData: _userData);
                } finally {
                    base.Dispose (disposing);
                }
            }

            GC.SuppressFinalize (this);

            // notify any listeners that care about this object's disposal
            DisposeNotify ();
        }

        /// <summary>
        /// <para>Attempts to gracefully exit the process by emulating sending Ctrl+C and Ctrl+Break signals to the process.</para>
        /// <para>Blocks until the process exits or the time limit elapses.</para>
        /// </summary>
        /// <param name="timeout">
        /// <para>The amount of time to wait for the process beforing assuming the signal failed.</para>
        /// <para>To wait indefinitely, use <see cref="TimeSpan.MaxValue"/>.</para>
        /// </param>
        /// <returns>True on success; otherwise false.</returns>
        public override Task<bool> Exit (TimeSpan timeout)
        {
            // Use this local object to manage the wait signal between parent and child thread
            object syncpoint = new object ();

            lock (syncpoint) {
                if (!_started)
                    throw new InvalidOperationException ("Cannot exit a process which has not yet started.");

                if (HasExited)
                    return Task.FromResult (true);

                Task<bool> task;

                // If the process is already exiting, do not request another exit just piggy-back on
                // the existing request.
                if (_exiting) {
                    task = Task.Run (() => {
                        lock (syncpoint) {
                            // Signal the creating thread that this thread has started
                            Monitor.Pulse (syncpoint);
                        }

                        // Actually acquire the process' mutex
                        lock (_syncpoint) {
                            ClosePipes ();

                            return WaitForProcessExit (timeout);
                        }
                    });
                } else {
                    _exiting = true;
                    task = Task.Run (() => {
                        lock (syncpoint) {
                            // Signal the creating thread that this thread has started
                            Monitor.Pulse (syncpoint);
                        }

                        // Actually acquire the process' mutex
                        lock (_syncpoint) {
                            ClosePipes ();

                            return ExitProcess (timeout);
                        }
                    });
                }

                // wait for the task to start before returning
                Monitor.Wait (syncpoint);

                return task;
            }
        }
        /// <summary>
        /// <para>Attempts to gracefully exit the process by emulating sending Ctrl+C and Ctrl+Break signals to the process.</para>
        /// <para>Blocks until the process exits.</para>
        /// </summary>
        /// <returns>True on success; otherwise false.</returns>
        public override Task<bool> Exit ()
            => Exit (TimeSpan.MaxValue);

        /// <summary>
        /// Terminates the process and all associated child processes.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        public override Task Kill ()
        {
            lock (_syncpoint) {
                if (!_started)
                    throw new InvalidOperationException ("Cannot kill a process which has not yet started.");
                if (_exiting)
                    return Task.FromResult (0);

                TestProcessState ();

                // Create a new, local syncpoint to manage the signal between parent and child thread
                object syncpoint = new object ();

                lock (syncpoint) {
                    // No need to do work on an exited process
                    if (!_hasExited) {
                        Task task;

                        if (_exiting) {
                            task = Task.Run (() => {
                                lock (syncpoint) {
                                    // Signal the creating thread that work has begun
                                    Monitor.PulseAll (syncpoint);
                                }

                                WaitForProcessExit ();
                            });
                        } else {
                            _exiting = true;

                            // Caller may want to or not want to wait on the termination of the process
                            // run the logic asynchronously and let the caller decide
                            task = Task.Run (() => {
                                lock (syncpoint) {
                                    // Signal the creating thread that work has begun
                                    Monitor.PulseAll (syncpoint);
                                }

                                try {
                                    // Actually acquire the process' mutex, which means the parent has returned to the caller
                                    lock (_syncpoint) {
                                        // Try exit process (the clean way) first
                                        if (!HasExited && !ExitProcess ()) {
                                            // Exit process failed, so use our "big gun"
                                            // this is not ideal, because kernel32!TerminateProcess doesn't DLL_PROCESS_DETACH
                                            // this can leave handles rotting and memory/address consumed needlessly
                                            if (Environment.IsWow64 == IsWow64 && !HasExited && !Win32.Kernel32.TerminateProcess (_processHandle, -1)) {
                                                int error = Marshal.GetLastWin32Error ();
                                                throw new Win32Exception (error, $"Failed `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.TerminateProcess)}`");
                                            }
                                        }
                                    }
                                } catch (Exception exception) {
                                    Tracer.TraceException (exception);

                                    throw;
                                } finally {
                                    Close ();
                                }
                            });
                        }

                        // Wait for the kill worker to begin
                        Monitor.Wait (syncpoint);

                        return task;
                    }
                }
            } // This releases the process' mutex enabling the child thread, if any, to do work

            // If we got here, no work need to be done
            return Task.FromResult (0);
        }

        /// <summary>
        /// Starts the process with associate job objects and, potentially, elevation limitations.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        /// <exception cref="InvalidOperationException">When <see cref="Command"/> is invalid</exception>
        /// <exception cref="ApplicationException">If the process failed to start correctly.</exception>
        public override void Start ()
        {
            Console.WriteLine ("Start:" + Command);
            return;
            lock (_syncpoint) {
                if (_started)
                    throw new InvalidOperationException ("Cannot start a process which has already started.");
                if (string.IsNullOrWhiteSpace (Command))
                    throw new InvalidOperationException ("Cannot start a process with an invalid command line value.");

                // Create a bunch of native handles and pointers for local usage
                Win32.SafeProcessHandle processHandle = null;
                Win32.SafeJobObjectHandle jobObjectHandle = null;
                Win32.ProcessInformation processInfo = default (Win32.ProcessInformation);
                GCHandle pinnedMemory = default (GCHandle);
                GCHandle pinnedCommand = default (GCHandle);
                GCHandle pinnedEnvBlock = default (GCHandle);
                IntPtr size = IntPtr.Zero;
                IntPtr procThreadAttrList = IntPtr.Zero;

                // Standard handles for the local side of the standard pipes
                SafeFileHandle localStdinHandle = null;
                SafeFileHandle localStdoutHandle = null;
                SafeFileHandle localStderrHandle = null;

                if (_redirectStandardPipes) {
                    // Query the API to acquire the size of the necessary allocation
                    if (!Win32.Kernel32.InitializeProcThreadAttributeList (processThreadAttributionList: IntPtr.Zero,
                                                                          attributeCount: 1,
                                                                          reserved: IntPtr.Zero,
                                                                          size: ref size)) {
                        int error = Marshal.GetLastWin32Error ();
                        // we expect either success (buffer is already large enough) or an insufficient buffer size error
                        if ((Win32.ErrorCode)error != Win32.ErrorCode.InsufficientBuffer)
                            throw new Win32Exception (error, FormattableString.Invariant ($"Failed to initialize process, `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.InitializeProcThreadAttributeList)}`."));
                    }

                    // Allocate the attribute list based on the size retrned by the original query
                    procThreadAttrList = Marshal.AllocHGlobal (size);
                }

                try {
                    if (_redirectStandardPipes) {
                        // Initialize the memory allocated to an attribute list
                        if (!Win32.Kernel32.InitializeProcThreadAttributeList (processThreadAttributionList: procThreadAttrList,
                                                                              attributeCount: 1,
                                                                              reserved: IntPtr.Zero,
                                                                              size: ref size)) {
                            int error = Marshal.GetLastWin32Error ();
                            throw new Win32Exception (error, FormattableString.Invariant ($"Failed to initialize process, `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.InitializeProcThreadAttributeList)}`."));
                        }
                    }

                    // Standard handles for child side side of the standard pipes
                    SafeFileHandle childStdinHandle = null;
                    SafeFileHandle childStderrHandle = null;
                    SafeFileHandle childStdoutHandle = null;

                    if (_redirectStandardPipes || _createStandardPipes) {
                        // Create the standard pipes with the local and child handles
                        CreatePipe (out localStdinHandle, out childStdinHandle, PipeKind.StandardInput);
                        CreatePipe (out localStdoutHandle, out childStdoutHandle, PipeKind.StandardOutput);
                        CreatePipe (out localStderrHandle, out childStderrHandle, PipeKind.StandardError);
                    }

                    try {
                        if (_redirectStandardPipes) {
                            // allocate an array of the standard handles to pass to the attribute list update function
                            IntPtr[] handles = new IntPtr[3]
                            {
                                childStdinHandle.DangerousGetHandle(),
                                childStdoutHandle.DangerousGetHandle(),
                                childStderrHandle.DangerousGetHandle(),
                            };

                            // pin the array of child handles so that we can safely pass it to native code
                            pinnedMemory = GCHandle.Alloc (handles, GCHandleType.Pinned);

                            // compute the size of the buffer containing the handles
                            size = new IntPtr (checked(IntPtr.Size * handles.Length));

                            // update the process thread attribute list with the list of inheritable handles
                            if (!Win32.Kernel32.UpdateProcThreadAttribute (processThreadAttributionList: procThreadAttrList,
                                                                          reserved1: IntPtr.Zero,
                                                                          attribute: Win32.ProcessThreadAttribute.InheritHandle,
                                                                          value: pinnedMemory.AddrOfPinnedObject (),
                                                                          size: size,
                                                                          reserved2: IntPtr.Zero,
                                                                          reserved3: IntPtr.Zero)) {
                                int error = Marshal.GetLastWin32Error ();
                                throw new Win32Exception (error, FormattableString.Invariant ($"Failed to initialize process, `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.InitializeProcThreadAttributeList)}`."));
                            }
                        }

                        processInfo = new Win32.ProcessInformation { };

                        // the application name will always be null, but fetch from a method in case it isn't
                        string applicationName = (CreateApplicationNameCallback == null)
                                               ? null
                                               : CreateApplicationNameCallback (Environment);

                        // Convert the command line string into a writable buffer because
                        // CreateProcess(AsUser) sometimes updates the buffer - const will cause AV
                        string commandLine = _createProcessCommandLineCallback (Command, Environment);
                        // Start the timer for performance reporting
                        _timer = Tracer.TraceProcess ("Process duration.", commandLine, userData: _userData);

                        // Allocate a buffer to contain the environment block for the new process. Windows supports
                        // environment blocks up to 64 KiB, or 32,768 wide-characters; so make a buffer that size.
                        var envChars = new char[32 * 1024];

                        // Get the process' environment block. 
                        (Environment ?? Context.EnvironmentCreate ()).GetEnvironmentBlock (ref envChars);

                        // Compile the process creation flags
                        Win32.ProcessCreationFlags creationFlags = Win32.ProcessCreationFlags.NoWindow
                                                                 | Win32.ProcessCreationFlags.Suspended
                                                                 | Win32.ProcessCreationFlags.UnicodeEnvironment
                                                                 | Win32.ProcessCreationFlags.NewProcessGroup;

                        // When using standard handles, use the handle-white-list
                        if (_redirectStandardPipes) {
                            creationFlags |= Win32.ProcessCreationFlags.ExtendedStartupInfoPresent;
                        }

                        // Use the `UseStandardHandles` if redirecting pipes, otherwise don't
                        Win32.ProcessStartupFlags startupFlags = _redirectStandardPipes
                            ? Win32.ProcessStartupFlags.UseStandardHandles
                            : Win32.ProcessStartupFlags.None;

                        // Create the extended process startup information
                        Win32.ProcessStartupInfoEx startupInfo = new Win32.ProcessStartupInfoEx {
                            StartupInfo = new Win32.ProcessStartupInfo {
                                Cb = (uint)Marshal.SizeOf (typeof (Win32.ProcessStartupInfoEx)),
                                Desktop = null,
                                Flags = startupFlags,
                                StandardInputHandle = childStdinHandle,
                                StandardOutputHandle = childStdoutHandle,
                                StandardErrorHandle = childStderrHandle,
                            },
                            AttributeList = procThreadAttrList,
                        };

                        var cmdChars = (commandLine + '\0').ToCharArray ();
                        pinnedCommand = GCHandle.Alloc (cmdChars, GCHandleType.Pinned);
                        pinnedEnvBlock = GCHandle.Alloc (envChars, GCHandleType.Pinned);

                        /*
                         * When redirecting handles:
                         *
                         * The child handles to the pipes created have been marked as not inheritble up to this point.
                         * They're marked as such to avoid other calls to `CreateProcess` with `inheritHandles: true`
                         * from with in the same process inadvertently inheriting them.
                         *
                         * The call to `SetChildHandlesInheritable` will make the handles inheritable, from here until
                         * the handles are disposed there's a window of opportunity for a call to `CreateProcess` with
                         * `inheritHandles: true` to inherit those handles. If that happens, then the handles will not
                         * close until all processes with a copy of the handle exit.
                         *
                         * This is unfortunate and unavoidable. Minimizing the window is a best effort work around, but
                         * it is not a solution. The solution to disallow calls to `CreateProcess` with
                         * `inheritHandles: true` unless a `ProcThreadAttributesList` has been associated with the call
                         * to `CreateProcess`.
                         */

                        if (AllowElevated) {
                            // When using standard handles, set the handle to the child side of the pipe to be inheritable.
                            // Doing so will allow the child process to inherit the handle, otherise the pipe would be "broken".
                            if (_redirectStandardPipes) {
                                SetChildHandlesInheritable (ref startupInfo.StartupInfo);
                            }

                            // Create a suspended process with all of the values we need, ignoring if
                            // the process will be elevated or not
                            if (!Win32.Kernel32.CreateProcess (applicationName: applicationName,
                                                              commandLine: pinnedCommand.AddrOfPinnedObject (),
                                                              processAttributes: IntPtr.Zero,
                                                              threadAttributes: IntPtr.Zero,
                                                              inheritHandles: _redirectStandardPipes,
                                                              creationFlags: creationFlags,
                                                              environment: pinnedEnvBlock.AddrOfPinnedObject (),
                                                              currentDirectory: WorkingDirectory,
                                                              startupInfo: ref startupInfo,
                                                              processInfo: out processInfo)) {
                                int error = Marshal.GetLastWin32Error ();
                                string errorMessage = null;

                                switch (error) {
                                case (int)Win32.ErrorCode.FileNotFound:
                                    errorMessage = "Process creation failed because the file cannot be found.";
                                    break;

                                case (int)Win32.ErrorCode.PathNotFound:
                                    errorMessage = "Process creation failed because the path cannot be found.";
                                    break;

                                default:
                                    errorMessage = "Process creation failed.";
                                    break;
                                }

                                throw new Win32Exception (error, errorMessage);
                            }
                        } else {
                            Win32.SafeSaferLevelHandle saferLevelHandle = null;
                            IntPtr saferLevelToken = IntPtr.Zero;

                            // Create a safer level scope: a non-elevated version of the same user
                            if (!Win32.Advapi32.SaferCreateLevel (scopeId: Win32.SaferScope.User,
                                                                 levelId: Win32.SaferLevel.NormalUser,
                                                                 openFlags: Win32.SaferOpen.Open,
                                                                 saferLevelHandle: out saferLevelHandle,
                                                                 reserved: IntPtr.Zero)) {
                                int error = Marshal.GetLastWin32Error ();
                                throw new Win32Exception (error, "Unable to create safe level during process creation.");
                            }

                            using (saferLevelHandle) {
                                // Create a safer level token for use in CreateProcessAsUser
                                if (!Win32.Advapi32.SaferComputeTokenFromLevel (saferLevelHandle: saferLevelHandle,
                                                                               inAccessToken: IntPtr.Zero,
                                                                               outAccessToken: out saferLevelToken,
                                                                               flags: Win32.SaferComputeTokenBehavior.Default,
                                                                               reserved: IntPtr.Zero)) {
                                    int error = Marshal.GetLastWin32Error ();
                                    throw new Win32Exception (error, "Unable to create safe token during process creation.");
                                }

                                try {
                                    // When using standard handles, set the handle to the child side of the pipe to be inheritable.
                                    // Doing so will allow the child process to inherit the handle, otherise the pipe would be "broken".
                                    if (_redirectStandardPipes) {
                                        SetChildHandlesInheritable (ref startupInfo.StartupInfo);
                                    }

                                    // Create a suspended process with all of the values we need including the
                                    // non-elevated user account association
                                    if (!Win32.Advapi32.CreateProcessAsUser (token: saferLevelToken,
                                                                            applicationName: applicationName,
                                                                            commandLine: pinnedCommand.AddrOfPinnedObject (),
                                                                            processAttributes: IntPtr.Zero,
                                                                            threadAttributes: IntPtr.Zero,
                                                                            inheritHandles: _redirectStandardPipes,
                                                                            creationFlags: creationFlags,
                                                                            environment: pinnedEnvBlock.AddrOfPinnedObject (),
                                                                            currentDirectory: WorkingDirectory,
                                                                            startupInfo: ref startupInfo,
                                                                            processInfo: out processInfo)) {
                                        int error = Marshal.GetLastWin32Error ();
                                        string errorMessage = null;

                                        switch (error) {
                                        case (int)Win32.ErrorCode.FileNotFound:
                                            errorMessage = "Process creation failed because the file cannot be found.";
                                            break;

                                        case (int)Win32.ErrorCode.PathNotFound:
                                            errorMessage = "Process creation failed because the path cannot be found.";
                                            break;

                                        default:
                                            errorMessage = "Process creation failed.";
                                            break;
                                        }

                                        throw new Win32Exception (error, errorMessage);
                                    }
                                } finally {
                                    // Release the token now that we're done with it
                                    ReleaseHandle (saferLevelToken);
                                }
                            }
                        }
                    } finally {
                        // clean up the child handles immediately; otherwise we'll deadlock waiting for the handles to
                        // close later when reading from the pipes.
                        childStdinHandle?.Dispose ();
                        childStdoutHandle?.Dispose ();
                        childStderrHandle?.Dispose ();

                        // Release/unpin the array of standard handles
                        if (pinnedCommand.IsAllocated) {
                            pinnedCommand.Free ();
                        }
                        if (pinnedEnvBlock.IsAllocated) {
                            pinnedEnvBlock.Free ();
                        }
                    }

                    // Local copy of the process handle
                    processHandle = new Win32.SafeProcessHandle (processInfo.ProcessHandle);

                    if (!Environment.IsProcessInJob) {
                        // Create the rules for the new job object we'll need to create to properly
                        // manage the process tree we're about to unleash via Git-filter invocation
                        var jobExInfo = new Win32.JobObjectExtendedLimitInformation {
                            BasicLimitInformation = new Win32.JobObjectBasicLimitInformation {
                                LimitFlags = Win32.JobObjectLimitFlags.KillOnJobClose
                            },
                        };

                        // Create a new job object to manage the new process
                        jobObjectHandle = Win32.Kernel32.CreateJobObject (securityAttributes: IntPtr.Zero,
                                                                         name: null);
                        if (jobObjectHandle == null || jobObjectHandle.IsInvalid) {
                            int error = Marshal.GetLastWin32Error ();
                            throw new Win32Exception (error, "Unable to create job object during process creation.");
                        }

                        // Assign the process to the newly create job object
                        if (!Win32.Kernel32.AssignProcessToJobObject (jobObjectHandle: jobObjectHandle,
                                                                     processHandle: processHandle)) {
                            int error = Marshal.GetLastWin32Error ();
                            throw new Win32Exception (error, "Unable to associate job object with the process during process creation.");
                        }

                        // Assign the limit rules to the job object and by proxy the process (and its children)
                        if (!Win32.Kernel32.SetInformationJobObject (jobObjectHandle: jobObjectHandle,
                                                                    jobObjectInfoClass: Win32.JobObjectInfoClass.ExtendedLimitInformation,
                                                                    info: ref jobExInfo,
                                                                    infoLength: Marshal.SizeOf (typeof (Win32.JobObjectExtendedLimitInformation)))) {
                            int error = Marshal.GetLastWin32Error ();
                            throw new Win32Exception (error, "Unable to set job information during process creation.");
                        }

                        // Success!, now track all of the handles and objects at the class level
                        _jobObjectHandle = jobObjectHandle;
                    }

                    _processHandle = processHandle;
                    _processId = processInfo.ProcessId;

                    // Resume the primary thread of the new process.
                    // A result of 1 or 0 (zero) means success, anything else indicates failure
                    int res = -1;
                    if ((res = Win32.Kernel32.ResumeThread (threadHandle: processInfo.ThreadHandle)) != 1 && res != 0) {
                        int error = Marshal.GetLastWin32Error ();
                        throw new Win32Exception (error, "Unable to resume suspended process during process creation.");
                    }

                    // When not redirecting standard pipes, and still relying on created named pipes: wait for the process
                    // to attached the the named pipes. Failure to wait will mean reading from / writing to broken pipes.
                    if (!_redirectStandardPipes && _createStandardPipes) {
                        // Create a synchronization object for cross-thread communication
                        object syncpoint = new object ();
                        bool waitingOnPipes = true;

                        // Create a timeout task which will wait for a period of time before attempting to open the client
                        // side of the named pipe. This is to prevent the call to `ConnectNamedPipe` from deadlocking this
                        // process if for some reason the child process in unable to open the client end of the pipe.
                        var timeout = Task.Run (() => {
                            // Take a handle on the environment as a `GitProcessEnvironment`, but give up early if we cannot
                            // manifest pipe names.
                            var env = Environment as GitProcessEnvironment;
                            if (env == null)
                                return false;

                            lock (syncpoint) {
                                // Wait a bit to give the child time to attach to the pipes, but get
                                // to work immediately if the parent signals that it has completed.
                                // If the parent is no longer waiting for pipes, don't even bother
                                // waiting here.
                                if (!waitingOnPipes
                                     || Monitor.Wait (syncpoint, TimeSpan.FromSeconds (8)))
                                    return false;
                            }

                            // Since we're here, the parent hasn't signaled that it has successfully waited for the child to
                            // open the client end of the named pipes. That means the child needs to open the client side of
                            // the named pipes (to unblock the waiting parent).

                            bool timedout = false;
                            Win32.SecurityAttributes secuityAttributes = new Win32.SecurityAttributes () {
                                InheritHandle = false,
                                Length = (uint)Marshal.SizeOf (typeof (Win32.SecurityAttributes)),
                                Descriptor = IntPtr.Zero,
                            };

                            // Attempt to open the child end of the standard input pipe.
                            using (var handle = Win32.Kernel32.CreateFile (fileName: env.StdInPipeName,
                                                                          desiredAccess: FileAccess.Read,
                                                                          fileShare: FileShare.Read,
                                                                          securityAttributes: ref secuityAttributes,
                                                                          fileMode: FileMode.Open,
                                                                          fileOptions: FileOptions.None)) {
                                // An invalid handle is what we want, if it is not invalid it means the child failed to
                                // open the child end of the pipe. If the handle is valid, return true the timeout occurred.
                                timedout |= !handle.IsInvalid;
                            }

                            using (var handle = Win32.Kernel32.CreateFile (fileName: env.StdOutPipeName,
                                                                          desiredAccess: FileAccess.Write,
                                                                          fileShare: FileShare.Write,
                                                                          securityAttributes: ref secuityAttributes,
                                                                          fileMode: FileMode.Open,
                                                                          fileOptions: FileOptions.None)) {
                                // An invalid handle is what we want, if it is not invalid it means the child failed to
                                // open the child end of the pipe. If the handle is valid, return true the timeout occurred.
                                timedout |= !handle.IsInvalid;
                            }

                            using (var handle = Win32.Kernel32.CreateFile (fileName: env.StdErrPipeName,
                                                                          desiredAccess: FileAccess.Write,
                                                                          fileShare: FileShare.Write,
                                                                          securityAttributes: ref secuityAttributes,
                                                                          fileMode: FileMode.Open,
                                                                          fileOptions: FileOptions.None)) {
                                // An invalid handle is what we want, if it is not invalid it means the child failed to
                                // open the child end of the pipe. If the handle is valid, return true the timeout occurred.
                                timedout |= !handle.IsInvalid;
                            }

                            return timedout;
                        });

                        // Wait for the client end of the standard input pipe to be opened.
                        WaitForPipe (localStdinHandle, PipeKind.StandardInput);

                        // Wait for the client end of the standard output pipe to be opened.
                        WaitForPipe (localStdoutHandle, PipeKind.StandardOutput);

                        // Wait for the client end of the standard error pipe to be opened.
                        WaitForPipe (localStderrHandle, PipeKind.StandardError);

                        // Notify the child that the parent thread has successfully receieve signals that the client end
                        // of the named pipes has been opened.
                        lock (syncpoint) {
                            // Update the waiting on pipe connections state in-case the anti-deadlock task hasn't started yet.
                            waitingOnPipes = false;

                            // Signal the anti-deadlock task that we've successfully connected to the pipes.
                            Monitor.Pulse (syncpoint);
                        }

                        // Check to see if the pipes were opened due to time, if so throw now to inform the caller of the failure.
                        if (timeout.Result)
                            throw new InvalidOperationException ("Timeout waiting for child process to open named pipes.");
                    }

                    // Create streams from the standard handles
                    if (_redirectStandardPipes || _createStandardPipes) {
                        _stdinStream = CreateStreamFromStandardHandle (localStdinHandle, FileAccess.Write);
                        _stdoutStream = CreateStreamFromStandardHandle (localStdoutHandle, FileAccess.Read);
                        _stderrStream = CreateStreamFromStandardHandle (localStderrHandle, FileAccess.Read);
                    }

                    _started = true;
                    _exiting = false;

                    TestProcessState ();

                    // Test this process for Windows-on-Windows-64 hosting
                    if (!Win32.Kernel32.IsWow64Process (processHandle: _processHandle,
                                                       isWow64Process: out _isWow)) {
                        int error = Marshal.GetLastWin32Error ();
                        throw new Win32Exception (error, "Unable to determine the Windows-on-Windows-64 state of the process.");
                    }
                } catch (Exception exception) {
                    // Test the process state, safely, because it is possible that there is some kind of corruption or installation
                    // error causing the failure. If that is the case, then the call to `TestProcessState()` will throw a
                    // `ModuleLoadFailureException` and we still need to dispose of all the native handles.
                    try {
                        TestProcessState ();
                    } finally {
                        localStdinHandle?.Dispose ();
                        localStdoutHandle?.Dispose ();
                        localStderrHandle?.Dispose ();
                        processHandle?.Dispose ();
                        jobObjectHandle?.Dispose ();
                    }

                    // We've cleaned up, now blow up the caller
                    throw new ProcessException ("Unable to start process. " + exception.Message, exception);
                } finally {
                    // We won't need these handles going forward, release them
                    ReleaseHandle (processInfo.ThreadHandle);

                    if (procThreadAttrList != IntPtr.Zero) {
                        // Delete the attribution list
                        Win32.Kernel32.DeleteProcThreadAttributeList (procThreadAttrList);
                        // Free the heap allocation
                        Marshal.FreeHGlobal (procThreadAttrList);
                    }

                    // Release/unpin the array of standard handles
                    if (pinnedMemory.IsAllocated) {
                        pinnedMemory.Free ();
                    }
                }
            }
        }

        /// <summary>
        /// Blocks the calling thread until the process exits or the the specified timeout, if any, is reached.
        /// </summary>
        /// <param name="timeout">
        /// <para>The maximum amount of time to block waiting for exit.</para>
        /// <para>To block until the process exits without a timeout, pass <see cref="TimeSpan.MaxValue"/>.</para>
        /// <para>If you pass <see cref="TimeSpan.Zero"/> to the method, it returns true only if the process has
        /// already exited; otherwise, it immediately returns false.</para>
        /// </param>
        /// <returns>True if the associated process has exited; otherwise False.</returns>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        /// <exception cref="ApplicationException">If the process failed to start correctly.</exception>
        public override bool WaitForExit (TimeSpan timeout)
        {
            if (!Volatile.Read (ref _started))
                throw new InvalidOperationException ("Cannot wait for a process to exit which has not yet started.");

            // If the process has already exited, just return success
            if (Volatile.Read (ref _hasExited))
                return true;

            // Check state and return if exited, needs to be atomic
            lock (_syncpoint) {
                TestProcessState ();

                if (_hasExited)
                    return true;
            }

            // Use the wait for process exit
            if (WaitForProcessExit (timeout)) {
                // since we exited, close up shop and return success
                Close ();

                return true;
            }

            // Check state and return state, needs to be atomic
            lock (_syncpoint) {
                TestProcessState ();
                return _hasExited;
            }
        }

        /// <summary>
        /// Blocks the calling thread until the process exits.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        /// <exception cref="ApplicationExcepti
        public override void WaitForExit ()
            => WaitForExit (TimeSpan.MaxValue);

        internal static FileStream CreateStreamFromStandardHandle (SafeFileHandle standardHandle, FileAccess fileAccess)
        {
            return new FileStream (standardHandle, fileAccess, BufferSize, false);
        }

        internal void Close ()
        {
            Debug.Assert (_started, "The process has not yet started.");

            lock (_syncpoint) {
                try {
                    ClosePipes ();
                } catch (ExceptionBase exception) {
                    Tracer.TraceException (exception, userData: _userData);
                }

                // notify any listeners that the process is closing
                ExitNotify ();
            }
        }

        internal void DisposeNotify ()
        {
            // use interlocked to read/write the handler atomically
            // guarantees it is only every called once
            EventHandler disposed = null;
            if ((disposed = Interlocked.Exchange (ref Disposed, null)) != null) {
                // run the disposed event handler in a separate thread the listeners need to know
                // about the event, but they do not need to block the caller
                Task.Run (() => disposed (this, new EventArgs ()));
            }
        }


        internal static Encoding GetEncoding (uint codepage)
        {
            int defaultEncCodePage = Encoding.Default.CodePage;

            if (defaultEncCodePage == codepage || defaultEncCodePage != Encoding.UTF8.CodePage) {
                try {
                    return Encoding.GetEncoding ((int)codepage);
                } catch (NotSupportedException) { /* if it is not supported, bummer - move on */ }
            }

            return new UTF8Encoding (encoderShouldEmitUTF8Identifier: false);
        }

        internal static string GetStandardPipeName (PipeKind pipeKind, string pipeNamespace)
        {
            const string Prefix = "\\\\.\\pipe\\VS";

            if (ReferenceEquals (pipeNamespace, null))
                throw new ArgumentNullException (nameof (pipeNamespace));

            string suffix;
            switch (pipeKind) {
            case PipeKind.StandardInput: {
                    suffix = "0";
                }
                break;

            case PipeKind.StandardOutput: {
                    suffix = "1";
                }
                break;

            case PipeKind.StandardError: {
                    suffix = "2";
                }
                break;

            case PipeKind.GenericInput: {
                    suffix = "in";
                }
                break;

            case PipeKind.GenericOutput: {
                    suffix = "out";
                }
                break;

            default:
                throw new ArgumentException (nameof (pipeKind));
            }

            string pipeName = FormattableString.Invariant ($"{Prefix}\\{pipeNamespace}\\{suffix}");

            return pipeName;
        }

        private void ClosePipes ()
        {
            Debug.Assert (Monitor.IsEntered (_syncpoint), "Expected lock not held.");

            // flush all of the streams, close stdin
            // do not close stderr or stdout as readers might still have useful data to extract
            if (_stderrStream != null && _stderrStream.CanRead) {
                _stderrStream.Flush ();
            }
            if (_stdinStream != null && _stdinStream.CanWrite) {
                _stdinStream.Flush ();
                _stdinStream.Close ();
            }
            if (_stdoutStream != null && _stdoutStream.CanRead) {
                _stdoutStream.Flush ();
            }
        }

        private void CreatePipe (out SafeFileHandle localHandle, out SafeFileHandle childHandle, PipeKind pipeKind)
        {
            // Reference documentation/code: https://msdn.microsoft.com/en-us/library/windows/desktop/ms682499(v=vs.85).aspx

            // Standard handles needs to be handles to blocking binary pipes
            Win32.PipeMode pipeMode = Win32.PipeMode.ReadBinary
                                    | Win32.PipeMode.Wait
                                    | Win32.PipeMode.WriteBinary;

            // Standard handles need to be handles to directional pipes,
            // with the direction determined by `pipeKind`
            Win32.PipeType serverType;
            FileAccess clientAccess;
            FileShare clientShare;
            string pipeName;

            switch (pipeKind) {
            case PipeKind.StandardInput: {
                    serverType = Win32.PipeType.Outbound;
                    clientAccess = FileAccess.Read;
                    clientShare = FileShare.Read;
                }
                break;

            case PipeKind.StandardOutput:
            case PipeKind.StandardError: {
                    serverType = Win32.PipeType.Inbound;
                    clientAccess = FileAccess.Write;
                    clientShare = FileShare.Write;
                }
                break;

            default:
                throw new ArgumentException (nameof (pipeKind));
            }

            Win32.SecurityAttributes secuityAttributes = new Win32.SecurityAttributes () {
                InheritHandle = false,
                Length = (uint)Marshal.SizeOf (typeof (Win32.SecurityAttributes)),
                Descriptor = IntPtr.Zero,
            };

            if (Environment is GitProcessEnvironment) {
                pipeName = (Environment as GitProcessEnvironment).GetPipeName (pipeKind);
            } else {
                pipeName = GetStandardPipeName (pipeKind, PipeNamespace);
            }

            // Create the named pipe, which returns the server (aka local) handle
            localHandle = Win32.Kernel32.CreateNamedPipe (pipeName: pipeName,
                                                         pipeType: serverType,
                                                         pipeMode: pipeMode,
                                                         maxInstances: 1,
                                                         outBufferSize: BufferSize,
                                                         inBufferSize: BufferSize,
                                                         defaultTimeOut: 0,
                                                         securityAttributes: ref secuityAttributes);

            // Validate the server handle
            if (localHandle == null || localHandle.IsInvalid) {
                int error = Marshal.GetLastWin32Error ();
                string message;

                switch ((Win32.ErrorCode)error) {
                case Win32.ErrorCode.BrokenPipe:
                case Win32.ErrorCode.BadPipe:
                    message = FormattableString.Invariant ($"Requested pipe is invalid, `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.CreateNamedPipe)}`.");
                    break;

                case Win32.ErrorCode.PipeBusy:
                case Win32.ErrorCode.PipeNotConnected:
                    message = FormattableString.Invariant ($"Requested pipe already exists, `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.CreateNamedPipe)}`.");
                    break;

                default:
                    message = FormattableString.Invariant ($"Failed to create pipe, `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.CreateNamedPipe)}`.");
                    break;
                }

                throw new Win32Exception (error, message);
            }

            if (_redirectStandardPipes) {
                // Create the client (aka child) handle
                childHandle = Win32.Kernel32.CreateFile (fileName: pipeName,
                                                        desiredAccess: clientAccess,
                                                        fileShare: clientShare,
                                                        securityAttributes: ref secuityAttributes,
                                                        fileMode: FileMode.Open,
                                                        fileOptions: FileOptions.None);

                // Validate the client handle
                if (childHandle == null || childHandle.IsInvalid) {
                    // close the server handle if the child is invalid
                    localHandle.Close ();

                    int error = Marshal.GetLastWin32Error ();
                    string message;

                    switch ((Win32.ErrorCode)error) {
                    case Win32.ErrorCode.AccessDenied:
                    case Win32.ErrorCode.PipeBusy:
                        message = FormattableString.Invariant ($"Requested pipe is busy, `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.CreateNamedPipe)}`.");
                        break;

                    case Win32.ErrorCode.BrokenPipe:
                    case Win32.ErrorCode.BadPipe:
                    case Win32.ErrorCode.PipeNotConnected:
                        message = FormattableString.Invariant ($"Requested pipe is invalid, `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.CreateNamedPipe)}`.");
                        break;

                    default:
                        message = FormattableString.Invariant ($"Failed to create pipe, `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.CreateNamedPipe)}`.");
                        break;
                    }

                    throw new Win32Exception (error, message);
                }
            } else {
                // Since we're not directing, do not create the child handle
                childHandle = new SafeFileHandle (IntPtr.Zero, false);
            }
        }

        private bool ExitProcess (TimeSpan timeout)
        {
            Debug.Assert (Monitor.IsEntered (_syncpoint), "Expected lock not held.");

            try {
                if (_terminateProcessCallback is null)
                    return false;

                // Make a copy of the callback while we're still under lock.
                var terminateProcessCallback = _terminateProcessCallback;

                // Stop a timer to track total time allotted for exiting
                Stopwatch timer = new Stopwatch ();
                timer.Start ();

                // Exit the lock before calling a potentially long running operation.
                Monitor.Exit (_syncpoint);
                try {
                    // Use the callback to terminate the process.
                    if (!terminateProcessCallback (this, timeout))
                        return false;
                } finally {
                    // Re-take the lock in the finally clause to insure it happens.
                    Monitor.Enter (_syncpoint);
                }

                // Calculate the amount of time remaining...
                timeout = timeout - timer.Elapsed;

                // ... but do not let a negative value be used, because
                // a negative value is translated into eternity; and the
                // value of `timer.Elapsed` could easily exceed the value
                // of `timeout` and result in a negative value.
                timeout = timeout < TimeSpan.Zero
                    ? TimeSpan.Zero
                    : timeout;

                WaitForProcessExit (timeout);
            } catch (Exception exception) when (!ExceptionBase.IsCriticalException (exception)) {
                Tracer.TraceException (exception, userData: _userData);
            }

            // Well, that didn't work
            return false;
        }

        private bool ExitProcess ()
            => ExitProcess (TimeSpan.MaxValue);

        private static void ReleaseHandle (IntPtr handle)
        {
            if (handle != null && handle != IntPtr.Zero && handle != Win32.Kernel32.InvalidHandleValue) {
                Win32.Kernel32.CloseHandle (handle);
            }
        }

        private static void SetChildHandlesInheritable (ref Win32.ProcessStartupInfo startupInfo)
        {
            if (!Win32.Kernel32.SetHandleInformation (handle: startupInfo.StandardInputHandle,
                                                     mask: Win32.HandleInformationFlags.Inherit,
                                                     flags: Win32.HandleInformationFlags.Inherit)) {
                int error = Marshal.GetLastWin32Error ();
                throw new Win32Exception (error, FormattableString.Invariant ($"Failed to create pipe, `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.SetHandleInformation)}`."));
            }

            if (!Win32.Kernel32.SetHandleInformation (handle: startupInfo.StandardOutputHandle,
                                                     mask: Win32.HandleInformationFlags.Inherit,
                                                     flags: Win32.HandleInformationFlags.Inherit)) {
                int error = Marshal.GetLastWin32Error ();
                throw new Win32Exception (error, FormattableString.Invariant ($"Failed to create pipe, `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.SetHandleInformation)}`."));
            }

            if (!Win32.Kernel32.SetHandleInformation (handle: startupInfo.StandardErrorHandle,
                                                     mask: Win32.HandleInformationFlags.Inherit,
                                                     flags: Win32.HandleInformationFlags.Inherit)) {
                int error = Marshal.GetLastWin32Error ();
                throw new Win32Exception (error, FormattableString.Invariant ($"Failed to create pipe, `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.SetHandleInformation)}`."));
            }
        }

        private void TestProcessState ()
        {
            const int CannotStartError = 127;
            const int ModuleLoadError = -1073741511;
            const int StillRunningCode = 259;

            Debug.Assert (Monitor.IsEntered (_syncpoint), "The expected lock is not held.");

            // Take a local reference to prevent other threads from setting the reference to null.
            Win32.SafeProcessHandle processHandle = _processHandle;

            if (processHandle is null || processHandle.IsClosed || processHandle.IsInvalid) {
                _hasExited = true;
            } else {
                // Query the operating system for the state of the process
                if (!Win32.Kernel32.GetExitCodeProcess (processHandle, out _exitCode)) {
                    int error = Marshal.GetLastWin32Error ();

                    // Failure to read the exit code to a process can happen for any number of reasons,
                    // including the process having been torn down by JobObjects. Log, do not throw.
                    Tracer.TraceError ($"Failed to get process' exit code, `{Win32.Kernel32.Name}!{nameof (Win32.Kernel32.GetExitCodeProcess)}` (0x{error:X8}).", userData: _userData);
                }

                _hasExited = (_exitCode != StillRunningCode);

                // If the subsystem is saying the process failed to start correctly, throw
                if (_exitCode == CannotStartError)
                    throw new ProcessException ("Process failed to start correctly due to internal errors.");
                if (_exitCode == ModuleLoadError)
                    throw new ModuleLoadFailureException ();

                if (_hasExited) {
                    Close ();
                }
            }
        }

        private void WaitForPipe (SafeFileHandle pipeHandle, PipeKind pipeKind)
        {
            // Wait for the client end of the standard error pipe to be opened.
            if (!Win32.Kernel32.ConnectNamedPipe (pipeHandle, IntPtr.Zero)) {
                int error = Marshal.GetLastWin32Error ();
                switch ((Win32.ErrorCode)error) {
                // Ignore white-listed error codes (https://msdn.microsoft.com/en-us/library/windows/desktop/aa365146(v=vs.85).aspx)
                case Win32.ErrorCode.BrokenPipe:
                case Win32.ErrorCode.NoData:
                case Win32.ErrorCode.PipeConnected:
                case Win32.ErrorCode.IoIncomplete:
                case Win32.ErrorCode.IoPending:
                    return;

                default: {
                        string pipeName = String.Empty;

                        switch (pipeKind) {
                        case PipeKind.StandardInput:
                            pipeName = "standard input";
                            break;

                        case PipeKind.StandardOutput:
                            pipeName = "standard output";
                            break;

                        case PipeKind.StandardError:
                            pipeName = "standard error";
                            break;
                        }

                        throw new Win32Exception (error, FormattableString.Invariant ($"Unable to open child process' {pipeName}."));
                    }
                }

            }
        }

        private bool WaitForProcessExit (TimeSpan timeout)
        {
            var processHandle = _processHandle;

            if (processHandle.IsClosed || processHandle.IsInvalid)
                return false;

            int milliseconds = timeout == TimeSpan.MaxValue
                ? -1
                : (int)timeout.TotalMilliseconds;

            // attempt to wait for the process to exit
            Win32.WaitReturnCode waitReturnCode = Win32.Kernel32.WaitForSingleObject (handle: processHandle,
                                                                                     milliseconds: milliseconds);

            // analyze the return code, Signal and Timeout are the only valid results
            switch (waitReturnCode) {
            case Win32.WaitReturnCode.Abandoned:
            case Win32.WaitReturnCode.Failed:
                int error = Marshal.GetLastWin32Error ();
                throw new Win32Exception (error, $"Failed `Kernel32!{nameof (Win32.Kernel32.WaitForSingleObject)}`");

            case Win32.WaitReturnCode.Signaled:
                return true;

            case Win32.WaitReturnCode.Timeout:
                return false;

            default:
                Debug.Fail ("Unexpected return code");
                goto case Win32.WaitReturnCode.Timeout;
            }
        }

        private bool WaitForProcessExit ()
            => WaitForProcessExit (TimeSpan.MaxValue);
    }
}
