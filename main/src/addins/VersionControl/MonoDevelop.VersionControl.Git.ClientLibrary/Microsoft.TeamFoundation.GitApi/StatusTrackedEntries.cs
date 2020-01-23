/**** Git Process Management Library ****
 *
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the ""Software""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/

using System.Collections;
using System.Collections.Generic;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// A list of tracked entries that have changes. This includes staged and unstaged changes.
    /// </summary>
    public interface IStatusTrackedEntries : IReadOnlyList<IStatusEntry>
    {
        /// <summary>
        /// Gets the count of staged, added entries in the collection.
        /// </summary>
        int SumStagedAdded { get; }

        /// <summary>
        /// Gets the count of staged, deleted entries in the collection.
        /// </summary>
        int SumStagedDeleted { get; }

        /// <summary>
        /// Gets the count of staged, modified entries in the collection.
        /// </summary>
        int SumStagedModified { get; }

        /// <summary>
        /// Gets the count of staged, renamed entries in the collection.
        /// </summary>
        int SumStagedRenamed { get; }

        /// <summary>
        /// Gets the count of staged, unmodified entries in the collection.
        /// </summary>
        int SumStagedUnmodified { get; }

        /// <summary>
        /// Gets the count of unstaged, added entries in the collection.
        /// </summary>
        int SumUnstagedAdded { get; }

        /// <summary>
        /// Gets the count of unstaged, deleted entries in the collection.
        /// </summary>
        int SumUnstagedDeleted { get; }

        /// <summary>
        /// Gets the count of unstaged, modified entries in the collection.
        /// </summary>
        int SumUnstagedModified { get; }

        /// <summary>
        /// Gets the count of unstaged, unmodified entries in the collection.
        /// </summary>
        int SumUnstagedUnmodified { get; }
    }

    internal class StatusTrackedEntries : IStatusTrackedEntries
    {
        public StatusTrackedEntries()
        {
            _entries = new List<IStatusEntry>();
        }

        private List<IStatusEntry> _entries;
        private int _sumStagedAdded;
        private int _sumStagedDeleted;
        private int _sumStagedModified;
        private int _sumStagedRenamed;
        private int _sumStagedUnmodified;
        private int _sumUnstagedAdded;
        private int _sumUnstagedDeleted;
        private int _sumUnstagedModified;
        private int _sumUnstagedUnmodified;

        public IStatusEntry this[int index]
        {
            get { return _entries[index]; }
        }

        public int Count
        {
            get { return _entries.Count; }
        }

        public int SumStagedAdded
        {
            get { return _sumStagedAdded; }
        }

        public int SumStagedDeleted
        {
            get { return _sumStagedDeleted; }
        }

        public int SumStagedModified
        {
            get { return _sumStagedModified; }
        }

        public int SumStagedRenamed
        {
            get { return _sumStagedRenamed; }
        }

        public int SumStagedUnmodified
        {
            get { return _sumStagedUnmodified; }
        }

        public int SumUnstagedAdded
        {
            get { return _sumUnstagedAdded; }
        }

        public int SumUnstagedDeleted
        {
            get { return _sumUnstagedDeleted; }
        }

        public int SumUnstagedModified
        {
            get { return _sumUnstagedModified; }
        }

        public int SumUnstagedUnmodified
        {
            get { return _sumUnstagedUnmodified; }
        }

        public void Add(StatusEntry statusEntry)
        {
            switch (statusEntry.IndexStatus)
            {
                case TreeDifferenceType.Added:
                    _sumStagedAdded += 1;
                    break;

                case TreeDifferenceType.Deleted:
                    _sumStagedDeleted += 1;
                    break;

                case TreeDifferenceType.Modified:
                    _sumStagedModified += 1;
                    break;

                case TreeDifferenceType.Renamed:
                    _sumStagedRenamed += 1;
                    break;

                case TreeDifferenceType.Unmodified:
                    _sumStagedUnmodified += 1;
                    break;

                default: break;
            }

            switch (statusEntry.WorktreeStatus)
            {
                case TreeDifferenceType.Added:
                    _sumUnstagedAdded += 1;
                    break;

                case TreeDifferenceType.Deleted:
                    _sumUnstagedDeleted += 1;
                    break;

                case TreeDifferenceType.Modified:
                    _sumUnstagedModified += 1;
                    break;

                case TreeDifferenceType.Unmodified:
                    _sumUnstagedUnmodified += 1;
                    break;

                default: break;
            }

            if (!(statusEntry is StatusEntry value))
            {
                value = new StatusEntry(statusEntry.IndexStatus,
                                        statusEntry.WorktreeStatus,
                                        statusEntry.SubmoduleStatus,
                                        statusEntry.HeadMode,
                                        statusEntry.IndexMode,
                                        statusEntry.WorkTreeMode,
                                        statusEntry.HeadSha,
                                        statusEntry.IndexSha,
                                        (StringUtf8)statusEntry.Path);
            }

            _entries.Add(statusEntry);
        }

        public IEnumerator<IStatusEntry> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
