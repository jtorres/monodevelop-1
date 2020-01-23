//*************************************************************************************************
// MergeRenormalizeOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Extended options for `<seealso cref="MergeOptions"/>`, `<seealso cref="CherryPickOptions"/>`, and `<seealso cref="RebaseOptions"/>`.
    /// </summary>
    public enum MergeRecursiveRenormalize
    {
        None,

        /// <summary>
        /// This runs a virtual check-out and check-in of all three stages of a file when resolving 
        /// a three-way merge. This option is meant to be used when merging branches with different 
        /// clean filters or end-of-line normalization rules.
        /// </summary>
        DoRenormalize,

        /// <summary>
        /// Disables the re-normalize option. This overrides the merge.renormalize configuration variable.
        /// </summary>
        NoRenormalize,
    }
}
