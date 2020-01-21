//*************************************************************************************************
// KnownDistribution.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum KnownDistribution
    {
        Unknown,

        /// <summary>
        /// 32-bit Git for Windows 1.*
        /// </summary>
        GitForWindows32v1,

        /// <summary>
        /// 32-bit Git for Windows 2.*
        /// </summary>
        GitForWindows32v2,

        /// <summary>
        /// 64-bit Git for Windows 2.*
        /// </summary>
        GitForWindows64v2,

        /// <summary>
        /// Minimum 32-bit Git for Windows' integrated development environments 2.*
        /// </summary>
        MinGitForWindows32v2,

        /// <summary>
        /// Minimum 64-bit Git for Windows' integrated development environments 2.*
        /// </summary>
        MinGitForWindows64v2,

        GitForOsX
    }
}
