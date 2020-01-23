//*************************************************************************************************
// Installation.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
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

        internal static readonly InstallationComparer Comparer = new InstallationComparer();

        internal Installation(IExecutionContext context, string localPath, string bin, string cmd, string config, string exe, string libexe, string msysbin, string sh, string usrbin, string credentialManagerExe)
        {
            _path = localPath;

            _bin = bin;
            _cmd = cmd;
            _config = config;
            _exe = exe;
            _libexec = libexe;
            _msysbin = msysbin;
            _sh = sh;
            _usrbin = usrbin;
            _credentialManagerExe = credentialManagerExe;
        }

        /// <summary>
        /// Gets the local path to the installation's "bin/" folder.
        /// </summary>
        public string Bin => _bin;
        private string _bin;
        /// <summary>
        /// Gets the local path to the installation's "cmd/" folder.
        /// </summary>
        public string Cmd => _cmd;
        private string _cmd;
        /// <summary>
        /// Gets the local path of the installation's system config file.
        /// </summary>
        public string Config => _config;
        private string _config;
        /// <summary>
        /// Gets the local path of the installation's "git.exe" file.
        /// </summary>
        public string Exe => _exe;
        private string _exe;
        /// <summary>
        /// Gets `<see langword="true"/>` if the installation is 64-bit; otherwise `<see langword="false"/>`
        /// </summary>
        public bool Is64Bit { get; internal set; }
        
        /// <summary>
        /// Gets <see langword="true"/> if the installation is the minimal version of Git;
        /// otherwise <see langword="false"/>
        /// </summary>
        public bool IsMinGit { get; internal set; }

        /// <summary>
        /// Gets the local path of the installation's "git-credential-manager.exe" file.
        /// </summary>
        public string CredentialManagerExe => _credentialManagerExe;
        private string _credentialManagerExe;

        /// <summary>
        /// Gets the local path of the installation's "\libexec" folder.
        /// </summary>
        public string Libexec => _libexec;
        private string _libexec;
        /// <summary>
        /// Gets the local path of the installations Msystem "\bin" folder.
        /// </summary>
        public string MsysBin => _msysbin;
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
        public string Sh => _sh;
        private string _sh;
        /// <summary>
        /// Gets the path to the installation's "usr/bin/" folder.
        /// </summary>
        public string UsrBin => _usrbin;
        private string _usrbin;

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


        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        internal static void CheckInstallation(IExecutionContext context, Installation installation)
        {
            if (installation == null)
                throw new ArgumentNullException(nameof(installation));
            if (installation.Path == null)
                throw new ArgumentNullException(nameof(installation.Path));
            if (installation.Bin == null)
                throw new ArgumentNullException(nameof(installation.Bin));
            if (installation.Exe == null)
                throw new ArgumentNullException(nameof(installation.Exe));
            if (installation.Libexec == null)
                throw new ArgumentNullException(nameof(installation.Libexec));
            if (installation.UsrBin == null)
                throw new ArgumentNullException(nameof(installation.UsrBin));
            if (installation.CredentialManagerExe == null)
                throw new ArgumentNullException(nameof(installation.CredentialManagerExe));

            if (!installation.IsMinGit && (installation.Cmd == null || !context.FileSystem.DirectoryExists(installation.Cmd)))
                throw new ArgumentException(nameof(installation.Cmd), $"Directory does not exist: {installation.Cmd}");
            if (!installation.IsMinGit && (installation.Sh == null || !context.FileSystem.FileExists(installation.Sh)))
                throw new ArgumentException(nameof(installation.Sh), $"File does not exist: {installation.Sh}");

            if (!context.FileSystem.DirectoryExists(installation.Path))
                throw new ArgumentException(nameof(installation.Path), $"Directory does not exist: {installation.Path}");
            if (!context.FileSystem.DirectoryExists(installation.Libexec))
                throw new ArgumentException(nameof(installation.Libexec), $"Directory does not exist: {installation.Libexec}");
            if (!context.FileSystem.FileExists(installation.Exe))
                throw new ArgumentException(nameof(installation.Exe), $"File does not exist: {installation.Exe}");
            if (!context.FileSystem.DirectoryExists(installation.UsrBin))
                throw new ArgumentException(nameof(installation.UsrBin), $"Directory does not exist: {installation.UsrBin}");
        }

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
                && value.CredentialManagerExe != null
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

                Installation installation = InstallationDetector.CreateInstallation(context, path, version);

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
            return $"{_path} {_cmd}";
        }
    }
}
