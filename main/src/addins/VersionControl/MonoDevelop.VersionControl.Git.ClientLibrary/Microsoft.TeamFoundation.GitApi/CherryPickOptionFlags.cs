//*************************************************************************************************
// CherryPickOptionFlags.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Flags related to `<seealso cref="CherryPickOptions"/>`.
    /// </summary>
    [Flags]
    public enum CherryPickOptionFlags
    {
        None = 0,

        /// <summary>
        /// By default, cherry-picking an empty commit will fail.
        /// <para/>
        /// This option overrides that behavior, allowing empty commits to be preserved automatically in a cherry-pick.
        /// <para/>
        /// Note that when `<see cref="AllowFastFoward"/>` is in effect, empty commits that meet the "fast-forward" requirement will be kept even without this option.
        /// <para/>
        /// Note also, that use of this option only keeps commits that were initially empty (i.e. the commit recorded the same tree as its parent). Commits which are made empty due to a previous commit are dropped. To force the inclusion of those commits use `<see cref="KeepRedundantCommits"/>`.
        /// </summary>
        AllowEmptyCommits = 1 << 0,

        /// <summary>
        /// By default, cherry-picking a commit with an empty message will fail. This option overrides that behavior, allowing commits with empty messages to be cherry picked.
        /// </summary>
        AllowEmptyMessage = 1 << 1,

        /// <summary>
        /// If the current HEAD is the same as the parent of the cherry-picked commit, then a fast
        /// forward to this commit will be performed.
        /// </summary>
        AllowFastFoward = 1 << 2,

        /// <summary>
        /// When recording the commit, append a line that says "(cherry picked from commit ...ΓÇï)" to the original commit message in order to indicate which commit this change was cherry-picked from.
        /// <para/>
        /// This is done only for cherry picks without conflicts. Do not use this option if you are cherry-picking from your private branch because the information is useless to the recipient.
        /// <para/>
        /// If on the other hand you are cherry-picking between two publicly visible branches (e.g. back-porting a fix to a maintenance branch for an older release from a development branch), adding this information can be useful.
        /// </summary>
        AppendCherryPickedMessage = 1 << 3,

        /// <summary>
        /// If a commit being cherry picked duplicates a commit already in the current history, it will become empty.
        /// <para/>
        /// By default these redundant commits cause cherry-pick to stop so the user can examine the commit.
        /// <para/>
        /// This option overrides that behavior and creates an empty commit object.
        /// <para/>
        /// Implies `<see cref="CommitOptionFlags.AllowEmpty"/>`.
        /// </summary>
        KeepRedundantCommits = 1 << 4,
    }
}
