//*************************************************************************************************
// MergeOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public struct MergeOptions
    {
        public static readonly MergeOptions Default = new MergeOptions
        {
            CommitFlags = MergeOptionCommitFlags.Default,
            ReReReFlags = MergeOptionReReReFlags.Default,
            FastForwardFlags = MergeOptionFastForwardFlags.Default,
            SquashFlags = MergeOptionSquashFlags.Default,
            Strategy = MergeStrategy.Default,
        };

        /// <summary>
        /// Controls commit creation after the merge operation has completed.
        /// </summary>
        public MergeOptionCommitFlags CommitFlags;

        /// <summary>
        /// Controls use of "rerere-autoupdate" features as part of the merge operation.
        /// </summary>
        public MergeOptionReReReFlags ReReReFlags;

        /// <summary>
        /// Controls the type of commit (single-parent, multi-parent) to create after the merge operation has completed.
        /// </summary>
        public MergeOptionFastForwardFlags FastForwardFlags;

        /// <summary>
        /// Controls squash behavior after the merge operation has completed.
        /// </summary>
        public MergeOptionSquashFlags SquashFlags;

        /// <summary>
        /// Controls the choice of merge strategy to be used during the merge operation.
        /// </summary>
        public MergeStrategy Strategy;
    }
}
