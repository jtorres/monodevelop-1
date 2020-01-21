//*************************************************************************************************
// CherryPickOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.CherryPick(InclusiveRange, CherryPickOptions)"/>`.
    /// <para/>
    /// Options related to `<see cref="IRepository.CherryPick(IRevision, CherryPickOptions)"/>`.
    /// </summary>
    public struct CherryPickOptions
    {
        public static readonly CherryPickOptions Default = new CherryPickOptions
        {
            Flags = CherryPickOptionFlags.None,
            MergeStrategy = MergeStrategy.Default,
        };

        /// <summary>
        /// Extended options related to a Git cherry-pick operation.
        /// </summary>
        public CherryPickOptionFlags Flags;

        /// <summary>
        /// The merge strategy to use when attempting to apply the cherry-picked commits.
        /// </summary>
        public MergeStrategy MergeStrategy;
    }
}
