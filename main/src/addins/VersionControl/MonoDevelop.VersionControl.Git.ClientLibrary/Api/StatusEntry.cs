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
    /// Object-model representation of a Git status entry for a file in the worktree.
    /// </summary>
    public interface IStatusEntry
    {
        /// <summary>
        /// Gets the tree type (or mode) of this entry's HEAD representation.
        /// </summary>
        TreeEntryDetailMode HeadMode { get; }

        /// <summary>
        /// Gets the `<see cref="ObjectId"/>` of this entry's HEAD representation.
        /// </summary>
        ObjectId HeadSha { get; }

        /// <summary>
        /// Gets the tree type (or mode) of this entry's index representation.
        /// </summary>
        TreeEntryDetailMode IndexMode { get; }

        /// <summary>
        /// Gets the `<see cref="ObjectId"/>` of this entry's index representation.
        /// </summary>
        ObjectId IndexSha { get; }

        /// <summary>
        /// Gets the type of tree difference this entry represents.
        /// </summary>
        TreeDifferenceType IndexStatus { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if this entry is a staged add; otherwise `<see langword="false"/>`.
        /// </summary>
        bool IsStagedAdd { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if this entry is a staged delete; otherwise `<see langword="false"/>`.
        /// </summary>
        bool IsStagedDelete { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if this entry is a staged edit; otherwise `<see langword="false"/>`.
        /// </summary>
        bool IsStagedEdit { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if this entry is a staged rename; otherwise `<see langword="false"/>`.
        /// </summary>
        bool IsStagedRename { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if this entry is unmodified; otherwise `<see langword="false"/>`.
        /// </summary>
        bool IsStagedUnmodified { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if this entry is an unstaged delete; otherwise `<see langword="false"/>`.
        /// </summary>
        bool IsUnstagedDelete { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if this entry is an unstaged edit; otherwise `<see langword="false"/>`.
        /// </summary>
        bool IsUnstagedEdit { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if this entry is unmodified; otherwise `<see langword="false"/>`.
        /// </summary>
        bool IsUnstagedUnmodified { get; }

        /// <summary>
        /// Gets the path, relative to the root of the worktree, of the entry.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if this entry represents a submodule with modified content; otherwise `<see langword="false"/>`.
        /// </summary>
        bool SubmoduleHasModified { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if this entry represents a submodule with new commits; otherwise `<see langword="false"/>`.
        /// </summary>
        bool SubmoduleHasNewCommit { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if this entry represents a submodule with untracked changes; otherwise `<see langword="false"/>`.
        /// </summary>
        bool SubmoduleHasUntracked { get; }

        /// <summary>
        /// Gets the submodule state of this entry.
        /// </summary>
        StatusSubmoduleState SubmoduleStatus { get; }

        /// <summary>
        /// Gets the tree type (or mode) of this entry's worktree representation.
        /// </summary>
        TreeEntryDetailMode WorkTreeMode { get; }

        /// <summary>
        /// Gets the tree difference type of this entry's worktree status.
        /// </summary>
        TreeDifferenceType WorktreeStatus { get; }
    }

    internal class StatusEntry: IStatusEntry
    {
        public StatusEntry(
            TreeDifferenceType stagedChange,
            TreeDifferenceType unstagedChange,
            StatusSubmoduleState submoduleState,
            TreeEntryDetailMode headMode,
            TreeEntryDetailMode indexMode,
            TreeEntryDetailMode worktreeMode,
            ObjectId headSha,
            ObjectId indexSha,
            StringUtf8 path)
        {
            _headMode = headMode;
            _headSha = headSha;
            _indexStatus = stagedChange;
            _indexSha = indexSha;
            _indexMode = indexMode;
            _path = path;
            _submoduleStatus = submoduleState;
            _worktreeMode = worktreeMode;
            _worktreeStatus = unstagedChange;
        }

        private TreeEntryDetailMode _headMode;
        private ObjectId _headSha;
        private TreeEntryDetailMode _indexMode;
        private ObjectId _indexSha;
        private TreeDifferenceType _indexStatus;
        private StringUtf8 _path;
        private StatusSubmoduleState _submoduleStatus;
        private TreeEntryDetailMode _worktreeMode;
        private TreeDifferenceType _worktreeStatus;

        public TreeEntryDetailMode HeadMode
        {
            get { return _headMode; }
        }

        public ObjectId HeadSha
        {
            get { return _headSha; }
        }

        public TreeEntryDetailMode IndexMode
        {
            get { return _indexMode; }
        }

        public ObjectId IndexSha
        {
            get { return _indexSha; }
        }

        public TreeDifferenceType IndexStatus
        {
            get { return _indexStatus; }
        }

        public bool IsStagedAdd
        {
            get { return IndexStatus == TreeDifferenceType.Added; }
        }

        public bool IsStagedEdit
        {
            get { return IndexStatus == TreeDifferenceType.Modified; }
        }

        public bool IsStagedDelete
        {
            get { return IndexStatus == TreeDifferenceType.Deleted; }
        }

        public bool IsStagedRename
        {
            get { return IndexStatus == TreeDifferenceType.Renamed; }
        }

        public bool IsStagedUnmodified
        {
            get { return IndexStatus == TreeDifferenceType.Unmodified; }
        }

        public bool IsUnstagedEdit
        {
            get { return WorktreeStatus == TreeDifferenceType.Modified; }
        }

        public bool IsUnstagedDelete
        {
            get { return WorktreeStatus == TreeDifferenceType.Deleted; }
        }

        public bool IsUnstagedUnmodified
        {
            get { return WorktreeStatus == TreeDifferenceType.Unmodified; }
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

        public TreeEntryDetailMode WorkTreeMode
        {
            get { return _worktreeMode; }
        }

        public TreeDifferenceType WorktreeStatus
        {
            get { return _worktreeStatus; }
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
