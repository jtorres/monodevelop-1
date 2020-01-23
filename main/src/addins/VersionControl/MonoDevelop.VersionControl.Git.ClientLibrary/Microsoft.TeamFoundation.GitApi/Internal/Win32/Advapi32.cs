//*************************************************************************************************
// Advapi32.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.TeamFoundation.GitApi.Internal.Win32
{
    internal static class Advapi32
    {
        public static readonly IntPtr InvalidHandleValue = new IntPtr(-1);
        public const string Name = "advapi32.dll";

        /// <summary>
        /// <para>Creates a new process and its primary thread. The new process runs in the 
        /// security context of the user represented by the specified token.</para>
        /// 
        /// <para>Typically, the process that calls the <see cref="CreateProcessAsUser"/> function 
        /// must have the SE_INCREASE_QUOTA_NAME privilege and may require the 
        /// SE_ASSIGNPRIMARYTOKEN_NAME privilege if the token is not assignable. If this function 
        /// fails with <see cref="ErrorCode.PrivilegeNotHeld"/>, use the 
        /// CreateProcessWithLogonW function instead. CreateProcessWithLogonW requires no special 
        /// privileges, but the specified user account must be allowed to log on interactively. 
        /// Generally, it is best to use CreateProcessWithLogonW to create a process with alternate 
        /// credentials.</para>
        /// </summary>
        /// <param name="token">
        /// <para>A handle to the primary token that represents a user. The handle must have the 
        /// TOKEN_QUERY, TOKEN_DUPLICATE, and TOKEN_ASSIGN_PRIMARY access rights. The user 
        /// represented by the token must have read and execute access to the application specified 
        /// by the applicationName or the commandLine parameter.</para>
        /// 
        /// <para>To get a primary token that represents the specified user, call the LogonUser 
        /// function. Alternatively, you can call the DuplicateTokenEx function to convert an 
        /// impersonation token into a primary token. This allows a server application that is 
        /// impersonating a client to create a process that has the security context of the 
        /// client.</para>
        /// 
        /// <para>If token is a restricted version of the caller's primary token, the 
        /// SE_ASSIGNPRIMARYTOKEN_NAME privilege is not required. If the necessary privileges are 
        /// not already enabled, <see cref="CreateProcessAsUser"/> enables them for the duration of 
        /// the call.</para>
        /// </param>
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
        /// <param name="processAttributes">A pointer to a <see cref="SecurityAttributes"/> 
        /// structure that specifies a security descriptor for the new process object and 
        /// determines whether child processes can inherit the returned handle to the process. If 
        /// processAttributes is NULL or <see cref="SecurityAttributes.Descriptor"/> is NULL, 
        /// the process gets a default security descriptor and the handle cannot be inherited. The 
        /// default security descriptor is that of the user referenced in the token parameter. This 
        /// security descriptor may not allow access for the caller, in which case the process may 
        /// not be opened again after it is run. The process handle is valid and will continue to 
        /// have full access rights.</param>
        /// <param name="threadAttributes">A pointer to a <see cref="SecurityAttributes"/> structure 
        /// that specifies a security descriptor for the new thread object and determines whether 
        /// child processes can inherit the returned handle to the thread. If threadAttributes is 
        /// NULL or <see cref="SecurityAttributes.Descriptor"/> is NULL, the thread gets a 
        /// default security descriptor and the handle cannot be inherited. The default security 
        /// descriptor is that of the user referenced in the hToken parameter. This security 
        /// descriptor may not allow access for the caller.</param>
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
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "CreateProcessAsUserW", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool CreateProcessAsUser(
                [In] IntPtr token,
                [In, MarshalAs(UnmanagedType.LPWStr)] string applicationName,
                [In] IntPtr commandLine,
                [In] IntPtr processAttributes,
                [In] IntPtr threadAttributes,
                [In, MarshalAs(UnmanagedType.Bool)] bool inheritHandles,
                ProcessCreationFlags creationFlags,
                [In] IntPtr environment,
                [MarshalAs(UnmanagedType.LPWStr)] string currentDirectory,
                [In] ref ProcessStartupInfoEx startupInfo,
                [Out] out ProcessInformation processInfo);

        /// <summary>
        /// Closes a <see cref="SafeSaferLevelHandle"/> that was opened by using the SaferIdentifyLevel function or 
        /// the <see cref="SaferCreateLevel"/> function.
        /// </summary>
        /// <param name="saferLevelPtr">The <see cref="SafeSaferLevelHandle"/> to be closed.</param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "SaferCloseLevel", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool SaferCloseLevel(IntPtr saferLevelPtr);

        /// <summary>
        /// Closes a <see cref="SafeSaferLevelHandle"/> that was opened by using the SaferIdentifyLevel function or 
        /// the <see cref="SaferCreateLevel"/> function.
        /// </summary>
        /// <param name="saferLevelPtr">The <see cref="SafeSaferLevelHandle"/> to be closed.</param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "SaferCloseLevel", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool SaferCloseLevel(SafeSaferLevelHandle saferLevelPtr);

        /// <summary>
        /// Restricts a token using restrictions specified by a <see cref="SafeSaferLevelHandle"/>.
        /// </summary>
        /// <param name="saferLevelHandle"><see cref="SafeSaferLevelHandle"/> that contains the 
        /// restrictions to place on the input token. Do not pass handles with a LevelId of 
        /// <see cref="SaferLevel.FullyTrusted"/> or <see cref="SaferLevel.Disallowed"/> to this 
        /// function. This is because <see cref="SaferLevel.FullyTrusted"/> is unrestricted and 
        /// <see cref="SaferLevel.Disallowed"/> does not contain a token.</param>
        /// <param name="inAccessToken">Token to be restricted. If this parameter is 
        /// <see cref="IntPtr.Zero"/>, the token of the current thread will be used. If the current 
        /// thread does not contain a token, the token of the current process is used.</param>
        /// <param name="outAccessToken">The resulting restricted token.</param>
        /// <param name="flags">Specifies the behavior of the method.</param>
        /// <param name="reserved">If the <see cref="SaferComputeTokenBehavior.CompareOnly"/> flag 
        /// is set, this parameter, on output, specifies the result of the token comparison. The 
        /// output value is an <see cref="IntPtr"/>. A value of –1 indicates that the resulting 
        /// token would be less privileged than the token specified by the inAccessToken parameter.
        /// </param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "SaferComputeTokenFromLevel", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool SaferComputeTokenFromLevel(
            SafeSaferLevelHandle saferLevelHandle,
            IntPtr inAccessToken,
            out IntPtr outAccessToken,
            SaferComputeTokenBehavior flags,
            IntPtr reserved);

        /// <summary>
        /// The SaferCreateLevel function opens a SAFER_LEVEL_HANDLE.
        /// </summary>
        /// <param name="scopeId">The scope of the level to be created.</param>
        /// <param name="levelId">The level of the handle to be opened. </param>
        /// <param name="openFlags">Value must be 1.</param>
        /// <param name="saferLevelHandle">The returned <see cref="SafeSaferLevelHandle"/>. When 
        /// you have finished using the handle, close it by calling the SaferCloseLevel function.
        /// </param>
        /// <param name="reserved">This parameter is reserved for future use. Set it to 
        /// <see cref="IntPtr.Zero"/>.</param>
        /// <returns>Returns true if successful or false if otherwise.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "SaferCreateLevel", ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool SaferCreateLevel(
            SaferScope scopeId,
            SaferLevel levelId,
            SaferOpen openFlags,
            out SafeSaferLevelHandle saferLevelHandle,
            IntPtr reserved);
    }
}
