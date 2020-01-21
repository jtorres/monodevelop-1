//*************************************************************************************************
// Installation.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of an installation of Git (for Windows).
    /// </summary>
    [DebuggerDisplay("{Version} '{Path}'")]
    public sealed class Installation : Base, IEquatable<Installation>
    {
        public static readonly StringComparer PathComparer = StringComparer.OrdinalIgnoreCase;

        internal const string AllVersionBinPath = "bin";
        internal const string AllVersionEtcPath = "etc";
        internal const string AllVersionGitConfig = @"\gitconfig";
        internal const string AllVersionGitExe = @"\git.exe";
        internal const string AllVersionLibexecPath = @"libexec\git-core";
        internal const string AllVersionShExe = @"\sh.exe";
        internal const string AllVersionUsrPath = @"usr";
        internal const string FullVersionCmdPath = "cmd";
        internal const string FullVersionShExePath = AllVersionBinPath + AllVersionShExe;
        internal const string MinVersionCmdPath = null;
        internal const string MinVersionShExePath = null;
        internal const string Version1BinPath = AllVersionBinPath;
        internal const string Version2BinPath = @"usr\" + AllVersionBinPath;
        internal const string Version1Bin32Path = Version1BinPath;
        internal const string Version2Bin32Path = Version2Msystem32Path + @"\" + AllVersionBinPath;
        internal const string Version2Bin64Path = Version2Msystem64Path + @"\" + AllVersionBinPath;
        internal const string Version1Config32Path = Version1Etc32Path + AllVersionGitConfig;
        internal const string Version2Config32Path = Version2Etc32Path + AllVersionGitConfig;
        internal const string Version2Config64Path = Version2Etc64Path + AllVersionGitConfig;
        internal const string Version1Etc32Path = AllVersionEtcPath;
        internal const string Version2Etc32Path = Version2Msystem32Path + @"\" + AllVersionEtcPath;
        internal const string Version2Etc64Path = Version2Msystem64Path + @"\" + AllVersionEtcPath;
        internal const string Version1GitExe32Path = Version1Bin32Path + AllVersionGitExe;
        internal const string Version2GitExe32Path = Version2Bin32Path + AllVersionGitExe;
        internal const string Version2GitExe64Path = Version2Bin64Path + AllVersionGitExe;
        internal const string Version1Libexec32Path = AllVersionLibexecPath;
        internal const string Version2Libexec32Path = Version2Msystem32Path + @"\" + AllVersionLibexecPath;
        internal const string Version2Libexec64Path = Version2Msystem64Path + @"\" + AllVersionLibexecPath;
        internal const string Version2Msystem32Path = "mingw32";
        internal const string Version2Msystem64Path = "mingw64";

        internal static readonly InstallationComparer Comparer = new InstallationComparer();

        public static readonly IReadOnlyDictionary<KnownDistribution, string> CommonBinPaths
            = new Dictionary<KnownDistribution, string>
                {
                    { KnownDistribution.GitForWindows32v1, Version1Bin32Path },
                    { KnownDistribution.GitForWindows32v2, Version2Bin32Path },
                    { KnownDistribution.GitForWindows64v2, Version2Bin64Path },
                    { KnownDistribution.MinGitForWindows32v2, Version2Bin32Path },
                    { KnownDistribution.MinGitForWindows64v2, Version2Bin64Path },
                };
        public static readonly IReadOnlyDictionary<KnownDistribution, string> CommonCmdPaths
            = new Dictionary<KnownDistribution, string>
                {
                    { KnownDistribution.GitForWindows32v1, FullVersionCmdPath },
                    { KnownDistribution.GitForWindows32v2, FullVersionCmdPath },
                    { KnownDistribution.GitForWindows64v2, FullVersionCmdPath },
                    { KnownDistribution.MinGitForWindows32v2, MinVersionCmdPath },
                    { KnownDistribution.MinGitForWindows64v2, MinVersionCmdPath },
                };
        public static readonly IReadOnlyDictionary<KnownDistribution, string> CommonConfigPaths
            = new Dictionary<KnownDistribution, string>
                {
                    { KnownDistribution.GitForWindows32v1, Version1Config32Path },
                    { KnownDistribution.GitForWindows32v2, Version2Config32Path },
                    { KnownDistribution.GitForWindows64v2, Version2Config64Path },
                    { KnownDistribution.MinGitForWindows32v2, Version2Config32Path },
                    { KnownDistribution.MinGitForWindows64v2, Version2Config64Path },
                };
        public static readonly IReadOnlyDictionary<KnownDistribution, string> CommonGitExePaths
            = new Dictionary<KnownDistribution, string>
                {
                    { KnownDistribution.GitForWindows32v1, Version1GitExe32Path },
                    { KnownDistribution.GitForWindows32v2, Version2GitExe32Path },
                    { KnownDistribution.GitForWindows64v2, Version2GitExe64Path },
                    { KnownDistribution.MinGitForWindows32v2, Version2GitExe32Path },
                    { KnownDistribution.MinGitForWindows64v2, Version2GitExe64Path },
                };
        public static readonly IReadOnlyDictionary<KnownDistribution, string> CommonLibexecPaths
            = new Dictionary<KnownDistribution, string>
                {
                    { KnownDistribution.GitForWindows32v1, Version1Libexec32Path },
                    { KnownDistribution.GitForWindows32v2, Version2Libexec32Path },
                    { KnownDistribution.GitForWindows64v2, Version2Libexec64Path },
                    { KnownDistribution.MinGitForWindows32v2, Version2Libexec32Path },
                    { KnownDistribution.MinGitForWindows64v2, Version2Libexec64Path },
                };
        public static readonly IReadOnlyDictionary<KnownDistribution, string> CommonMsystemBinPaths
            = new Dictionary<KnownDistribution, string>
                {
                    { KnownDistribution.GitForWindows32v1, Version1Bin32Path },
                    { KnownDistribution.GitForWindows32v2, Version2Bin32Path },
                    { KnownDistribution.GitForWindows64v2, Version2Bin64Path },
                    { KnownDistribution.MinGitForWindows32v2, Version2Bin32Path },
                    { KnownDistribution.MinGitForWindows64v2, Version2Bin64Path },
                };
        public static readonly IReadOnlyDictionary<KnownDistribution, string> CommonShExePaths
            = new Dictionary<KnownDistribution, string>
                {
                    { KnownDistribution.GitForWindows32v1, FullVersionShExePath },
                    { KnownDistribution.GitForWindows32v2, FullVersionShExePath },
                    { KnownDistribution.GitForWindows64v2, FullVersionShExePath },
                    { KnownDistribution.MinGitForWindows32v2, MinVersionShExePath },
                    { KnownDistribution.MinGitForWindows64v2, MinVersionShExePath },
                };

        public Installation(IExecutionContext context, string localPath, KnownDistribution distribution)
            : base()
        {
            SetContext(context);

            Debug.Assert(!string.IsNullOrWhiteSpace(localPath), $"The `{nameof(localPath)}` parameter is null or invalid.");
            Debug.Assert(Enum.IsDefined(typeof(KnownDistribution), distribution), $"The `{nameof(distribution)}` parameter is undefined.");
            Debug.Assert(CommonBinPaths.ContainsKey(distribution), $"The `{nameof(distribution)}` parameter not found in `{nameof(CommonBinPaths)}`.");
            Debug.Assert(CommonCmdPaths.ContainsKey(distribution), $"The `{nameof(distribution)}` parameter not found in `{nameof(CommonCmdPaths)}`.");
            Debug.Assert(CommonConfigPaths.ContainsKey(distribution), $"The `{nameof(distribution)}` parameter not found in `{nameof(CommonConfigPaths)}`.");
            Debug.Assert(CommonGitExePaths.ContainsKey(distribution), $"The `{nameof(distribution)}` parameter not found in `{nameof(CommonGitExePaths)}`.");
            Debug.Assert(CommonLibexecPaths.ContainsKey(distribution), $"The `{nameof(distribution)}` parameter not found in `{nameof(CommonLibexecPaths)}`.");
            Debug.Assert(CommonMsystemBinPaths.ContainsKey(distribution), $"The `{nameof(distribution)}` parameter not found in `{nameof(CommonMsystemBinPaths)}`.");
            Debug.Assert(CommonShExePaths.ContainsKey(distribution), $"The `{nameof(distribution)}` parameter is not found in `{nameof(CommonShExePaths)}`.");

            // trim off trailing '\' characters to increase compatibility
            localPath = PathHelper.ToLocalPath(localPath);

            _path = localPath;
            _version = distribution;

            _bin = null;
            _cmd = null;
            _config = null;
            _exe = null;
            _libexec = null;
            _msysbin = null;
            _sh = null;
            _usrbin = null;
        }

        public Installation(string localPath, KnownDistribution distribution)
            : this(ExecutionContext.Current, localPath, distribution)
        { }

        /// <summary>
        /// Gets the local path to the installation's "bin/" folder.
        /// </summary>
        public string Bin
        {
            get
            {
                if (_config == null)
                {
                    string path;
                    if (_path != null
                        && CommonBinPaths.TryGetValue(Version, out path))
                    {
                        _bin = System.IO.Path.Combine(_path, path);
                        _bin = FileSystem.CanonicalizePath(_bin);
                        _bin = PathHelper.ToLocalPath(_bin);
                    }
                }
                return _bin;
            }
        }
        private string _bin;
        /// <summary>
        /// Gets the local path to the installation's "cmd/" folder.
        /// </summary>
        public string Cmd
        {
            get
            {
                if (_config == null)
                {
                    string path;
                    if (_path != null && !IsMinGit
                        && CommonCmdPaths.TryGetValue(Version, out path))
                    {
                        _cmd = System.IO.Path.Combine(_path, path);
                        _cmd = FileSystem.CanonicalizePath(_cmd);
                        _cmd = PathHelper.ToLocalPath(_cmd);
                    }
                }
                return _cmd;
            }
        }
        private string _cmd;
        /// <summary>
        /// Gets the local path of the installation's system config file.
        /// </summary>
        public string Config
        {
            get
            {
                if (_config == null)
                {
                    string path;
                    if (_path != null
                        && CommonConfigPaths.TryGetValue(Version, out path))
                    {
                        _config = System.IO.Path.Combine(_path, path);
                        _config = FileSystem.CanonicalizePath(_config);
                        _config = PathHelper.ToLocalPath(_config);
                    }
                }
                return _config;
            }
        }
        private string _config;
        /// <summary>
        /// Gets the local path of the installation's "git.exe" file.
        /// </summary>
        public string Exe
        {
            get
            {
                if (_exe == null)
                {
                    string path;
                    if (_path != null
                        && CommonGitExePaths.TryGetValue(Version, out path))
                    {
                        _exe = System.IO.Path.Combine(_path, path);
                        _exe = FileSystem.CanonicalizePath(_exe);
                        _exe = PathHelper.ToLocalPath(_exe);
                    }
                }
                return _exe;
            }
        }
        private string _exe;
        /// <summary>
        /// Gets `<see langword="true"/>` if the installation is 64-bit; otherwise `<see langword="false"/>`
        /// </summary>
        public bool Is64Bit
        {
            get
            {
                return _version == KnownDistribution.GitForWindows64v2
                    || _version == KnownDistribution.MinGitForWindows64v2;
            }
        }
        /// <summary>
        /// Gets <see langword="true"/> if the installation is the minimal version of Git;
        /// otherwise <see langword="false"/>
        /// </summary>
        public bool IsMinGit
        {
            get
            {
                return _version == KnownDistribution.MinGitForWindows32v2
                    || _version == KnownDistribution.MinGitForWindows64v2;
            }
        }
        /// <summary>
        /// Gets the local path of the installation's "\libexec" folder.
        /// </summary>
        public string Libexec
        {
            get
            {
                if (_libexec == null)
                {
                    string path;
                    if (_path != null
                        && CommonLibexecPaths.TryGetValue(Version, out path))
                    {
                        _libexec = System.IO.Path.Combine(_path, path);
                        _libexec = FileSystem.CanonicalizePath(_libexec);
                        _libexec = PathHelper.ToLocalPath(_libexec);
                    }
                }
                return _libexec;
            }
        }
        private string _libexec;
        /// <summary>
        /// Gets the local path of the installations Msystem "\bin" folder.
        /// </summary>
        public string MsysBin
        {
            get
            {
                if (_msysbin == null)
                {
                    string path;
                    if (_path != null
                        && CommonMsystemBinPaths.TryGetValue(Version, out path))
                    {
                        _msysbin = System.IO.Path.Combine(_path, path);
                        _msysbin = FileSystem.CanonicalizePath(_msysbin);
                        _msysbin = PathHelper.ToLocalPath(_msysbin);
                    }
                }
                return _msysbin;
            }
        }
        private string _msysbin;
        /// <summary>
        /// Gets the local path of the root of the installation.
        /// </summary>
        public string Path
        {
            get { return _path; }
        }
        private readonly string _path;
        /// <summary>
        /// Gets the local path of the installation's "sh.exe" file.
        /// </summary>
        public string Sh
        {
            get
            {
                if (_sh == null)
                {
                    string path;
                    if (_path != null && !IsMinGit
                        && CommonShExePaths.TryGetValue(Version, out path))
                    {
                        _sh = System.IO.Path.Combine(_path, path);
                        _sh = FileSystem.CanonicalizePath(_sh);
                        _sh = PathHelper.ToLocalPath(_sh);
                    }
                }
                return _sh;
            }
        }
        private string _sh;
        /// <summary>
        /// Gets the path to the installation's "usr/bin/" folder.
        /// </summary>
        public string UsrBin
        {
            get
            {
                if (_usrbin == null)
                {
                    if(_path!=null)
                    {
                        _usrbin = System.IO.Path.Combine(_path, AllVersionUsrPath, AllVersionBinPath);
                        _usrbin = FileSystem.CanonicalizePath(_usrbin);
                        _usrbin = PathHelper.ToLocalPath(_usrbin);
                    }
                }
                return _usrbin;
            }
        }
        private string _usrbin;
        /// <summary>
        /// The distribution of Git (for Windows) of the installation.
        /// </summary>
        public KnownDistribution Version
        {
            get { return _version; }
        }
        private readonly KnownDistribution _version;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as Installation)
                || base.Equals(obj);
        }

        public bool Equals(Installation other)
            => Comparer.Equals(this, other);

        public static KnownDistribution GetDistribution(IExecutionContext context, string path)
        {
            if (context.FileSystem.DirectoryExists(path))
            {
                var git64v2path = System.IO.Path.Combine(path, Version2Libexec64Path);
                if (context.FileSystem.DirectoryExists(git64v2path))
                    return KnownDistribution.GitForWindows64v2;

                var git32v2path = System.IO.Path.Combine(path, Version2Libexec32Path);
                if (context.FileSystem.DirectoryExists(git32v2path))
                    return KnownDistribution.GitForWindows32v2;

                var git32v1path = System.IO.Path.Combine(path, Version1Libexec32Path);
                if (context.FileSystem.DirectoryExists(git32v1path))
                    return KnownDistribution.GitForWindows32v1;
            }

            return KnownDistribution.Unknown;
        }

        public static KnownDistribution GetDistribution(string path)
            => GetDistribution(ExecutionContext.Current, path);

        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        internal static bool IsValid(IExecutionContext context, Installation value)
        {
            // test the value.Path value first, if it is null
            // no need to ask the type to generate the other
            // values needlessly
            return value != null
                && value.Path != null
                && value.Bin != null
                && value.Exe != null
                && value.Libexec != null
                && value.UsrBin != null
                && (value.IsMinGit || (value.Cmd != null && context.FileSystem.DirectoryExists(value.Cmd)))
                && (value.IsMinGit || (value.Sh != null && context.FileSystem.FileExists(value.Sh)))
                && context.FileSystem.DirectoryExists(value.Path)
                && context.FileSystem.DirectoryExists(value.Libexec)
                && context.FileSystem.FileExists(value.Exe)
                && context.FileSystem.DirectoryExists(value.UsrBin);
        }

        public static Installation Open(IExecutionContext context, string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (!context.FileSystem.DirectoryExists(path))
                throw new ArgumentException(nameof(path), new DirectoryNotFoundException(path));

            foreach (KnownDistribution version in Enum.GetValues(typeof(KnownDistribution)))
            {
                if (version == KnownDistribution.Unknown)
                    continue;

                Installation installation = new Installation(context, path, version);

                if (Installation.IsValid(context, installation))
                    return installation;
            }

            throw new InstallationNotFoundException(path);
        }

        public static Installation Open(string path)
        {
            var context = ExecutionContext.Current;

            return Open(context, path);
        }

        public override string ToString()
        {
            return $"{_version} {_path}";
        }
    }
}
