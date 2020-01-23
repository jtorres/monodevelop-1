// PullOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.Push(IBranchName, IRemoteName, IBranchName, PushOptions)"/>`.
    /// </summary>
    public struct PullOptions
    {
        public static readonly PullOptions Default = new PullOptions
        {
            CommitFlags = PullOptionCommitFlags.Default,
            FastForwardFlags = PullOptionFastForwardFlags.Default,
            Flags = PullOptionFlags.None,
            ProgressCallback = null,
            SquashFlags = PullOptionSquashFlags.Default,
            SubmoduleRecursion = PullSubmoduleOptions.Default,
        };

        /// <summary>
        /// Controls commit creation after the merge operation has completed.
        /// </summary>
        public PullOptionCommitFlags CommitFlags;

        /// <summary>
        /// Controls the type of commit (single-parent, multi-parent) to create after the merge operation has completed.
        /// </summary>
        public PullOptionFastForwardFlags FastForwardFlags;

        /// <summary>
        /// Extended options related to a pull operation.
        /// </summary>
        public PullOptionFlags Flags;

        /// <summary>
        /// Callback delegate to receive progress updates during an operation.
        /// </summary>
        public OperationProgressDelegate ProgressCallback;

        /// <summary>
        /// Controls squash behavior after the merge operation has completed.
        /// </summary>
        public PullOptionSquashFlags SquashFlags;

        public PullSubmoduleOptions SubmoduleRecursion;
    }
}
