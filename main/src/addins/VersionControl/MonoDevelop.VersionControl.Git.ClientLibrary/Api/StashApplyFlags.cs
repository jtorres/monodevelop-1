//*************************************************************************************************
// StashApplyFlags.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum StashApplyFlags
    {
        None = 0,

        /// <summary>
        /// Tries to reinstate not only the working tree’s changes, but also the index’s ones.
        /// <para/>
        /// However, this can fail, when you have conflicts (which are stored in the index, where you
        /// therefore can no longer apply the changes as they were originally).
        /// </summary>
        ApplyIndex = 1 << 0,
    }
}
