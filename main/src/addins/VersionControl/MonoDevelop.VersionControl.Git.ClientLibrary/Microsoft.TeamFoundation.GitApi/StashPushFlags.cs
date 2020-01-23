//*************************************************************************************************
// StashPushFlags.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum StashPushFlags
    {
        None = 0,

        /// <summary>
        /// Include untracked files in the stash.
        /// </summary>
        IncludeUntracked = 1 << 0,

        /// <summary>
        /// Include ignored files and untracked file in the stash.
        /// </summary>
        IncludeAll = 1 << 1,

        /// <summary>
        /// Changes already added to the index should be left intact.
        /// </summary>
        KeepIndex = 1 << 2,
    }
}
