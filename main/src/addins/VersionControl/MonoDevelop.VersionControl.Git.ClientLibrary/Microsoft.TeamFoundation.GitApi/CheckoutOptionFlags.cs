//*************************************************************************************************
// CheckoutOptionFlags.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum CheckoutOptionFlags
    {
        None = 0,

#if no
        /// <summary>
        /// Rather than checking out a branch to work on it, check out a commit for inspection and discardable experiments.
        /// <para/>
        /// This is the default behavior of `<see cref="Repository.CheckoutRevision(IRevision, CheckoutOptions, object)"/>` when `<see cref="IRevision"/>` is not a branch name.
        /// </summary>
        Detach = 1 << 0,
#endif
        /// <summary>
        /// When switching branches, proceed even if the index or the working tree differs from "HEAD".
        /// <para/>
        /// This is used to throw away local changes.
        /// <para/>
        /// When checking out paths from the index, do not fail upon unmerged entries; instead, unmerged entries are ignored.
        /// </summary>
        Force = 1 << 1,

        /// <summary>
        /// In sparse checkout mode, "Repository.CheckoutPaths" would update only entries matched by {paths} and sparse patterns in "$GIT_DIR/info/sparse-checkout".
        /// <para/>
        /// This option ignores the sparse patterns and adds back any files in {paths}.
        /// </summary>
        IgnoreSparse = 1 << 2,

        /// <summary>
        /// Git checkout refuses when the wanted ref is already checked out by another worktree.
        /// <para/>
        /// This option makes it check the ref out anyway. In other words, the ref can be held by more than one worktree.
        /// </summary>
        IgnoreWorktrees = 1 << 3,

        /// <summary>
        /// When switching branches, if you have local modifications to one or more files that are different between the current branch and the branch to which you are switching, the command refuses to switch branches in order to preserve your modifications in context.
        /// <para/>
        /// However, with this option, a three-way merge between the current branch, your working tree contents, and the new branch is done, and you will be on the new branch.
        /// <para/>
        /// When a merge conflict happens, the index entries for conflicting paths are left unmerged, and you need to resolve the conflicts and mark the resolved paths with git-add (or git-rm if the merge should result in deletion of the path).
        /// <para/>
        /// When checking out paths from the index, this option lets you recreate the conflicted merge in the specified paths.
        /// </summary>
        Merge = 1 << 4,

        /// <summary>
        /// When checking out paths from the index, check out stage #2 for unmerged paths.
        /// </summary>
        Ours = 1 << 5,

        /// <summary>
        /// When checking out paths from the index, check out stage #3 for unmerged paths.
        /// </summary>
        Theirs = 1 << 6,
    }
}
