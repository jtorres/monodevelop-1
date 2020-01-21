//
// InstallationDetector.cs
//
// Author:
//       Mike Krüger <mikkrg@microsoft.com>
//
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    public class InstallationDetector
    {
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

        public static Installation CreateInstallation (string localPath, KnownDistribution distribution)
            => CreateInstallation (ExecutionContext.Current, localPath, distribution);

        public static Installation CreateInstallation (IExecutionContext context2, string _path, KnownDistribution distribution)
        {
            if (distribution == KnownDistribution.GitForOsX)
                return CreateInstallationForOsx(context2);
            var context = context2 as ExecutionContext;
            Debug.Assert (!string.IsNullOrWhiteSpace (_path), $"The `{nameof (_path)}` parameter is null or invalid.");
            Debug.Assert (Enum.IsDefined (typeof (KnownDistribution), distribution), $"The `{nameof (distribution)}` parameter is undefined.");
            Debug.Assert (CommonBinPaths.ContainsKey (distribution), $"The `{nameof (distribution)}` parameter not found in `{nameof (CommonBinPaths)}`.");
            Debug.Assert (CommonCmdPaths.ContainsKey (distribution), $"The `{nameof (distribution)}` parameter not found in `{nameof (CommonCmdPaths)}`.");
            Debug.Assert (CommonConfigPaths.ContainsKey (distribution), $"The `{nameof (distribution)}` parameter not found in `{nameof (CommonConfigPaths)}`.");
            Debug.Assert (CommonGitExePaths.ContainsKey (distribution), $"The `{nameof (distribution)}` parameter not found in `{nameof (CommonGitExePaths)}`.");
            Debug.Assert (CommonLibexecPaths.ContainsKey (distribution), $"The `{nameof (distribution)}` parameter not found in `{nameof (CommonLibexecPaths)}`.");
            Debug.Assert (CommonMsystemBinPaths.ContainsKey (distribution), $"The `{nameof (distribution)}` parameter not found in `{nameof (CommonMsystemBinPaths)}`.");
            Debug.Assert (CommonShExePaths.ContainsKey (distribution), $"The `{nameof (distribution)}` parameter is not found in `{nameof (CommonShExePaths)}`.");


            bool is64Bit = distribution == KnownDistribution.GitForWindows64v2 || distribution == KnownDistribution.MinGitForWindows64v2;
            bool isMinGit = distribution == KnownDistribution.MinGitForWindows32v2 || distribution == KnownDistribution.MinGitForWindows64v2;

            // trim off trailing '\' characters to increase compatibility
            _path = context.PathHelper.ToLocalPath (_path);

            string _bin = "";
            if (CommonBinPaths.TryGetValue (distribution, out var binPath)) {
                _bin = System.IO.Path.Combine (_path, binPath);
                _bin = context.FileSystem.CanonicalizePath (_bin);
                _bin = context.PathHelper.ToLocalPath (_bin);
            }

            string _cmd = "";
            if (isMinGit && CommonCmdPaths.TryGetValue (distribution, out var cmdPath)) {
                _cmd = System.IO.Path.Combine (_path, cmdPath);
                _cmd = context.FileSystem.CanonicalizePath (_cmd);
                _cmd = context.PathHelper.ToLocalPath (_cmd);
            }

            string _config = "";
            if (CommonConfigPaths.TryGetValue (distribution, out var cfgPath)) {
                _config = System.IO.Path.Combine (_path, cfgPath);
                _config = context.FileSystem.CanonicalizePath (_config);
                _config = context.PathHelper.ToLocalPath (_config);
            }

            string _exe = "";
            if (CommonGitExePaths.TryGetValue (distribution, out var exePath)) {
                _exe = System.IO.Path.Combine (_path, exePath);
                _exe = context.FileSystem.CanonicalizePath (_exe);
                _exe = context.PathHelper.ToLocalPath (_exe);
            }

            string _libexec = "";
            if (CommonLibexecPaths.TryGetValue (distribution, out var libExePath)) {
                _libexec = System.IO.Path.Combine (_path, libExePath);
                _libexec = context.FileSystem.CanonicalizePath (_libexec);
                _libexec = context.PathHelper.ToLocalPath (_libexec);
            }


            string _msysbin = "";
            if (CommonMsystemBinPaths.TryGetValue (distribution, out var msysBinPath)) {
                _msysbin = System.IO.Path.Combine (_path, msysBinPath);
                _msysbin = context.FileSystem.CanonicalizePath (_msysbin);
                _msysbin = context.PathHelper.ToLocalPath (_msysbin);
            }


            string _sh = "";
            if (!isMinGit
                && CommonShExePaths.TryGetValue (distribution, out var shPath)) {
                _sh = System.IO.Path.Combine (_path, shPath);
                _sh = context.FileSystem.CanonicalizePath (_sh);
                _sh = context.PathHelper.ToLocalPath (_sh);
            }

            var _usrbin = System.IO.Path.Combine (_path, AllVersionUsrPath, AllVersionBinPath);
            _usrbin = context.FileSystem.CanonicalizePath (_usrbin);
            _usrbin = context.PathHelper.ToLocalPath (_usrbin);

            return new Installation (
                context,
                _path,
                _bin,
                _cmd,
                _config,
                _exe,
                _libexec,
                _msysbin,
                _sh,
                _usrbin,
                Path.Combine(_libexec, "git-credential-manager.exe")
            ) {
                Is64Bit = is64Bit,
                IsMinGit = isMinGit
            };
        }

        public static KnownDistribution GetDistribution (IExecutionContext context, string path)
        {
            if (context.FileSystem.DirectoryExists (path)) {
                var git64v2path = System.IO.Path.Combine (path, Version2Libexec64Path);
                if (context.FileSystem.DirectoryExists (git64v2path))
                    return KnownDistribution.GitForWindows64v2;

                var git32v2path = System.IO.Path.Combine (path, Version2Libexec32Path);
                if (context.FileSystem.DirectoryExists (git32v2path))
                    return KnownDistribution.GitForWindows32v2;

                var git32v1path = System.IO.Path.Combine (path, Version1Libexec32Path);
                if (context.FileSystem.DirectoryExists (git32v1path))
                    return KnownDistribution.GitForWindows32v1;
            }

            return KnownDistribution.Unknown;
        }

        public static KnownDistribution GetDistribution (string path)
            => GetDistribution(ExecutionContext.Current, path);


        static readonly string[] lookupPaths = {
           "/usr/bin",
           "/usr/local/bin",
           "/opt/bin",
           "/opt/local/bin",
           "/usr/local/git/bin"
        };

        private static Installation CreateInstallationForOsx (IExecutionContext context)
        {
            string bin = "";
            string cmd = "";
            string exe = "";
            foreach (var lookupPath in lookupPaths) {
                var path = Path.Combine (lookupPath, "git");
                if (File.Exists (path)) {
                    bin = lookupPath;
                    cmd = lookupPath;
                    exe = path;
                    break;
                }

            }

            string libexe = bin;
            string credentialManagerExe = "";
            foreach (var lookupPath in lookupPaths) {
                var path = Path.Combine (lookupPath, "git-credential-osxkeychain");
                if (File.Exists (path)) {
                    libexe = lookupPath;
                    credentialManagerExe = path;
                    break;
                }

            }


            string config = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal), ".gitconfig");
            return new Installation (context,
                "/",
                bin: bin,
                cmd: cmd,
                config: config,
                exe: exe,
                libexe: libexe,
                msysbin: bin,
                sh: "/bin/sh",
                usrbin: "/usr/bin",
                credentialManagerExe: credentialManagerExe
            );
        }
    }

}
