//*************************************************************************************************
// DifferenceOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.OpenDifferenceEngine(DifferenceOptions)"/>`.
    /// </summary>
    public struct DifferenceOptions
    {
        public static readonly DifferenceOptions Default = new DifferenceOptions
        {
            Algorithm = DifferenceAlgorithm.Default,
            Filters = DifferenceFilterFlags.Default,
            Format = DifferenceFormat.Default,
            IgnoreSubmodules = StatusIgnoreSubmodule.None,
            HintPath = null,
            RenameDetection = RenameDetection.Default,
            Whitespace = DifferenceWhitespace.Default,
        };

        /// <summary>
        /// The algorithm used to generate the difference.
        /// </summary>
        public DifferenceAlgorithm Algorithm;

        /// <summary>
        /// Restricts the types of differences reported.
        /// <para/>
        /// Any combination of the filter characters (including none) can be used.
        /// </summary>
        public DifferenceFilterFlags Filters;

        /// <summary>
        /// Determines the output format of the difference.
        /// </summary>
        public DifferenceFormat Format;

        /// <summary>
        /// Determines how submodules are handled and reported by the difference algorithm.
        /// </summary>
        public StatusIgnoreSubmodule IgnoreSubmodules;

        /// <summary>
        /// Path used to filter the difference.
        /// </summary>
        public string HintPath;

        /// <summary>
        /// Determines how rename detection is handled and reported by the difference algorithm.
        /// </summary>
        public RenameDetection RenameDetection;

        /// <summary>
        /// Determines how whitespace is handled by the difference algorithm.
        /// </summary>
        public DifferenceWhitespace Whitespace;
    }
}
