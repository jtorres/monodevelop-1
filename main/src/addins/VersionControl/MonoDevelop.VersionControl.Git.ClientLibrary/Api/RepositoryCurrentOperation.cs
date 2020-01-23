//*************************************************************************************************
// RepositoryCurrentOperation.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of the current operation state of a Git repository.
    /// </summary>
    public enum RepositoryCurrentOperation
    {
        None = 0,

        /// <summary>
        /// Merge operation is currently underway.
        /// </summary>
        Merge = 1,

        /// <summary>
        /// Revert operation is currently underway.
        /// </summary>
        Revert = 2,

        /// <summary>
        /// Revert operation, of multiple revisions, is underway.
        /// </summary>
        RevertSequence = 3,

        /// <summary>
        /// Cherry-pick operation is underway.
        /// </summary>
        CherryPick = 4,

        /// <summary>
        /// Cherry-pick operation, of multiple revisions, is underway.
        /// </summary>
        CherryPickSequence = 5,

        /// <summary>
        /// Bisect operation is underway.
        /// </summary>
        Bisect = 6,

        /// <summary>
        /// Rebase operation is underway.
        /// </summary>
        Rebase = 7,

        /// <summary>
        /// Rebase operation, with interactivity, is underway.
        /// </summary>
        RebaseInteractive = 8,

        RebaseMerge = 9,

        ApplyMailbox = 10,

        ApplyMailboxOrRebase = 11,
    }
}
