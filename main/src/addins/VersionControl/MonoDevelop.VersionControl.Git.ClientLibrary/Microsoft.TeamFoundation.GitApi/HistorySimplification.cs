//*************************************************************************************************
// HistorySimplification.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// History simplification options related to `<see cref="HistoryOptions"/>`.
    /// </summary>
    public enum HistorySimplification
    {
        /// <summary>
        /// Simplifies the history to the simplest history explaining the final state of the tree.
        /// <para/>
        /// Simplest because it prunes some side branches if the end result is the same (i.e. merging branches with the same content).
        /// </summary>
        Default = 0,

        /// <summary>
        /// Same as `<see cref="Default"/>`, but does not prune some history.
        /// </summary>
        FullHistory,

        /// <summary>
        /// Only the selected commits are shown, plus some to have a meaningful history.
        /// </summary>
        Dense,

        /// <summary>
        /// All commits in the simplified history are shown.
        /// </summary>
        Sparse,

        /// <summary>
        /// Additional option to `<see cref="FullHistory"/>` to remove some needless merges from the resulting history, as there are no selected commits contributing to this merge.
        /// </summary>
        SimplifyMerges,

        /// <summary>
        /// When given a range of commits to display, only display commits that exist directly on the ancestry chain between the commit1 and commit2.
        /// <para/>
        /// Example: commit1..commit2 or commit2 ^commit1 => commits that are both descendants of commit1, and ancestors of commit2.
        /// </summary>
        AncestryPath,
    }
}
