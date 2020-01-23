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
    /// Object-model representation of a Git status entry of a probably copied, then added file.
    /// <para/>
    /// Copy detection only occurs once a file has been staged.
    /// </summary>
    public interface IStatusCopiedEntry: IStatusEntry
    {
        /// <summary>
        /// Get the confidence [0 to 100] that this entry was copied from `<see cref="OriginalPath"/>`.
        /// <para/>
        /// Since Git does not track files explicitly, and therefore does not explicitly track file copying,
        /// it must use heuristics to determine if an  "added" entry is actually a copy of another entry.
        /// </summary>
        int Confidence { get; }

        /// <summary>
        /// Gets the current path the entry was likely copied to, relative to the root of the worktree.
        /// </summary>
        string CurrentPath { get; }

        /// <summary>
        /// Gets the original path the entry was likely copied from, relative to the root of the worktree.
        /// </summary>
        string OriginalPath { get; }
    }

    internal class StatusCopiedEntry: StatusEntry, IStatusCopiedEntry
    {
        public StatusCopiedEntry(
            TreeDifferenceType stagedChange,
            TreeDifferenceType unstagedChange,
            StatusSubmoduleState submoduleState,
            TreeEntryDetailMode modeHead,
            TreeEntryDetailMode modeIndex,
            TreeEntryDetailMode modeWorktree,
            ObjectId shaHead,
            ObjectId shaIndex,
            StringUtf8 currentPath,
            StringUtf8 originalPath,
            int confidence)
            : base(stagedChange, unstagedChange, submoduleState, modeHead, modeIndex, modeWorktree, shaHead, shaIndex, currentPath)
        {
            if (ReferenceEquals(originalPath, null))
                throw new ArgumentNullException(nameof(originalPath));
            if (confidence < 0 || confidence > 100)
                throw new ArgumentOutOfRangeException(nameof(confidence));

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
        /// Gets the current path the entry was likely copied to, relative to the root of the worktree.
        /// </summary>
        internal StringUtf8 CurrentPathUtf8
            => PathUtf8;

        /// <summary>
        /// Gets the original path the entry was likely copied from, relative to the root of the worktree.
        /// </summary>
        internal StringUtf8 OriginalPathUtf8
        {
            get { return _originalPath; }
        }
    }
}
