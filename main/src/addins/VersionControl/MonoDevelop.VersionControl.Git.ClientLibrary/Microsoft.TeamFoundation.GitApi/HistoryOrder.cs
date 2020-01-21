//*************************************************************************************************
// HistoryOrder.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// History specific options related to `<see cref="HistoryOptions"/>`.
    /// <para/>
    /// Effect the ordering of commits returned by `<seealso cref="IRepository.EnumerateCommits(IRevision, HistoryOptions)"/>`.
    /// </summary>
    public enum HistoryOrder
    {
        /// <summary>
        /// By default, the commits are shown in reverse chronological order.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Show no parents before all of its children are shown, but otherwise show commits in the author time stamp order.
        /// <para/>
        /// Commits are sorted in chronological order, oldest to newest (reverse).
        /// </summary>
        AuthorDateOrderAscending,

        /// <summary>
        /// Show no parents before all of its children are shown, but otherwise show commits in the author time stamp order.
        /// <para/>
        /// Commits are sorted in chronological order, newest to oldest.
        /// </summary>

        AuthorDateOrderDecending,

        /// <summary>
        /// Show no parents before all of its children are shown, but otherwise show commits in the commit time stamp order.
        /// <para/>
        /// Commits are sorted in chronological order, oldest to newest (reverse).
        /// </summary>

        DateOrderAscending,

        /// <summary>
        /// Show no parents before all of its children are shown, but otherwise show commits in the commit time stamp order.
        /// <para/>
        /// Commits are sorted in chronological order, newest to oldest.
        /// </summary>


        DateOrderDecending,

        /// <summary>
        /// Show no parents before all of its children are shown, and avoid showing commits on multiple lines of history intermixed.
        /// <para/>
        /// Commits are sorted in chronological order, oldest to newest (reverse).
        /// </summary>

        TopographicalOrderAscending,

        /// <summary>
        /// Show no parents before all of its children are shown, and avoid showing commits on multiple lines of history intermixed.
        /// <para/>
        /// Commits are sorted in chronological order, newest to oldest.
        /// </summary>

        TopographicalOrderDecending,
    }
}
