//*************************************************************************************************
// IProcess.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Returned by types which derive `<see cref="Command.CreateProcess(string)"/>` such as `<seealso cref="GitProcessCommand"/>` and `<seealso cref="ProcessCommand"/>`.
    /// <para/>
    /// Maps to a specific operating system process and reports the status of the process.
    /// <para/>
    /// It is important to read from the `<see cref="StdErr"/>` and `<see cref="StdOut"/>` to prevent their underlying pipes from filling up; otherwise the mapped process will block wait for a pipe read to make sufficient space.
    /// </summary>
    public interface IProcess : IDisposable
    {
        /// <summary>
        /// Gets or sets if the filter process is allowed to be elevated or not. The process will only be elevated if this process is elevated. Non-elevated processes do not spawn elevated processes.
        /// </summary>
        /// <exception cref="InvalidOperationException">After `<see cref="Start"/>` has been called.</exception>
        bool AllowElevated { get; set; }

        /// <summary>
        /// Get or sets the command line used when starting the process.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        string Command { get; set; }

        /// <summary>
        /// Gets or set the environmental variables which will be passed to the newly created process when `<see cref="Start"/>` is called.
        /// </summary>
        /// <exception cref="InvalidOperationException">After `<see cref="Start"/>` has been called.</exception>
        Environment Environment { get; set; }

        /// <summary>
        /// Gets the value that the associated process specified when it terminated.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        /// <exception cref="InvalidOperationException">Before the process has exited.</exception>
        /// <exception cref="ApplicationException">If the process failed to start correctly.</exception>
        int ExitCode { get; }

        /// <summary>
        /// Gets a value indicating whether the associated process has been terminated.
        /// </summary>
        bool HasExited { get; }

        /// <summary>
        /// Gets if the process is relying on the Windows-on-Windows-64 subsystem for 32-bit application on 64-bit Windows support.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        bool IsWow64 { get; }

        /// <summary>
        /// Gets the operating system assigned identity of the `<see cref="IProcess"/>`.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        int ProcessId { get; }

        /// <summary>
        /// Gets or sets if the process' standard pipes are redirected or not.
        /// When `<see langword="true"/>` pipes given to the new process during creation; otherwise `<see langword="false"/>`.
        /// <para/>
        /// Does not affect creation of named pipes for `<see cref="StdIn"/>`, `<see cref="StdOut"/>`, or `<see cref="StdErr"/>`.
        /// </summary>
        /// <exception cref="InvalidOperationException">After `<see cref="Start"/>` has been called.</exception>
        bool RedirectStandardPipes { get; set; }

        /// <summary>
        /// Gets a `<see cref="StreamReader"/>` wrapper around `<see cref="StdErr"/>`, with the correct encoding, for reading textual content from.
        /// </summary>
        /// <remarks>Unused by actual Git filters - exists for debugging primarily</remarks>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        /// <exception cref="InvalidOperationException">After process has exited.</exception>
        StreamReader StandardError { get; }

        /// <summary>
        /// Gets a `<see cref="StreamWriter"/>` wrapper around `<see cref="StdIn"/>`, with the correct encoding, for writing textual content to.
        /// </summary>
        /// <remarks>Unused by actual Git filters - exists for debugging primarily</remarks>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        /// <exception cref="InvalidOperationException">After process has exited.</exception>
        StreamWriter StandardInput { get; }

        /// <summary>
        /// Gets a `<see cref="Stream"/>` wrapper around the stderr handle, for reading  binary output from.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        /// <exception cref="InvalidOperationException">After process has exited.</exception>
        Stream StdErr { get; }

        /// <summary>
        /// Gets a `<see cref="Stream"/>` wrapper around the stdin handle, for writing binary
        /// input to.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        /// <exception cref="InvalidOperationException">After process has exited.</exception>
        Stream StdIn { get; }

        /// <summary>
        /// Gets a `<see cref="Stream"/>` wrapper around the stdout handle, for reading binary output from.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        Stream StdOut { get; }

        /// <summary>
        /// Gets or sets the working directory for the process to be started. If not set the value will default to `<see cref="Environment.WorkingDirectory"/>`.
        /// </summary>
        /// <exception cref="InvalidOperationException">After `<see cref="Start"/>` has been called.</exception>
        /// <exception cref="ArgumentNullException">When the value set is null or white-space.</exception>
        /// <exception cref="DirectoryNotFoundException">When the value set is not a valid and accessible directory.</exception>
        string WorkingDirectory { get; set; }

        /// <summary>
        /// Occurs when a process exits.
        /// </summary>
        event EventHandler Exited;

        /// <summary>
        /// Occurs when process outputs text to StdOut/StdErr
        /// </summary>
        event EventHandler<OperationOutput> ProcessOutput;

        /// <summary>
        /// Attempts to asynchronously, and gracefully, exit the process by emulating sending Ctrl+C and Ctrl+Break signals to the process.
        /// <para/>
        /// Blocks until the process exits or the time limit elapses.
        /// <para/>
        /// Returns `<see langword="true"/>` on success; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="timeout">
        /// The amount of time to wait for the process before assuming the signal failed.
        /// <para/>
        /// To wait indefinitely, use `<see cref="TimeSpan.MaxValue"/>`.
        /// </param>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        Task<bool> Exit(TimeSpan timeout);

        /// <summary>
        /// Attempts to asynchronously, and gracefully, exit the process by emulating sending Ctrl+C and Ctrl+Break signals to the process.
        /// <para/>
        /// Blocks until the process exits.
        /// <para/>
        /// Returns `<see langword="true"/>` on success; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        Task<bool> Exit();

        /// <summary>
        /// Terminates the process and all associated child processes.
        /// <para/>
        /// Blocks the calling thread until the process has been killed.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        Task Kill();

        /// <summary>
        /// Starts the process with associate job objects and, potentially, elevation limitations.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before <see cref="Start"/> has been called.</exception>
        /// <exception cref="InvalidOperationException">When <see cref="Command"/> is invalid</exception>
        /// <exception cref="ApplicationException">If the process failed to start correctly.</exception>
        void Start();

        /// <summary>
        /// Blocks the calling thread until the process exits or the specified timeout, if any, is reached.
        /// <para/>
        /// Returns `<see langword="true"/>` if the associated process has exited; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="timeout">
        /// The maximum amount of time to block waiting for exit.
        /// <para/>
        /// To block until the process exits without a timeout, pass `<see cref="TimeSpan.MaxValue"/>`.
        /// <para/>
        /// If you pass `<see cref="TimeSpan.Zero"/>` to the method, it returns true only if the process has already exited; otherwise, it immediately returns false.
        /// </param>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        /// <exception cref="ApplicationException">If the process failed to start correctly.</exception>
        bool WaitForExit(TimeSpan timeout);

        /// <summary>
        /// Blocks the calling thread until the process exits.
        /// </summary>
        /// <exception cref="InvalidOperationException">Before `<see cref="Start"/>` has been called.</exception>
        /// <exception cref="ApplicationException">If the process failed to start correctly.</exception>
        void WaitForExit();
    }
}