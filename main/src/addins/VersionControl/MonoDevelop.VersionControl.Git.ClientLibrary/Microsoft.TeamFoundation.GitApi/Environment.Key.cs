//*************************************************************************************************
// Environment.Key.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    partial class Environment
    {
        /// <summary>
        /// Well-known operating system environment variable keys.
        /// </summary>
        public static class Key
        {
            /// <summary>
            /// The fallback email address in case the user.email configuration value isn't set. If this
            /// isn't set, Git falls back to the system user and host names.
            /// </summary>
            public const string Email = "EMAIL";

            /// <summary>
            /// Used by some POSIX to determine the character encoding of strings.
            /// </summary>
            public const string CharSet = "CHARSET";

            public const string GcmAuthority = "GCM_AUTHORITY";

            public const string GcmHttpUserAgent = "GCM_HTTP_USER_AGENT";

            public const string GcmInteractive = "GCM_INTERACTIVE";

            public const string GcmModalPrompt = "GCM_MODAL_PROMPT";

            public const string GcmValidate = "GCM_VALIDATE";

            public const string GcmTrace = "GCM_TRACE";

            public const string GcmWritelog = "GCM_WRITELOG";

            /// <summary>
            /// Colon-separated list (formatted like "/dir/one:/dir/two:...") which tells Git where to
            /// check for objects if they aren't in `<see cref="GitObjectDirectoryPath"/>`. If you happen
            /// to have a lot of projects with large files that have the exact same contents, this can be
            /// used to avoid storing too many copies of them.
            /// </summary>
            public const string GitAlternateObjectDirectories = "GIT_ALTERNATE_OBJECT_DIRECTORIES";

            /// <summary>
            /// An override for the core.askpass configuration value.
            /// <para/>
            /// This is the program invoked whenever Git needs to ask the user for credentials, which can
            /// expect a text prompt as a command-line argument, and should return the answer on stdout.
            /// </summary>
            public const string GitAskpass = "GIT_ASKPASS";

            /// <summary>
            /// The timestamp used for the "author" field.
            /// </summary>
            public const string GitAuthorDate = "GIT_AUTHOR_DATE";

            /// <summary>
            /// The email for the "author" field.
            /// </summary>
            public const string GitAuthorEmail = "GIT_AUTHOR_EMAIL";

            /// <summary>
            /// The human-readable name in the "author" field.
            /// </summary>
            public const string GitAuthorName = "GIT_AUTHOR_NAME";

            /// <summary>
            /// Controls the behavior of searching for a ".git/" directory. If you access directories
            /// that are slow to load (such as those on a tape drive, or across a slow network
            /// connection), you may want to have Git stop trying earlier than it might otherwise,
            /// especially if Git is invoked when building your shell prompt.
            /// </summary>
            public const string GitCeilingDirectories = "GIT_CEILING_DIRECTORIES";

            /// <summary>
            /// Used for the timestamp in the "committer" field.
            /// </summary>
            public const string GitCommitterDate = "GIT_COMMITTER_DATE";

            /// <summary>
            /// The email address for the "committer" field.
            /// </summary>
            public const string GitCommitterEmail = "GIT_COMMITTER_EMAIL";

            /// <summary>
            /// Sets the human name for the "committer" field.
            /// </summary>
            public const string GitCommitterName = "GIT_COMMITTER_NAME";

            /// <summary>
            /// If this variable is set to a path, non-worktree files that are normally in $GIT_DIR will
            /// be taken from this path instead. Worktree-specific files such as HEAD or index are taken
            /// from $GIT_DIR. This variable has lower precedence than other path variables such as
            /// GIT_INDEX_FILE, GIT_OBJECT_DIRECTORY...
            /// </summary>
            public const string GitCommonDirPath = "GIT_COMMON_DIR";

            /// <summary>
            /// If set, disables the use of the system-wide configuration file. This is useful if your
            /// system config is interfering with your commands, but you don't have access to change or
            /// remove it.
            /// </summary>
            public const string GitConfigNoSystem = "GIT_CONFIG_NOSYSTEM";

            /// <summary>
            /// Tells Git to emit all the messages generated by that library. This is similar to doing
            /// "curl -v" on the command line.
            /// </summary>
            public const string GitCurlVerbose = "GIT_CURL_VERBOSE";

            /// <summary>
            /// Used by `<see cref="GitExternalDiffPath"/>`, represents which file in a series is being
            /// differenced (starting with 1).
            /// </summary>
            public const string GitDiffPathCounter = "GIT_DIFF_PATH_COUNTER";

            /// <summary>
            /// Used by `<see cref="GitExternalDiffPath"/>`, represents the total number of files in the batch.
            /// </summary>
            public const string GitDiffPathTotal = "GIT_DIFF_PATH_TOTAL";

            /// <summary>
            /// The location of the ".git/" folder. If this isn't specified, Git walks up the directory
            /// tree until it gets to "~" or "/", looking for a ".git/" directory at every step.
            /// </summary>
            public const string GitDirPath = "GIT_DIR";

            /// <summary>
            /// The editor Git will launch when the user needs to edit some text (a commit message, for
            /// example). If unset, %EDITOR% will be used.
            /// </summary>
            public const string GitEditorPath = "GIT_EDITOR";

            /// <summary>
            /// Determines where Git looks for its sub-programs (like git-commit, git-diff, and others).
            /// You can check the current setting by running git --exec-path.
            /// </summary>
            public const string GitExecPath = "GIT_EXEC_PATH";

            /// <summary>
            /// Used to force Git to use non-buffered I/O when writing incrementally to stdout.
            /// <para/>
            /// A value of 1 causes Git to flush more often, a value of 0 causes all output to be buffered.
            /// <para/>
            /// The default value (if this variable is not set) is to choose an appropriate buffering
            /// scheme depending on the activity and the output mode.
            /// </summary>
            public const string GitFlush = "GIT_FLUSH";

            /// <summary>
            /// Similar, but for the system-wide configuration. Git looks for this file at "$PREFIX/etc/gitconfig".
            /// </summary>
            public const string GitExecPrefix = "PREFIX";

            /// <summary>
            /// An override for the diff.external configuration value. If it's set, Git will invoke this
            /// program when git diff is invoked.
            /// </summary>
            public const string GitExternalDiffPath = "GIT_EXTERNAL_DIFF";

            /// <summary>
            /// `<see cref="GitGlobPathspecs"/>` and `<see cref="GitNoGlobPathspecs"/>` control the default
            /// behavior of wildcards in pathspecs. If `<see cref="GitGlobPathspecs"/>` is set to 1,
            /// wildcard characters act as wildcards (which is the default); if
            /// `<see cref="GitNoGlobPathspecs"/>` is set to 1, wildcard characters only match themselves,
            /// meaning something like *.c would only match a file named "*.c", rather than any file
            /// whose name ends with .c. You can override this in individual cases by starting the
            /// pathspec with :(glob) or :(literal), as in :(glob)*.c.
            /// </summary>
            public const string GitGlobPathspecs = "GIT_GLOB_PATHSPECS";

            /// <summary>
            /// If the data rate of an HTTP operation is lower than `<see cref="GitHttpLowSpeedLimit"/>`
            /// bytes per second for longer than `<see cref="GitHttpLowSpeedTime"/>` seconds, Git will
            /// abort that operation. These values override the http.lowSpeedLimit and http.lowSpeedTime
            /// configuration values.
            /// </summary>
            public const string GitHttpLowSpeedLimit = "GIT_HTTP_LOW_SPEED_LIMIT";

            /// <summary>
            /// If the data rate of an HTTP operation is lower than `<see cref="GitHttpLowSpeedLimit"/>`
            /// bytes per second for longer than `<see cref="GitHttpLowSpeedTime"/>` seconds, Git will
            /// abort that operation. These values override the http.lowSpeedLimit and http.lowSpeedTime
            /// configuration values.
            /// </summary>
            public const string GitHttpLowSpeedTime = "GIT_HTTP_LOW_SPEED_TIME";

            /// <summary>
            /// Sets the user-agent string used by Git when communicating over HTTP. The default is a
            /// value like "git/2.0.0".
            /// </summary>
            public const string GitHttpUserAgent = "GIT_HTTP_USER_AGENT";

            /// <summary>
            /// The path to the index file (non-bare repositories only).
            /// </summary>
            public const string GitIndexFilePath = "GIT_INDEX_FILE";

            /// <summary>
            /// All pathspecs work in a case-insensitive manner.
            /// </summary>
            public const string GitInsensitivePathspecs = "GIT_ICASE_PATHSPECS";

            /// <summary>
            /// Disables both `<see cref="GitGlobPathspecs"/>` and `<see cref="GitNoGlobPathspecs"/>`; no
            /// wildcard characters will work, and the override prefixes are disabled as well.
            /// </summary>
            public const string GitLiteralPathspecs = "GIT_LITERAL_PATHSPECS";

            /// <summary>
            /// Used to specify the location of the directory that usually resides at ".git/objects/".
            /// </summary>
            public const string GitObjectDirectoryPath = "GIT_OBJECT_DIRECTORY";

            /// <summary>
            /// Controls the output for the recursive merge strategy. The allowed values are as follows:
            /// <para/>
            /// 0 outputs nothing, except possibly a single error message.
            /// <para/>
            /// 1 shows only conflicts.
            /// <para/>
            /// 2 also shows file changes.
            /// <para/>
            /// 3 shows when files are skipped because they haven't changed.
            /// <para/>
            /// 4 shows all paths as they are processed.
            /// <para/>
            /// 5 and above show detailed debugging information.
            /// <para/>
            /// The default value is 2.
            /// </summary>
            public const string GitMergeVerbosity = "GIT_MERGE_VERBOSITY";

            /// <summary>
            /// Controls the program used to display multi-page output on the command line. If this is
            /// unset, %PAGER% will be used as a fallback.
            /// </summary>
            public const string GitPager = "GIT_PAGER";

            /// <summary>
            /// controls access to namespaced refs, and is equivalent to the --namespace flag.
            /// <para/>
            /// This is mostly useful on the server side, where you may want to store multiple forks of a
            /// single repository in one repository, only keeping the refs separate.
            /// </summary>
            public const string GitNamespace = "GIT_NAMESPACE";

            /// <summary>
            /// `<see cref="GitGlobPathspecs"/>` and `<see cref="GitNoGlobPathspecs"/>` control the default
            /// behavior of wildcards in pathspecs. If `<see cref="GitGlobPathspecs"/>` is set to 1,
            /// wildcard characters act as wildcards (which is the default); if
            /// `<see cref="GitNoGlobPathspecs"/>` is set to 1, wildcard characters only match themselves,
            /// meaning something like *.c would only match a file named "*.c", rather than any file
            /// whose name ends with .c. You can override this in individual cases by starting the
            /// pathspec with :(glob) or :(literal), as in :(glob)*.c.
            /// </summary>
            public const string GitNoGlobPathspecs = "GIT_NOGLOB_PATHSPECS";

            /// <summary>
            /// If set, Git will use the named object as its standard error; ignoring the supplied
            /// standard error handle.
            /// </summary>
            public const string GitRedirectStdErr = "GIT_REDIRECT_STDERR";

            /// <summary>
            /// If set, Git will use the named object as its standard input; ignoring the supplied
            /// standard input handle.
            /// </summary>
            public const string GitRedirectStdIn = "GIT_REDIRECT_STDIN";

            /// <summary>
            /// If set, Git will use the named object as its standard output; ignoring the supplied
            /// standard output handle.
            /// </summary>
            public const string GitRedirectStdOut = "GIT_REDIRECT_STDOUT";

            /// <summary>
            /// Specifies the descriptive text written to the reflog.
            /// </summary>
            public const string GitReflogAction = "GIT_REFLOG_ACTION";

            /// <summary>
            /// If specified, is a program that is invoked instead of ssh when Git tries to connect to an
            /// SSH host.
            /// <para/>
            /// It is invoked like $GIT_SSH [username@]host [-p &lt;port&gt;] &lt;command&gt;.
            /// </summary>
            public const string GitSshPath = "GIT_SSH";

            /// <summary>
            /// Tells Git not to verify SSL certificates. This can sometimes be necessary if you're using
            /// a self-signed certificate to serve Git repositories over HTTPS, or you're in the middle
            /// of setting up a Git server but haven't installed a full certificate yet.
            /// </summary>
            public const string GitSslNoVerify = "GIT_SSL_NO_VERIFY";

            /// <summary>
            /// Sets whether Git should attempt terminal prompting or not.
            /// <para/>
            /// "0" or "false" to disallow.
            /// <para/>
            /// "1" or "true" to allow.
            /// <para/>
            /// Default is to allow.
            /// </summary>
            public const string GitTerminalPrompt = "GIT_TERMINAL_PROMPT";

            /// <summary>
            /// Controls tracing of pack file access. The first field is the pack file being accessed,
            /// the second is the offset within that file
            /// <para/>
            /// "true", "1", or "2" ΓÇô the trace category is written to stderr.
            /// <para/>
            /// An absolute path starting with "/" ΓÇô the trace output will be written to that file.
            /// </summary>
            public const string GitTracePackAccess = "GIT_TRACE_PACK_ACCESS";

            /// <summary>
            /// Controls general traces, which don't fit into any specific category. This includes the
            /// expansion of aliases, and delegation to other sub-programs.
            /// <para/>
            /// "true", "1", or "2" ΓÇô the trace category is written to stderr.
            /// <para/>
            /// An absolute path starting with "/" ΓÇô the trace output will be written to that file.
            ///
            /// </summary>
            public const string GitTrace = "GIT_TRACE";

            /// <summary>
            /// Enables packet-level tracing for network operations.
            /// <para/>
            /// "true", "1", or "2" ΓÇô the trace category is written to stderr.
            /// <para/>
            /// An absolute path starting with "/" ΓÇô the trace output will be written to that file.
            /// </summary>
            public const string GitTracePacket = "GIT_TRACE_PACKET";

            /// <summary>
            /// Controls logging of performance data. The output shows how long each particular git
            /// invocation takes.
            /// <para/>
            /// "true", "1", or "2" ΓÇô the trace category is written to stderr.
            /// <para/>
            /// An absolute path starting with "/" ΓÇô the trace output will be written to that file.
            /// </summary>
            public const string GitTracePerformance = "GIT_TRACE_PERFORMANCE";

            /// <summary>
            /// Shows information about what Git is discovering about the repository and environment it's
            /// interacting with.
            /// <para/>
            /// "true", "1", or "2" ΓÇô the trace category is written to stderr.
            /// <para/>
            /// An absolute path starting with "/" ΓÇô the trace output will be written to that file.
            /// </summary>
            public const string GitTraceSetup = "GIT_TRACE_SETUP";

            /// <summary>
            /// The location of the root of the working directory for a non-bare repository. If not
            /// specified, the parent directory of $GIT_DIR is used.
            /// </summary>
            public const string GitWorkTreePath = "GIT_WORK_TREE";

            /// <summary>
            /// Isn't usually considered customizable (too many other things depend on it), but it's
            /// where Git looks for the global configuration file.
            /// </summary>
            public const string Home = "HOME";

            /// <summary>
            /// Specifies the drive letter to which to map the UNC path specified by homeDirectory.
            /// <para/>
            /// The drive letter must be specified in the form DriveLetter: where DriveLetter is the
            /// letter of the drive to map.
            /// <para/>
            /// The DriveLetter must be a single, uppercase letter and the colon (:) is required.
            /// </summary>
            public const string HomeDrive = "HOMEDRIVE";

            /// <summary>
            /// HomePath environment variable returns the complete path of the user's home directory, as
            /// defined in the user's account properties within the domain.
            /// <para/>
            /// HomePath environment variable is based on the value of the home directory.
            /// </summary>
            public const string HomePath = "HOMEPATH";

            /// <summary>
            /// Overrides all "LC_*" environment variables.
            /// </summary>
            public const string LocaleCategoryAll = "LC_ALL";

            /// <summary>
            /// Used by the Git for Windows Msys POSIX adapter layer.
            /// </summary>
            public const string MsystemPath = "MSYSTEM";

            /// <summary>
            /// An environment variable specifying a set of directories where executable programs are located.
            /// <para/>
            /// In general, each executing process or user session has its own PATH setting.
            /// </summary>
            public const string Path = "PATH";

            public const string Plink = "PLINK";

            /// <summary>
            /// Used by the Git Process Management library, when communicating with git.exe.
            /// </summary>
            public const string ProcessNamespace = "PROCESS_NAMESPACE";

            /// <summary>
            /// This is the program invoked whenever SSH needs to ask the user for credentials, which can
            /// expect a text prompt as a command-line argument, and should return the answer on stdout.
            /// </summary>
            public const string SshAskpass = "SSH_ASKPASS";

            /// <summary>
            /// The file system directory that serves as a common repository for application-specific data.
            /// </summary>
            public const string WindowsApplicationDataRoaming = "APPDATA";

            /// <summary>
            /// The file system directory that serves as a common repository for local only application-specific data.
            /// </summary>
            public const string WindowsApplicationDataLocal = "LOCALAPPDATA";

            /// <summary>
            /// Specifies the path to the program data folder.
            /// </summary>
            public const string WindowsProgramData = "ProgramData";

            /// <summary>
            /// The file system directory that serves as the common location for installed application.
            /// </summary>
            public const string WindowsProgramFiles = "ProgramFiles";

            /// <summary>
            /// The file system directory that serves as the common location for installed 32-bit application on a 64-bit system.
            /// </summary>
            public const string WindowsProgramFiles32 = "ProgramFiles(x86)";

            /// <summary>
            /// The file system directory that serves as the common locaiton for installed 32-bit applications on a 64-bit system.
            /// </summary>
            public const string WindowsProgramFiles64 = "ProgramW6432";

            /// <summary>
            /// The file system directory that serves as the common location for the current user's profile data.
            /// </summary>
            public const string WindowsUserProfile = "USERPROFILE";

            /// <summary>
            /// Where XDG user-specific configurations should be written.
            /// <para/>
            /// Should default to $HOME/.config
            /// <para/>
            /// Git configuration data is contained in $XDG_CONFIG_HOME/.config/git/config
            /// <para/>
            /// Git ignore data is contained in $XDG_CONFIG_HOME/.config/git/ignore
            /// <para/>
            /// Git attribute data is contained in $XDG_CONFIG_HOME/.config/git/attributes
            /// <para/>
            /// Git credential data is contained in $XDG_CONFIG_HOME/.config/git/credentials
            /// </summary>
            public const string XdgConfigHome = "XDG_CONFIG_HOME";

            /// <summary>
            /// The most important environment variable for X Window System clients is DISPLAY. When a user logs in at an X terminal, the DISPLAY environment variable in each xterm window is set to her X terminal's hostname followed by ":0.0".
            /// <para/>
            /// This is particularly important for SSH, which uses the variable to determine if it can execute `<see cref="SshAskpass"/>`.
            /// <para/>
            /// Windows operating system should always set this value to ":0.0" if `<see cref="SshAskpass"/>` should be executed.
            /// </summary>
            public const string XtermDisplay = "DISPLAY";
        }
    }
}
