//*************************************************************************************************
// DiffOptionFlags.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum DifferenceFilterFlags
    {
        None = 0,

        /// <summary>
        /// Includes added files when differencing two commits or trees.
        /// </summary>
        IncludeAdded = 1 << 0,

        /// <summary>
        /// Includes copied files when differencing two commits or trees.
        /// </summary>
        IncludeCopied = 1 << 1,

        /// <summary>
        /// Includes deleted files when differencing two commits or trees.
        /// </summary>
        IncludeDeleted = 1 << 2,

        /// <summary>
        /// Includes modified files when differencing two commits or trees.
        /// </summary>
        IncludeModified = 1 << 3,

        /// <summary>
        /// Includes renamed files when differencing two commits or trees.
        /// <para/>
        /// Renamed files are only reported if rename detection is enabled.
        /// </summary>
        IncludeRenamed = 1 << 4,

        /// <summary>
        /// Includes entries with mode changes when differencing two commits or trees.
        /// </summary>
        IncludeModeChanges = 1 << 5,

        /// <summary>
        /// Include unmerged changes when differencing two commits or trees.
        /// </summary>
        IncludeUnmerged = 1 << 6,

        /// <summary>
        /// Include unmerged files when differencing two commits or trees.
        /// </summary>
        IncludeUnknown = 1 << 7,

        /// <summary>
        /// Include files which have had their pairing broken when differencing two commits or trees.
        /// </summary>
        IncludePairBroken = 1 << 8,

        /// <summary>
        /// Complete set of all possible flags.
        /// </summary>
        All = IncludeAdded
            | IncludeCopied
            | IncludeDeleted
            | IncludeModeChanges
            | IncludeModified
            | IncludePairBroken
            | IncludeRenamed
            | IncludeUnknown
            | IncludeUnmerged,

        /// <summary>
        /// Default set of added, deleted, modified, and unmerged.
        /// </summary>
        Default = IncludeAdded
                | IncludeDeleted
                | IncludeModified
                | IncludeUnmerged,
    }
}
