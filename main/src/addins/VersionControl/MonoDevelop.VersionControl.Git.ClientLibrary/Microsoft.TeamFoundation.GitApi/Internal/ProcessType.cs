//*************************************************************************************************
// ProcessType.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal enum ProcessType
    {
        /// <summary>
        /// Unspecified process type, could be any Windows process
        /// </summary>
        Generic,
        /// <summary>
        /// Git for Windows "cmd/git.exe" process
        /// </summary>
        Git,
        /// <summary>
        /// Git for Windows "cmd/sh.exe" process
        /// </summary>
        Bash,
        /// <summary>
        /// (not used) Windows "System32/cmd.exe" process
        /// </summary>
        Cmd,
        /// <summary>
        /// (not used) Windows "System32\...\powershell.exe" process
        /// </summary>
        PowerShell,
    }
}
