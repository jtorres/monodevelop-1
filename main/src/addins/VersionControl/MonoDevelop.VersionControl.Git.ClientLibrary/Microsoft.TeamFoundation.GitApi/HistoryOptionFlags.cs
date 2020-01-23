//*************************************************************************************************
// HistoryOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum HistoryOptionFlags
    {
        None = 0,

        /// <summary>
        /// Follow only the first parent commit upon seeing a merge commit.
        /// <para/>
        /// This option can give a better overview when viewing the evolution of a particular topic branch, because merges into a topic branch tend to be only about adjusting to updated upstream from time to time, and this option allows you to ignore the individual commits brought in to your history by such a merge.
        /// </summary>
        FirstParent = 1 << 0,

        /// <summary>
        /// Only consider commits with more than one parent.
        /// <para/>
        /// Incompatible with `<see cref="OmitMerges"/>`.
        /// </summary>
        OnlyMerges = 1 << 1,

        /// <summary>
        /// Skip consideration of commits with more than one parent.
        /// <para/>
        /// Incompatible with `<see cref="OnlyMerges"/>`.
        /// </summary>
        OmitMerges = 1 << 2,

        /// <summary>
        /// Omit any commit that introduces the same change as another commit on the "other side" when the set of commits are limited with symmetric difference.
        /// </summary>
        CherryPick = 1 << 3,

        /// <summary>
        /// List only commits on the left side of a symmetric range.
        /// <para/>
        /// Incompatible with `<see cref="RightOnly"/>`.
        /// </summary>
        LeftOnly = 1 << 4,

        /// <summary>
        /// List only commits on the right side of a symmetric range.
        /// <para/>
        /// Incompatible with `<see cref="LeftOnly"/>`.
        /// </summary>
        RightOnly = 1 << 5,

        /// <summary>
        /// Try to speed up the traversal using the pack bitmap index (if one is available).
        /// </summary>
        UseBitmapIndex = 1 << 6,

        /// <summary>
        /// Include values contained in the reflog as if they were explicit.
        /// <para/>
        /// Incomparable with `<see cref="WalkReflogs"/>`.
        /// </summary>
        IncludeReflog = 1 << 7,

        /// <summary>
        /// Instead of walking the commit ancestry chain, walk reflog entries from the most recent one to older ones.
        /// <para/>
        /// Incompatible with `<see cref="IncludeReflog"/>`.
        /// </summary>
        WalkReflogs = 1 << 8,
    }
}
