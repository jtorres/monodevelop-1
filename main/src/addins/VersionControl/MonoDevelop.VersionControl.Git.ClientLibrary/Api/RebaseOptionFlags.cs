//*************************************************************************************************
// RebaseOptionFlags.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum RebaseOptionFlags
    {
        Default = 0,

        /// <summary>
        /// Use merging strategies to rebase.
        /// <para/>
        /// When `<see cref="MergeRecursiveStrategy"/>` is used, this allows rebase to be aware of
        /// renames on the upstream side.
        /// <para/>
        /// Note that a rebase merge works by replaying each commit from the working branch on top of the {upstream} branch.
        /// <para/>
        /// Because of this, when a merge conflict happens, the side reported as ours is the so-far rebased series, starting with {upstream}, and theirs is the working branch.
        /// <para/>
        /// In other words, the sides are swapped.
        /// </summary>
        Merge = 1 << 0,

        /// <summary>
        /// Keep the commits that do not change anything from its parents in the result.
        /// </summary>
        KeepEmpty = 1 << 1,

        /// <summary>
        /// Recreate merge commits instead of flattening the history by replaying commits a merge commit introduces.
        /// <para/>
        /// Merge conflict resolutions or manual amendments to merge commits are  not preserved.
        /// <para/>
        /// Cannot be combined used with `<see cref="Cli.RebaseCommand.BeginInteractiveRebase"/>` safely.
        /// </summary>
        PreserveMerges = 1 << 2,

        /// <summary>
        /// Use reflog to find a better common ancestor between {upstream} and {revision} when calculating which commits have been introduced by {revision}.
        /// <para/>
        /// Find the point at which a branch (or any history that leads to {revision}) forked from another branch (or any reference).
        /// <para/>
        /// This does not just look for the common ancestor of the two commits, but also takes into account the reflog of {revision} to see if the history leading to {revision} forked from an earlier incarnation of the branch.
        /// <para/>
        /// The {upstream} will be used as a fall-back.
        /// <para/>
        /// If either {upstream} or `<see cref="RebaseOptions.Root"/>` is given, then the default is `NoForkPoint`, otherwise this is the default.
        /// <para/>
        /// Not compatible with `NoForkPoint`.
        /// </summary>
        UseForkPoint = 1 << 3,
    }
}
