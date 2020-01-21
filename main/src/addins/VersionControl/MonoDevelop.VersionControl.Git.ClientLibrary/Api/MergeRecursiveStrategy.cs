//*************************************************************************************************
// CherryPick.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************
namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Extened options related to `<seealso cref="MergeStrategyRecursive"/>`.
    /// </summary>
    public enum MergeRecursiveStrategy
    {
        None = 0,

        /// <summary>
        /// Forces conflicting hunks to be auto-resolved cleanly by favoring our version.
        /// <para/>
        /// Changes from the other tree that do not conflict with our side are reflected to the merge result.
        /// <para/>
        /// For a binary file, the entire contents are taken from our side.
        /// <para/>
        /// This should not be confused with the `<see cref="MergeStrategyOurs"/>` merge strategy, which does not even look at what the other tree contains at all.
        /// <para/>
        /// It discards everything the other tree did, declaring our history contains all that happened in it.
        /// </summary>
        Ours,

        /// <summary>
        /// This option forces conflicting hunks to be auto-resolved cleanly by favoring the other version.
        /// <para/>
        /// Changes from our tree that do not conflict with their side are reflected to the merge result.
        /// <para/>
        /// For a binary file, the entire contents are taken from our side.
        /// </summary>
        Theirs,
    }
}
