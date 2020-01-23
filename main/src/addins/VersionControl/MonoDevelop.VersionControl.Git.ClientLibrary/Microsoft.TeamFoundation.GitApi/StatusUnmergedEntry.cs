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

using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git status unmerged/conflicted entry.
    /// <para/>
    /// Gets the tree entry type, or mode, of the entry's ancestor, or "stage 1" value.
    /// </summary>
    public interface IStatusUnmergedEntry
    {
        /// <summary>
        /// Gets the tree entry type, or mode, of the entry's ancestor, or "stage 1" value.
        /// </summary>
        TreeEntryDetailMode AncestorMode { get; }

        /// <summary>
        /// Gets the `<see cref="ObjectId"/>` of this entry's ancestor.
        /// </summary>
        ObjectId AncestorSha { get; }

        /// <summary>
        /// Gets the tree entry type, or mode, of the entry's ours, or "stage 2" value.
        /// </summary>
        TreeEntryDetailMode OursMode { get; }

        /// <summary>
        /// Gets the `<see cref="ObjectId"/>` of this entry's ours value.
        /// </summary>
        ObjectId OursSha { get; }

        /// <summary>
        /// Gets the path of the entry relative to the root of the worktree.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if the entry is a submodule which has been modified; otherwise `<see langword="false"/>`.
        /// </summary>
        bool SubmoduleHasModified { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if the entry is a submodule with new commits; otherwise `<see langword="false"/>`.
        /// </summary>
        bool SubmoduleHasNewCommit { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if the entry is a submodule which has untracked changes; otherwise `<see langword="false"/>`.
        /// </summary>
        bool SubmoduleHasUntracked { get; }

        /// <summary>
        /// Gets the submodule state of the entry if the entry is a submodule; otherwise `<see cref="StatusSubmoduleState.None"/>`.
        /// </summary>
        StatusSubmoduleState SubmoduleStatus { get; }

        /// <summary>
        /// Gets the tree entry type, or mode, of the entry's theirs, or "stage 3" value.
        /// </summary>
        TreeEntryDetailMode TheirsMode { get; }

        /// <summary>
        /// Gets the `<see cref="ObjectId"/>` of this entry's theirs value.
        /// </summary>
        ObjectId ThiersSha { get; }

        /// <summary>
        /// Gets the conflict state of the entry.
        /// </summary>
        StatusUnmergedState UnmergedState { get; }

        /// <summary>
        /// Gets the tree type (or mode) of this entry's worktree representation.
        /// </summary>
        TreeEntryDetailMode WorktreeMode { get; }
    }

    internal class StatusUnmergedEntry: IStatusUnmergedEntry
    {
        public StatusUnmergedEntry(
            StatusUnmergedState unmergedChange,
            StatusSubmoduleState submoduleState,
            TreeEntryDetailMode modeAncestor, /* stage 1 */
            TreeEntryDetailMode modeOurs,     /* stage 2 */
            TreeEntryDetailMode modeTheirs,   /* stage 3 */
            TreeEntryDetailMode modeWorktree,
            ObjectId shaAncestor,
            ObjectId shaOurs,
            ObjectId shaTheirs,
            StringUtf8 path)
        {
            _modeAncestor = modeAncestor;
            _modeOurs = modeOurs;
            _modeTheirs = modeTheirs;
            _modeWorktree = modeWorktree;
            _path = path;
            _shaAncestor = shaAncestor;
            _shaOurs = shaOurs;
            _shaTheirs = shaTheirs;
            _submoduleStatus = submoduleState;
            _unmergedState = unmergedChange;
        }

        private TreeEntryDetailMode _modeAncestor;
        private TreeEntryDetailMode _modeOurs;
        private TreeEntryDetailMode _modeTheirs;
        private TreeEntryDetailMode _modeWorktree;
        private StringUtf8 _path;
        private ObjectId _shaAncestor;
        private ObjectId _shaOurs;
        private ObjectId _shaTheirs;
        private StatusSubmoduleState _submoduleStatus;
        private StatusUnmergedState _unmergedState;

        public TreeEntryDetailMode AncestorMode
        {
            get { return _modeAncestor; }
        }

        public ObjectId AncestorSha
        {
            get { return _shaAncestor; }
        }

        public TreeEntryDetailMode OursMode
        {
            get { return _modeOurs; }
        }

        public ObjectId OursSha
        {
            get { return _shaOurs; }
        }

        public string Path
        {
            get { return (string)_path; }
        }

        public bool SubmoduleHasNewCommit
        {
            get { return ((SubmoduleStatus & StatusSubmoduleState.NewCommit) != 0); }
        }

        public bool SubmoduleHasModified
        {
            get { return ((SubmoduleStatus & StatusSubmoduleState.Modified) != 0); }
        }

        public bool SubmoduleHasUntracked
        {
            get { return ((SubmoduleStatus & StatusSubmoduleState.UntrackedChanges) != 0); }
        }

        public StatusSubmoduleState SubmoduleStatus
        {
            get { return _submoduleStatus; }
        }

        public ObjectId ThiersSha
        {
            get { return _shaTheirs; }
        }

        public TreeEntryDetailMode TheirsMode
        {
            get { return _modeTheirs; }
        }

        public StatusUnmergedState UnmergedState
        {
            get { return _unmergedState; }
        }

        public TreeEntryDetailMode WorktreeMode
        {
            get { return _modeWorktree; }
        }

        /// <summary>
        /// Gets the entry's path relative to the root of the worktree.
        /// </summary>
        internal StringUtf8 PathUtf8
        {
            get { return _path; }
        }
    }
}
