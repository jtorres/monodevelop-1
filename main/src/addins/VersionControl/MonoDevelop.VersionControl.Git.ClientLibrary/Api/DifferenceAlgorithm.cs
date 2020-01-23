//*************************************************************************************************
// DifferenceAlgorithm.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum DifferenceAlgorithm
    {
        Default = 0,

        /// <summary>
        /// Instead of trying to minimize the number of added/removed lines, first tries to match lines that are unique in the pre and post image, then filling the gaps using `<see cref="Myers"/>`.
        /// <para/>
        /// Use this when the branches to be merged have diverged wildly.
        /// <para/>
        /// Note: if there are multiple optimal difference, the reverse difference may be different from the result of the `<see cref="Myers"/>` difference when exchanging pre and post image.
        /// </summary>
        Patience,

        Minimal,

        /// <summary>
        /// Similar to `<see cref="Patience"/>`, tries to match near-unique lines in pre and post image before filling the gaps with the traditional `<see cref="Myers"/>` algorithm.
        /// </summary>
        Histogram,

        /// <summary>
        /// Determines an optimal difference by minimizing the added/removed lines, using a dynamic programming approach, trading time for space complexity.
        /// </summary>
        Myers,
    }
}
