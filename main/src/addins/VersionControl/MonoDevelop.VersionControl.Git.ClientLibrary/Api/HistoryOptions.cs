//*************************************************************************************************
// HistoryOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.EnumerateCommits(IRevision, HistoryOptions)"/>`.
    /// </summary>
    public struct HistoryOptions
    {
        public static readonly HistoryOptions Default = new HistoryOptions
        {
            Flags = HistoryOptionFlags.None,
            HintPath = null,
            MaxCount = -1,
            Order = HistoryOrder.Default,
            Simplification = HistorySimplification.Default,
        };

        /// <summary>
        /// Options for changing the type of history to be enumerated.
        /// </summary>
        public HistoryOptionFlags Flags;

        /// <summary>
        /// Only commits modifying the path(s) are enumerated.
        /// </summary>
        public string HintPath;

        /// <summary>
        /// The maximum number of commits to enumerate.
        /// </summary>
        public int MaxCount;

        /// <summary>
        /// The desired order history should be presented in.
        /// </summary>
        public HistoryOrder Order;

        /// <summary>
        /// The desired amount of pruning and cleaning, or simplification to perform while enumerating history.
        /// </summary>
        public HistorySimplification Simplification;
    }
}
