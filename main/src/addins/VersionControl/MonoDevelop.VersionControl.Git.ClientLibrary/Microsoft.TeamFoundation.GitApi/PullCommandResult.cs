//*************************************************************************************************
// PullCommandResult.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum PullCommandResult
    {
        Undefined = 0,

        /// <summary>
        /// The target branch is a child of the source branch.
        /// </summary>
        AlreadyUpToDate,

        /// <summary>
        /// The target branch is an ancestor of the source branch.
        /// </summary>
        FastForwardMerge,

        /// <summary>
        /// The target and source branches were merged.
        /// </summary>
        NonFastForwardMerge,

        /// <summary>
        /// Normal merge, but --no-commit requested.
        /// </summary>
        NonFastForwadMergeNoCommit,

        /// <summary>
        /// Merge stopped because of a conflict.
        /// </summary>
        Conflict,

        /// <summary>
        /// Pull Rebase
        /// </summary>
        Rebase,

        /// <summary>
        /// Pull Rebase with conflicts
        /// </summary>
        RebaseConflicts,

        // Note that we DO NOT have an ERROR result code.
    }

}
