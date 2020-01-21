//*************************************************************************************************
// MergeOptionFlags.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum MergeOptionCommitFlags
    {
        Default = 0,

        /// <summary>
        /// Perform the merge and commit the result.
        /// </summary>
        CommitAfterMerge = 1,

        /// <summary>
        /// Perform the merge but do not commit the result.
        /// <para/>
        /// The successful merge results will be staged and in the worktree.
        /// <para/>
        /// Intended to give the user a chance to inspect and further tweak the merge result before committing.
        /// </summary>
        NoCommitAfterMerge = 2,
    }

    public enum MergeOptionReReReFlags
    {
        Default = 0,

        /// <summary>
        /// See "git merge --rerere-autoupdate"
        /// </summary>
        ReReReAutoUpdate = 1,

        /// <summary>
        /// See "git merge --no-rerere-autoupdate"
        /// </summary>
        NoReReReAutoUpdate = 2,
    }

    public enum MergeOptionFastForwardFlags
    {
        Default = 0,

        /// <summary>
        /// When the merge resolves as a fast-forward, only update the branch pointer, without creating a merge commit.
        /// <para/>
        /// This is the default behavior.
        /// </summary>
        FastForwardOrMerge = 1,

        /// <summary>
        /// Create a merge commit even when the merge resolves as a fast-forward.
        /// <para/>
        /// This is the default behavior when merging a tag.
        /// </summary>
        NoFastForward = 2,

        /// <summary>
        /// Refuse to merge and exit with a non-zero status unless the current HEAD is already up-to-date or the merge can be resolved as a fast-forward.
        /// </summary>
        FastForwardOnly = 3,
    }

    public enum MergeOptionSquashFlags
    {
        Default = 0,

        /// <summary>
        /// Produce the working tree and index state as if a real merge happened, but do not actually make a commit, move the HEAD.
        /// <para/>
        /// Allows the user to create a single commit on top of the current branch with the same result as merging another branch, without combining histories.
        /// </summary>
        Squash = 1,

        /// <summary>
        /// Perform the merge and commit the result.
        /// <para/>
        /// Provided as an override for user configured `branch.mergeoptions`.
        /// </summary>
        NoSquash = 2,
    }
}
