//*************************************************************************************************
// CleanOptionFlags.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// <see cref="RemoveIgnored"/>, <see cref="RemoveUntracked"/>, or both flags must be present or the value is invalid.
    /// </summary>
    [Flags]
    public enum CleanOptionFlags
    {
        None = 0,

        /// <summary>
        /// Removes directories left empty from file removal.
        /// </summary>
        RemoveDirectories = 1 << 0,
        /// <summary>
        /// <para>Removes ignored files from the worktree.</para>
        /// <para><see cref="RemoveIgnored"/>, <see cref="RemoveUntracked"/>, or both flags must be present or the value is invalid.</para>
        /// </summary>
        RemoveIgnored = 1 << 1,
        /// <summary>
        /// <para>Removes untracked files from the worktree.</para>
        /// <para><see cref="RemoveIgnored"/>, <see cref="RemoveUntracked"/>, or both flags must be present or the value is invalid.</para>
        /// </summary>
        RemoveUntracked = 1 << 2,
    }
}
