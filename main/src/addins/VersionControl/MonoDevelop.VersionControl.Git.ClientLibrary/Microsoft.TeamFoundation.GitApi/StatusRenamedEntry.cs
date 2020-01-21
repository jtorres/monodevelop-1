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

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git status renamed entry.
    /// <para/>
    /// Status entry of a probably renamed or moved, then added (both original and updated) file.
    /// <para/>
    /// Renamed detection only happens for staged add/deleted pairs.
    /// </summary>
    public interface IStatusRenamedEntry: IStatusEntry
    {
        /// <summary>
        /// Get the confidence [0 to 100] that this entry is a renamed entry from `<see cref="OriginalPath"/>`.
        /// <para/>
        /// Since Git does not track files explicitly, and therefore does not explicitly track file renaming,
        /// it must use heuristics to determine if an "added" entry is actually a rename of a "deleted" entry.
        /// <para/>
        /// This only staged changes can be reported as "renamed".
        /// </summary>
        int Confidence { get; }

        /// <summary>
        /// Gets the current path the entry was likely moved (renamed) to, relative to the root of the worktree.
        /// </summary>
        string CurrentPath { get; }

        /// <summary>
        /// Gets the original path the entry was likely moved (renamed) from, relative to the root of the worktree.
        /// </summary>
        string OriginalPath { get; }
    }

    internal class StatusRenamedEntry: StatusEntry, IStatusRenamedEntry
    {
        public StatusRenamedEntry(
            TreeDifferenceType stagedChange,
            TreeDifferenceType unstagedChange,
            StatusSubmoduleState submoduleChange,
            TreeEntryDetailMode modeHead,
            TreeEntryDetailMode modeIndex,
            TreeEntryDetailMode modeWorktree,
            ObjectId shaHead,
            ObjectId shaIndex,
            int confidence,
            StringUtf8 originalPath,
            StringUtf8 currentPath)
            : base(stagedChange, unstagedChange, submoduleChange, modeHead, modeIndex, modeWorktree, shaHead, shaIndex, currentPath)
        {
            if (confidence < 0 || confidence > 100)
                throw new ArgumentOutOfRangeException(nameof(confidence));
            if (originalPath is null)
                throw new ArgumentNullException(nameof(originalPath));

            _confidence = confidence;
            _originalPath = originalPath;

        }

        private int _confidence;
        private StringUtf8 _originalPath;

        public int Confidence
        {
            get { return _confidence; }
        }

        public string CurrentPath
            => Path;

        public string OriginalPath
        {
            get { return (string)_originalPath; }
        }

        /// <summary>
        /// Gets the current path the entry was likely moved (renamed) to, relative to the root of the worktree.
        /// </summary>
        internal StringUtf8 CurrentPathUtf8
            => PathUtf8;

        /// <summary>
        /// Gets the original path the entry was likely moved (renamed) from, relative to the root of the worktree.
        /// </summary>
        internal StringUtf8 OriginalPathUtf8
        {
            get { return _originalPath; }
        }
    }
}
