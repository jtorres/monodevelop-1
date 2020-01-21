//*************************************************************************************************
// StashApplyOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.ApplyStash(int, StashApplyOptions)"/>`.
    /// </summary>
    public struct StashApplyOptions
    {
        public static readonly StashApplyOptions Default = new StashApplyOptions
        {
            Flags = StashApplyFlags.None,
        };

        /// <summary>
        /// Extended options related to a Git stash Apply operation.
        /// </summary>
        public StashApplyFlags Flags;

        /// <summary>
        /// Callback delegate to receive progress updates during an operation.
        /// </summary>
        public OperationProgressDelegate ProgressCallback;
    }
}
