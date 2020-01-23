//*************************************************************************************************
// StatusSnapshot.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of Git status.
    /// </summary>
    public interface IStatusSnapshot
    {
        /// <summary>
        /// Get information about the branch at the time this snapshot was taken.
        /// </summary>
        IStatusBranchInfo BranchInfo { get; }

        /// <summary>
        /// Gets a simple list of ignored pathnames from the collection.
        /// </summary>
        IReadOnlyList<string> IgnoredItems { get; }

        /// <summary>
        /// Gets a list of tracked entries which are either staged, unstaged, or both from the collection.
        /// </summary>
        IStatusTrackedEntries TrackedItems { get; }

        /// <summary>
        /// Gets a list of entries which failed to merge and are in a conflict state from the collection.
        /// </summary>
        IReadOnlyList<IStatusUnmergedEntry> UnmergedItems { get; }

        /// <summary>
        /// Gets a simple list of untracked pathnames from the collection.
        /// </summary>
        IReadOnlyList<string> UntrackedItems { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class StatusSnapshot : IStatusSnapshot
    {
        public StatusSnapshot()
        {
            const int InitialSize = 32;

            _branchInfo = new StatusBranchInfo();
            _ignoredItems = new List<string>(InitialSize);
            _trackedEntries = new StatusTrackedEntries();
            _unmergedEntries = new List<IStatusUnmergedEntry>(InitialSize);
            _untrackedItems = new List<string>(InitialSize);
        }

        internal StatusSnapshot(StatusBranchInfo branchInfo,
            List<string> ignoredItems,
            StatusTrackedEntries trackedEntries,
            List<IStatusUnmergedEntry> unmergedEntries,
            List<string> untrackedItems)
        {
            _branchInfo = branchInfo;
            _ignoredItems = ignoredItems;
            _trackedEntries = trackedEntries;
            _unmergedEntries = unmergedEntries;
            _untrackedItems = untrackedItems;
        }

        private readonly StatusBranchInfo _branchInfo;
        private readonly List<string> _ignoredItems;
        private readonly StatusTrackedEntries _trackedEntries;
        private readonly List<IStatusUnmergedEntry> _unmergedEntries;
        private readonly List<string> _untrackedItems;

        [JsonProperty]
        public IStatusBranchInfo BranchInfo
        {
            get { return _branchInfo; }
        }

        [JsonProperty]
        public IReadOnlyList<string> IgnoredItems
        {
            get { return _ignoredItems; }
        }

        [JsonProperty]
        public IStatusTrackedEntries TrackedItems
        {
            get { return _trackedEntries; }
        }

        [JsonProperty]
        public IReadOnlyList<IStatusUnmergedEntry> UnmergedItems
        {
            get { return _unmergedEntries; }
        }

        [JsonProperty]
        public IReadOnlyList<string> UntrackedItems
        {
            get { return _untrackedItems; }
        }

        internal void AddIgnoredItem(string item)
        {
            System.Diagnostics.Debug.Assert(item != null, $"`{nameof(item)}` parameter is null.");

            _ignoredItems.Add(item);
        }

        internal void AddUnmergedEntry(IStatusUnmergedEntry entry)
        {
            System.Diagnostics.Debug.Assert(entry != null, $"`{nameof(entry)}` parameter is null.");

            _unmergedEntries.Add(entry);
        }

        internal void AddUntrackedItem(string item)
        {
            System.Diagnostics.Debug.Assert(item != null, $"`{nameof(item)}` parameter is null.");

            _untrackedItems.Add(item);
        }
    }
}
