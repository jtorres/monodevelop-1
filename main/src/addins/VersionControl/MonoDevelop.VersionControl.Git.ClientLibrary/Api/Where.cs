//*************************************************************************************************
// Where.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Microsoft.TeamFoundation.GitApi
{
    public interface IWhere
    {
        /// <summary>
        /// Finds the "best" path to an application of a given name.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="name">The name of the application, without extension, to find.</param>
        /// <param name="path">Path to the first match file which the operating system considers executable.</param>
        bool FindApplication(string name, out string path);

        /// <summary>
        /// Finds an installation of Git-Askpass for Windows.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="installation">The installation of Git the system config is desired from.</param>
        /// <param name="path">Path to the Git global configuration</param>
        bool FindGitAskpass(Installation installation, out string path);

        /// <summary>
        /// Finds an installation of the Git Credential Manager for Windows.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="installation">The installation of Git the system config is desired from.</param>
        /// <param name="path">Path to the Git global configuration</param>
        bool FindGitCredentialManager(Installation installation, out string path);

        /// <summary>
        /// Finds an installation of Git for Windows at the given path.
        /// </summary>
        /// <param name="path">The assumed path to the root of the installation.</param>
        /// <param name="distro">The assumed distribution and version of the installation.</param>
        /// <param name="installation">
        /// Populated structure containing details of the installations if successful.
        /// </param>
        bool FindGitInstallation(string path, KnownDistribution distro, out Installation installation);

        /// <summary>
        /// Finds an returns a list of Git for Windows installations found on the local system.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="installations">List of installations if successful.</param>
        bool FindGitInstallations(out List<Installation> installations);

        /// <summary>
        /// Finds an returns the path to the user's home directory to be used by Git.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="environment">The instance of environment to use when scanning for existing values.</param>
        /// <param name="homePath">The path to the user's home directory.</param>
        bool FindHomePath(Environment environment, out string homePath);

        /// <summary>
        /// Finds an returns the path to the user's home directory to be used by Git.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="homePath">The path to the user's home directory.</param>
        bool FindHomePath(out string homePath);

        /// <summary>
        /// Gets the path to the Git global configuration file.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="environment">The environment to use when discovering the global configuration file.</param>
        /// <param name="path">Path to the Git global configuration.</param>
        bool GitGlobalConfig(Environment environment, out string path);

        /// <summary>
        /// Gets the path to the Git global configuration file.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="path">Path to the Git global configuration.</param>
        bool GitGlobalConfig(out string path);

        /// <summary>
        /// Gets the path to the Git local configuration file based on the `<paramref name="startingDirectory"/>`.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="environment">The instance of environment to use when scanning for existing values.</param>
        /// <param name="startingDirectory">A directory of the repository where the configuration file is contained.</param>
        /// <param name="path">Path to the Git local configuration.</param>
        bool GitLocalConfig(string startingDirectory, out string path);

        /// <summary>
        /// Gets the path to the Git local configuration file.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="path">Path to the Git local configuration.</param>
        bool GitLocalConfig(out string path);

        /// <summary>
        /// Gets the path to the Git portable system configuration file.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="environment">The instance of environment to use when scanning for existing values.</param>
        /// <param name="path">Path to the Git portable system configuration.</param>
        bool GitPortableConfig(out string path);

        /// <summary>
        /// Gets the path to the Git system configuration file.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="path">Path to the Git system configuration.</param>
        bool GitSystemConfig(out string path);

        /// <summary>
        /// Gets the path to the Git system configuration file.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="installation">The installation of Git the system config is desired from.</param>
        /// <param name="path">Path to the Git system configuration.</param>
        bool GitSystemConfig(Installation installation, out string path);

        /// <summary>
        /// Gets the path to the Git Xdg configuration file.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="path">Path to the Git portable system configuration</param>
        bool GitXdgConfig(out string path);
    }

    internal class Where : IWhere
    {
        public static readonly StringComparer FileSystemComparer = StringComparer.OrdinalIgnoreCase;

        private const string CommonDirFileName = "commondir";
        private static readonly Lazy<Regex> DotGitFileRegex = new Lazy<Regex>(() => { return new Regex(@"gitdir\s*:\s*([^\r\n]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase); });
        private const string HeadFileName = "HEAD";
        private const string ObjectsDirName = "objects";
        private const string RefsDirName = "refs";
        private static readonly char[] StringSplitChars = new[] { ';' };

        internal Where(ExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _context = context;
            _environment = _context.EnvironmentCreate();
        }

        private readonly ExecutionContext _context;
        private readonly Environment _environment;
        
        private IFileSystem FileSystem
        {
            get { return _context.FileSystem; }
        }

        public bool FindApplication(string name, out string path)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Environment.Variable pathextVariable;
                Environment.Variable pathVariable;

                // Combine %PATH% and %PATHEXT% to find the first match
                if (_environment.TryGet("PATHEXT", out pathextVariable)
                    && !string.IsNullOrEmpty(pathextVariable.Value)
                    && _environment.TryGet(Environment.Key.Path, out pathVariable)
                    && !string.IsNullOrEmpty(pathVariable.Value))
                {
                    string pathext = pathextVariable.Value;
                    string envpath = pathVariable.Value;

                    string[] exts = pathext.Split(StringSplitChars, StringSplitOptions.RemoveEmptyEntries);
                    string[] paths = envpath.Split(StringSplitChars, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < paths.Length; i++)
                    {
                        if (paths[i] == null)
                            continue;

                        for (int j = 0; j < exts.Length; j++)
                        {
                            if (exts[j] == null)
                                continue;

                            // Concatenate the path together (value = $"{paths[0]}\\{name}{exts[j]}")
                            // then validate that it actually exists. If it does, it's "the one" otherwise
                            // keep trying until success or all potential values are exhausted.
                            string value = string.Concat(paths[i], Path.DirectorySeparatorChar, name, exts[j]);
                            if (FileSystem.FileExists(value))
                            {
                                path = value;
                                return true;
                            }
                        }
                    }
                }
            }

            path = null;
            return false;
        }

        public bool FindGitAskpass(Installation git, out string path)
        {
            const string GitAppName = @"Git";
            const string GcmAppName = "git-askpass.exe";

            Debug.Assert(Installation.IsValid(_context, git));

            if (Installation.IsValid(_context, git))
            {
                Environment.Variable variable;
                if (_environment.TryGet(Environment.Key.WindowsApplicationDataRoaming, out variable)
                    && !string.IsNullOrEmpty(variable.Value))
                {
                    string libgexecPath = Path.Combine(git.Libexec, GcmAppName);
                    string commonPath = Path.Combine(variable.Value, GitAppName, GcmAppName);

                    libgexecPath = FileSystem.CanonicalizePath(libgexecPath);
                    commonPath = FileSystem.CanonicalizePath(commonPath);

                    if (FileSystem.FileExists(libgexecPath))
                    {
                        path = _context.PathHelper.ToPosixPath(libgexecPath);
                        return true;
                    }
                    else if (FileSystem.FileExists(commonPath))
                    {
                        path = _context.PathHelper.ToPosixPath(commonPath);
                        return true;
                    }
                }
            }

            path = null;
            return false;
        }

        public bool FindGitCredentialManager(Installation git, out string path)
        {
            const string GitAppName = @"Git";
            const string GcmAppName = "git-credential-manager.exe";

            Debug.Assert(Installation.IsValid(_context, git));

            if (Installation.IsValid(_context, git))
            {
                Environment.Variable variable;
                if (_environment.TryGet(Environment.Key.WindowsApplicationDataRoaming, out variable)
                    && !string.IsNullOrEmpty(variable.Value))
                {
                    string libgexecPath = Path.Combine(git.Libexec, GcmAppName);
                    string commonPath = Path.Combine(variable.Value, GitAppName, GcmAppName);

                    libgexecPath = FileSystem.CanonicalizePath(libgexecPath);
                    commonPath = FileSystem.CanonicalizePath(commonPath);

                    if (FileSystem.FileExists(libgexecPath))
                    {
                        path = _context.PathHelper.ToPosixPath(libgexecPath);
                        return true;
                    }
                    else if (FileSystem.FileExists(commonPath))
                    {
                        path = _context.PathHelper.ToPosixPath(commonPath);
                        return true;
                    }
                }
            }

            path = null;
            return false;
        }

        public bool FindGitInstallation(string path, KnownDistribution distro, out Installation installation)
        {
            // If the distribution is unknown, attempt to reconcile it
            if (distro == KnownDistribution.Unknown)
            {
                distro = Installation.GetDistribution(_context, path);
            }

            installation = new Installation(_context, path, distro);
            return Installation.IsValid(_context, installation);
        }

        public bool FindGitInstallations(out List<Installation> installations)
        {
            const string GitAppName = @"Git";
            const string GitSubkeyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Git_is1";
            const string GitValueName = "InstallLocation";

            installations = null;

            var programFiles32Path = string.Empty;
            var programFiles64Path = string.Empty;
            var appDataRoamingPath = string.Empty;
            var appDataLocalPath = string.Empty;
            var programDataPath = string.Empty;
            var reg32HklmPath = string.Empty;
            var reg64HklmPath = string.Empty;
            var reg32HkcuPath = string.Empty;
            var reg64HkcuPath = string.Empty;
            var findGitPath = string.Empty;

            using (var reg32HklmKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            using (var reg32HkcuKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
            using (var reg32HklmSubKey = reg32HklmKey?.OpenSubKey(GitSubkeyName))
            using (var reg32HkcuSubKey = reg32HkcuKey?.OpenSubKey(GitSubkeyName))
            {
                reg32HklmPath = reg32HklmSubKey?.GetValue(GitValueName, reg32HklmPath) as string;
                reg32HkcuPath = reg32HkcuSubKey?.GetValue(GitValueName, reg32HkcuPath) as string;
            }

            // Check %PROGRAMFILES(X86)%
            if ((programFiles32Path = _environment.GetEnvironmentVariable(Environment.Key.WindowsProgramFiles32)) != null)
            {
                programFiles32Path = Path.Combine(programFiles32Path, GitAppName);
            }

            if (Environment.Is64BitSystem)
            {
                using (var reg64HklmKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var reg64HkcuKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
                using (var reg64HklmSubKey = reg64HklmKey?.OpenSubKey(GitSubkeyName))
                using (var reg64HkcuSubKey = reg64HkcuKey?.OpenSubKey(GitSubkeyName))
                {
                    reg64HklmPath = reg64HklmSubKey?.GetValue(GitValueName, reg64HklmPath) as string;
                    reg64HkcuPath = reg64HkcuSubKey?.GetValue(GitValueName, reg64HkcuPath) as string;
                }

                // Check %PROGRAMW6432%
                if ((programFiles64Path = _environment.GetEnvironmentVariable(Environment.Key.WindowsProgramFiles64)) != null)
                {
                    programFiles64Path = Path.Combine(programFiles64Path, GitAppName);
                }
            }

            // Check %APPDATA%
            if ((appDataRoamingPath = _environment.GetEnvironmentVariable(Environment.Key.WindowsApplicationDataRoaming)) != null)
            {
                appDataRoamingPath = Path.Combine(appDataRoamingPath, GitAppName);
            }

            // Check %LOCALAPPDATA%
            if ((appDataLocalPath = _environment.GetEnvironmentVariable(Environment.Key.WindowsApplicationDataLocal)) != null)
            {
                appDataLocalPath = Path.Combine(appDataLocalPath, GitAppName);
            }

            // Check %PROGRAMDATA%
            if ((programDataPath = _environment.GetEnvironmentVariable(Environment.Key.WindowsProgramData)) != null)
            {
                programDataPath = Path.Combine(programDataPath, GitAppName);
            }

            var candidates = new List<Installation>();
            // Add candidate locations in order of preference
            if (FindApplication(GitAppName, out findGitPath))
            {
                // `Where.App` returns the path to the executable, truncate to the installation root
                if (findGitPath.EndsWith(Installation.AllVersionGitExe, StringComparison.OrdinalIgnoreCase))
                {
                    string cmdpath = @"cmd\" + Installation.AllVersionGitExe;

                    if (findGitPath.EndsWith(cmdpath, StringComparison.OrdinalIgnoreCase))
                    {
                        findGitPath = findGitPath.Substring(0, findGitPath.Length - cmdpath.Length);
                    }
                    else if (findGitPath.EndsWith(Installation.Version2Bin64Path, StringComparison.OrdinalIgnoreCase))
                    {
                        findGitPath = findGitPath.Substring(0, findGitPath.Length - Installation.Version2Bin64Path.Length);
                    }
                    else if (findGitPath.EndsWith(Installation.Version2Bin32Path, StringComparison.OrdinalIgnoreCase))
                    {
                        findGitPath = findGitPath.Substring(0, findGitPath.Length - Installation.Version2Bin32Path.Length);
                    }
                    else if (findGitPath.EndsWith(Installation.Version1Bin32Path, StringComparison.OrdinalIgnoreCase))
                    {
                        findGitPath = findGitPath.Substring(0, findGitPath.Length - Installation.Version1Bin32Path.Length);
                    }
                }

                candidates.Add(new Installation(_context, findGitPath, KnownDistribution.GitForWindows64v2));
                candidates.Add(new Installation(_context, findGitPath, KnownDistribution.GitForWindows32v2));
                candidates.Add(new Installation(_context, findGitPath, KnownDistribution.GitForWindows32v1));
            }

            if (!string.IsNullOrEmpty(reg64HklmPath))
            {
                candidates.Add(new Installation(_context, reg64HklmPath, KnownDistribution.GitForWindows64v2));
            }
            if (!string.IsNullOrEmpty(programFiles64Path))
            {
                candidates.Add(new Installation(_context, programFiles64Path, KnownDistribution.GitForWindows64v2));
            }
            if (!string.IsNullOrEmpty(reg64HkcuPath))
            {
                candidates.Add(new Installation(_context, reg64HkcuPath, KnownDistribution.GitForWindows64v2));
            }
            if (!string.IsNullOrEmpty(reg32HklmPath))
            {
                candidates.Add(new Installation(_context, reg32HklmPath, KnownDistribution.GitForWindows32v2));
                candidates.Add(new Installation(_context, reg32HklmPath, KnownDistribution.GitForWindows32v1));
            }
            if (!string.IsNullOrEmpty(programFiles32Path))
            {
                candidates.Add(new Installation(_context, programFiles32Path, KnownDistribution.GitForWindows32v2));
                candidates.Add(new Installation(_context, programFiles32Path, KnownDistribution.GitForWindows32v1));
            }
            if (!string.IsNullOrEmpty(reg32HkcuPath))
            {
                candidates.Add(new Installation(_context, reg32HkcuPath, KnownDistribution.GitForWindows32v2));
                candidates.Add(new Installation(_context, reg32HkcuPath, KnownDistribution.GitForWindows32v1));
            }
            if (!string.IsNullOrEmpty(programDataPath))
            {
                candidates.Add(new Installation(_context, programDataPath, KnownDistribution.GitForWindows64v2));
                candidates.Add(new Installation(_context, programDataPath, KnownDistribution.GitForWindows32v2));
                candidates.Add(new Installation(_context, programDataPath, KnownDistribution.GitForWindows32v1));
            }
            if (!string.IsNullOrEmpty(appDataLocalPath))
            {
                candidates.Add(new Installation(_context, appDataLocalPath, KnownDistribution.GitForWindows64v2));
                candidates.Add(new Installation(_context, appDataLocalPath, KnownDistribution.GitForWindows32v2));
                candidates.Add(new Installation(_context, appDataLocalPath, KnownDistribution.GitForWindows32v1));
            }
            if (!string.IsNullOrEmpty(appDataRoamingPath))
            {
                candidates.Add(new Installation(_context, appDataRoamingPath, KnownDistribution.GitForWindows64v2));
                candidates.Add(new Installation(_context, appDataRoamingPath, KnownDistribution.GitForWindows32v2));
                candidates.Add(new Installation(_context, appDataRoamingPath, KnownDistribution.GitForWindows32v1));
            }

            var pathSet = new HashSet<Installation>();
            foreach (var candidate in candidates)
            {
                if (Installation.IsValid(_context, candidate))
                {
                    pathSet.Add(candidate);
                }
            }

            installations = pathSet.ToList();

            _context.Tracer.TraceMessage($"found {installations.Count} Git installation(s).", level: TracerLevel.Diagnostic);

            return installations.Count > 0;
        }

        public bool FindHomePath(Environment environment, out string homePath)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            try
            {
                Environment.Variable homeVar;
                // Check to see if the user has a $home environment variable set, if they do use
                // it and we're done.
                if (environment.TryGet(Environment.Key.Home, out homeVar))
                {
                    homePath = homeVar.Value;
                    homePath = FileSystem.CanonicalizePath(homePath);
                    homePath = _context.PathHelper.ToLocalPath(homePath);
                    return true;
                }
                else
                {
                    Environment.Variable homeDriveVar;
                    Environment.Variable homePathVar;
                    // The user doesn't have a %Home% environment variable set, but they could
                    // have %HomeDrive% and %HomePath% set. If they have combine the two values
                    // into %Home%.
                    if (environment.TryGet(Environment.Key.HomeDrive, out homeDriveVar)
                        && homeDriveVar.Value != null
                        && environment.TryGet(Environment.Key.HomePath, out homePathVar)
                        && homePathVar.Value != null)
                    {
                        // Verify that the homePathVar.Value isn't invalid, it should never be
                        // but safety is a good practice.
                        if (homeDriveVar.Value.Length > 0 && homePathVar.Value.Length > 0)
                        {
                            // Combine the value of HomeDrive and HomePath. Check to see if HomePath
                            // begins with, or if HomeDrive ends with, a directory separator character.
                            // If they do combine them directly, otherwise combine and insert a separator.
                            if (homePathVar.Value[0] == Path.DirectorySeparatorChar
                                || homePathVar.Value[0] == Path.AltDirectorySeparatorChar
                                || homeDriveVar.Value[homeDriveVar.Value.Length - 1] == Path.DirectorySeparatorChar
                                || homeDriveVar.Value[homeDriveVar.Value.Length - 1] == Path.AltDirectorySeparatorChar)
                            {
                                homePath = string.Concat(homeDriveVar.Value, homePathVar.Value);
                            }
                            else
                            {
                                homePath = string.Concat(homeDriveVar.Value, Path.DirectorySeparatorChar, homePathVar.Value);
                            }

                            // Validate that the path exists, before returning with success.
                            if (!string.IsNullOrWhiteSpace(homePath))
                            {
                                homePath = FileSystem.CanonicalizePath(homePath);
                                homePath = _context.PathHelper.ToLocalPath(homePath);

                                if (FileSystem.DirectoryExists(homePath))
                                    return true;
                            }
                        }
                    }

                    // The user didn't have a useful / meaningful way to construct %Home%,
                    // therefore we just fall back to %UserProfile%.
                    Environment.Variable variable;
                    if (environment.TryGet(Environment.Key.WindowsUserProfile, out variable)
                        && !string.IsNullOrEmpty(variable.Value))
                    {
                        homePath = variable.Value;
                        homePath = FileSystem.CanonicalizePath(homePath);
                        homePath = _context.PathHelper.ToLocalPath(homePath);
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                _context.Tracer.TraceException(exception, TracerLevel.Diagnostic);
            }

            homePath = null;
            return false;
        }

        public bool FindHomePath(out string homePath)
            => FindHomePath(_environment, out homePath);

        public bool GitGlobalConfig(Environment environment, out string path)
        {
            const string GlobalConfigFileName = ".gitconfig";

            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            string homePath;
            if (FindHomePath(environment, out homePath))
            {
                string globalPath = Path.Combine(homePath, GlobalConfigFileName);
                globalPath = FileSystem.CanonicalizePath(globalPath);

                if (FileSystem.FileExists(globalPath))
                {
                    path = _context.PathHelper.ToLocalPath(globalPath);
                    return true;
                }
            }

            path = null;
            return false;
        }

        public bool GitGlobalConfig(out string path)
            => GitGlobalConfig(_context.EnvironmentCreate(), out path);

        public bool GitLocalConfig(string startingDirectory, out string path)
        {
            // Extensive experimentation with "git rev-parse --show-toplevel" shows that, as of
            // Git for Windows version 2.12.2.windows.2, git.exe's logic for finding the local
            // config is as follows (all valid usages I tried, including repos with
            // "--separate-git-dir", submodules, multiple worktrees, etc., fit this logic):
            //
            // 1. <gitDir> is a valid "git repository directory" if the following steps succeed:
            // - <gitDir> contains a file "HEAD" whose first characters are "ref:" and the
            //   following characters after skipping blanks are "refs/";
            // - find <commonDir> (see
            //   https://git-scm.com/docs/gitrepository-layout#gitrepository-layout-commondir, https://git-scm.com/docs/git#git-codeGITCOMMONDIRcode):
            // - if $GIT_COMMON_DIR if defined, use that for <commonDir> (relative to the current
            //   directory, but the behavior for relative paths is inconsistent in that it also
            //   attempts to use the ancestor containing the ".git" FSO if that was used to
            //   discover <gitDir> to find "objects" and "refs" - this inconsistency appears to
            //   be a bug);
            // - else if contains "commondir" FSO, expect it to be a file, read it and use its
            //   complete contents (minus the trailing line ends) as the (relative to that file's
            // directory) path to a directory <commonDir>;
            // - else, <commonDir> = <gitDir>.
            // - <commonDir> contains "objects" FSO and "refs" FSO.
            //
            // 2. Find <gitDir> (see https://git-scm.com/docs/git#git-codeGITDIRcode):
            // - if $GIT_DIR is defined, expect that to be the (relative to the current
            // directory) path to a valid "git repository directory" <gitDir>;
            // - else starting with the current directory, find the first directory in the
            //   ancestor chain that:
            // - either contains a ".git" file (in which case, expect that file to contain a
            //   valid "gitdir: <gitDir>" specification and <gitDir> to be the (relative to that
            //   file's directory) path to a valid "git repository directory")
            // - or contains a ".git" subdirectory that is a valid "git repository directory" (in
            //   which case, <gitDir> = that ".git" subdirectory)
            //
            // 3. Local config is "<the found commonDir>\config".

            const string DotGitName = ".git";
            const string LocalConfigFileName = "config";

            path = null;

            if (string.IsNullOrWhiteSpace(startingDirectory))
                return false;

            Environment.Variable variable;
            var env = _context.Git.GetProcessEnvironment(startingDirectory);
            var envCommonDir = env.TryGet(Environment.Key.GitCommonDirPath, out variable) ? variable.Value : null;
            var envGitDir = env.TryGet(Environment.Key.GitCommonDirPath, out variable) ? variable.Value : null;
            var dir = FileSystem.CanonicalizePath(startingDirectory);
            string commonDirPath = null;

            // 2. Find <gitDir>
            if (!string.IsNullOrWhiteSpace(envGitDir))
            {
                if (!IsValidGitDir(GetFullPath(startingDirectory, envGitDir), startingDirectory, envCommonDir, out commonDirPath))
                    return false;
            }
            else
            {
                while (dir != null && FileSystem.DirectoryExists(dir))
                {
                    var dotGitPath = Path.Combine(dir, DotGitName);

                    if (FileSystem.FileExists(dotGitPath))
                    {
                        // If file exists, use that file whether it gives a valid git repo with
                        // config or not. If anything goes wrong, return false.
                        using (var stream = FileSystem.OpenFile(dotGitPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var reader = new StreamReader(stream))
                        {
                            var content = reader.ReadToEnd();

                            lock (DotGitFileRegex)
                            {
                                Match match;
                                if (!((match = DotGitFileRegex.Value.Match(content)).Success && match.Groups.Count > 1))
                                    return false;

                                if (!IsValidGitDir(GetFullPath(dir, match.Groups[1].Value.Replace('/', '\\')), startingDirectory, envCommonDir, out commonDirPath))
                                    return false;

                                // Found a valid commonDir, break out of the loop
                                break;
                            }
                        }
                    }
                    else if (FileSystem.DirectoryExists(dotGitPath))
                    {
                        string result;
                        // only break out of the loop if this is a valid repo
                        if (IsValidGitDir(dotGitPath, startingDirectory, envCommonDir, out result))
                        {
                            commonDirPath = result;
                            break;
                        }
                    }

                    dir = FileSystem.GetParentPath(dir);
                }
            }

            if (commonDirPath == null)
                return false;

            // 3. Local config is "<the found commonDir>\config". By this point, we have a valid
            // commonDirPath. Build and canonicalize (in case there are ".." instances) the
            // config file's path
            var commonConfig = Path.Combine(commonDirPath, LocalConfigFileName);
            if (!FileSystem.FileExists(commonConfig))
                return false;

            commonConfig = FileSystem.CanonicalizePath(commonConfig);

            path = _context.PathHelper.ToLocalPath(commonConfig);
            return true;
        }

        public bool GitLocalConfig(out string path)
            => GitLocalConfig(System.Environment.CurrentDirectory, out path);

        public bool GitPortableConfig(out string path)
        {
            const string PortableConfigFolder = "Git";
            const string PortableConfigFileName = "config";

            path = null;

            Environment.Variable variable;
            if (_environment.TryGet(Environment.Key.WindowsProgramData, out variable)
                && !string.IsNullOrEmpty(variable.Value))
            {
                string appdataPath = variable.Value;
                string portableConfigPath = Path.Combine(appdataPath, PortableConfigFolder, PortableConfigFileName);
                portableConfigPath = FileSystem.CanonicalizePath(portableConfigPath);

                if (FileSystem.FileExists(portableConfigPath))
                {
                    path = _context.PathHelper.ToLocalPath(portableConfigPath);
                }
            }

            return path != null;
        }

        public bool GitSystemConfig(out string path)
        {
            List<Installation> installations;
            // Find Git on the local disk - the system config is stored relative to it
            if (FindGitInstallations(out installations))
            {
                if (GitSystemConfig(installations[0], out path))
                {
                    path = FileSystem.CanonicalizePath(path);
                    path = _context.PathHelper.ToLocalPath(path);
                    return true;
                }
            }

            path = null;
            return false;
        }

        public bool GitSystemConfig(Installation installation, out string path)
        {
            if (Installation.IsValid(_context, installation))
            {
                string gitSystemConfigPath = installation.Config;
                gitSystemConfigPath = FileSystem.CanonicalizePath(gitSystemConfigPath);

                if (FileSystem.FileExists(gitSystemConfigPath))
                {
                    path = _context.PathHelper.ToLocalPath(gitSystemConfigPath);
                    return true;
                }
            }

            path = null;
            return false;
        }

        public bool GitXdgConfig(out string path)
        {
            const string XdgConfigFolder = "Git";
            const string XdgConfigFileName = "config";

            string xdgConfigHome;
            string xdgConfigPath;

            // The XDG config home is defined by an environment variable.
            Environment.Variable variable;
            if (_environment.TryGet(Environment.Key.XdgConfigHome, out variable)
                && !string.IsNullOrEmpty(variable.Value))
            {
                xdgConfigHome = variable.Value;

                if (FileSystem.DirectoryExists(xdgConfigHome))
                {
                    xdgConfigPath = Path.Combine(xdgConfigHome, XdgConfigFolder, XdgConfigFileName);
                    xdgConfigPath = FileSystem.CanonicalizePath(xdgConfigPath);

                    if (FileSystem.FileExists(xdgConfigPath))
                    {
                        path = _context.PathHelper.ToLocalPath(xdgConfigPath);
                        return true;
                    }
                }
            }

            // Fall back to using the %AppData% folder, and try again.
            if (_environment.TryGet(Environment.Key.WindowsApplicationDataRoaming, out variable)
                && !string.IsNullOrEmpty(variable.Value))
            {
                xdgConfigHome = variable.Value;
                xdgConfigPath = Path.Combine(xdgConfigHome, XdgConfigFolder, XdgConfigFileName);
                xdgConfigPath = FileSystem.CanonicalizePath(xdgConfigPath);

                if (FileSystem.FileExists(xdgConfigPath))
                {
                    path = _context.PathHelper.ToLocalPath(xdgConfigPath);
                    return true;
                }
            }

            path = null;
            return false;
        }

        private static string GetFullPath(string directory, string content)
        {
            if (Path.IsPathRooted(content))
            {
                return content;
            }
            else
            {
                return Path.Combine(directory, content);
            }
        }

        private bool IsValidGitDir(string gitDirPath, string startingDirectory, string envCommonDir, out string commonDirPath)
        {
            // 1. <gitDir> is a valid "git repository directory" if the following steps succeed
            commonDirPath = null;

            if (!FileSystem.DirectoryExists(gitDirPath))
                return false;

            if (!FileSystem.FileExists(Path.Combine(gitDirPath, HeadFileName)))
                return false;

            if (!string.IsNullOrWhiteSpace(envCommonDir))
            {
                commonDirPath = GetFullPath(startingDirectory, envCommonDir);
            }
            else
            {
                var commonDirFilePath = Path.Combine(gitDirPath, CommonDirFileName);

                commonDirPath = gitDirPath;
                if (FileSystem.FileExists(commonDirFilePath))
                {
                    using (var reader = new StreamReader(commonDirFilePath))
                    {
                        var content = reader.ReadLine().Replace('/', '\\');
                        var localPath = GetFullPath(gitDirPath, content);

                        if (!FileSystem.DirectoryExists(localPath))
                            return false;

                        commonDirPath = localPath;
                    }
                }
            }

            return FileSystem.DirectoryExists(Path.Combine(commonDirPath, ObjectsDirName))
                && FileSystem.DirectoryExists(Path.Combine(commonDirPath, RefsDirName));
        }
    }
}
