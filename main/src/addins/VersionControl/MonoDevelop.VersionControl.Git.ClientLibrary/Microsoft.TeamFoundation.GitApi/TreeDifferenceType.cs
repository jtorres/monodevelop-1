//*************************************************************************************************
// TreeDifferenceType.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************


namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Type of difference a `<see cref="ITreeDifferenceEntry"/>` represents.
    /// <para/>
    /// The value is always `<see cref="Merged"/>` if the entry has multiple sources.
    /// </summary>
    public enum TreeDifferenceType
    {
        /// <summary>
        /// "Unknown" change type (most probably a bug, please report it).
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// File added to repository.
        /// <para/>
        /// Could be an undetected renamed or copied file.
        /// <para/>
        /// See also: `<seealso cref="Copied"/>` and `<see cref="Renamed"/>`.
        /// </summary>
        Added = 1,

        /// <summary>
        /// Copy of a file into a new one.
        /// <para/>
        /// Since Git does not explicitly track files, and therefore does not explicitly track file copying,
        /// it must use heuristics to determine if an "added" entry is actually a copy of another entry.
        /// <para/>
        /// Not to be confused with `<see cref="Renamed"/>`.
        /// <para/>
        /// See also: `<seealso cref="ITreeDifferenceCopiedEntry.Confidence"/>`.
        /// </summary>
        /// <seealso cref="ITreeDifferenceCopiedEntry.Confidence"/>
        Copied = 2,

        /// <summary>
        /// File removed from repository.
        /// <para/>
        /// Could be an undetected renamed file.
        /// <para/>
        /// See also: `<see cref="Renamed"/>`.
        /// </summary>
        Deleted = 3,

        /// <summary>
        /// File ignored.
        /// </summary>
        Ignored = 4,

        /// <summary>
        /// File's contents, or mode, have been modified.
        /// </summary>
        Modified = 5,

        /// <summary>
        /// File's name was changed.
        /// <para/>
        /// Since Git does not explicitly track files, and therefore does not explicitly track file renaming,
        /// it must use heuristics to determine if an "added" entry is actually a rename of a "deleted" entry.
        /// <para/>
        /// Sometimes represented as `<see cref="Added"/>` + `<see cref="Deleted"/>`.
        /// <para/>
        /// Not to be confused with `<see cref="Copied"/>`.
        /// <para/>
        /// See also: `<seealso cref="ITreeDifferenceRenamedEntry.Confidence"/>`.
        /// </summary>
        /// <seealso cref="ITreeDifferenceRenamedEntry.Confidence"/>
        Renamed = 6,

        /// <summary>
        /// File's type changed.
        /// </summary>
        TypeChange = 7,

        /// <summary>
        /// File is unmerged, contains conflicts.
        /// </summary>
        Unmerged = 8,

        /// <summary>
        /// File is unchanged.
        /// </summary>
        Unmodified = 9,

        /// <summary>
        /// File is not tracked.
        /// </summary>
        Untracked = 10,

        /// <summary>
        /// File skipped due to skip-tree bits.
        /// </summary>
        Skipped = 11,

        /// <summary>
        /// File not skipped despite skip-tree bits.
        /// </summary>
        Unskipped = 12,

        /// <summary>
        /// Same as `<see cref="Copied"/>` without detailed information.
        /// <para/>
        /// Merge commits do not have detailed copied information.
        /// </summary>
        CopiedInParent = 13,

        /// <summary>
        /// Same as `<see cref="Renamed"/>` without detailed information.
        /// <para/>
        /// Merge commits do not have detailed rename information.
        /// </summary>
        RenamedInParent = 14,

        /// <summary>
        /// File is the result of a merge of two-or-more parents.
        /// </summary>
        Merged = 15,
    }
}
