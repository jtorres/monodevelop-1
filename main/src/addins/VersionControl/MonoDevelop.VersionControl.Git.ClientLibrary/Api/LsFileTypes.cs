//*************************************************************************************************
// LsFileTypes.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum LsFileTypes
    {
        Default = 0,

        /// <summary>
        /// Show cached files in the output (default).
        /// </summary>
        Cached = 1 << 0,

        /// <summary>
        /// Show deleted files in the output.
        /// </summary>
        Deleted = 1 << 1,

        /// <summary>
        /// Show modified files in the output.
        /// </summary>
        Modified = 1 << 2,

        /// <summary>
        /// Show other (i.e. untracked) files in the output.
        /// </summary>
        Others = 1 << 3,

        /// <summary>
        /// Show only ignored files in the output. When showing files in the index, print only those matched by an exclude pattern.
        /// <para/>
        /// When showing "other" files, show only those matched by an exclude pattern.
        /// </summary>
        Ignored = 1 << 4,

        /// <summary>
        /// Show unmerged files in the output.
        /// </summary>
        Unmerged = 1 << 5,

        /// <summary>
        /// Show files on the file system that need to be removed due to file/directory conflicts for checkout-index to succeed.
        /// </summary>
        Killed = 1 << 6,
    }
}
