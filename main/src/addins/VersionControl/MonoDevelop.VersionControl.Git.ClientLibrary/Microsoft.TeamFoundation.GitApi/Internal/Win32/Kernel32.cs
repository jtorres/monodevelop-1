//*************************************************************************************************
// Kernel32.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.TeamFoundation.GitApi.Internal.Win32
{
    internal static class Kernel32
    {
        public static readonly IntPtr InvalidHandleValue = new IntPtr(-1);
        public const string Name = "kernel32.dll";

        /// <summary>
        /// Assigns a process to an existing job object.
        /// </summary>
        /// <param name="jobObjectHandle">A handle to the job object to which the process will be
        /// associated. The <see cref="CreateJobObject"/> or OpenJobObject function
        /// returns this handle. The handle must have the JOB_OBJECT_ASSIGN_PROCESS access right.
        /// </param>
        /// <param name="processHandle">
        /// <para>A handle to the process to associate with the job object. The handle must have
        /// the <see cref="DesiredAccess.SetQuota"/> and <see cref="DesiredAccess.Terminate"/> access rights.</para>
        /// <para>If the process is already associated with a job, the job specified by hJob must
        /// be empty or it must be in the hierarchy of nested jobs to which the process already
        /// belongs, and it cannot have UI limits set(<see cref="SetInformationJobObject"/> with
        /// JobObjectBasicUIRestrictions).</para>
        /// </param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        /// <remarks>If the function fails, the return value is zero. To get extended error
        /// information, call <see cref="Marshal.GetLastWin32Error"/>.</remarks>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "AssignProcessToJobObject", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool AssignProcessToJobObject(
            SafeHandle jobObjectHandle,
            SafeHandle processHandle);

        /// <summary>
        /// Allocates a new console for the calling process.
        /// </summary>
        /// <returns>True if success; otherwise false.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "AllocConsole", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool AllocConsole();

        /// <summary>
        /// Attaches the calling process to the console of the specified process.
        /// </summary>
        /// <param name="processId">
        /// <para>The identifier of the process whose console is to be used.</para>
        /// <para>Use -1 to use the console of the parent of the current process.</para>
        /// </param>
        /// <returns></returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "AttachConsole", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool AttachConsole(uint processId);

        /// <summary>
        /// Closes an open object handle.
        /// </summary>
        /// <param name="handle">A valid handle to an open object.</param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        /// <remarks>If the function fails, the return value is zero. To get extended error
        /// information, call <see cref="Marshal.GetLastWin32Error"/>.</remarks>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "CloseHandle", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool CloseHandle(IntPtr handle);

        /// <summary>
        /// Enables a named pipe server process to wait for a client process to connect to an instance of a named pipe.
        /// </summary>
        /// <param name="pipeHandle">
        /// <para>A handle to the server end of a named pipe instance.</para>
        /// <para>This handle is returned by the <see cref="CreateNamedPipe"/> function.</para>
        /// </param>
        /// <param name="reserved"></param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "ConnectNamedPipe", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool ConnectNamedPipe(SafeFileHandle pipeHandle, IntPtr reserved);

        /// <summary>
        /// <para>Creates or opens a file or I/O device.</para>
        /// <para>The most commonly used I/O devices are as follows: file, file stream, directory, physical disk, volume, console buffer, tape drive, communications resource, mailslot, and pipe.</para>
        /// <para>The function returns a handle that can be used to access the file or device for various types of I/O depending on the file or device and the flags and attributes specified.</para>
        /// </summary>
        /// <param name="fileName">
        /// <para>The name of the file or device to be created or opened. You may use either forward slashes (/) or backslashes (\) in this name.</para>
        /// <para>To create a file stream, specify the name of the file, a colon, and then the name of the stream.</para>
        /// </param>
        /// <param name="desiredAccess">The requested access to the file or device, which can be summarized as read, write, both or neither zero).</param>
        /// <param name="fileShare">
        /// <para>The requested sharing mode of the file or device, which can be read, write, both, delete, all of these, or none (refer to the following table).</para>
        /// <para>Access requests to attributes or extended attributes are not affected by this flag.</para>
        /// <para>If this parameter is <see cref="System.IO.FileShare.None"/> and <see cref="CreateFile"/>  succeeds, the file or device cannot be shared and cannot be opened again until the handle to the file or device is closed.</para>
        /// </param>
        /// <param name="securityAttributes">
        /// <para>A pointer to a <see cref="SecurityAttributes"/> structure that contains two separate but related data members: an optional security descriptor, and a Boolean value that determines whether the returned handle can be inherited by child processes.</para>
        /// <para>This parameter can be <see langword="null"/>.</para>
        /// <para>If this parameter is <see langword="null"/>, the handle returned cannot be inherited by any child processes the application may create and the file or device associated with the returned handle gets a default security descriptor.</para>
        /// </param>
        /// <param name="fileMode">An action to take on a file or device that exists or does not exist.</param>
        /// <param name="fileOptions">The file or device attributes and flags.</param>
        /// <param name="templates">
        /// <para>A valid handle to a template file with the <see cref="System.IO.FileShare.Read"/> access right.</para>
        /// <para>The template file supplies file attributes and extended attributes for the file that is being created.</para>
        /// </param>
        /// <returns></returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern SafeFileHandle CreateFile(
            [In][MarshalAs(UnmanagedType.LPWStr)] string fileName,
            [In] System.IO.FileAccess desiredAccess,
            [In] System.IO.FileShare fileShare,
            [Optional][In] ref SecurityAttributes securityAttributes,
            [In] System.IO.FileMode fileMode,
            [In] System.IO.FileOptions fileOptions,
            [Optional][In] IntPtr templates);

        /// <summary>
        /// <para>Creates an instance of a named pipe and returns a handle for subsequent pipe operations.</para>
        /// <para>A named pipe server process uses this function either to create the first instance of a specific named pipe and establish its basic attributes or to create a new instance of an existing named pipe.</para>
        /// </summary>
        /// <param name="pipeName">
        /// <para>The unique pipe name. This string must have the following form: "\\.\pipe\pipename"</para>
        /// <para>The pipename part of the name can include any character other than a backslash, including numbers and special characters.</para>
        /// <para>The entire pipe name string can be up to 256 characters long.</para>
        /// <para>Pipe names are not case sensitive.</para>
        /// </param>
        /// <param name="pipeType">The open mode.</param>
        /// <param name="pipeMode">The pipe mode.</param>
        /// <param name="maxInstances">
        /// <para>The maximum number of instances that can be created for this pipe.</para>
        /// <para>The first instance of the pipe can specify this value; the same number must be specified for other instances of the pipe.</para>
        /// <para>Acceptable values are in the range 1 through `PIPE_UNLIMITED_INSTANCES` (255).</para>
        /// </param>
        /// <param name="outBufferSize">The number of bytes to reserve for the output buffer.</param>
        /// <param name="inBufferSize">The number of bytes to reserve for the input buffer. </param>
        /// <param name="defaultTimeOut">
        /// <para>The default time-out value, in milliseconds, if the `WaitNamedPipe` function specifies `NMPWAIT_USE_DEFAULT_WAIT`.</para>
        /// <para>Each instance of a named pipe must specify the same value.</para>
        /// <para>A value of zero will result in a default time-out of 50 milliseconds.</para>
        /// </param>
        /// <param name="securityAttributes">
        /// <para>A pointer to a <see cref="SecurityAttributes"/> structure that specifies a security descriptor for the new named pipe and determines whether child processes can inherit the returned handle.</para>
        /// <para>If <paramref name="securityAttributes"/> is <see langword="null"/>, the named pipe gets a default security descriptor and the handle cannot be inherited.</para>
        /// </param>
        /// <returns>On success, a handle to the server end of a named pipe instance; otherwise <see cref="InvalidHandleValue"/>.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "CreateNamedPipeW", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern SafeFileHandle CreateNamedPipe(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pipeName,
            [In] PipeType pipeType,
            [In] PipeMode pipeMode,
            [In] uint maxInstances,
            [In] uint outBufferSize,
            [In] uint inBufferSize,
            [In] uint defaultTimeOut,
            [Optional][In] ref SecurityAttributes securityAttributes);

        /// <summary>
        /// Creates or opens a job object.
        /// </summary>
        /// <param name="securityAttributes">A pointer to a <see cref="SecurityAttributes"/>
        /// structure that specifies the security descriptor for the job object and determines
        /// whether child  processes can inherit the returned handle. If securityAttributes is NULL,
        /// the job object gets a default security descriptor and the handle cannot be inherited.
        /// The ACLs in the default security descriptor for a job object come from the primary or
        /// impersonation token of the creator.</param>
        /// <param name="name">
        /// <para>The name of the job. The name is limited to MAX_PATH characters. Name comparison
        /// is case-sensitive.</para>
        ///
        /// <para>If <paramref name="name"/> is <see langword="null"/>, the job is created without a name.</para>
        ///
        /// <para>If <paramref name="name"/> matches the name of an existing event, semaphore, mutex,
        /// waitable timer, or file-mapping object, the function fails and the
        /// <see cref="Marshal.GetLastWin32Error"/> function returns
        /// <see cref="InvalidHandleValue"/>. This occurs because these objects share the same
        /// namespace.</para>
        /// </param>
        /// <returns>If the function succeeds, the return value is a handle to the job object. The
        /// handle has the <see cref="DesiredAccess.AllAccess"/> access right. If the object existed before the
        /// function call, the function returns a handle to the existing job object and
        /// <see cref="Marshal.GetLastWin32Error"/> returns
        /// <see cref="ErrorCode.AlreadyExists"/>.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "CreateJobObjectW", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern SafeJobObjectHandle CreateJobObject(
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.LPWStr)]string name);

        /// <summary>
        /// Creates an anonymous pipe, and returns handles to the read and write ends of the pipe.
        /// </summary>
        /// <param name="pipeReadableHandle">A pointer to a variable that receives the read handle for
        /// the pipe.</param>
        /// <param name="pipeWritableHandle">A pointer to a variable that receives the write handle for
        /// the pipe.</param>
        /// <param name="pipeAttributes">A pointer to a <see cref="SecurityAttributes"/> structure
        /// that determines whether the returned handle can be inherited by child processes. If
        /// pipeAttributes is <see langword="null"/>, the handle cannot be inherited.</param>
        /// <param name="pipeSize">The size of the buffer for the pipe, in bytes. The size is only a
        /// suggestion; the system uses the value to calculate an appropriate buffering mechanism.
        /// If this parameter is zero, the system uses the default buffer size.</param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "CreatePipe", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool CreatePipe(
            out SafeFileHandle pipeReadableHandle,
            out SafeFileHandle pipeWritableHandle,
            ref SecurityAttributes pipeAttributes,
            uint pipeSize);

        /// <summary>
        /// Creates a new process and its primary thread. The new process runs in the security
        /// context of the calling process.
        /// </summary>
        /// <param name="applicationName">
        /// <para>The name of the module to be executed. This module can be a Windows-based
        /// application. It can be some other type of module (for example, MS-DOS or OS/2) if the
        /// appropriate subsystem is available on the local computer.</para>
        ///
        /// <para>The string can specify the full path and file name of the module to execute or it
        /// can specify a partial name. In the case of a partial name, the function uses the
        /// current drive and current directory to complete the specification. The function will
        /// not use the search path. This parameter must include the file name extension; no
        /// default extension is assumed.</para>
        ///
        /// <para>The applicationName parameter can be NULL. In that case, the module name must be
        /// the first white space–delimited token in the commandLine string. If you are using a
        /// long file name that contains a space, use quoted strings to indicate where the file
        /// name ends and the arguments begin; otherwise, the file name is ambiguous.</para>
        /// </param>
        /// <param name="commandLine">
        /// <para>The command line to be executed. The maximum length of this string is 32K
        /// characters. If applicationName is NULL, the module name portion of commandLine is
        /// limited to MAX_PATH characters</para>
        ///
        /// <para>The Unicode version of this function, <see cref="CreateProcessAsUser"/>, can
        /// modify the contents of this string. Therefore, this parameter cannot be a pointer to
        /// read-only memory (such as a const variable or a literal string). If this parameter is a
        /// constant string, the function may cause an access violation.</para>
        ///
        /// <para>The commandLine parameter can be NULL. In that case, the function uses the string
        /// pointed to by applicationName as the command line.</para>
        ///
        /// <para>If applicationName is NULL, the first white space–delimited token of the command
        /// line specifies the module name. If you are using a long file name that contains a space,
        /// use quoted strings to indicate where the file name ends and the arguments begin (see
        /// the explanation for the applicationName parameter). If the file name does not contain
        /// an extension, .exe is appended.</para>
        /// </param>
        /// <param name="processAttributes">
        /// <para>A pointer to a <see cref="SecurityAttributes"/> structure that determines whether
        /// the returned handle to the new process object can be inherited by child processes. If
        /// processAttributes is NULL, the handle cannot be inherited.</para>
        ///
        /// <para>The <see cref="SecurityAttributes.Descriptor"/> member of the structure
        /// specifies a security descriptor for the new process. If processAttributes is NULL or
        /// <see cref="SecurityAttributes.Descriptor"/> is NULL, the process gets a default
        /// security descriptor. The ACLs in the default security descriptor for a process come
        /// from the primary token of the creator.</para>
        /// </param>
        /// <param name="threadAttributes">
        /// <para>A pointer to a <see cref="SecurityAttributes"/> structure that determines whether
        /// the returned handle to the new thread object can be inherited by child processes. If
        /// threadAttributes is NULL, the handle cannot be inherited.</para>
        ///
        /// <para>The <see cref="SecurityAttributes.Descriptor"/> member of the structure
        /// specifies a security descriptor for the main thread. If threadAttributes is NULL or
        /// <see cref="SecurityAttributes.Descriptor"/> is NULL, the thread gets a default
        /// security descriptor. The ACLs in the default security descriptor for a thread come from
        /// the process token.</para>
        /// </param>
        /// <param name="inheritHandles">If this parameter is TRUE, each inheritable handle in the
        /// calling process is inherited by the new process. If the parameter is FALSE, the handles
        /// are not inherited. Note that inherited handles have the same value and access rights as
        /// the original handles.</param>
        /// <param name="creationFlags">
        /// <para>The flags that control the priority class and the creation of the process.</para>
        ///
        /// <para>This parameter also controls the new process's priority class, which is used to
        /// determine the scheduling priorities of the process's threads.</para>
        /// </param>
        /// <param name="environment">
        /// <para>A pointer to an environment block for the new process. If this parameter is NULL,
        /// the new process uses the environment of the calling process.</para>
        ///
        /// <para>An environment block consists of a null-terminated block of null-terminated
        /// strings. Because the equal sign is used as a separator, it must not be used in the name
        /// of an environment variable.</para>
        ///
        /// <para>An environment block can contain either Unicode or ANSI characters. If the
        /// environment block pointed to by environment contains Unicode characters, be sure that
        /// creationFlags includes <see cref="ProcessCreationFlags.UnicodeEnvironment"/>. If this
        /// parameter is NULL and the environment block of the parent process contains Unicode
        /// characters, you must also ensure that creationFlags includes
        /// <see cref="ProcessCreationFlags.UnicodeEnvironment"/>.</para>
        /// </param>
        /// <param name="currentDirectory">
        /// <para>The full path to the current directory for the process. The string can also
        /// specify a UNC path.</para>
        ///
        /// <para>If this parameter is NULL, the new process will have the same current drive and
        /// directory as the calling process.</para>
        /// </param>
        /// <param name="startupInfo">
        /// <para>The user must have full access to both the specified window station and desktop.
        /// If you want the process to be interactive, specify winsta0\default. If the
        /// <see cref="ProcessStartupInfo.Desktop"/> member is NULL, the new process inherits the
        /// desktop and window station of its parent process. If this member is an empty string, "",
        /// the new process connects to a window station using the rules described in Process
        /// Connection to a Window Station.</para>
        ///
        /// <para>Handles in <see cref="ProcessStartupInfo"/> must be closed with
        /// <see cref="CloseHandle"/> when they are no longer needed.</para>
        ///
        /// <para>Important: The caller is responsible for ensuring that the standard handle fields
        /// in <see cref="ProcessStartupInfo"/> contain valid handle values. These fields are
        /// copied unchanged to the child process without validation, even when the
        /// <see cref=ProcessStartupInfo.Flags"/> member specifies
        /// <see cref="ProcessStartupFlags.UseStandardHandles"/>. Incorrect values can cause the child
        /// process to misbehave or crash. Use the Application Verifier runtime verification tool
        /// to detect invalid handles.</para>
        /// </param>
        /// <param name="processInfo">
        /// <para>A pointer to a <see cref="ProcessStartupInfo"/> structure that receives
        /// identification information about the new process.</para>
        ///
        /// <para>Handles in <see cref="ProcessStartupInfo"/> must be closed with
        /// <see cref="CloseHandle"/> when they are no longer needed.</para>
        /// </param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "CreateProcessW", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool CreateProcess(
            [In, MarshalAs(UnmanagedType.LPWStr)] string applicationName,
            [In] IntPtr commandLine,
            [In] IntPtr processAttributes,
            [In] IntPtr threadAttributes,
            [In, MarshalAs(UnmanagedType.Bool)] bool inheritHandles,
            [In, MarshalAs(UnmanagedType.U4)] ProcessCreationFlags creationFlags,
            [In] IntPtr environment,
            [In, MarshalAs(UnmanagedType.LPWStr)] string currentDirectory,
            [In] ref ProcessStartupInfoEx startupInfo,
            [Out] out ProcessInformation processInfo);

        /// <summary>
        /// Creates a thread that runs in the virtual address space of another process.
        /// </summary>
        /// <param name="processHandle">A handle to the process in which the thread is to be
        /// created. The handle must have the PROCESS_CREATE_THREAD, PROCESS_QUERY_INFORMATION,
        /// PROCESS_VM_OPERATION, PROCESS_VM_WRITE, and PROCESS_VM_READ access rights, and may
        /// fail without these rights on certain platforms.</param>
        /// <param name="threadAttributes">A pointer to a SECURITY_ATTRIBUTES structure that
        /// specifies a security descriptor for the new thread and determines whether child
        /// processes can inherit the returned handle. If lpThreadAttributes is NULL, the thread
        /// gets a default security descriptor and the handle cannot be inherited.</param>
        /// <param name="stackSize">The initial size of the stack, in bytes. The system rounds
        /// this value to the nearest page. If this parameter is 0 (zero), the new thread uses the
        /// default size for the executable.</param>
        /// <param name="threadProc">A pointer to the application-defined function of type
        /// LPTHREAD_START_ROUTINE to be executed by the thread and represents the starting address
        /// of the thread in the remote process.</param>
        /// <param name="parameter">A pointer to a variable to be passed to the thread function
        /// pointed to by lpStartAddress.</param>
        /// <param name="flags">The flags that control the creation of the thread.</param>
        /// <param name="threadId">A pointer to a variable that receives the thread identifier.</param>
        /// <returns>If successful, the return value is a handle to the new thread; otherwise null.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "CreateRemoteThread", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern SafeThreadHandle CreateRemoteThread(
            SafeProcessHandle processHandle,
            IntPtr threadAttributes,
            uint stackSize,
            IntPtr threadProc,
            IntPtr parameter,
            ThreadCreationFlags flags,
            out uint threadId);

        /// <summary>
        /// Takes a snapshot of the specified processes, as well as the heaps, modules, and threads
        /// used by these processes.
        /// </summary>
        /// <param name="flags">The portions of the system to be included in the snapshot. </param>
        /// <param name="processId">
        /// <para>The process identifier of the process to be included in the snapshot. This parameter
        /// can be zero to indicate the current process. This parameter is used when the
        /// <see cref="ToolhelpSnapshotFlags.HeapList"/>, <see cref="ToolhelpSnapshotFlags.Module"/>,
        /// <see cref="ToolhelpSnapshotFlags.Module32"/>, or <see cref="ToolhelpSnapshotFlags.All"/>
        /// value is specified. Otherwise, it is ignored and all processes are included in the snapshot.</para>
        /// <para>If the specified process is the Idle process or one of the CSRSS processes, this
        /// function fails and the last error code is <see cref="ErrorCode.AccessDenied"/> because
        /// their access restrictions prevent user-level code from opening them.</para>
        /// <para>If the specified process is a 64-bit process and the caller is a 32-bit process, this
        /// function fails and the last error code is <see cref="ErrorCode.PartialCopy"/>.</para>
        /// </param>
        /// <returns>
        /// Handle to the snapshot is successful; otherwise <see cref="IntPtr.Zero"/> or <see cref="InvalidHandleValue"/>.
        /// </returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "CreateToolhelp32Snapshot", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern SafeSnapshotHandle CreateToolhelp32Snapshot(
            ToolhelpSnapshotFlags flags,
            uint processId);

        /// <summary>
        /// Deletes the specified list of attributes for process and thread creation.
        /// </summary>
        /// <param name="processThreadAttributionList">
        /// <para>The attribute list.</para>
        /// <para>This list is created by the <see cref="InitializeProcThreadAttributeList"/> function.</para>
        /// </param>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "DeleteProcThreadAttributeList", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern void DeleteProcThreadAttributeList(IntPtr processThreadAttributionList);

        /// <summary>
        /// Duplicates an object handle.
        /// </summary>
        /// <param name="sourceProcessHandle">A handle to the process with the handle to be
        /// duplicated.</param>
        /// <param name="sourceHandle">The handle to be duplicated. This is an open object handle
        /// that is valid in the context of the source process. </param>
        /// <param name="targetProcessHandle">A handle to the process that is to receive the
        /// duplicated handle. The handle must have the PROCESS_DUP_HANDLE access right.</param>
        /// <param name="targetHandle">A pointer to a variable that receives the duplicate handle.
        /// This handle value is valid in the context of the target process.</param>
        /// <param name="desiredAccess">The access requested for the new handle.</param>
        /// <param name="inheritHandle">A variable that indicates whether the handle is inheritable.
        /// If TRUE, the duplicate handle can be inherited by new processes created by the target
        /// process. If FALSE, the new handle cannot be inherited.</param>
        /// <param name="options">Optional actions.</param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "DuplicateHandle", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool DuplicateHandle(
            SafeProcessHandle sourceProcessHandle,
            SafeFileHandle sourceHandle,
            SafeProcessHandle targetProcessHandle,
            out SafeFileHandle targetHandle,
            DesiredAccess desiredAccess,
            bool inheritHandle,
            DuplicateHandleOptions options);

        /// <summary>
        /// Duplicates an object handle.
        /// </summary>
        /// <param name="sourceProcessHandle">A handle to the process with the handle to be
        /// duplicated.</param>
        /// <param name="sourceHandle">The handle to be duplicated. This is an open object handle
        /// that is valid in the context of the source process. </param>
        /// <param name="targetProcessHandle">A handle to the process that is to receive the
        /// duplicated handle. The handle must have the PROCESS_DUP_HANDLE access right.</param>
        /// <param name="targetHandle">A pointer to a variable that receives the duplicate handle.
        /// This handle value is valid in the context of the target process.</param>
        /// <param name="desiredAccess">The access requested for the new handle.</param>
        /// <param name="inheritHandle">A variable that indicates whether the handle is inheritable.
        /// If TRUE, the duplicate handle can be inherited by new processes created by the target
        /// process. If FALSE, the new handle cannot be inherited.</param>
        /// <param name="options">Optional actions.</param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "DuplicateHandle", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool DuplicateHandle(
            Microsoft.Win32.SafeHandles.SafeProcessHandle sourceProcessHandle,
            SafeFileHandle sourceHandle,
            Microsoft.Win32.SafeHandles.SafeProcessHandle targetProcessHandle,
            out SafeFileHandle targetHandle,
            DesiredAccess desiredAccess,
            bool inheritHandle,
            DuplicateHandleOptions options);

        /// <summary>
        /// Ends the calling process and all its threads.
        /// </summary>
        /// <param name="exitCode">The exit code for the process and all threads.</param>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "ExitProcess", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern void ExitProcess(int exitCode);

        /// <summary>
        /// Detaches the calling process from its console.
        /// </summary>
        /// <returns>True if succeeds; false otherwise.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "FreeConsole", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool FreeConsole();

        /// <summary>
        /// Sends a specified signal to a console process group that shares the console associated with the calling process.
        /// </summary>
        /// <param name="ctrlEvent">The type of signal to be generated. This parameter can be one of the following values.</param>
        /// <param name="processGroupId">
        /// <para>The identifier of the process group to receive the signal.</para>
        /// <para>A process group is created when the <see cref="ProcessCreationFlags.NewProcessGroup"/> flag is specified in a call
        /// <see cref="CreateProcess(string, IntPtr, IntPtr, IntPtr, bool, ProcessCreationFlags, IntPtr, string, ref ProcessStartupInfoEx, out ProcessInformation)"/> 
        /// The process identifier of the new process is also the process group identifier of a new process group. 
        /// The process group includes all processes that are descendants of the root process. Only those processes in the 
        /// group that share the same console as the calling process receive the signal. In other words, if a process in the 
        /// group creates a new console, that process does not receive the signal, nor do its descendants.</para>
        /// <para>If this parameter is zero, the signal is generated in all processes that share the console of the calling process.</para>
        /// </param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GenerateConsoleCtrlEvent", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool GenerateConsoleCtrlEvent(
            ConsoleCtrlEvent ctrlEvent,
            uint processGroupId);

        /// <summary>
        /// Retrieves the input code page used by the console associated with the calling process.
        /// A console uses its input code page to translate keyboard input into the corresponding
        /// character value.
        /// </summary>
        /// <returns>A code that identifies the code page.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetConsoleCP", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern uint GetConsoleInputCP();

        /// <summary>
        /// Retrieves the output code page used by the console associated with the calling process.
        /// A console uses its output code page to translate the character values written by the
        /// various output functions into the images displayed in the console window.
        /// </summary>
        /// <returns>A code that identifies the code page.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetConsoleOutputCP", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern uint GetConsoleOutputCP();

        /// <summary>
        /// <para>Retrieves the termination status of the specified process.</para>
        /// <para>Important  The GetExitCodeProcess function returns a valid error code defined by
        /// the application only after the thread terminates.</para>
        /// <para>Therefore, an application should not use STILL_ACTIVE (259) as an error code.</para>
        /// <para>If a thread returns STILL_ACTIVE (259) as an error code, applications that test
        /// for this value could interpret it to mean that the thread is still running and continue
        /// to test for the completion of the thread after the thread has terminated, which could
        /// put the application into an infinite loop.</para>
        /// </summary>
        /// <param name="processHandle">A handle to the process.</param>
        /// <param name="exitCode">The exit code of the process.</param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetExitCodeProcess", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool GetExitCodeProcess(
            SafeProcessHandle processHandle,
            out int exitCode);

        /// <summary>
        /// Retrieves a handle to the specified standard device (standard input, standard output,
        /// or standard error).
        /// </summary>
        /// <param name="stdHandleType">The standard device.</param>
        /// <returns>
        /// <para>If the function succeeds, the return value is a handle to the specified device,
        /// or a redirected handle set by a previous call to SetStdHandle. The handle has
        /// GENERIC_READ and GENERIC_WRITE access rights, unless the application has used
        /// SetStdHandle to set a standard handle with lesser access.</para>
        ///
        /// <para>If an application does not have associated standard handles, such as a service
        /// running on an interactive desktop, and has not redirected them, the return value is
        /// NULL.</para>
        /// </returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetStdHandle", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr GetStdHandle(StandardHandleType stdHandleType);

        /// <summary>
        /// Retrieves a pseudo handle for the current process.
        /// </summary>
        /// <returns>The return value is a pseudo handle to the current process.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetCurrentProcess", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern SafeProcessHandle GetCurrentProcess();

        /// <summary>
        /// Retrieves the process identifier of the calling process.
        /// </summary>
        /// <returns>Process identifier of the calling process.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetCurrentProcessId", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern uint GetCurrentProcessId();

        /// <summary>
        /// Retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).
        /// </summary>
        /// <param name="moduleHandle">
        /// A handle to the DLL module that contains the function or variable. The <see cref="LoadLibrary(string)"/> function
        /// returns this handle.
        /// </param>
        /// <param name="procName">
        /// The function or variable name, or the function's ordinal value. If this parameter is an ordinal value, it must be
        /// in the low-order word; the high-order word must be zero.
        /// </param>
        /// <returns>If successful, the address of the function or variable; otherwise <see cref="IntPtr.Zero"/>.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr GetProcAddress(
            IntPtr moduleHandle,
            [MarshalAs(UnmanagedType.LPStr)]string procName);

        /// <summary>
        /// Initializes the specified list of attributes for process and thread creation.
        /// </summary>
        /// <param name="processThreadAttributionList">The attribute list. This parameter can be <see cref="IntPtr.Zero"/> to determine the buffer size required to support the specified number of attributes.</param>
        /// <param name="attributeCount">The count of attributes to be added to the list.</param>
        /// <param name="reserved">This parameter is reserved and must be zero.</param>
        /// <param name="size">
        /// <para>When `<paramref name="processThreadAttributionList"/>` is not <see langword="null"/>, specifies the size in bytes of the uninitialized buffer and receives the size in bytes of the initialized buffer.</para>
        /// <para>Otherwise, this parameter receives the required buffer size in bytes.</para>
        /// </param>
        /// <returns><see langword="true"/> if success; otherwise <see langword="false"/>.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "InitializeProcThreadAttributeList", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool InitializeProcThreadAttributeList(
            IntPtr processThreadAttributionList,
            uint attributeCount,
            IntPtr reserved,
            ref IntPtr size);

        /// <summary>
        /// Determines whether the process is running in the specified job.
        /// </summary>
        /// <param name="processHandle">
        /// A handle to the process to be tested.
        /// The handle must have the <see cref="DesiredAccess.QueryInformation"/> or <see cref="DesiredAccess.QueryLimitedInformation"/> access right.
        /// </param>
        /// <param name="jobHandle">
        /// An optional handle to the job.
        /// If this parameter is <see langword="null"/>, the function tests if the process is running under any job.
        /// </param>
        /// <param name="isProcessInJob"><see langword="true"/> if the process is running in the specified <paramref name="jobHandle"/>; otherwise false</param>
        /// <returns><see langword="true"/> if success; otherwise <see langword="false"/>.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "IsProcessInJob", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsProcessInJob(
            SafeProcessHandle processHandle,
            SafeJobObjectHandle jobHandle,
            out bool isProcessInJob);

        /// <summary>
        /// <para>Determines whether the specified process is running under WOW64.</para>
        /// <para>WOW64 is the x86 emulator that allows 32-bit Windows-based applications to run seamlessly on 64-bit Windows.</para>
        /// </summary>
        /// <param name="processHandle">A handle to the process. The handle must have the <see cref="DesiredAccess.QueryInformation"/>
        /// or <see cref="DesiredAccess.QueryLimitedInformation"/> access right.</param>
        /// <param name="isWow64Process">
        /// <para><see lang="true"/> if the process is running under WOW64. If the process is running under 32-bit Windows, the value is set to FALSE.</para>
        /// <para>If the process is a 64-bit application running under 64-bit Windows, the value is also set to <see langword="false"/>.</para>
        /// </param>
        /// <returns>True if success; otherwise false.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "IsWow64Process", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool IsWow64Process(
            SafeProcessHandle processHandle,
            out bool isWow64Process);

        /// <summary>
        /// Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded.
        /// </summary>
        /// <param name="libraryFileName">
        /// <para>The name of the module. This can be either a library module (a .dll file) or an executable module (an .exe file). The name
        /// specified is the file name of the module and is not related to the name stored in the library module itself, as specified by the
        /// LIBRARY keyword in the module-definition (.def) file.</para>
        /// <para>If the string specifies a full path, the function searches only that path for the module.</para>
        /// <para>If the string specifies a relative path or a module name without a path, the function uses a standard search strategy to
        /// find the module; for more information, see the Remarks.</para>
        /// <para>If the function cannot find the module, the function fails. When specifying a path, be sure to use backslashes (\), not
        /// forward slashes (/).</para>
        /// <para>If the string specifies a module name without a path and the file name extension is omitted, the function appends the default
        /// library extension .dll to the module name. To prevent the function from appending .dll to the module name, include a trailing point
        /// character (.) in the module name string.</para>
        /// </param>
        /// <returns>If successfull, a handle to the loaded module; otherwise <see cref="IntPtr.Zero"/>.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "LoadLibraryW", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr LoadLibrary(
            [MarshalAs(UnmanagedType.LPWStr)]string libraryFileName);

        /// <summary>
        /// Opens an existing local process object.
        /// </summary>
        /// <param name="desiredAccess">The access to the process object. This access right is checked against the security
        /// descriptor for the process.</param>
        /// <param name="inheritHandle">If this value is TRUE, processes created by this process will inherit the handle.
        /// Otherwise, the processes do not inherit this handle.</param>
        /// <param name="processId">
        /// <para>The identifier of the local process to be opened.</para>
        /// <para>If the specified process is the System Process (0x00000000), the function fails and the last error code is
        /// <see cref="ErrorCode.InvalidParameter"/>. If the specified process is the Idle process or one of the CSRSS
        /// processes, this function fails and the last error code is <see cref="ErrorCode.AccessDenied"/> because their
        /// access restrictions prevent user-level code from opening them.</para>
        /// </param>
        /// <returns>If the function succeeds, the return value is an open handle to the specified process.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "OpenProcess", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern SafeProcessHandle OpenProcess(
            DesiredAccess desiredAccess,
            bool inheritHandle,
            uint processId);

        /// <summary>
        /// Retrieves information about the first process encountered in a system snapshot.
        /// </summary>
        /// <param name="snapshotHandle">A handle to the snapshot returned from a previous call to the
        /// <see cref="CreateToolhelp32Snapshot"/> function.</param>
        /// <param name="processEntry">
        /// A pointer to a <see cref="ProcessEntry32"/> structure. It contains process information such as the name of the executable file,
        /// the process identifier, and the process identifier of the parent process.
        /// </param>
        /// <returns><see langword="true"/> if the first entry of the process list has been copied to the buffer; otherwise <see langword="false"/>.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "Process32FirstW", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool Process32First(
            SafeSnapshotHandle snapshotHandle,
            ref ProcessEntry32 processEntry);

        /// <summary>
        /// Retrieves information about the first process encountered in a system snapshot.
        /// </summary>
        /// <param name="snapshotHandle">A handle to the snapshot returned from a previous call to the
        /// <see cref="CreateToolhelp32Snapshot"/> function.</param>
        /// <param name="processEntry">
        /// A pointer to a <see cref="ProcessEntry32"/> structure. It contains process information such as the name of the executable file,
        /// the process identifier, and the process identifier of the parent process.
        /// </param>
        /// <returns><see langword="true"/> if the next entry of the process list has been copied to the buffer; otherwise <see langword="false"/>.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "Process32NextW", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool Process32Next(
            SafeSnapshotHandle snapshotHandle,
            ref ProcessEntry32 processEntry);

        /// <summary>
        /// Decrements a thread's suspend count. When the suspend count is decremented to zero, the
        /// execution of the thread is resumed.
        /// </summary>
        /// <param name="threadHandle">A handle to the thread to be restarted.</param>
        /// <returns>If the function succeeds, the return value is the thread's previous suspend
        /// count, else -1</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "ResumeThread", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern int ResumeThread(IntPtr threadHandle);

        /// <summary>
        /// Decrements a thread's suspend count. When the suspend count is decremented to zero, the
        /// execution of the thread is resumed.
        /// </summary>
        /// <param name="threadHandle">A handle to the thread to be restarted.</param>
        /// <returns>If the function succeeds, the return value is the thread's previous suspend
        /// count, else -1</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "ResumeThread", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern int ResumeThread(SafeThreadHandle threadHandle);

        /// <summary>
        /// Sets the contents of the specified environment variable for the current process.
        /// </summary>
        /// <param name="name">The name of the environment variable. The operating system creates the environment variable if it does not exist and <paramref name="name"/> is not <see langword="null"/>.</param>
        /// <param name="value">
        /// <para>The contents of the environment variable. The maximum size of a user-defined environment variable is <see cref="Int16.MaxValue"/> characters.</para>
        /// <para>If this parameter is <see langword="null"/>, the variable is deleted from the current process's environment.</para>
        /// </param>
        /// <returns></returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "SetEnvironmentVariableW", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool SetEnvironmentVariable(
            [MarshalAs(UnmanagedType.LPWStr)]string name,
            [MarshalAs(UnmanagedType.LPWStr)]string value);

        /// <summary>
        /// Sets certain properties of an object handle.
        /// </summary>
        /// <param name="handle">
        /// <para>A handle to an object whose information is to be set.</para>
        ///
        /// <para>You can specify a handle to one of the following types of objects: access token,
        /// console input buffer, console screen buffer, event, file, file mapping, job, mailslot,
        /// mutex, pipe, printer, process, registry key, semaphore, serial communication device,
        /// socket, thread, or waitable timer.</para>
        /// </param>
        /// <param name="mask">A mask that specifies the bit flags to be changed.</param>
        /// <param name="flags">Set of bit flags that specifies properties of the object handle.</param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "SetHandleInformation", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool SetHandleInformation(
            SafeHandle handle,
            HandleInformationFlags mask,
            HandleInformationFlags flags);

        /// <summary>
        /// Sets limits for a job object.
        /// </summary>
        /// <param name="jobObjectHandle">A handle to the job whose limits are being set. The
        /// <see cref="CreateJobObject"/> or OpenJobObject function returns this
        /// handle. The handle must have the JOB_OBJECT_SET_ATTRIBUTES access right.</param>
        /// <param name="jobObjectInfoClass">The information class for the limits to be set.</param>
        /// <param name="info">The limits or job state to be set for the job. The format of this
        /// data depends on the value of <see cref="JobObjectInfoClass"/>. Value must be
        /// <see cref="JobObjectInfoClass.BasicLimitInformation"/>.</param>
        /// <param name="infoLength">The size of the job information being set, in bytes.</param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        /// <seealso cref="JobObjectInfoClass"/>
        /// <seealso cref="JobObjectBasicLimitInformation"/>
        /// <seealso cref="JobObjectLimitFlags"/>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "SetInformationJobObject", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool SetInformationJobObject(
            SafeJobObjectHandle jobObjectHandle,
            JobObjectInfoClass jobObjectInfoClass,
            ref JobObjectBasicLimitInformation info,
            int infoLength);

        /// <summary>
        /// Sets limits for a job object.
        /// </summary>
        /// <param name="jobObjectHandle">A handle to the job whose limits are being set. The
        /// <see cref="CreateJobObject"/> or OpenJobObject function returns this
        /// handle. The handle must have the JOB_OBJECT_SET_ATTRIBUTES access right.</param>
        /// <param name="jobObjectInfoClass">The information class for the limits to be set.</param>
        /// <param name="info">The limits or job state to be set for the job. The format of this
        /// data depends on the value of <see cref="JobObjectInfoClass"/>. The value must be
        /// <see cref="JobObjectInfoClass.ExtendedLimitInformation"/>.</param>
        /// <param name="infoLength">The size of the job information being set, in bytes.</param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        /// <seealso cref="JobObjectInfoClass"/>
        /// <seealso cref="JobObjectExtendedLimitInformation"/>
        /// <seealso cref="JobObjectBasicLimitInformation"/>
        /// <seealso cref="JobObjectLimitFlags"/>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "SetInformationJobObject", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool SetInformationJobObject(
            SafeJobObjectHandle jobObjectHandle,
            JobObjectInfoClass jobObjectInfoClass,
            ref JobObjectExtendedLimitInformation info,
            int infoLength);

        /// <summary>
        /// Terminates the specified process and all of its threads.
        /// </summary>
        /// <param name="processHandle">A handle to the process to be terminated.</param>
        /// <param name="exitCode">The exit code to be used by the process and threads terminated as a result of this call.</param>
        /// <returns>True if successful; otherwise false.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "TerminateProcess", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool TerminateProcess(
            SafeProcessHandle processHandle,
            int exitCode);

        /// <summary>
        /// Updates the specified attribute in a list of attributes for process and thread creation.
        /// </summary>
        /// <param name="processThreadAttributionList">A pointer to an attribute list created by the <see cref="InitializeProcThreadAttributeList"/> function.</param>
        /// <param name="reserved1">This parameter is reserved and must be zero.</param>
        /// <param name="attribute">The attribute key to update in the attribute list.</param>
        /// <param name="attributeValue">
        /// <para>A pointer to the attribute value.</para>
        /// <para>This value should persist until the attribute is destroyed using the DeleteProcThreadAttributeList function.</para>
        /// </param>
        /// <param name="attributeSize">The size of the attribute value specified by the <paramref name="attribute"/> parameter.</param>
        /// <param name="reserved2"></param>
        /// <param name="reserved3"></param>
        /// <returns></returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "UpdateProcThreadAttribute", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UpdateProcThreadAttribute(
            IntPtr processThreadAttributionList,
            IntPtr reserved1, ProcessThreadAttribute
            attribute,
            IntPtr value,
            IntPtr size,
            IntPtr reserved2,
            IntPtr reserved3);

        /// <summary>
        /// Waits until the specified object is in the signaled state or the time-out interval elapses.
        /// </summary>
        /// <param name="handle">
        /// <para>A handle to the object.</para>
        /// <para>If this handle is closed while the wait is still pending, the function's behavior is undefined.</para>
        /// </param>
        /// <param name="milliseconds">
        /// <para>The time-out interval, in milliseconds.</para>
        /// <para>If a nonzero value is specified, the function waits until the object is signaled or the interval elapses.</para>
        /// <para>If dwMilliseconds is zero, the function does not enter a wait state if the object is not signaled; it always returns immediately.</para>
        /// </param>
        /// <returns>If the function succeeds, the return value indicates the event that caused the function to return.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "WaitForSingleObject", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern WaitReturnCode WaitForSingleObject(
            SafeHandle handle,
            int milliseconds);
    }
}
