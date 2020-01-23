//*************************************************************************************************
// CleanOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System.Collections.Generic;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.Clean(CleanOptions)"/>`.
    /// <para/>
    /// `<see cref="Flags"/>` must include `<see cref="CleanOptionFlags.RemoveIgnored"/>`, `<see cref="CleanOptionFlags.RemoveUntracked"/>`, or both; otherwise the value is invalid.
    /// <para/>
    /// Options can be validate with `<see cref="HasValidFlags()"/>`.
    /// </summary>
    public struct CleanOptions
    {
        /// <summary>
        /// Gets the default options for `<see cref="Cli.CleanCommand"/>`.
        /// </summary>
        public static readonly CleanOptions Default = new CleanOptions()
        {
            ExcludedPaths = null,
            Flags = CleanOptionFlags.RemoveUntracked,
            IncludedPaths = null,
        };

        /// <summary>
        /// Set of paths to be skipped when cleaning the working directory.
        /// <para/>
        /// This can override `<see cref="IncludedPaths"/>` by excluding a child of an included directory.
        /// </summary>
        public ICollection<string> ExcludedPaths;

        /// <summary>
        /// Extended options related to a Git clean operation.
        /// </summary>
        public CleanOptionFlags Flags;

        /// <summary>
        /// Set of paths to be included when cleaning the working directory.
        /// <para/>
        /// This can override `<see cref="ExcludedPaths"/>` by including a child of an excluded directory.
        /// </summary>
        public ICollection<string> IncludedPaths;

        /// <summary>
        /// Returns `<see langword="true"/>` if the flags are valid; otherwise `<see langword="false"/>`.
        /// </summary>
        public bool HasValidFlags()
        {
            return (Flags & CleanOptionFlags.RemoveIgnored) != 0
                || (Flags & CleanOptionFlags.RemoveUntracked) != 0;
        }
    }
}
