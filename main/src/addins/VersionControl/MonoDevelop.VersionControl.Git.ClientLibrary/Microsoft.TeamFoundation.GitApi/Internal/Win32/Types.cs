//*************************************************************************************************
// Types.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.TeamFoundation.GitApi.Internal.Win32
{
    internal enum ConsoleCtrlEvent : uint
    {
        /// <summary>
        /// Generates a CTRL+C signal. This signal cannot be generated for process groups. If process group Id 
        /// is nonzero, this function will succeed, but the CTRL+C signal will not be received by processes 
        /// within the specified process group.
        /// </summary>
        CtrlC = 0,
        /// <summary>
        /// Generates a CTRL+BREAK signal.
        /// </summary>
        CtrlBreak = 1,
        /// <summary>
        /// Generates a Ctrl+Close signal (console closing)
        /// </summary>
        CtrlClose = 2,
    }

    internal enum DesiredAccess : uint
    {
        Unchanged = 0,
        /// <summary>
        /// Required to terminate a process using <see cref="Kernel32.TerminateProcess(SafeProcessHandle, int)"/>.
        /// </summary>
        Terminate = 0x00000001,
        /// <summary>
        /// Required to create a thread.
        /// </summary>
        CreateThread = 0x00000002,
        /// <summary>
        /// Required to perform an operation on the address space of a process.
        /// </summary>
        VmOperation = 0x00000008,
        /// <summary>
        /// Required to read memory in a process using ReadProcessMemory.
        /// </summary>
        VmRead = 0x00000010,
        /// <summary>
        /// Required to write to memory in a process using WriteProcessMemory.
        /// </summary>
        VmWrite = 0x00000020,
        /// <summary>
        /// Required to duplicate a handle using <see cref="Kernel32.DuplicateHandle"/>.
        /// </summary>
        DuplicateHandle = 0x00000040,
        /// <summary>
        /// Required to create a process
        /// </summary>
        CreateProcess = 0x00000080,
        /// <summary>
        /// Required to set memory limits using SetProcessWorkingSetSize.
        /// </summary>
        SetQuota = 0x00000100,
        /// <summary>
        /// Required to set certain information about a process, such as its priority class.
        /// </summary>
        SetInformation = 0x00000200,
        /// <summary>
        /// Required to retrieve certain information about a process, such as its token, exit code, and priority class.
        /// </summary>
        QueryInformation = 0x00000400,
        /// <summary>
        /// Required to suspend or resume a process.
        /// </summary>
        SuspendResume = 0x00000800,
        /// <summary>
        /// Required to retrieve certain information about a process.
        /// </summary>
        QueryLimitedInformation = 0x00001000,
        /// <summary>
        /// Required to wait for the process to terminate using the wait functions.
        /// </summary>
        Synchronize = 0x00100000,
        /// <summary>
        /// All possible access rights for a process object.
        /// </summary>
        AllAccess = Terminate
                  | CreateThread
                  | VmOperation
                  | VmRead
                  | VmWrite
                  | DuplicateHandle
                  | CreateProcess
                  | SetQuota
                  | SetInformation
                  | QueryInformation
                  | SuspendResume
                  | QueryLimitedInformation
                  | Synchronize,

    }

    [Flags]
    internal enum DuplicateHandleOptions : uint
    {
        None = 0,
        /// <summary>
        /// Closes the source handle. This occurs regardless of any error status returned.
        /// </summary>
        CloseSource = 0x00000001,
        /// <summary>
        /// Ignores the desiredAccess parameter. The duplicate handle has the same access as 
        /// the source handle.
        /// </summary>
        SameAccess = 0x00000002,
    }

    /// <summary>
    /// Description of error code values returned by <see cref="Marshal.GetLastWin32Error"/>.
    /// </summary>
    internal enum ErrorCode
    {
        /// <summary>
        /// The system cannot find the file specified.
        /// </summary>
        FileNotFound = 2,
        /// <summary>
        /// The system cannot find the path specified.
        /// </summary>
        PathNotFound = 3,
        /// <summary>
        /// Access is denied.
        /// </summary>
        AccessDenied = 5,
        /// <summary>
        /// The handle is invalid.
        /// </summary>
        InvalidHandle = 6,
        /// <summary>
        /// The program issued a command but the command length is incorrect.
        /// </summary>
        BadLength = 24,
        /// <summary>
        /// The parameter is incorrect.
        /// </summary>
        InvalidParameter = 87,
        /// <summary>
        /// The pipe has been ended.
        /// </summary>
        BrokenPipe = 109,
        /// <summary>
        /// The data area passed to a system call is too small.
        /// </summary>
        InsufficientBuffer = 122,
        /// <summary>
        /// Cannot create a file when that file already exists.
        /// </summary>
        AlreadyExists = 183,
        /// <summary>
        /// The pipe is local.
        /// </summary>
        PipeLocal = 229,
        /// <summary>
        /// The pipe state is invalid.
        /// </summary>
        BadPipe = 230,
        /// <summary>
        /// All pipe instances are busy.
        /// </summary>
        PipeBusy = 231,
        /// <summary>
        /// The pipe is being closed.
        /// </summary>
        NoData = 232,
        /// <summary>
        /// No process is on the other end of the pipe.
        /// </summary>
        PipeNotConnected = 233,
        /// <summary>
        /// More data is available.
        /// </summary>
        MoreData = 234,
        /// <summary>
        /// Only part of a ReadProcessMemory or WriteProcessMemory request was completed.
        /// </summary>
        PartialCopy = 299,
        /// <summary>
        /// The named pipe has already been connected to.
        /// </summary>
        PipeConnected = 535,
        /// <summary>
        /// Overlapped I/O event is not in a signaled state.
        /// </summary>
        IoIncomplete = 996,
        /// <summary>
        /// Overlapped I/O operation is in progress.
        /// </summary>
        IoPending = 997,
        /// <summary>
        /// Invalid flags.
        /// </summary>
        InvalidFlags = 1004,
        /// <summary>
        /// An attempt was made to reference a token that does not exist.
        /// </summary>
        NoToken = 1008,
        /// <summary>
        /// A required privilege is not held by the client.
        /// </summary>
        PrivilegeNotHeld = 1314,
        /// <summary>
        /// Not enough quota is available to process this command.
        /// </summary>
        NotEnoughQuota = 1816,
    }

    [Flags]
    internal enum HandleInformationFlags : uint
    {
        None = 0,
        /// <summary>
        /// If this flag is set, a child process created with the <see cref="SecurityAttributes.InheritHandle"/> parameter of 
        /// <see cref="Kernel32.CreateProcess"/> or <see cref="Kernel32.SetHandleInformation"/> set to <see langword="true"/> will 
        /// inherit the object handle.
        /// </summary>
        Inherit = 0x00000001,
        /// <summary>
        /// If this flag is set, calling the <see cref="Kernel32.CloseHandle"/> function will not close 
        /// the object handle.
        /// </summary>
        ProtectFromClose = 0x00000002,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IoCounters
    {
        /// <summary>
        /// The number of read operations performed.
        /// </summary>
        internal ulong ReadOperationCount;
        /// <summary>
        /// The number of write operations performed.
        /// </summary>
        internal ulong WriteOperationCount;
        /// <summary>
        /// The number of I/O operations performed, other than read and write operations.
        /// </summary>
        internal ulong OtherOperationCount;
        /// <summary>
        /// The number of bytes read.
        /// </summary>
        internal ulong ReadTransferCount;
        /// <summary>
        /// The number of bytes written.
        /// </summary>
        internal ulong WriteTransferCount;
        /// <summary>
        /// The number of bytes transferred during operations other than read and write operations.
        /// </summary>
        internal ulong OtherTransferCount;
    }

    /// <summary>
    /// Enumeration of potential class types to pass into <see cref="Kernel32.SetInformationJobObject"/>.
    /// </summary>
    internal enum JobObjectInfoClass : uint
    {
        None = 0,
        /// <summary>
        /// The value of jobObjectInfoClass parameter of <see cref="Kernel32.CreateJobObject"/> when 
        /// passing a pointer to a <see cref="BasicLimitInformation"/> structure to the info 
        /// parameter.
        /// </summary>
        BasicLimitInformation = 2,
        /// <summary>
        /// The value of jobObjectInfoClass parameter of <see cref="Kernel32.CreateJobObject"/> when 
        /// passing a pointer to a <see cref="ExtendedLimitInformation"/> structure to the info 
        /// parameter.
        /// </summary>
        ExtendedLimitInformation = 9,
    }

    /// <summary>
    /// Flags for <see cref="JobObjectBasicLimitInformation"/> which determine which structures
    /// are used.
    /// </summary>
    [Flags]
    internal enum JobObjectLimitFlags : uint
    {
        /// <summary>
        /// <para>Causes all processes associated with the job to use the same minimum and maximum 
        /// working set sizes. The 
        /// <see cref="JobObjectBasicLimitInformation.MinimumWorkingSetSize"/> and 
        /// <see cref="JobObjectBasicLimitInformation.MaximumWorkingSetSize"/> members contain 
        /// additional information.</para>
        /// 
        /// <para>If the job is nested, the effective working set size is the smallest working set 
        /// size in the job chain.</para>
        /// </summary>
        WorkingSet = 0x00000001,
        /// <summary>
        /// Establishes a user-mode execution time limit for each currently active process and for 
        /// all future processes associated with the job. The 
        /// <see cref="JobObjectBasicLimitInformation.PerProcessUserTimeLimit"/> member contains 
        /// additional information.
        /// </summary>
        ProcessTime = 0x00000002,
        /// <summary>
        /// Establishes a user-mode execution time limit for the job. The 
        /// <see cref="JobObjectBasicLimitInformation.PerJobUserTimeLimit"/> member contains 
        /// additional information. This flag cannot be used with 
        /// <see cref="PreserveJobTime"/>.
        /// </summary>
        JobTime = 0x00000004,
        /// <summary>
        /// Establishes a maximum number of simultaneously active processes associated with the 
        /// job. The ActiveProcessLimit member contains additional information.
        /// </summary>
        ActiveProcess = 0x00000008,
        /// <summary>
        /// <para>Causes all processes associated with the job to use the same processor 
        /// affinity. The Affinity member contains additional information.</para>
        /// 
        /// <para>If the job is nested, the specified processor affinity must be a subset of 
        /// the effective affinity of the parent job. If the specified affinity a superset of 
        /// the affinity of the parent job, it is ignored and the affinity of the parent job 
        /// is used.</para>
        /// </summary>
        Affinity = 0x00000010,
        /// <summary>
        /// <para>Causes all processes associated with the job to use the same priority class. For 
        /// more information, see Scheduling Priorities. The PriorityClass member contains 
        /// additional information.</para>
        /// 
        /// <para>If the job is nested, the effective priority class is the lowest priority class 
        /// in the job chain.</para>
        /// </summary>
        PriorityClass = 0x00000020,
        /// <summary>
        /// Preserves any job time limits you previously set. As long as this flag is set, you can 
        /// establish a per-job time limit once, then alter other limits in subsequent calls. 
        /// This flag cannot be used with <see cref="JobTime"/>.
        /// </summary>
        PreserveJobTime = 0x00000040,
        /// <summary>
        /// <para>Causes all processes in the job to use the same scheduling class. The 
        /// SchedulingClass member contains additional information.</para>
        /// 
        /// <para>If the job is nested, the effective scheduling class is the lowest scheduling 
        /// class in the job chain.</para>
        /// </summary>
        SchedulingClass = 0x00000080,
        /// <summary>
        /// <para>Causes all processes associated with the job to limit their committed memory. 
        /// When a process attempts to commit memory that would exceed the per-process limit, it 
        /// fails. If the job object is associated with a completion port, a 
        /// <see cref="JobObjectExtendedLimitInformation.ProcessMemoryLimit"/> message is sent to 
        /// the completion port.</para>
        /// 
        /// <para>If the job is nested, the effective memory limit is the most restrictive memory 
        /// limit in the job chain.</para>
        /// 
        /// <para>This limit requires use of a <see cref="JobObjectExtendedLimitInformation"/> 
        /// structure. Its <see cref="JobObjectExtendedLimitInformation.BasicLimitInformation"/> 
        /// member is a <see cref="JobObjectBasicLimitInformation"/> structure.</para>
        /// </summary>
        ProcessMemory = 0x00000100,
        /// <summary>
        /// <para>Causes all processes associated with the job to limit the job-wide sum of their 
        /// committed memory. When a process attempts to commit memory that would exceed the 
        /// job-wide limit, it fails. If the job object is associated with a completion port, a 
        /// <see cref="ProcessMemory"/> message is sent to the 
        /// completion port.</para>
        /// 
        /// <para>This limit requires use of a <see cref="JobObjectExtendedLimitInformation"/> 
        /// structure. Its <see cref="JobObjectExtendedLimitInformation.BasicLimitInformation"/> 
        /// member is a <see cref="JobObjectBasicLimitInformation"/> structure.</para>
        /// 
        /// <para>To register for notification when this limit is exceeded while allowing processes 
        /// to continue to commit memory, use the SetInformationJobObject function with the 
        /// JobObjectNotificationLimitInformation information class.</para>
        /// </summary>
        JobMemory = 0x00000200,
        /// <summary>
        /// <para>Forces a call to the SetErrorMode function with the SEM_NOGPFAULTERRORBOX flag 
        /// for 
        /// each process associated with the job.</para>
        /// 
        /// <para>If an exception occurs and the system calls the UnhandledExceptionFilter function, 
        /// the debugger will be given a chance to act. If there is no debugger, the functions 
        /// returns EXCEPTION_EXECUTE_HANDLER. Normally, this will cause termination of the process 
        /// with the exception code as the exit status.</para>
        /// 
        /// <para>This limit requires use of a <see cref="JobObjectExtendedLimitInformation"/> 
        /// structure. Its <see cref="JobObjectExtendedLimitInformation.BasicLimitInformation"/> 
        /// member is a <see cref="JobObjectBasicLimitInformation"/> structure.</para>
        /// </summary>
        DieOnUnhandledException = 0x00000400,
        /// <summary>
        /// <para>If any process associated with the job creates a child process using the 
        /// CREATE_BREAKAWAY_FROM_JOB flag while this limit is in effect, the child process is not 
        /// associated with the job.</para>
        /// 
        /// <para>This limit requires use of a <see cref="JobObjectExtendedLimitInformation"/> 
        /// structure. Its <see cref="JobObjectExtendedLimitInformation.BasicLimitInformation"/> 
        /// member is a <see cref="JobObjectBasicLimitInformation"/> structure.</para>
        /// </summary>
        BreakawayOk = 0x00000800,
        /// <summary>
        /// <para>Allows any process associated with the job to create child processes that are not 
        /// associated with the job.</para>
        /// 
        /// <para>If the job is nested and its immediate job object allows breakaway, the child 
        /// process breaks away from the immediate job object and from each job in the parent job 
        /// chain, moving up the hierarchy until it reaches a job that does not permit breakaway. 
        /// If the immediate job object does not allow breakaway, the child process does not break 
        /// away even if jobs in its parent job chain allow it.</para>
        /// 
        /// <para>This limit requires use of a <see cref="JobObjectExtendedLimitInformation"/> 
        /// structure. Its <see cref="JobObjectExtendedLimitInformation.BasicLimitInformation"/> 
        /// member is a <see cref="JobObjectBasicLimitInformation"/> structure.</para>
        /// </summary>
        SilentBreakawayOk = 0x00001000,
        /// <summary>
        /// <para>Causes all processes associated with the job to terminate when the last handle to 
        /// the job is closed.</para>
        /// 
        /// <para>This limit requires use of a <see cref="JobObjectExtendedLimitInformation"/> 
        /// structure. Its <see cref="JobObjectExtendedLimitInformation.BasicLimitInformation"/> 
        /// member is a <see cref="JobObjectBasicLimitInformation"/> structure.</para>
        /// </summary>
        KillOnJobClose = 0x00002000,
        /// <summary>
        /// Allows processes to use a subset of the processor affinity for all processes associated 
        /// with the job. This value must be combined with <see cref="Affinity"/>.
        /// </summary>
        /// <remarks>
        /// Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This flag is 
        /// supported starting with Windows 7 and Windows Server 2008 R2.
        /// </remarks>
        SubsetAffinity = 0x00004000,
    }

    /// <summary>
    /// Contains basic limit information for a job object.
    /// </summary>
    /// <remarks>
    /// Processes can still empty their working sets using the SetProcessWorkingSetSize function 
    /// with (SIZE_T)-1, even when <see cref="JobObjectLimitFlags.WorkingSet"/> is 
    /// used. However, you cannot use SetProcessWorkingSetSize change the minimum or maximum 
    /// working set size of a process in a job object.
    /// 
    /// The system increments the active process count when you attempt to associate a process with 
    /// a job. If the limit is exceeded, the system decrements the active process count only when 
    /// the process terminates and all handles to the process are closed. Therefore, if you have an 
    /// open handle to a process that has been terminated in such a manner, you cannot associate 
    /// any new processes until the handle is closed and the active process count is below the 
    /// limit.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct JobObjectBasicLimitInformation
    {
        /// <summary>
        /// <para>If <see cref="JobObjectLimitFlags"/> specifies 
        /// <see cref="JobObjectLimitFlags.ProcessTime"/>, this member is the 
        /// per-process user-mode execution time limit, in 100-nanosecond ticks. Otherwise, 
        /// this member is ignored.</para>
        /// 
        /// <para>The system periodically checks to determine whether each process associated with 
        /// the job has accumulated more user-mode time than the set limit.If it has, the process 
        /// is terminated.</para>
        /// 
        /// <para>If the job is nested, the effective limit is the most restrictive limit in the 
        /// job chain.</para>
        /// </summary>
        internal ulong PerProcessUserTimeLimit;
        /// <summary>
        /// <para>If <see cref="JobObjectLimitFlags"/> specifies 
        /// <see cref="JobObjectLimitFlags.JobTime"/>, this member is the per-job 
        /// user-mode execution time limit, in 100-nanosecond ticks. Otherwise, this member is 
        /// ignored.</para>
        /// 
        /// <para>The system adds the current time of the processes associated with the job to this 
        /// limit.For example, if you set this limit to 1 minute, and the job has a process that 
        /// has accumulated 5 minutes of user-mode time, the limit actually enforced is 6 
        /// minutes.</para>
        /// 
        /// <para>The system periodically checks to determine whether the sum of the user-mode 
        /// execution time for all processes is greater than this end-of-job limit.If it is, the 
        /// action specified in the EndOfJobTimeAction member of the 
        /// JOBOBJECT_END_OF_JOB_TIME_INFORMATION structure is carried out. By default, all 
        /// processes are terminated and the status code is set to 
        /// <see cref="ErrorCode.NotEnoughQuota"/>.</para>
        /// 
        /// <para>To register for notification when this limit is exceeded without terminating 
        /// processes, use the SetInformationJobObject function with the 
        /// JobObjectNotificationLimitInformation information class.</para>
        /// </summary>
        internal ulong PerJobUserTimeLimit;
        /// <summary>
        /// The limit flags that are in effect. This member is a bitfield that determines whether 
        /// other structure members are used.
        /// </summary>
        internal JobObjectLimitFlags LimitFlags;
        /// <summary>
        /// <para>If <see cref="JobObjectLimitFlags"/> specifies 
        /// <see cref="JobObjectLimitFlags.WorkingSet"/>, this member is the minimum 
        /// working set size for each process associated with the job. Otherwise, this member is 
        /// ignored.</para>
        /// 
        /// <para>If <see cref="MaximumWorkingSetSize"/> is nonzero, 
        /// <see cref="MinimumWorkingSetSize"/> cannot be zero.</para>
        /// </summary>
        internal UIntPtr MinimumWorkingSetSize;
        /// <summary>
        /// <para>If <see cref="JobObjectLimitFlags"/> specifies 
        /// <see cref="JobObjectLimitFlags.WorkingSet"/>, this member is the maximum 
        /// working set size for each process associated with the job. Otherwise, this member is 
        /// ignored.</para>
        /// 
        /// <para>If <see cref="MinimumWorkingSetSize"/> is nonzero, 
        /// <see cref="MaximumWorkingSetSize"/> cannot be zero.</para>
        /// </summary>
        internal UIntPtr MaximumWorkingSetSize;
        /// <summary>
        /// <para>If <see cref="JobObjectLimitFlags"/> specifies 
        /// <see cref="JobObjectLimitFlags.ActiveProcess"/>, this member is the 
        /// active process limit for the job. Otherwise, this member is ignored.</para>
        /// 
        /// <para>If you try to associate a process with a job, and this causes the active process 
        /// count to exceed this limit, the process is terminated and the association fails.</para>
        /// </summary>
        internal uint ActiveProcessLimit;
        /// <summary>
        /// <para>If <see cref="JobObjectLimitFlags"/> specifies 
        /// <see cref="JobObjectLimitFlags.Affinity"/>, this member is the processor 
        /// affinity for all processes associated with the job. Otherwise, this member is ignored.
        /// </para>
        /// 
        /// <para>The affinity must be a subset of the system affinity mask obtained by calling the 
        /// GetProcessAffinityMask function. The affinity of each thread is set to this value, but 
        /// threads are free to subsequently set their affinity, as long as it is a subset of the 
        /// specified affinity mask. Processes cannot set their own affinity mask.</para>
        /// </summary>
        internal IntPtr Affinity;
        /// <summary>
        /// <para>If <see cref="JobObjectLimitFlags"/> specifies 
        /// <see cref="JobObjectLimitFlags.PriorityClass"/>, this member is the 
        /// priority class for all processes associated with the job. Otherwise, this member is 
        /// ignored.</para>
        /// 
        /// <para>Processes and threads cannot modify their priority class. The calling process 
        /// must enable the SE_INC_BASE_PRIORITY_NAME privilege.</para>
        /// </summary>
        internal uint PriorityClass;
        /// <summary>
        /// <para>If <see cref="JobObjectLimitFlags"/> specifies 
        /// <see cref="JobObjectLimitFlags.SchedulingClass"/>, this member is the 
        /// scheduling class for all processes associated with the job. Otherwise, this member is 
        /// ignored.</para>
        /// 
        /// <para>The valid values are 0 to 9. Use 0 for the least favorable scheduling class 
        /// relative to other threads, and 9 for the most favorable scheduling class relative to 
        /// other threads. By default, this value is 5. To use a scheduling class greater than 5, 
        /// the calling process must enable the SE_INC_BASE_PRIORITY_NAME privilege.</para>
        /// </summary>
        internal uint SchedulingClass;
    }

    /// <summary>
    /// Contains basic and extended limit information for a job object.
    /// </summary>
    /// <remarks>
    /// The system tracks the value of <see cref="PeakProcessMemoryUsed"/> and 
    /// <see cref="PeakJobMemoryUsed"/> constantly. This allows you know the peak memory usage of 
    /// each job. You can use this information to establish a memory limit using the 
    /// <see cref="JobObjectLimitFlags.ProcessMemory"/> or 
    /// <see cref="JobObjectLimitFlags.JobMemory"/> value.
    /// 
    /// Note that the job memory and process memory limits are very similar in operation, but they 
    /// are independent. You could set a job-wide limit of 100 MB with a per-process limit of 10 MB. 
    /// In this scenario, no single process could commit more than 10 MB, and the set of processes 
    /// associated with a job could never exceed 100 MB.
    /// 
    /// To register for notifications that a job has exceeded its peak memory limit while allowing 
    /// processes to continue to commit memory, use the SetInformationJobObject function with the 
    /// JobObjectNotificationLimitInformation information class.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct JobObjectExtendedLimitInformation
    {
        /// <summary>
        /// A <see cref="JobObjectBasicLimitInformation"/> structure that contains basic limit 
        /// information.
        /// </summary>
        internal JobObjectBasicLimitInformation BasicLimitInformation;
        /// <summary>
        /// Reserved
        /// </summary>
        internal IoCounters IoInfo;
        /// <summary>
        /// If the <see cref="JobObjectLimitFlags"/> member of the 
        /// <see cref="JobObjectBasicLimitInformation"/> structure specifies the 
        /// <see cref="JobObjectLimitFlags.ProcessMemory"/> value, this member 
        /// specifies the limit for the virtual memory that can be committed by a process. 
        /// Otherwise, this member is ignored.
        /// </summary>
        internal UIntPtr ProcessMemoryLimit;
        /// <summary>
        /// If the <see cref="JobObjectLimitFlags"/> member of the 
        /// <see cref="JobObjectBasicLimitInformation"/> structure specifies the 
        /// <see cref="JobObjectLimitFlags.JobMemory"/> value, this member 
        /// specifies the limit for the virtual memory that can be committed by a process. 
        /// Otherwise, this member is ignored.
        /// </summary>
        internal UIntPtr JobMemoryLimit;
        /// <summary>
        /// The peak memory used by any process ever associated with the job.
        /// </summary>
        internal UIntPtr PeakProcessMemoryUsed;
        /// <summary>
        /// The peak memory usage of all processes currently associated with the job.
        /// </summary>
        internal UIntPtr PeakJobMemoryUsed;
    }

    [Flags]
    internal enum PipeMode : uint
    {
        /// <summary>
        /// <para>Data is written to the pipe as a stream of bytes.</para>
        /// <para>This mode cannot be used with <see cref="ReadMessage"/> .</para>
        /// <para>The pipe does not distinguish bytes written during different write operations.</para>
        /// </summary>
        WriteBinary = 0x00000000,
        /// <summary>
        /// <para>Data is written to the pipe as a stream of messages.</para>
        /// <para>The pipe treats the bytes written during each write operation as a message unit.</para> 
        /// <para>The GetLastError function returns <see cref="ErrorCode.MoreData"/> when a message is not read completely.</para>
        /// <para>This mode can be used with either <see cref="ReadMessage"/> or <see cref="ReadBinary"/> .</para>
        /// </summary>
        WriteMessage = 0x00000004,
        /// <summary>
        /// <para>Data is read from the pipe as a stream of bytes.</para>
        /// <para>This mode can be used with either <see cref="WriteMessage"/> or <see cref="WriteBinary"/>.</para>
        /// </summary>
        ReadBinary = 0x00000000,
        /// <summary>
        /// <para>Data is read from the pipe as a stream of messages.</para>
        /// <para>This mode can be only used if <see cref="WriteMessage"/> is also specified.</para>
        /// </summary>
        ReadMessage = 0x00000002,
        /// <summary>
        /// <para>Blocking mode is enabled.</para> 
        /// <para>When the pipe handle is specified in the `ReadFile`, `WriteFile`, or `ConnectNamedPipe` function, the operations are not completed until there is data to read, all data is written, or a client is connected.</para>
        /// <para>Use of this mode can mean waiting indefinitely in some situations for a client process to perform an action.</para>
        /// </summary>
        Wait = 0x00000000,
        /// <summary>
        /// <para>Nonblocking mode is enabled.</para>
        /// <para>In this mode `ReadFile`, `WriteFile`, and `ConnectNamedPipe` always return immediately.</para>
        /// </summary>
        NoWait = 0x00000001,
        /// <summary>
        /// Connections from remote clients can be accepted and checked against the security descriptor for the pipe.
        /// </summary>
        AcceptRemoteClients = 0x00000000,
        /// <summary>
        /// Connections from remote clients are automatically rejected.
        /// </summary>
        RejectRemoteClients = 0x00000008,
    }

    [Flags]
    internal enum PipeType : uint
    {
        /// <summary>
        /// <para>The pipe is bi-directional; both server and client processes can read from and write to the pipe.</para> 
        /// <para>This mode gives the server the equivalent of <see cref="System.IO.FileAccess.Read"/> and <see cref="System.IO.FileAccess.Write"/> access to the pipe.</para>
        /// <para>The client can specify <see cref="System.IO.FileAccess.Read"/> or <see cref="System.IO.FileAccess.Write"/>, or both, when it connects to the pipe using the `CreateFile` function.</para>
        /// </summary>
        Duplex = 0x00000003,
        /// <summary>
        /// If you attempt to create multiple instances of a pipe with this flag, creation of the 
        /// first instance succeeds, but creation of the next instance fails with 
        /// <see cref="ErrorCode.AccessDenied"/>.
        /// </summary>
        FirstPipeInstance = 0x00080000,
        /// <summary>
        /// <para>The flow of data in the pipe goes from client to server only.</para> 
        /// <para>This mode gives the server the equivalent of <see cref="System.IO.FileAccess.Read"/> access to the pipe.</para>
        /// <para>The client must specify <see cref="System.IO.FileAccess.Write"/> access when connecting to the pipe.</para>
        /// <para>If the client must read pipe settings by calling the `GetNamedPipeInfo` or `GetNamedPipeHandleState` functions, the client must specify <see cref="System.IO.FileAccess.Write"/> and <see cref="System.IO.FileShare.Read"/> access when connecting to the pipe.</para>
        /// </summary>
        Inbound = 0x00000001,
        /// <summary>
        /// <para>The flow of data in the pipe goes from server to client only.</para>
        /// <para>This mode gives the server the equivalent of <see cref="System.IO.FileAccess.Write"/> access to the pipe.</para>
        /// <para>The client must specify <see cref="System.IO.FileAccess.Read"/> access when connecting to the pipe.</para>
        /// <para>If the client must change pipe settings by calling the `SetNamedPipeHandleState` function, the client must specify <see cref="System.IO.FileAccess.Read"/> and <see cref="System.IO.FileShare.Write"/> access when connecting to the pipe.</para>
        /// </summary>
        Outbound = 0x00000002,
        /// <summary>
        /// <para>Overlapped mode is enabled.</para>
        /// <para>If this mode is enabled, functions performing read, write, and connect operations that may take a significant time to be completed can return immediately.</para>
        /// <para>This mode enables the thread that started the operation to perform other operations while the time-consuming operation executes in the background.</para>
        /// <para>For example, in overlapped mode, a thread can handle simultaneous input and output (I/O) operations on multiple instances of a pipe or perform simultaneous read and write operations on the same pipe handle.</para>
        /// <para>If overlapped mode is not enabled, functions performing read, write, and connect operations on the pipe handle do not return until the operation is finished.</para>
        /// <para>The `ReadFileEx` and `WriteFileEx` functions can only be used with a pipe handle in overlapped mode.</para>
        /// <para>The `ReadFile`, `WriteFile`, `ConnectNamedPipe`, and `TransactNamedPipe` functions can execute either synchronously or as overlapped operations.</para>
        /// </summary>
        Overlapped = 0x40000000,
        /// <summary>
        /// <para>Write-through mode is enabled.</para>
        /// <para>This mode affects only write operations on byte-type pipes and, then, only when the client and server processes are on different computers.</para>
        /// <para>If this mode is enabled, functions writing to a named pipe do not return until the data written is transmitted across the network and is in the pipe's buffer on the remote computer.</para>
        /// <para>If this mode is not enabled, the system enhances the efficiency of network operations by buffering data until a minimum number of bytes accumulate or until a maximum time elapses.</para>
        /// </summary>
        WriteThrough = 0x80000000,
    }

    [Flags]
    internal enum ProcessCreationFlags : uint
    {
        None = 0,
        /// <summary>
        /// <para>The calling thread starts and debugs the new process and all child processes 
        /// created by the new process. It can receive all related debug events using the 
        /// WaitForDebugEvent function.</para>
        /// <para>A process that uses DEBUG_PROCESS becomes the root of a debugging chain. This 
        /// continues until another process in the chain is created with DEBUG_PROCESS.</para>
        /// <para>If this flag is combined with DEBUG_ONLY_THIS_PROCESS, the caller debugs only 
        /// the new process, not any child processes.</para>
        /// </summary>
        DebugProcess = 0x00000001,
        /// <summary>
        /// The calling thread starts and debugs the new process. It can receive all related 
        /// debug events using the WaitForDebugEvent function.
        /// </summary>
        DebugOnlyThisProcess = 0x00000002,
        /// <summary>
        /// The primary thread of the new process is created in a suspended state, and does not 
        /// run until the <see cref="Kernel32.ResumeThread"/> function is called.
        /// </summary>
        Suspended = 0x00000004,
        /// <summary>
        /// <para>For console processes, the new process does not inherit its parent's console 
        /// (the default). The new process can call the <see cref="Kernel32.AllocConsole"/> function at 
        /// a later time to create a console.</para>
        /// <para>This value cannot be used with <see cref="NewConsole"/>.</para>
        /// </summary>
        DetachProcess = 0x00000008,
        /// <summary>
        /// <para>The new process has a new console, instead of inheriting its parent's console 
        /// (the default).</para>
        /// 
        /// <para>This flag cannot be used with <see cref="DetachProcess"/>.</para>
        /// </summary>
        NewConsole = 0x00000010,
        /// <summary>
        /// <para>The new process is the root process of a new process group. The process group 
        /// includes all processes that are descendants of this root process. The process 
        /// identifier of the new process group is the same as the process identifier, which is 
        /// returned in the processInformation parameter. Process groups are used by the 
        /// <see cref="Kernel32.GenerateConsoleCtrlEvent"/> function to enable sending a <see cref="ConsoleCtrlEvent.CtrlBreak"/> 
        /// signal to a group of console processes.</para>
        /// <para>If this flag is specified, <see cref="ConsoleCtrlEvent.CtrlC"/> signals will be 
        /// disabled for all processes within the new process group.</para>
        /// <para>This flag is ignored if specified with <see cref="NewConsole"/>.</para>
        /// </summary>
        NewProcessGroup = 0x00000200,
        /// <summary>
        /// If this flag is set, the environment block pointed to by environment uses Unicode 
        /// characters. Otherwise, the environment block uses ANSI characters.
        /// </summary>
        UnicodeEnvironment = 0x00000400,
        /// <summary>
        /// This flag is valid only when starting a 16-bit Windows-based application. If set, 
        /// the new process runs in a private Virtual DOS Machine (VDM). By default, all 16-bit 
        /// Windows-based applications run as threads in a single, shared VDM. The advantage of 
        /// running separately is that a crash only terminates the single VDM; any other 
        /// programs running in distinct VDMs continue to function normally. Also, 16-bit 
        /// Windows-based applications that are run in separate VDMs have separate input queues. 
        /// That means that if one application stops responding momentarily, applications in 
        /// separate VDMs continue to receive input. The disadvantage of running separately is 
        /// that it takes significantly more memory to do so. You should use this flag only if 
        /// the user requests that 16-bit applications should run in their own VDM.
        /// </summary>
        SeparateWowVdm = 0x00000800,
        /// <summary>
        /// The flag is valid only when starting a 16-bit Windows-based application. If the 
        /// DefaultSeparateVDM switch in the Windows section of WIN.INI is TRUE, this flag 
        /// overrides the switch. The new process is run in the shared Virtual DOS Machine.
        /// </summary>
        SharedWowVdm = 0x00001000,
        /// <summary>
        /// The process inherits its parent's affinity. If the parent process has threads in 
        /// more than one processor group, the new process inherits the group-relative affinity 
        /// of an arbitrary group in use by the parent.
        /// </summary>
        InheritParentAffinity = 0x00010000,
        /// <summary>
        /// <para>The process is to be run as a protected process. The system restricts access 
        /// to protected processes and the threads of protected processes.</para>
        /// <para>To activate a protected process, the binary must have a special signature. 
        /// This signature is provided by Microsoft but not currently available for 
        /// non-Microsoft binaries. There are currently four protected processes: media 
        /// foundation, audio engine, Windows error reporting, and system. Components that load 
        /// into these binaries must also be signed. Multimedia companies can leverage the 
        /// first two protected processes.</para>
        /// </summary>
        /// <remarks>Windows Server 2003 and Windows XP:  This value is not supported.</remarks>
        ProtectedProcess = 0x00040000,
        /// <summary>
        /// The process is created with extended startup information; the startupInfo parameter 
        /// specifies a STARTUPINFOEX structure.
        /// </summary>
        /// <remarks>Windows Server 2003 and Windows XP:  This value is not supported.</remarks>
        ExtendedStartupInfoPresent = 0x00080000,
        /// <summary>
        /// <para>The child processes of a process associated with a job are not associated 
        /// with the job.</para>
        /// <para>If the calling process is not associated with a job, this constant has no 
        /// effect. If the calling process is associated with a job, the job must set the 
        /// <see cref="JobObjectLimitFlags.BreakawayOk"/> limit</para>
        /// </summary>
        BreakAwayFromJob = 0x01000000,
        /// <summary>
        /// Allows the caller to execute a child process that bypasses the process restrictions 
        /// that would normally be applied automatically to the process.
        /// </summary>
        PreserveCodeAuthzLevel = 0x02000000,
        /// <summary>
        /// <para>The new process does not inherit the error mode of the calling process. 
        /// Instead, the new process gets the default error mode.</para>
        /// <para>This feature is particularly useful for multithreaded shell applications that 
        /// run with hard errors disabled.</para>
        /// <para>The default behavior is for the new process to inherit the error mode of the 
        /// caller. Setting this flag changes that default behavior.</para>
        /// </summary>
        DefaultErrorMode = 0x04000000,
        /// <summary>
        /// <para>The process is a console application that is being run without a console 
        /// window. Therefore, the console handle for the application is not set.</para>
        /// <para>This flag is ignored if the application is not a console application, or if 
        /// it is used with either <see cref="NewConsole"/> or <see cref="DetachProcess"/>.</para>
        /// </summary>
        NoWindow = 0x08000000,
    }

    /// <summary>
    /// Describes an entry from a list of the processes residing in the system address space when a snapshot was taken.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct ProcessEntry32
    {
        /// <summary>
        /// The size of the structure, in bytes.
        /// </summary>
        internal int Size;
        /// <summary>
        /// This member is no longer used and is always set to zero.
        /// </summary>
        internal uint Usage;
        /// <summary>
        /// The process identifier.
        /// </summary>
        internal uint ProcessId;
        /// <summary>
        /// This member is no longer used and is always set to zero.
        /// </summary>
        internal UIntPtr DefaultHeapId;
        /// <summary>
        /// This member is no longer used and is always set to zero.
        /// </summary>
        internal uint ModuleId;
        /// <summary>
        /// The number of execution threads started by the process.
        /// </summary>
        internal uint Threads;
        /// <summary>
        /// The identifier of the process that created this process (its parent process).
        /// </summary>
        internal uint ParentProcessId;
        /// <summary>
        /// The base priority of any threads created by this process.
        /// </summary>
        internal int PriClassBase;
        /// <summary>
        /// This member is no longer used, and is always set to zero.
        /// </summary>
        internal uint Flags;
        /// <summary>
        /// The name of the executable file for the process.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        internal string ExeFileName;
    }

    /// <summary>
    /// Contains information about a newly created process and its primary thread. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ProcessInformation
    {
        /// <summary>
        /// A handle to the newly created process. The handle is used to specify the process in 
        /// all functions that perform operations on the process object.
        /// </summary>
        internal IntPtr ProcessHandle;
        /// <summary>
        /// A handle to the primary thread of the newly created process. The handle is used to 
        /// specify the thread in all functions that perform operations on the thread object.
        /// </summary>
        internal IntPtr ThreadHandle;
        /// <summary>
        /// A value that can be used to identify a process. The value is valid from the time 
        /// the process is created until all handles to the process are closed and the process 
        /// object is freed; at this point, the identifier may be reused.
        /// </summary>
        internal uint ProcessId;
        /// <summary>
        /// A value that can be used to identify a thread. The value is valid from the time the 
        /// thread is created until all handles to the thread are closed and the thread object 
        /// is freed; at this point, the identifier may be reused.
        /// </summary>
        internal uint ThreadId;
    }

    [Flags]
    internal enum ProcessStartupFlags : uint
    {
        None = 0,
        /// <summary>
        /// The <see cref="ProcessStartupInfo.ShowWindow"/> member contains additional 
        /// information.
        /// </summary>
        UseShowWindow = 0x00000001,
        /// <summary>
        /// The <see cref="ProcessStartupInfo.SizeX"/> and <see cref="ProcessStartupInfo.SizeY"/> 
        /// members contain additional information.
        /// </summary>
        UseSize = 0x00000002,
        /// <summary>
        /// The <see cref="ProcessStartupInfo.PositionX"/> and 
        /// <see cref="ProcessStartupInfo.PositionY"/> members contain additional information.
        /// </summary>
        UsePosition = 0x00000004,
        /// <summary>
        /// The <see cref="ProcessStartupInfo.CountCharsX"/> and 
        /// <see cref="ProcessStartupInfo.CountCharsY"/> members contain additional information.
        /// </summary>
        UseCountChars = 0x00000008,
        /// <summary>
        /// The <see cref="ProcessStartupInfo.FillAttribute"/> member contains additional 
        /// information.
        /// </summary>
        UseFillAttribute = 0x00000010,
        /// <summary>
        /// Indicates that the process should be run in full-screen mode, rather than in 
        /// windowed mode.
        /// </summary>
        RunFullScreen = 0x00000020,
        /// <summary>
        /// <para>Indicates that the cursor is in feedback mode for two seconds after 
        /// <see cref="Advapi32.CreateProcessAsUser"/> is called. The Working in Background cursor is 
        /// displayed (see the Pointers tab in the Mouse control panel utility).</para>
        /// 
        /// <para>If during those two seconds the process makes the first GUI call, the system 
        /// gives five more seconds to the process. If during those five seconds the process 
        /// shows a window, the system gives five more seconds to the process to finish drawing 
        /// the window.</para>
        /// </summary>
        ForceOnFeedback = 0x00000040,
        /// <summary>
        /// Indicates that the feedback cursor is forced off while the process is starting. The 
        /// Normal Select cursor is displayed.
        /// </summary>
        ForceOffFeedback = 0x00000080,
        /// <summary>
        /// <para>The <see cref="ProcessStartupInfo.StandardErrorHandle"/>, 
        /// <see cref="ProcessStartupInfo.StandardInputHandle"/>, and 
        /// <see cref="ProcessStartupInfo.StandardOutputHandle"/> members contain additional information.</para>
        /// 
        /// <para>If this flag is specified when calling one of the process creation functions, 
        /// the handles must be inheritable and the function's inheritHandles parameter must be 
        /// set to TRUE.</para>
        /// 
        /// <para>If this flag is specified when calling the GetStartupInfo function, these 
        /// members are either the handle value specified during process creation or 
        /// <see cref="Kernel32.InvalidHandleValue"/>.</para>
        /// 
        /// <para>Handles must be closed with <see cref="Kernel32.CloseHandle"/> when they are no longer 
        /// needed.</para>
        /// 
        /// <para>This flag cannot be used with <see cref="UseHotKey"/>.</para>
        /// </summary>
        UseStandardHandles = 0x00000100,
        /// <summary>
        /// <para>The <see cref="ProcessStartupInfo.StandardInputHandle"/> member contains additional 
        /// information.</para>
        /// </summary>
        UseHotKey = 0x00000200,
        /// <summary>
        /// <para>The <see cref="ProcessStartupInfo.Title"/> member contains the path of the 
        /// shortcut file (.lnk) that the user invoked to start this process. This is typically 
        /// set by the shell when a .lnk file pointing to the launched application is invoked. 
        /// Most applications will not need to set this value.</para>
        /// </summary>
        TitleIsLinkName = 0x00000800,
        /// <summary>
        /// <para>The <see cref="ProcessStartupInfo.Title"/> member contains an AppUserModelID. 
        /// This identifier controls how the taskbar and Start menu present the application, 
        /// and enables it to be associated with the correct shortcuts and Jump Lists. 
        /// Generally, applications will use the SetCurrentProcessExplicitAppUserModelID and 
        /// GetCurrentProcessExplicitAppUserModelID functions instead of setting this flag.</para>
        /// 
        /// <para>If <see cref="PreventPinning"/> is used, application windows cannot be pinned 
        /// on the taskbar. The use of any AppUserModelID-related window properties by the 
        /// application overrides this setting for that window only.</para>
        /// 
        /// <para>This flag cannot be used with <see cref="TitleIsLinkName"/>.</para>
        /// </summary>
        TitleIsAppId = 0x00001000,
        /// <summary>
        /// <para>Indicates that any windows created by the process cannot be pinned on the 
        /// taskbar.</para>
        /// 
        /// <para>This flag must be combined with <see cref="TitleIsAppId"/>.</para>
        /// </summary>
        PreventPinning = 0x00002000,
    }

    /// <summary>
    /// Specifies the window station, desktop, standard handles, and appearance of the main 
    /// window for a process at creation time.
    /// </summary>
    /// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/ms686331(v=vs.85).aspx</remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct ProcessStartupInfo
    {
        /// <summary>
        /// The size of the structure, in bytes.
        /// </summary>
        internal uint Cb;
        /// <summary>
        /// Reserved; must be NULL.
        /// </summary>
        internal string Reserved;
        /// <summary>
        /// The name of the desktop, or the name of both the desktop and window station for 
        /// this process. A backslash in the string indicates that the string includes both the 
        /// desktop and window station names.
        /// </summary>
        internal string Desktop;
        /// <summary>
        /// For console processes, this is the title displayed in the title bar if a new 
        /// console window is created. If NULL, the name of the executable file is used as the 
        /// window title instead. This parameter must be NULL for GUI or console processes that 
        /// do not create a new console window.
        /// </summary>
        internal string Title;
        /// <summary>
        /// <para>If <see cref="Flags"/> specifies <see cref="ProcessStartupFlags.UsePosition"/>, 
        /// this member is the x offset of the upper left corner of a window if a new window is 
        /// created, in pixels. Otherwise, this member is ignored.</para>
        /// 
        /// <para>The offset is from the upper left corner of the screen. For GUI processes, 
        /// the specified position is used the first time the new process calls CreateWindow to 
        /// create an overlapped window if the x parameter of CreateWindow is CW_USEDEFAULT.</para>
        /// </summary>
        internal uint PositionX;
        /// <summary>
        /// <para>If <see cref="Flags"/> specifies <see cref="ProcessStartupFlags.UsePosition"/>, 
        /// this member is the y offset of the upper left corner of a window if a new window is 
        /// created, in pixels. Otherwise, this member is ignored.</para>
        /// 
        /// <para>The offset is from the upper left corner of the screen. For GUI processes, 
        /// the specified position is used the first time the new process calls CreateWindow to 
        /// create an overlapped window if the y parameter of CreateWindow is CW_USEDEFAULT.</para>
        /// </summary>
        internal uint PositionY;
        /// <summary>
        /// <para>If <see cref="Flags"/> specifies <see cref="ProcessStartupFlags.UseSize"/>, 
        /// this member is the width of the window if a new window is created, in pixels. 
        /// Otherwise, this member is ignored.</para>
        /// 
        /// <para>For GUI processes, this is used only the first time the new process calls 
        /// CreateWindow to create an overlapped window if the width parameter of CreateWindow 
        /// is CW_USEDEFAULT.</para>
        /// </summary>
        internal uint SizeX;
        /// <summary>
        /// <para>If <see cref="Flags"/> specifies <see cref="ProcessStartupFlags.UseSize"/>, 
        /// this member is the height of the window if a new window is created, in pixels. 
        /// Otherwise, this member is ignored.</para>
        /// 
        /// <para>For GUI processes, this is used only the first time the new process calls 
        /// CreateWindow to create an overlapped window if the height parameter of CreateWindow 
        /// is CW_USEDEFAULT.</para>
        /// </summary>
        internal uint SizeY;
        /// <summary>
        /// If <see cref="Flags"/> specifies <see cref="ProcessStartupFlags.UseCountChars"/>, 
        /// if a new console window is created in a console process, this member specifies the 
        /// screen buffer width, in character columns. Otherwise, this member is ignored.
        /// </summary>
        internal uint CountCharsX;
        /// <summary>
        /// If <see cref="Flags"/> specifies <see cref="ProcessStartupFlags.UseCountChars"/>, 
        /// if a new console window is created in a console process, this member specifies the 
        /// screen buffer height, in character columns. Otherwise, this member is ignored.
        /// </summary>
        internal uint CountCharsY;
        /// <summary>
        /// If <see cref="Flags"/> specifies <see cref="ProcessStartupFlags.UseFillAttribute"/>, 
        /// this member is the initial text and background colors if a new console window is 
        /// created in a console application. Otherwise, this member is ignored.
        /// </summary>
        internal uint FillAttribute;
        /// <summary>
        /// A bitfield that determines whether certain <see cref="ProcessStartupInfo"/> members 
        /// are used when the process creates a window.
        /// </summary>
        internal ProcessStartupFlags Flags;
        /// <summary>
        /// If <see cref="Flags"/> specifies <see cref="ProcessStartupFlags.UseShowWindow"/>, 
        /// this member can be any of the values that can be specified by 
        /// <see cref="ShowWindowCommand"/> except for <see cref="ShowWindowCommand.Default"/>.
        /// Otherwise, this member is ignored.
        /// </summary>
        internal ushort ShowWindow;
        /// <summary>
        /// Reserved for use by the C Run-time; must be zero.
        /// </summary>
        internal ushort Reserved2;
        /// <summary>
        /// Reserved for use by the C Run-time; must be <see cref="IntPtr.Zero"/>.
        /// </summary>
        internal IntPtr Reserved3;
        /// <summary>
        /// <para>If <see cref="Flags"/> specifies <see cref="ProcessStartupFlags.UseStandardHandles"/>, 
        /// this member is the standard input handle for the process. If 
        /// <see cref="ProcessStartupFlags.UseStandardHandles"/> is not specified, the default for 
        /// standard input is the keyboard buffer.</para>
        /// 
        /// <para>If <see cref="Flags"/> specifies <see cref="ProcessStartupFlags.UseHotKey"/>, 
        /// this member specifies a hotkey value that is sent as the param parameter of a 
        /// WM_SETHOTKEY message to the first eligible top-level window created by the 
        /// application that owns the process. If the window is created with the WS_POPUP 
        /// window style, it is not eligible unless the WS_EX_APPWINDOW extended window style 
        /// is also set.</para>
        /// 
        /// <para>Otherwise, this member is ignored.</para>
        /// </summary>
        internal SafeFileHandle StandardInputHandle;
        /// <summary>
        /// <para>If <see cref="Flags"/> specifies <see cref="ProcessStartupFlags.UseStandardHandles"/>, 
        /// this member is the standard output handle for the process. Otherwise, this member 
        /// is ignored and the default for standard output is the console window's buffer.</para>
        /// 
        /// <para>If a process is launched from the taskbar or jump list, the system sets 
        /// <see cref="StandardOutputHandle"/> to a handle to the monitor that contains the taskbar or 
        /// jump list used to launch the process.</para>
        /// </summary>
        /// <remarks>
        /// Windows 7, Windows Server 2008 R2, Windows Vista, Windows Server 2008, Windows XP, 
        /// and Windows Server 2003:  This behavior was introduced in Windows 8 and Windows 
        /// Server 2012.
        /// </remarks>
        internal SafeFileHandle StandardOutputHandle;
        /// <summary>
        /// If <see cref="Flags"/> specifies <see cref="ProcessStartupFlags.UseStandardHandles"/>, 
        /// this member is the standard error handle for the process. Otherwise, this member is 
        /// ignored and the default for standard error is the console window's buffer.
        /// </summary>
        internal SafeFileHandle StandardErrorHandle;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct ProcessStartupInfoEx
    {
        /// <summary>
        /// Actual process startup information.
        /// </summary>
        public ProcessStartupInfo StartupInfo;

        /// <summary>
        /// An attribute list. This list is created by the InitializeProcThreadAttributeList function.
        /// </summary>
        public IntPtr AttributeList;
    }

    internal enum ProcessThreadAttribute : uint
    {
        ParentProcess = 0x00020000,
        InheritHandle = 0x00020002,
    }

    /// <summary>
    /// The level of the handle to be opened when calling <see cref="Advapi32.SaferCreateLevel"/>.
    /// </summary>
    [Flags]
    internal enum SaferLevel : uint
    {
        /// <summary>
        /// Software will not run, regardless of the user rights of the user.
        /// </summary>
        Disallowed = 0x00000,
        /// <summary>
        /// Allows programs to execute with access only to resources granted to open well-known 
        /// groups, blocking access to Administrator and Power User privileges and personally 
        /// granted rights.
        /// </summary>
        Untrusted = 0x01000,
        /// <summary>
        /// Software cannot access certain resources, such as cryptographic keys and 
        /// credentials, regardless of the user rights of the user.
        /// </summary>
        Constrained = 0x10000,
        /// <summary>
        /// Allows programs to execute as a user that does not have Administrator or Power User 
        /// user rights. Software can access resources accessible by normal users.
        /// </summary>
        NormalUser = 0x20000,
        /// <summary>
        /// Software user rights are determined by the user rights of the user.
        /// </summary>
        FullyTrusted = 0x40000,
    }

    [Flags]
    internal enum SaferOpen : uint
    {
        Open = 1
    }

    /// <summary>
    /// The scope of the level to be created when calling <see cref="Advapi32.SaferCreateLevel"/>
    /// </summary>
    internal enum SaferScope : uint
    {
        /// <summary>
        /// The scope of the created level is by computer.
        /// </summary>
        Machine = 1,
        /// <summary>
        /// The scope of the created level is by user.
        /// </summary>
        User = 2,
    }

    /// <summary>
    /// Specifies the behavior of the <see cref="Advapi32.SaferComputeTokenFromLevel"/> method.
    /// </summary>
    [Flags]
    internal enum SaferComputeTokenBehavior : uint
    {
        /// <summary>
        /// 
        /// </summary>
        Default = 0,
        /// <summary>
        /// If the OutAccessToken parameter is not more restrictive than the InAccessToken 
        /// parameter, the OutAccessToken parameter returns NULL.
        /// </summary>
        NullIfEqual = 0x1,
        /// <summary>
        /// <para>The token specified by the InAccessToken parameter is compared with the token 
        /// that would be created if the restrictions specified by the LevelHandle parameter 
        /// were applied. The restricted token is not actually created.</para>
        /// 
        /// <para>On output, the value of the reserved parameter specifies the result of the 
        /// comparison.</para>
        /// </summary>
        CompareOnly = 0x2,
        /// <summary>
        /// <para>If this flag is set, the system does not check AppLocker rules or apply 
        /// Software Restriction Policies. For AppLocker, this flag disables checks for all 
        /// four rule collections: Executable, Windows Installer, Script, and DLL.</para>
        /// 
        /// <para>Set this flag when creating a setup program that must run extracted DLLs 
        /// during installation.</para>
        /// 
        /// <para>A token can be queried for existence of this flag by using 
        /// GetTokenInformation.</para>
        /// </summary>
        /// <remarks>
        /// Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  AppLocker 
        /// is not supported
        /// </remarks>
        MakeInert = 0x4,
        /// <summary>
        /// On output, the value of the reserved parameter specifies the set of flags used to 
        /// create the restricted token.
        /// </summary>
        WantFlags = 0x8,
    }

    /// <summary>
    /// Contains the security descriptor for an object and specifies whether the handle 
    /// retrieved by specifying this structure is inheritable.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityAttributes
    {
        /// <summary>
        /// The size, in bytes, of this structure. Set this value to the size of the 
        /// <see cref="SecurityAttributes"/> structure.
        /// </summary>
        internal uint Length;
        /// <summary>
        /// A pointer to a SECURITY_DESCRIPTOR structure that controls access to the object. 
        /// If the value of this member is <see cref="IntPtr.Zero"/>, the object is assigned the 
        /// default security descriptor associated with the access token of the calling process. 
        /// This is not the same as granting access to everyone by assigning a NULL discretionary 
        /// access control list (DACL). By default, the default DACL in the access token of a process 
        /// allows access only to the user represented by the access token.
        /// </summary>
        internal IntPtr Descriptor;
        /// <summary>
        /// Specifies whether the returned handle is inherited when a new process is created. If this 
        /// member is <see langword="true"/>, the new process inherits the handle.
        /// </summary>
        internal bool InheritHandle;
    }

    internal enum ShowWindowCommand : ushort
    {
        /// <summary>
        /// Hides the window and activates another window.
        /// </summary>
        Hide = 0,
        /// <summary>
        /// Activates and displays a window. If the window is minimized or maximized, the 
        /// system restores it to its original size and position. An application should specify 
        /// this flag when displaying the window for the first time.
        /// </summary>
        Normal = 1,
        /// <summary>
        /// Activates the window and displays it as a minimized window.
        /// </summary>
        Minimized = 2,
        /// <summary>
        /// Activates the window and displays it as a maximized window.
        /// </summary>
        Maximized = 3,
        /// <summary>
        /// Displays a window in its most recent size and position. This value is similar to 
        /// <see cref="Normal"/>, except that the window is not activated.
        /// </summary>
        NoActivate = 4,
        /// <summary>
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        Show = 5,
        /// <summary>
        /// Minimizes the specified window and activates the next top-level window in the Z 
        /// order.
        /// </summary>
        Minimize = 6,
        /// <summary>
        /// Displays the window as a minimized window. This value is similar to 
        /// <see cref="Minimized"/>, except the window is not activated.
        /// </summary>
        ShowMinimized = 7,
        /// <summary>
        /// Displays the window in its current size and position. This value is similar to 
        /// <see cref="Show"/>, except that the window is not activated.
        /// </summary>
        ShowNa = 8,
        /// <summary>
        /// Activates and displays the window. If the window is minimized or maximized, the 
        /// system restores it to its original size and position. An application should specify 
        /// this flag when restoring a minimized window.
        /// </summary>
        Restore = 9,
        /// <summary>
        /// Sets the show state based on the value specified in the 
        /// <see cref="ProcessStartupInfo"/> structure passed to the 
        /// <see cref="Advapi32.CreateProcessAsUser"/> function by the program that started the 
        /// application.
        /// </summary>
        Default = 10,
        /// <summary>
        /// Minimizes a window, even if the thread that owns the window is not responding. This 
        /// flag should only be used when minimizing windows from a different thread.
        /// </summary>
        ForceMinimize = 11,
    }

    /// <summary>
    /// The standard device. 
    /// </summary>
    internal enum StandardHandleType : int
    {
        /// <summary>
        /// The standard input device. Initially, this is the console input buffer, CONIN$.
        /// </summary>
        StandardInput = -10,
        /// <summary>
        /// The standard output device. Initially, this is the active console screen buffer, 
        /// CONOUT$.
        /// </summary>
        StandardOutput = -11,
        /// <summary>
        /// The standard error device. Initially, this is the active console screen buffer, 
        /// CONOUT$.
        /// </summary>
        StandardError = -12,
    }

    /// <summary>
    /// The flags that control the creation of the thread.
    /// </summary>
    [Flags]
    internal enum ThreadCreationFlags : uint
    {
        /// <summary>
        /// The thread runs immediately after creation.
        /// </summary>
        None = 0,
        /// <summary>
        /// The thread is created in a suspended state, and does not run until the ResumeThread function is called.
        /// </summary>
        Suspended = 0x00000004,
        /// <summary>
        /// The stackSize parameter specifies the initial reserve size of the stack. If this flag is not specified, dwStackSize specifies the commit size.
        /// </summary>
        StackSizeParamIsReservation = 0x00010000
    }

    [Flags]
    internal enum ToolhelpSnapshotFlags : uint
    {
        /// <summary>
        /// Includes all heaps of the process specified in th32ProcessID in the snapshot. To enumerate the heaps, see Heap32ListFirst.
        /// </summary>
        HeapList = 0x00000001,
        /// <summary>
        /// Includes all processes in the system in the snapshot. To enumerate the processes, see 
        /// <see cref="Kernel32.Process32First(SafeSnapshotHandle, ref ProcessEntry32)"/>.
        /// </summary>
        Process = 0x00000002,
        /// <summary>
        /// <para>Includes all threads in the system in the snapshot. To enumerate the threads, see Thread32First.</para>
        /// <para>To identify the threads that belong to a specific process, compare its process identifier to the 
        /// processId member of the THREADENTRY32 structure when enumerating the threads.</para>
        /// </summary>
        Thread = 0x00000004,
        /// <summary>
        /// <para>Includes all modules of the process specified in processId in the snapshot. To enumerate the modules, see Module32First. 
        /// If the function fails with ERROR_BAD_LENGTH, retry the function until it succeeds.</para>
        /// <para>64-bit Windows:  Using this flag in a 32-bit process includes the 32-bit modules of the process specified in processId, 
        /// while using it in a 64-bit process includes the 64-bit modules. To include the 32-bit modules of the process specified in 
        /// processId from a 64-bit process, use the <see cref="Module32"/> flag.</para>
        /// </summary>
        Module = 0x00000008,
        /// <summary>
        /// Includes all 32-bit modules of the process specified in processId in the snapshot when called from a 64-bit process.
        /// </summary>
        Module32 = 0x00000010,
        /// <summary>
        /// Indicates that the snapshot handle is to be inheritable.
        /// </summary>
        Inherit = 0x80000000,
        /// <summary>
        /// Includes all processes and threads in the system, plus the heaps and modules of the process specified in th32ProcessID. 
        /// </summary>
        All = HeapList
            | Module
            | Process
            | Thread,
    }

    internal enum WaitReturnCode : long
    {
        /// <summary>
        /// <para>The specified object is a mutex object that was not released by the thread that owned the mutex object before the owning thread terminated. Ownership of the mutex object is granted to the calling thread and the mutex state is set to nonsignaled.</para>
        /// <para>If the mutex was protecting persistent state information, you should check it for consistency.</para>
        /// </summary>
        Abandoned = 0x00000080L,
        /// <summary>
        /// The function has failed. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        /// </summary>
        Failed = 0xFFFFFFFF,
        /// <summary>
        /// The state of the specified object is signaled.
        /// </summary>
        Signaled = 0x00000000L,
        /// <summary>
        /// The time-out interval elapsed, and the object's state is nonsignaled.
        /// </summary>
        Timeout = 0x00000102L,
    }
}
