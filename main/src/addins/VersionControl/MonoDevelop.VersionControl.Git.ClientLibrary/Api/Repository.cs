//*************************************************************************************************
// Repository.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    public interface IRepository : IDisposable, IEquatable<IRepository>
    {
        /// <summary>
        /// <para>Gets the path to the repository's common directory (usually <see cref="GitDirectory"/>).</para>
        /// <para>This value represents the path, but not guarantee the existence of the path.</para>
        /// </summary>
        string CommonDirectory { get; }

        /// <summary>
        /// Gets the <see cref="Environment"/> related to the repository.
        /// </summary>
        Environment Environment { get; }

        /// <summary>
        /// Gets the repository's .git/ directory (aka $GIT_DIR)
        /// </summary>
        string GitDirectory { get; }

        /// <summary>
        /// <para>Gets the path to the repository's index file (usually .git/index)</para>
        /// <para>This value represents the path, but not guarantee the existence of the path.</para>
        /// </summary>
        string IndexFile { get; }

        /// <summary>
        /// Gets <see langword="true"/> if the repository is bare; otherwise <see langword="false"/>.
        /// </summary>
        bool IsBare { get; }

        /// <summary>
        /// <para>Gets the root of the repository's object directory (usually .git/objects)</para>
        /// <para>This value represents the path, but not guarantee the existence of the path.</para>
        /// </summary>
        string ObjectsDirectory { get; }

        /// <summary>
        /// Gets or sets data attached to any trace messages sent from any commands on this repository instance.
        /// </summary>
        object UserData { get; set; }

        /// <summary>
        /// Gets a set of details related to repository.
        /// </summary>
        IRepositoryDetails RepositoryDetails { get; }

        /// <summary>
        /// <para>Gets the path to the repository's shared index file, if any (usually <see langword="null"/>).</para>
        /// <para>This value represents the path, but not guarantee the existence of the path.</para>
        /// </summary>
        string SharedIndexFile { get; }

        /// <summary>
        /// <para>Gets the repository's working directory (aka $WORK_DIR).</para>
        /// <para>This is initial set the path provided at <see cref="Repository"/> creation.</para>
        /// </summary>
        string WorkingDirectory { get; }

        /// <summary>
        /// <para>Adds, updates, or removes <paramref name="paths"/> from the index.</para>
        /// <para>The action taken is based on the state of the file in the worktree, if the state of
        /// the file does correspond to any of the supported <see cref="UpdateOptions.Flags"/> then no
        /// action will be taken.</para>
        /// </summary>
        /// <param name="paths">The repository based paths to be updated.</param>
        /// <param name="options">The options to be applied to the operation.</param>
        /// <returns>Collection of updated paths along with the type of action taken.</returns>
        IReadOnlyCollection<IUpdatedIndexEntry> Add(IEnumerable<string> paths, UpdateOptions options);

        /// <summary>
        /// Add a remote.
        /// </summary>
        /// <param name="url">Remote URL</param>
        /// <param name="name">Rename name</param>
        /// <param name="options">Remote tag options</param>
        void AddRemote(string url, string name, RemoteTagOptions options);

        /// <summary>
        /// Sets the upstream value of <paramref name="branch"/> to <paramref name="upstreamBranch"/>.
        /// </summary>
        /// <param name="branch">The local branch.</param>
        /// <param name="upstreamBranch">The upstream branch.</param>
        void BranchSetUpstream(IBranchName branch, IBranchName upstreamBranch);

        /// <summary>
        /// Sets the upstream value of <paramref name="branch"/> to <paramref name="upstreamBranch"/>.
        /// </summary>
        /// <param name="branch">The local branch.</param>
        /// <param name="upstreamRemote">The remote of the upstream branch.</param>
        /// <param name="upstreamBranch">The upstream branch.</param>
        void BranchSetUpstream(IBranchName branch, IRemoteName upstreamRemote, IBranchName upstreamBranch);

        /// <summary>
        /// Removes any upstream branch set for <paramref name="branch"/>.
        /// </summary>
        /// <param name="branch">The local branch.</param>
        void BranchUnsetUpstream(IBranchName branch);

        /// <summary>
        /// Rename a remote.
        /// </summary>
        /// <param name="name">Rename name</param>
        void RemoveRemote(string name);

        /// <summary>
        /// Rename a remote.
        /// </summary>
        void RenameRemote(string oldName, string newName);

        /// <summary>
        /// Set remote fetch URL.
        /// </summary>
        void SetRemoteFetchUrl(string name, string url);

        /// <summary>
        /// Set remote push URL.
        /// </summary>
        void SetRemotePushUrl(string name, string url);

        /// <summary>
        /// <para>Updates the working directory and HEAD to <paramref name="revision"/>.</para>
        /// <para>If <paramref name="revision"/> is not a <see cref="IBranch"/> and <paramref name="options"/>
        /// does not contain limiting paths, the repository will be put into "Detached HEAD State".</para>
        /// </summary>
        /// <param name="revision">The branch to update to.</param>
        /// <param name="options">Options for the checkout operation.</param>
        void Checkout(IRevision revision, CheckoutOptions options);

        /// <summary>
        /// <para>Copy all files from the index to the working tree.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-checkout-index.html</para>
        /// </summary>
        /// <param name="options">Checkout index options</param>
        void CheckoutIndex(CheckoutIndexOptions options);

        /// <summary>
        /// <para>Copy files from the index to the working tree.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-checkout-index.html</para>
        /// </summary>
        /// <param name="paths">File list</param>
        /// <param name="options">Checkout index options</param>
        void CheckoutIndex(IEnumerable<string> paths, CheckoutIndexOptions options);

        /// <summary>
        /// Applies the change, of a given commit, introduces, recording a new commit for each. This
        /// requires your working tree to be clean (no modifications from the HEAD commit).
        /// </summary>
        /// <param name="range">Commits to cherry-pick.</param>
        /// <param name="options">Options which affect how the cherry-pick operation is completed
        /// see <see cref="CherryPickOptions"/> for more information.</param>
        void CherryPick(IRevision revision, CherryPickOptions options);

        /// <summary>
        /// Given a range of existing commits, apply the change each one introduces, recording a new
        /// commit for each. This requires your working tree to be clean (no modifications from the
        /// HEAD commit).
        /// </summary>
        /// <param name="range">Commits to cherry-pick.</param>
        /// <param name="options">Options which affect how the cherry-pick operation is completed
        /// see <see cref="CherryPickOptions"/> for more information.</param>
        void CherryPick(InclusiveRange range, CherryPickOptions options);

        void CherryPickContinue();

        void CherryPickAbort();

        IEnumerable<IUpdatedWorktreeEntry> Clean(CleanOptions options);

        /// <summary>
        /// <para>Record changes to the repository.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-commit.html</para>
        /// </summary>
        ObjectId Commit(string message, CommitOptions options);

        /// <summary>
        /// <para>Create a new branch.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-branch.html</para>
        /// </summary>
        void CreateBranch(string branchName, IRevision createAt);

        /// <summary>
        /// <para>Create a tag.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-tag.html</para>
        /// </summary>
        void CreateTag(IRevision revision, string name, string message, TagOptions options);

        /// <summary>
        /// <para>Delete a branch.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-branch.html</para>
        /// </summary>
        void DeleteBranch(string branchName, DeleteBranchOptions options);

        /// <summary>
        /// <para>Delete a branch.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-branch.html</para>
        /// </summary>
        void DeleteBranch(IBranch branch, DeleteBranchOptions options);

        /// <summary>
        /// <para>Delete a remote branch.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-push.html</para>
        /// </summary>
        void DeleteRemoteBranch(IRevision remoteBranch, IRemote remote, PushOptions options);

        /// <summary>
        /// <para>Delete a tag.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-tag.html</para>
        /// </summary>
        void DeleteTag(ITagName tag);

        IEnumerable<ICommit> EnumerateCommits(IRevision revision, HistoryOptions options);

        IEnumerable<ICommit> EnumerateCommits(IRevision revision);

        IEnumerable<ICommit> EnumerateCommits(IRevision since, IRevision until, HistoryOptions options);

        IEnumerable<ICommit> EnumerateCommits(IRevision since, IEnumerable<IRevision> until, HistoryOptions options);

        void Fetch(IRemote remote, FetchOptions options);

        /// <summary>
        /// <para>Find as good common ancestors as possible for a merge.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-merge-base.html</para>
        /// </summary>
        /// <returns>The ID of the best common ancestor for the two commits</returns>
        ObjectId FindMergeBase(IRevision revisionA, IRevision revisionB);

        /// <summary>
        /// <para>Returns true if the repository contains any commits reachable by any refs.  This
        /// uses the --all parameter of rev-list.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-rev-list.html</para>
        /// </summary>
        /// <returns>True if the repository contains any commits reachable by any refs</returns>
        bool HasCommits();

        /// <summary>
        /// <para>Check if revisionA is an ancestor of revisionB.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-merge-base.html</para>
        /// </summary>
        bool IsAncestor(IRevision revisionA, IRevision revisionB);

        IReadOnlyList<string> GetCanonicalPaths(string path);

        IReadOnlyDictionary<string, ITreeDifferenceDetail> ReadFileInfo(string path);

        MergeCommandResult Merge(IRevision revision, MergeOptions options);

        void MergeAbort();

        /// <summary>
        /// <para>Opens a blob from the object database for streaming, and applies any necessary filters.</para>
        /// <para>Unlike other methods for reading from the object database, <see cref="OpenBlob"/>
        /// will apply any and all filters to the blob's content before returning bytes to the caller.</para>
        /// <para>The type of the object must be a blob, or result is indeterminate.</para>
        /// </summary>
        /// <param name="revision">The revision to use when reading from the object database.</param>
        /// <param name="path">
        /// <para>A path to the object on disk relative to the root of the repository.</para>
        /// <para>The path must be in "repository format" and be complete (pathish and/or fnmatch *not* supported)</para>
        /// </param>
        /// <returns>A <see cref="Stream"/> of bytes from a blob stored in the object database.</returns>
        Stream OpenBlob(IRevision revision, string path);

        /// <summary>
        /// <para>Opens a blob from the object database for streaming, and applies any necessary filters.</para>
        /// <para>Unlike other methods for reading from the object database, <see cref="OpenBlob"/>
        /// will apply any and all filters to the blob's content before returning bytes to the caller.</para>
        /// <para>The type of the object must be a blob, or result is indeterminate.</para>
        /// </summary>
        /// <param name="blobId">The blob ID to use when reading from the object database.</param>
        /// <param name="path">
        /// <para>A path to the object on disk relative to the root of the repository.</para>
        /// <para>The path must be in "repository format" and be complete (pathish and/or fnmatch *not* supported)</para>
        /// </param>
        /// <returns>A <see cref="Stream"/> of bytes from a blob stored in the object database.</returns>
        Stream OpenBlob(ObjectId blobId, string path);

        void CopyBlobToStream(Stream stream, ObjectId blobId, string path);

        /// <summary>
        /// <para>Opens a blob from the index for streaming, and applies any necessary filters.</para>
        /// <para>Unlike other methods for reading from the object database, <see cref="OpenBlob"/>
        /// will apply any and all filters to the blob's content before returning bytes to the caller.</para>
        /// <para>The type of the object must be a blob, or result is indeterminate.</para>
        /// </summary>
        /// <param name="index">The version of the "index" to read the blob contents from.</param>
        /// <param name="path">
        /// <para>A path to the object in the index relative to the root of the repository.</para>
        /// <para>The path must be in "repository format" and be complete (pathish and/or fnmatch *not* supported)</para>
        /// </param>
        /// <returns>A <see cref="Stream"/> of bytes from a blob stored in the object database.</returns>
        Stream OpenBlob(IndexVersion index, string path);

        IDifferenceEngine OpenDifferenceEngine(DifferenceOptions options);

        IObjectDatabase OpenObjectDatabase();

        PullCommandResult Pull(IRemote remote, PullOptions options);

        void Push(IBranch localBranch, IRemote remote, PushOptions options);

        void Push(IBranch localBranch, IBranch remoteBranch, PushOptions options);

        void Push(IBranchName localBranch, IRemoteName remote, IBranchName remoteBranch, PushOptions options);

        /// <summary>
        /// Push a tag to the specified remote.
        /// </summary>
        /// <param name="localTag">Name of tag to push</param>
        /// <param name="remote">Remote to push to</param>
        /// <param name="options">Push options</param>
        void Push(ITagName localTag, IRemote remote, PushOptions options);

        /// <summary>
        /// Push all tags to the specified remote.
        /// </summary>
        /// <param name="remote">Remote to push to</param>
        /// <param name="options">Push options</param>
        void PushAllTags(IRemote remote, PushOptions options);

        long ReadCommitCount(InclusiveRange range, HistoryOptions options);

        ITreeDifference ReadCommitTreeDifference(ICommit commit, DifferenceOptions options);

        string ReadCurrentBranchName(bool shortName, out HeadType headType);

        /// <summary>
        /// Read the object ID for HEAD.
        /// </summary>
        ObjectId ReadCurrentHeadId();

        string ReadCurrentHeadValue();

        /// <summary>
        /// Read the config list for the repository.
        /// </summary>
        IReadOnlyList<ConfigurationEntry> ReadConfigList();

        string ReadCurrentMergeMessage();

        RepositoryCurrentOperation ReadCurrentOperation();

        /// <summary>
        /// Reads the current HEAD state of the repository.
        /// </summary>
        IHead ReadHead();

        ObjectHeader ReadHeader(ObjectId objectId, string path);

        bool ReadIsIndexLocked();

        ITreeDifference ReadIndexTreeDifference(DifferenceOptions options);

        T ReadObject<T>(ObjectId objectId)
            where T : class, IObject;

        IReferenceCollection ReadReferences(ReferenceOptions options);

        IRemoteCollection ReadRemotes();

        /// <summary>
        /// Reports statistics on the repository's object database.
        /// </summary>
        /// <returns>Details related to object database health.</returns>
        ObjectDatabaseDetails ReadObjectDatabaseDetails();

        ICommit ReadRevision(IRevision revision);

        /// <summary>
        /// Returns a list of, up-to `<paramref name="maxCount"/>`, stash entries that the repository currently has
        /// <para/>
        /// The stash entries are listed in the inverse order they were created.
        /// </summary>
        /// <param name="maxCount">
        /// Limit the number of entries to output.
        /// <para/>
        /// Use -1 to not limited the output.
        /// </param>
        IReadOnlyList<IStash> ReadStashList(int maxCount);

        /// <summary>
        /// Returns a list of all stash entries that the repository currently has.
        /// <para/>
        /// The stash entries are listed in the inverse order they were created.
        /// </summary>
        IReadOnlyList<IStash> ReadStashList();

        IStatusSnapshot ReadStatus(StatusOptions options);

        ITreeDifference ReadStatusIndex(DifferenceOptions options);

        ITreeDifference ReadTreeDifference(ICommit source, ICommit target, DifferenceOptions options);

        ITreeDifference ReadTreeDifference(ITree source, ITree target, DifferenceOptions options);

        ITreeDifference ReadTreeDifference(IRevision source, IRevision target, DifferenceOptions options);

        ITreeDifference ReadStatusWorktree(DifferenceOptions options);

        RebaseResult RebaseAbort(OperationProgressDelegate progressCallback);

        RebaseResult RebaseBegin(IRevision upstream, RebaseOptions options);

        RebaseResult RebaseContinue(OperationProgressDelegate progressCallback);

        RebaseResult RebaseSkip(OperationProgressDelegate progressCallback);

        /// <summary>
        /// Remove the upstream branch for the specified local branch.
        /// </summary>
        void RemoveUpstream(IBranch branch);

        /// <summary>
        /// Rename the branch to the new name.
        /// </summary>
        /// <param name="sourceBranchName">Branch to rename</param>
        /// <param name="newBranchName">New name of branch</param>
        void RenameBranch(IBranchName sourceBranchName, string newBranchName);

        /// <summary>
        /// Rename the branch to the new name.
        /// </summary>
        /// <param name="sourceBranchName">Name of branch to rename</param>
        /// <param name="newBranchName">New name of branch</param>
        void RenameBranch(string sourceBranchName, string newBranchName);

        /// <summary>
        /// <para>Reset current <see cref="Head"/> to <paramref name="revision"/>.</para>
        /// <para>Resets the index and working tree. Any changes to tracked files in the working
        /// tree since <paramref name="revision"/> are discarded.</para>
        /// </summary>
        /// <param name="revision">The revision to reset HEAD to.</param>
        void ResetHard(IRevision revision, OperationProgressDelegate progressCallback);

        /// <summary>
        /// <para>Reset current <see cref="Head"/> to  <paramref name="revision"/>.</para>
        /// <para>Resets index entries and updates files in the working tree that are different
        /// between <paramref name="revision"/> and HEAD. If a file that is different between <commit>
        /// and HEAD has local changes, reset is aborted.</para>
        /// </summary>
        /// <param name="revision">The revision to reset HEAD to.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        bool ResetKeep(IRevision revision);

        /// <summary>
        /// <para>Reset current <see cref="Head"/> to <paramref name="revision"/>.</para>
        /// <para>Resets the index and updates the files in the working tree that are different
        /// between <paramref name="revision"/> and <see cref="Head"/>, but keeps those
        /// which are different between the index and working tree (i.e. which have changes which
        /// have not been added).</para>
        /// <para>If a file that is different between <paramref name="revision"/> and the index has
        /// unstaged changes, <see cref="ResetMerge(IRevision)"/> is aborted.</para>
        /// </summary>
        /// <param name="revision">The revision to reset HEAD to.</param>
        bool ResetMerge(IRevision revision);

        /// <summary>
        /// <para>Reset current <see cref="Head"/> to the <paramref name="revision"/>.</para>
        /// <para>Resets the index but not the working tree (i.e., the changed files are preserved but
        /// not marked for commit) and reports what has not been updated.</para>
        /// </summary>
        /// <param name="revision">The revision to reset HEAD to.</param>
        void ResetMixed(IRevision revision);

        /// <summary>
        /// <para>Resets the index entries for all <paramref name="paths"/> to their state at
        /// <see cref="Revision.HeadRevision"/>. (It does not affect the working tree or the current
        /// branch.)</para>
        /// <para>This means that <see cref="ResetPaths(ICollection{string})"/> is the opposite
        /// of git-add <paramref name="paths"/>.</para>
        /// <para>After running git reset <paramref name="paths"/> to update the index entry,
        /// you can use <see cref="Checkout(IRevision, CheckoutOptions)"/> to check
        /// the contents out of the index to the working tree. Alternatively, using
        /// <see cref="Checkout(IRevision, CheckoutOptions)"/> and specifying a commit,
        /// you can copy the contents of a path out of a commit to the index and to the working
        /// tree in one go.</para>
        /// </summary>
        /// <param name="paths">The paths to reset.</param>
        void ResetPaths(ICollection<string> paths);

        /// <summary>
        /// <para>Reset current <see cref="Head"/> to the <paramref name="revision"/>.</para>
        /// <para>Does not touch the index file or the working tree at all (but resets
        /// <see cref="Head"/> to <paramref name="revision"/>, just like all modes do).</para>
        /// <para>This leaves all your changed files "Changes to be committed".</para>
        /// </summary>
        /// <param name="revision">The revision to reset HEAD to.</param>
        void ResetSoft(IRevision revision);

        /// <summary>
        /// <para>Revert a commit.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-revert.html</para>
        /// </summary>
        RevertResult Revert(IRevision revision, RevertOptions options);

        /// <summary>
        /// <para>Abort the revert a commit.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-revert.html</para>
        /// </summary>
        RevertResult RevertAbort(OperationProgressDelegate progressCallback);

        /// <summary>
        /// <para>Continue the revert a commit.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-revert.html</para>
        /// </summary>
        RevertResult RevertContinue(OperationProgressDelegate progressCallback);

        /// <summary>
        /// Set a config value.
        /// </summary>
        void ConfigurationSetValue(string name, string value, ConfigurationLevel level);

        /// <summary>
        /// Set the upstream branch for the specified local branch.
        /// </summary>
        void SetUpstream(IBranch branch, IBranch upstream);

        /// <summary>
        /// See "git submodule update".
        /// </summary>
        /// <param name="options"></param>
        void SubmoduleUpdate(SubmoduleUpdateOptions options);

        /// <summary>
        /// Unset a config value.
        /// </summary>
        void ConfigurationUnsetValue(string name, ConfigurationLevel level);

        /// <summary>
        /// Save local modifications to a new Git stash entry and return the working directory to HEAD.
        /// <para/>
        /// By default only tracked files are stashed and cleaned; use `<paramref name="options"/>` to change how untracked and ignored files are handled.
        /// </summary>
        /// <param name="options">Options related to the stash operation.</param>
        void PushStash(StashPushOptions options);

        /// <summary>
        /// Like `<seealso cref="PopStash(int, StashApplyOptions)"/>`, but do not remove the state from the stash list.
        /// <para/>
        /// The working directory must match the index.
        /// <para/>
        /// Applying the state can fail with conflicts; in this case, it is not removed from the stash list; conflicts need to resolved and dropped manually afterwards.
        /// <para/>
        /// If the `<seealso cref="StashApplyFlags.ApplyIndex"/>` flag is used, then tries to reinstate not only the working tree’s changes, but also the index’s.
        /// <para/>
        /// Returns the list working directory files affected by application of the stash entry.
        /// </summary>
        /// <param name="stashRevision">The zero-based index of the stash entry to apply, where zero is the most recently added entry.</param>
        /// <param name="options">Options related to the operation.</param>
        /// <exception cref="ArgumentOutOfRangeException">When `<paramref name="stashRevision"/>` is less than zero.</exception>
        IReadOnlyList<StashUpdatedFile> ApplyStash(int stashRevision, StashApplyOptions options);

        /// <summary>
        /// Remove a single stashed state from the stash list and apply it on top of the current working tree state.
        /// <para/>
        /// The working directory must match the index.
        /// <para/>
        /// Applying the state can fail with conflicts; in this case, it is not removed from the stash list; conflicts need to resolved and dropped manually afterwards.
        /// <para/>
        /// If the `<seealso cref="StashApplyFlags.ApplyIndex"/>` flag is used, then tries to reinstate not only the working tree’s changes, but also the index’s. 
        /// <para/>
        /// Returns the list working directory files affected by application of the stash entry.
        /// </summary>
        /// <param name="stashRevision">The zero-based index of the stash entry to apply, where zero is the most recently added entry.</param>
        /// <param name="options">Options related to the operation.</param>
        /// <exception cref="ArgumentOutOfRangeException">When `<paramref name="stashRevision"/>` is less than zero.</exception>
        IReadOnlyList<StashUpdatedFile> PopStash(int stashRevision, StashApplyOptions options);

        /// <summary>
        /// Remove a single stash entry from the list of stash entries.
        /// </summary>
        /// <param name="stashRevision">The zero-based index of the stash entry, where zero is the most recently added entry.</param>
        /// <exception cref="ArgumentOutOfRangeException">When `<paramref name="stashRevision"/>` is less than zero.</exception>
        void DropStash(int stashRevision);

        /// <summary>
        /// Remove all the stash entries. Note that those entries will then be subject to pruning, and may be impossible to recover.
        /// </summary>
        void ClearStash();

        /// <summary>
        /// Temporary API to get the shared working directory.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        string SharedWorkingDirectory { get; }

        /// <summary>
        /// Temporary API to get the local working directory.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        string LocalWorkingDirectory { get; }
    }

    public sealed class Repository : Base, IEquatable<Repository>, IRepository, IStringCache
    {
        public static readonly StringComparer PathComparer = StringComparer.InvariantCulture;

        internal Repository(IExecutionContext context, string workingDirectory, object userData = null)
            : base()
        {
            SetContext(context);

            if (workingDirectory == null)
                throw new ArgumentNullException(nameof(workingDirectory));
            if (!FileSystem.DirectoryExists(workingDirectory))
                throw new DirectoryNotFoundException(workingDirectory);

            _stringCache = new StringCache();
            _syncpoint = new object();
            _initialDirectory = workingDirectory;
            _userData = userData;
        }

        internal Repository(IExecutionContext context, IRepositoryDetails details, object userData = null)
            : base()
        {
            SetContext(context);

            if (details == null)
                throw new ArgumentNullException(nameof(details));
            if (!FileSystem.DirectoryExists(details.WorkingDirectory))
                throw new DirectoryNotFoundException(details.WorkingDirectory);

            _stringCache = new StringCache();
            _syncpoint = new object();
            _initialDirectory = details.WorkingDirectory;
            _repositoryDetails = details;
            _userData = userData;
        }

        private Environment _environment;
        private string _initialDirectory;
        private IRepositoryDetails _repositoryDetails;
        private StringCache _stringCache;
        private readonly object _syncpoint;
        private object _userData;

        public Environment Environment
        {
            get
            {
                lock (_syncpoint)
                {
                    if (ReferenceEquals(_environment, null))
                    {
                        _environment = Git.GetProcessEnvironment(this);
                    }
                }

                return _environment;
            }
        }

        public string CommonDirectory
        {
            get { return RepositoryDetails.CommonDirectory; }
        }

        public string GitDirectory
        {
            get { return RepositoryDetails.GitDirectory; }
        }

        public string IndexFile
        {
            get { return RepositoryDetails.IndexFile; }
        }

        public bool IsBare
        {
            get { return RepositoryDetails.IsBareRepository; }
        }

        public string ObjectsDirectory
        {
            get { return RepositoryDetails.ObjectsDirectory; }
        }

        public IRepositoryDetails RepositoryDetails
        {
            get
            {
                lock (_syncpoint)
                {
                    if (_repositoryDetails == null)
                    {
                        _repositoryDetails = new Cli.RevParseCommand(Context, this).GetRepositoryDetails();
                    }
                    return _repositoryDetails;
                }
            }
        }

        public string SharedIndexFile
        {
            get { return RepositoryDetails.SharedIndexFile; }
        }

        public object UserData
        {
            get { return Volatile.Read(ref _userData); }
            set { Volatile.Write(ref _userData, value); }
        }

        public string WorkingDirectory
        {
            get { return _repositoryDetails?.WorkingDirectory ?? _initialDirectory; }
        }

        public string SharedWorkingDirectory => WorkingDirectory;

        public string LocalWorkingDirectory => WorkingDirectory;

        public IReadOnlyCollection<IUpdatedIndexEntry> Add(IEnumerable<string> paths, UpdateOptions options)
        {
            return new Cli.UpdateIndexCommand(Context, this).Add(paths, options);
        }

        public void AddRemote(string url, string name, RemoteTagOptions options)
        {
            new Cli.RemoteCommand(Context, this).Add(url, name, options);
        }

        public void BranchSetUpstream(IBranchName branch, IBranchName upstreamBranch)
        {
            new Cli.BranchCommand(Context, this).SetUpstream(branch, upstreamBranch);
        }

        public void BranchSetUpstream(IBranchName branch, IRemoteName upstreamRemote, IBranchName upstreamBranch)
        {
            new Cli.BranchCommand(Context, this).SetUpstream(branch, upstreamRemote, upstreamBranch);
        }

        public void BranchUnsetUpstream(IBranchName branch)
        {
            new Cli.BranchCommand(Context, this).RemoveUpstream(branch);
        }

        /// <summary>
        /// Sets a configuration value in the Git global configuration store.
        /// </summary>
        /// <param name="context">The execution context settings to use with this operation.</param>
        /// <param name="name">The name of the configuration entry to add.</param>
        /// <param name="value">The value of the configuration entry to add.</param>
        public static void ConfigurationSetGlobalValue(IExecutionContext context, string name, string value, object userData)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            string tempPath = context.FileSystem.GetTempDirectoryPath();
            Environment environment = context.Git.GetProcessEnvironment(tempPath);

            if (!(context is ExecutionContext executionContext))
            {
                executionContext = new ExecutionContext(context);
            }

            new Cli.ConfigCommand(executionContext, environment, userData).Set(name, value, ConfigurationLevel.Global);
        }

        /// <summary>
        /// Sets a configuration value in the Git global configuration store.
        /// </summary>
        /// <param name="context">The execution context settings to use with this operation.</param>
        /// <param name="name">The name of the configuration entry to add.</param>
        /// <param name="value">The value of the configuration entry to add.</param>
        public static void ConfigurationSetGlobalValue(IExecutionContext context, string name, string value)
            => ConfigurationSetGlobalValue(context, name, value, null);

        /// <summary>
        /// Sets a configuration value, using the current execution context settings, in the Git global configuration store.
        /// </summary>
        /// <param name="name">The name of the configuration entry to add.</param>
        /// <param name="value">The value of the configurtion entry to add.</param>
        public static void ConfigurationSetGlobalValue(string name, string value, object userData)
        {
            var context = ExecutionContext.Current;

            ConfigurationSetGlobalValue(context, name, value, userData);
        }

        /// <summary>
        /// Sets a configuration value, using the current execution context settings, in the Git global configuration store.
        /// </summary>
        /// <param name="name">The name of the configuration entry to add.</param>
        /// <param name="value">The value of the configurtion entry to add.</param>
        public static void ConfigurationSetGlobalValue(string name, string value)
            => ConfigurationSetGlobalValue(name, value, null);

        public void ConfigurationSetValue(string name, string value, ConfigurationLevel level)
        {
            new Cli.ConfigCommand(Context, this).Set(name, value, level);
        }

        /// <summary>
        /// Unsets a configuration value from the Git global configuration store.
        /// </summary>
        /// <param name="context">The execution context settings for this operation.</param>
        /// <param name="name">The name of the configuration entry to unset.</param>
        public static void ConfigurationUnsetGlobalValue(IExecutionContext context, string name, object userData)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            string tempPath = context.FileSystem.GetTempDirectoryPath();
            Environment environment = context.Git.GetProcessEnvironment(tempPath);

            if (!(context is ExecutionContext executionContext))
            {
                executionContext = new ExecutionContext(context);
            }

            new Cli.ConfigCommand(executionContext, environment, userData).Unset(name, ConfigurationLevel.Global);
        }

        /// <summary>
        /// Unsets a configuration value from the Git global configuration store.
        /// </summary>
        /// <param name="context">The execution context settings for this operation.</param>
        /// <param name="name">The name of the configuration entry to unset.</param>
        public static void ConfigurationUnsetGlobalValue(IExecutionContext context, string name)
            => ConfigurationUnsetGlobalValue(context, name, null);

        /// <summary>
        /// Unsets a configuration value from the Git global configuration store.
        /// </summary>
        /// <param name="name">The name of the configuration entry to unset.</param>
        public static void ConfigurationUnsetGlobalValue(string name, object userData)
        {
            var context = ExecutionContext.Current;

            ConfigurationUnsetGlobalValue(context, name, userData);
        }

        /// <summary>
        /// Unsets a configuration value from the Git global configuration store.
        /// </summary>
        /// <param name="name">The name of the configuration entry to unset.</param>
        public static void ConfigurationUnsetGlobalValue(string name)
            => ConfigurationUnsetGlobalValue(name, null);

        public void ConfigurationUnsetValue(string name, ConfigurationLevel level)
        {
            new Cli.ConfigCommand(Context, this).Unset(name, level);
        }

        public void PushStash(StashPushOptions options)
        {
            new Cli.StashCommand(Context, this).Push(options);
        }

        public IReadOnlyList<StashUpdatedFile> ApplyStash(int stashRevision, StashApplyOptions options)
        {
            return new Cli.StashCommand(Context, this).Apply(stashRevision, options);
        }

        public IReadOnlyList<StashUpdatedFile> PopStash(int stashRevision, StashApplyOptions options)
        {
            return new Cli.StashCommand(Context, this).Pop(stashRevision, options);
        }

        public void DropStash(int stashRevision)
        {
            new Cli.StashCommand(Context, this).Drop(stashRevision);
        }

        public void ClearStash()
        {
            new Cli.StashCommand(Context, this).Clear();
        }

        public void Checkout(IRevision revision, CheckoutOptions options)
        {
            new Cli.CheckoutCommand(Context, this).Revision(revision, options);
        }

        public void CheckoutIndex(CheckoutIndexOptions options)
        {
            new Cli.CheckoutIndexCommand(Context, this).CheckoutAll(options);
        }

        public void CheckoutIndex(IEnumerable<string> paths, CheckoutIndexOptions options)
        {
            new Cli.CheckoutIndexCommand(Context, this).CheckoutPaths(paths, options);
        }

        public IEnumerable<IUpdatedWorktreeEntry> Clean(CleanOptions options)
        {
            return new Cli.CleanCommand(Context, this).RemoveFiles(options);
        }

        public void CherryPick(IRevision revision, CherryPickOptions options)
        {
            new Cli.CherryPickCommand(Context, this).PickRevision(revision, options);
        }

        public void CherryPick(InclusiveRange range, CherryPickOptions options)
        {
            new Cli.CherryPickCommand(Context, this).PickRange(range, options);
        }

        public void CherryPickContinue()
        {
            new Cli.CherryPickCommand(Context, this).Continue();
        }

        public void CherryPickAbort()
        {
            new Cli.CherryPickCommand(Context, this).Abort();
        }

        /// <summary>
        /// Creates a new repository by cloning it from a remote source, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="remoteUrl">The uniformed resource locater of the remote source.</param>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        /// <param name="options">Options for customizing the clone operation.</param>
        /// <param name="userData">Optional data attached to any trace messages sent from this command to listeners.</param>
        public static IRepository Clone(IExecutionContext context, string remoteUrl, string workingDirectory, CloneOptions options, object userData)

        {
            Environment environment = context.Git.GetProcessEnvironment(workingDirectory,
                                                                        ignorePathCasing: false);

            if (!(context is ExecutionContext executionContext))
            {
                executionContext = new ExecutionContext(context);
            }

            new Cli.CloneCommand(executionContext, environment, userData).NormalFromRemote(remoteUrl, workingDirectory, options);

            return new Repository(context, workingDirectory, userData);
        }

        /// <summary>
        /// Creates a new repository by cloning it from a remote source, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="remoteUrl">The uniformed resource locater of the remote source.</param>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        /// <param name="options">Options for customizing the clone operation.</param>
        public static IRepository Clone(IExecutionContext context, string remoteUrl, string workingDirectory, CloneOptions options)
            => Clone(context, remoteUrl, workingDirectory, options, null);

        /// <summary>
        /// Creates a new repository by cloning it from a remote source, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="remoteUrl">The uniformed resource locater of the remote source.</param>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        /// <param name="userData">Optional data attached to any trace messages sent from this command to listeners.</param>
        public static IRepository Clone(IExecutionContext context, string remoteUrl, string workingDirectory, object userData)
            => Clone(context, remoteUrl, workingDirectory, CloneOptions.Default, userData);

        /// <summary>
        /// Creates a new repository by cloning it from a remote source, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="remoteUrl">The uniformed resource locater of the remote source.</param>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        public static IRepository Clone(IExecutionContext context, string remoteUrl, string workingDirectory)
            => Clone(context, remoteUrl, workingDirectory, CloneOptions.Default, null);

        /// <summary>
        /// Creates a new repository by cloning it from a remote source, using the current context settings, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="remoteUrl">The uniformed resource locater of the remote source.</param>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        /// <param name="options">Options for customizing the clone operation.</param>
        /// <param name="userData">Optional data attached to any trace messages sent from this command to listeners.</param>
        public static IRepository Clone(string remoteUrl, string workingDirectory, CloneOptions options, object userData)
        {
            IExecutionContext context = ExecutionContext.Current;

            return Clone(context, remoteUrl, workingDirectory, options, userData);
        }

        /// <summary>
        /// Creates a new repository by cloning it from a remote source, using the current context settings, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="remoteUrl">The uniformed resource locater of the remote source.</param>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        /// <param name="options">Options for customizing the clone operation.</param>
        public static IRepository Clone(string remoteUrl, string workingDirectory, CloneOptions options)
            => Clone(remoteUrl, workingDirectory, options, null);

        public ObjectId Commit(string message, CommitOptions options)
        {
            return new Cli.CommitCommand(Context, this).Commit(message, options);
        }

        public void CreateBranch(string branchName, IRevision createAt)
        {
            new Cli.BranchCommand(Context, this).Create(branchName, createAt);
        }

        public void CreateTag(IRevision revision, string name, string message, TagOptions options)
        {
            new Cli.TagCommand(Context, this).Create(revision, name, message, options);
        }

        public void DeleteBranch(string branchCanonicalName, DeleteBranchOptions options)
        {
            new Cli.BranchCommand(Context, this).Delete(branchCanonicalName, options);
        }

        public void DeleteBranch(IBranch branch, DeleteBranchOptions options)
        {
            new Cli.BranchCommand(Context, this).Delete(branch, options);
        }

        public void DeleteRemoteBranch(IRevision remoteBranch, IRemote remote, PushOptions options)
        {
            new Cli.PushCommand(Context, this).DeleteReference(remoteBranch, remote, options);
        }

        public void DeleteTag(ITagName tag)
        {
            new Cli.TagCommand(Context, this).Delete(tag);
        }

        public void Dispose()
        {
            StringCache cache;
            if ((cache = Interlocked.Exchange(ref _stringCache, null)) != null)
            {
                cache.Dispose();
            }
        }

        public IEnumerable<ICommit> EnumerateCommits(IRevision revision, HistoryOptions options)
        {
            return new Cli.RevListCommand(Context, this).EnumerateCommits(revision, options);
        }

        public IEnumerable<ICommit> EnumerateCommits(IRevision revision)
        {
            return new Cli.RevListCommand(Context, this).EnumerateCommits(revision);
        }

        public IEnumerable<ICommit> EnumerateCommits(IRevision since, IRevision until, HistoryOptions options)
        {
            return new Cli.RevListCommand(Context, this).EnumerateCommits(since, until, options);
        }

        public IEnumerable<ICommit> EnumerateCommits(IRevision since, IEnumerable<IRevision> until, HistoryOptions options)
        {
            return new Cli.RevListCommand(Context, this).EnumerateCommits(since, until, options);
        }

        public static bool Equals(IRepository left, IRepository right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(null, right))
                return false;

            return PathComparer.Equals(left.GitDirectory, right.GitDirectory);
        }

        public bool Equals(IRepository other)
            => Repository.Equals(this as IRepository, other);

        public bool Equals(Repository other)
            => Repository.Equals(this as IRepository, other as IRepository);

        public override bool Equals(object obj)
            => Repository.Equals(this as IRepository, obj as IRepository);

        public void Fetch(IRemote remote, FetchOptions options)
        {
            new Cli.FetchCommand(Context, this).FromRemote(remote, options);
        }

        public IReadOnlyList<string> GetCanonicalPaths(string path)
        {
            return new Cli.LsFilesCommand(Context, this).GetCanonicalPaths(path);
        }

        public override int GetHashCode()
        {
            return PathComparer.GetHashCode(GitDirectory);
        }

        public ObjectId FindMergeBase(IRevision revisionA, IRevision revisionB)
        {
            return new Cli.MergeBaseCommand(Context, this).FindMergeBase(revisionA, revisionB);
        }

        public bool HasCommits()
        {
            return new Cli.RevListCommand(Context, this).HasCommits();
        }

        /// <summary>
        /// Creates a new repository, initializes it, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        /// <param name="options">Options for customizing the <see cref="Initialize"/> operation.</param>
        /// <param name="userData">Optional data attached to any trace messages sent from this command to listeners.</param>
        public static IRepository Initialize(IExecutionContext context, string workingDirectory, InitializationOptions options, object userData)
        {
            Environment environment = context.Git.GetProcessEnvironment(workingDirectory,
                                                                        ignorePathCasing: false);

            if (!(context is ExecutionContext executionContext))
            {
                executionContext = new ExecutionContext(context);
            }

            new Cli.InitCommand(executionContext, environment, userData).CreateRepository(options);

            return Open(context, workingDirectory, userData);
        }

        /// <summary>
        /// Creates a new repository, initializes it, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        /// <param name="options">Options for customizing the <see cref="Initialize"/> operation.</param>
        /// <param name="tracerData">Optional data attached to any trace messages sent from this command to listeners.</param>
        public static IRepository Initialize(IExecutionContext context, string workingDirectory, InitializationOptions options)
            => Initialize(context, workingDirectory, options, null);

        /// <summary>
        /// Creates a new repository, initializes it using default options, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        /// <param name="userData">Optional data attached to any trace messages sent from this command to listeners.</param>
        public static IRepository Initialize(IExecutionContext context, string workingDirectory, object userData)
            => Initialize(context, workingDirectory, InitializationOptions.Default, userData);

        /// <summary>
        /// Creates a new repository, initializes it using default options, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        public static IRepository Initialize(IExecutionContext context, string workingDirectory)
            => Initialize(context, workingDirectory, InitializationOptions.Default, null);

        /// <summary>
        /// Creates a new repository using default context settings, initializes it, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        /// <param name="options">Options for customizing the <see cref="Initialize"/> operation.</param>
        /// <param name="userData">Optional data attached to any trace messages sent from this command to listeners.</param>
        public static IRepository Initialize(string workingDirectory, InitializationOptions options, object userData)
        {
            var context = ExecutionContext.Current;

            return Initialize(context, workingDirectory, options, userData);
        }

        /// <summary>
        /// Creates a new repository using default context settings, initializes it, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        /// <param name="options">Options for customizing the <see cref="Initialize"/> operation.</param>
        /// <param name="tracerData">Optional data attached to any trace messages sent from this command to listeners.</param>
        public static IRepository Initialize(string workingDirectory, InitializationOptions options)
            => Initialize(workingDirectory, options, null);

        /// <summary>
        /// Creates a new repository using default context settings, initializes it, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        /// <param name="userData">Optional data attached to any trace messages sent from this command to listeners.</param>
        public static IRepository Initialize(string workingDirectory, object userData)
            => Initialize(workingDirectory, InitializationOptions.Default, userData);

        /// <summary>
        /// Creates a new repository using default context settings, initializes it, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="workingDirectory">The path, in the local file system, where the new repository will be created.</param>
        public static IRepository Initialize(string workingDirectory)
            => Initialize(workingDirectory, InitializationOptions.Default, null);

        public bool IsAncestor(IRevision revisionA, IRevision revisionB)
        {
            return new Cli.MergeBaseCommand(Context, this).IsAncestor(revisionA, revisionB);
        }

        public MergeCommandResult Merge(IRevision revision, MergeOptions options)
        {
            return new Cli.MergeCommand(Context, this).DoMerge(revision, options);
        }

        public void MergeAbort()
        {
            new Cli.MergeCommand(Context, this).DoAbort();
        }

        /// <summary>
        /// Opens an existing repository and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="workingDirectory">Path to the worktree of the repository.</param>
        /// <param name="userData">Optional data attached to any trace messages sent from this command to listeners.</param>
        public static IRepository Open(IExecutionContext context, string workingDirectory, object userData)
        {
            return new Repository(context, workingDirectory, userData);
        }

        /// <summary>
        /// Opens an existing repository and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="workingDirectory">Path to the worktree of the repository.</param>
        public static IRepository Open(IExecutionContext context, string workingDirectory)
            => Open(context, workingDirectory, null);

        /// <summary>
        /// Opens an existing repository and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="details">
        /// Details of a previously opened repository used to initialize this instance.
        /// <para>Avoids querying repository details at initialization.</para>
        /// </param>
        /// <param name="userData">Optional data attached to any trace messages sent from this command to listeners.</param>
        public static IRepository Open(IExecutionContext context, IRepositoryDetails details, object userData)
        {
            return new Repository(context, details, userData);
        }

        /// <summary>
        /// Opens an existing repository and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="details">
        /// Details of a previously opened repository used to initialize this instance.
        /// <para>Avoids querying repository details at initialization.</para>
        /// </param>
        public static IRepository Open(IExecutionContext context, IRepositoryDetails details)
            => Open(context, details, null);

        /// <summary>
        /// Opens an existing repository using the current execution context settings, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="workingDirectory">Path to the worktree of the repository.</param>
        /// <param name="userData">Optional data attached to any trace messages sent from this command to listeners.</param>
        public static IRepository Open(string workingDirectory, object userData)
        {
            var context = ExecutionContext.Current;

            return Open(context, workingDirectory, userData);
        }

        /// <summary>
        /// Opens an existing repository using the current execution context settings, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="workingDirectory">Path to the worktree of the repository.</param>
        public static IRepository Open(string workingDirectory)
            => Open(workingDirectory, null);

        /// <summary>
        /// Opens an existing repository using the current execution context settings, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="details">
        /// <para>Details of a previously opened repository used to initialize this instance.</para>
        /// <para>Avoids querying repository details at initialization.</para>
        /// </param>
        public static IRepository Open(IRepositoryDetails details, object userData)
        {
            var context = ExecutionContext.Current;

            return Open(context, details, userData);
        }

        /// <summary>
        /// Opens an existing repository using the current execution context settings, and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="details">
        /// <para>Details of a previously opened repository used to initialize this instance.</para>
        /// <para>Avoids querying repository details at initialization.</para>
        /// </param>
        public static IRepository Open(IRepositoryDetails details)
            => Open(details, null);

        public Stream OpenBlob(IRevision revision, string path)
        {
            return new Cli.CatFileCommand(Context, this).OpenBlob(revision, path);
        }

        public Stream OpenBlob(ObjectId blobId, string path)
        {
            return new Cli.CatFileCommand(Context, this).OpenBlob(blobId, path);
        }

        public Stream OpenBlob(IndexVersion index, string path)
        {
            return new Cli.CatFileCommand(Context, this).OpenBlob(index, path);
        }

        public void CopyBlobToStream(Stream stream, ObjectId blobId, string path)
        {
            new Cli.CatFileCommand(Context, this).CopyBlobToStream(stream, blobId, path);
        }

        public IDifferenceEngine OpenDifferenceEngine(DifferenceOptions options)
        {
            return new Cli.DiffTreeCommand(Context, this).OpenDifferenceEngine(options);
        }

        public IObjectDatabase OpenObjectDatabase()
        {
            return new Cli.CatFileCommand(Context, this).OpenObjectDatabase();
        }

        public PullCommandResult Pull(IRemote remote, PullOptions options)
        {
            return new Cli.PullCommand(Context, this).FromRemote(remote, options);
        }

        public void Push(IBranch localBranch, IRemote remote, PushOptions options)
        {
            new Cli.PushCommand(Context, this).UpdateReference(localBranch, remote, options);
        }

        public void Push(IBranch localBranch, IBranch remoteBranch, PushOptions options)
        {
            new Cli.PushCommand(Context, this).UpdateReference(localBranch, remoteBranch, options);
        }

        public void Push(IBranchName localBranch, IRemoteName remote, IBranchName remoteBranch, PushOptions options)
        {
            new Cli.PushCommand(Context, this).UpdateReference(localBranch, remote, remoteBranch, options);
        }

        public void Push(ITagName localTag, IRemote remote, PushOptions options)
        {
            new Cli.PushCommand(Context, this).UpdateReference(localTag, remote, options);
        }

        public void PushAllTags(IRemote remote, PushOptions options)
        {
            new Cli.PushCommand(Context, this).UpdateAllTags(remote, options);
        }

        public long ReadCommitCount(InclusiveRange range, HistoryOptions options)
        {
            return new Cli.RevListCommand(Context, this).CountCommits(range, options);
        }

        public ITreeDifference ReadCommitTreeDifference(ICommit commit, DifferenceOptions options)
        {
            return new Cli.DiffTreeCommand(Context, this).ReadCommitDifference(commit, options);
        }

        public string ReadCurrentBranchName(bool shortName, out HeadType headType)
        {
            headType = HeadType.Unknown;

            try
            {
                string currentBranchName = new Cli.RevParseCommand(Context, this).ReadCurrentBranchName(shortName, out headType);

                // if rev-parse returned unborn, try harder with symbolic-ref
                if (headType != HeadType.Unborn)
                    return currentBranchName;
            }
            catch
            { /* squelch: errors are likely meaningless here */ }

            try
            {
                HeadType type;
                string currentBranchName = new Cli.SymbolicRefCommand(Context, this).ReadCurrentBranchName(shortName, out type);

                // avoid override rev-parse result when it is known, unless symbolic-ref reports malformed
                if (type == HeadType.Malformed || headType == HeadType.Unknown)
                {
                    headType = type;
                }

                return currentBranchName;
            }
            catch
            { /* squelch: errors are likely meaningless here */ }

            // apparently we do not know what the current branch is
            headType = HeadType.Malformed;
            return GitApi.Head.MalformedLabel;
        }

        public ObjectId ReadCurrentHeadId()
        {
            try
            {
                // Since this variant of rev-parse doesn't pass --symbolic-ref, it should always
                // return an object ID when successful
                string headId = new Cli.RevParseCommand(Context, this).ReadCurrentHeadValue();
                return ObjectId.FromString(headId);
            }
            catch
            { /* squelch: errors are likely meaningless here */ }

            // apparently we do not know what the current branch is
            return ObjectId.Zero;
        }

        public string ReadCurrentHeadValue()
        {
            try
            {
                return new Cli.SymbolicRefCommand(Context, this).ReadCurrentHeadValue();
            }
            catch
            { /* squelch: errors are likely meaningless here */ }

            try
            {
                return new Cli.RevParseCommand(Context, this).ReadCurrentHeadValue();
            }
            catch
            { /* squelch: errors are likely meaningless here */ }

            // apparently we do not know what the current branch is
            return GitApi.Head.MalformedLabel;
        }

        public IReadOnlyList<ConfigurationEntry> ReadConfigList()
        {
            return new Cli.ConfigCommand(Context, this).ReadConfiguration();
        }

        public static IReadOnlyList<ConfigurationEntry> ReadGlobalConfigList(IExecutionContext context, object userData)
        {
            // We need to run the query for global configuration values in a non-repository;
            // using the system's temp folder seems reasonable, as nobody should place the
            // temp folder under version control.
            string tempPath = context.FileSystem.GetTempDirectoryPath();

            // Reading of global configuration values doesn't need the worktree value, in fact
            // setting can cause a whole host of unanticipated adverse effects. Therefore, we
            // craft an environment with cwd = system's temp dir, ignores path cases, and skips
            // writing the expected working directory into the environment.
            Environment environment = context.Git.GetProcessEnvironment(tempPath, true);

            if (!(context is ExecutionContext executionContext))
            {
                executionContext = new ExecutionContext(context);
            }

            return new Cli.ConfigCommand(executionContext, environment, userData).ReadConfiguration();
        }

        public static IReadOnlyList<ConfigurationEntry> ReadGlobalConfigList(object userData)
            => ReadGlobalConfigList(ExecutionContext.Current, userData);

        public static IReadOnlyList<ConfigurationEntry> ReadGlobalConfigList()
            => ReadGlobalConfigList(ExecutionContext.Current, null);

        public string ReadCurrentMergeMessage()
        {
            return new RepositoryCurrentState(Context, this).ReadCurrentMergeMessage();
        }

        public RepositoryCurrentOperation ReadCurrentOperation()
        {
            return new RepositoryCurrentState(Context, this).ReadCurrentOperation();
        }

        public IReadOnlyDictionary<string, ITreeDifferenceDetail> ReadFileInfo(string path)
        {
            return new Cli.LsFilesCommand(Context, this).ReadFileInfo(path);
        }

        public IHead ReadHead()
        {
            var head = new Head();
            head.SetContextAndCache(Context, this as IStringCache);

            // Initialize the HEAD value now to avoid surprise calls to git later
            head.Initialize(this, true, true);

            return head;
        }

        public ObjectHeader ReadHeader(ObjectId objectId, string path)
        {
            return new Cli.CatFileCommand(Context, this).ReadHeader(objectId, path);
        }

        public bool ReadIsIndexLocked()
        {
            return new RepositoryCurrentState(Context, this).ReadIsIndexCurrentlyLocked();
        }

        public ITreeDifference ReadIndexTreeDifference(DifferenceOptions options)
        {
            return new Cli.DiffIndexCommand(Context, this).ReadStatusTreeDifference(options);
        }

        public T ReadObject<T>(ObjectId objectId)
            where T : class, IObject
        {
            return new Cli.CatFileCommand(Context, this).ReadObject<T>(objectId);
        }

        public IReferenceCollection ReadReferences(ReferenceOptions options)
        {
            return new Cli.ForEachRefCommand(Context, this).ReadCollection(options);
        }

        public IRemoteCollection ReadRemotes()
        {
            return new Cli.RemoteCommand(Context, this).ReadCollection();
        }

        public ObjectDatabaseDetails ReadObjectDatabaseDetails()
        {
            return new Cli.CountObjectsCommand(Context, this).GetObjectCounts();
        }

        public ICommit ReadRevision(IRevision revision)
        {
            return new Cli.ShowCommand(Context, this).ReadRevision(revision);
        }

        public IReadOnlyList<IStash> ReadStashList(int maxCount)
        {
            return new Cli.StashCommand(Context, this).List(maxCount);
        }

        public IReadOnlyList<IStash> ReadStashList()
            => ReadStashList(-1);

        public IStatusSnapshot ReadStatus(StatusOptions options)
        {
            return new Cli.StatusCommand(Context, this).ReadSnapshot(options);
        }

        public ITreeDifference ReadTreeDifference(ICommit source, ICommit target, DifferenceOptions options)
        {
            return new Cli.DiffTreeCommand(Context, this).ReadTreeDifference(source, target, options);
        }

        public ITreeDifference ReadTreeDifference(ITree source, ITree target, DifferenceOptions options)
        {
            return new Cli.DiffTreeCommand(Context, this).ReadTreeDifference(source, target, options);
        }

        public ITreeDifference ReadTreeDifference(IRevision source, IRevision target, DifferenceOptions options)
        {
            return new Cli.DiffTreeCommand(Context, this).ReadTreeDifference(source, target, options);
        }

        public ITreeDifference ReadStatusIndex(DifferenceOptions options)
        {
            return new Cli.DiffTreeCommand(Context, this).ReadStatusTreeDifference(options);
        }

        public ITreeDifference ReadStatusWorktree(DifferenceOptions options)
        {
            return new Cli.DiffTreeCommand(Context, this).ReadStatusTreeDifference(options);
        }

        public RebaseResult RebaseAbort(OperationProgressDelegate progressCallback)
        {
            return new Cli.RebaseCommand(Context, this).AbortRebase(progressCallback);
        }

        public RebaseResult RebaseBegin(IRevision upstream, RebaseOptions options)
        {
            return new Cli.RebaseCommand(Context, this).BeginRebase(upstream, options);
        }

        public RebaseResult RebaseContinue(OperationProgressDelegate progressCallback)
        {
            return new Cli.RebaseCommand(Context, this).ContinueRebase(progressCallback);
        }

        public RebaseResult RebaseSkip(OperationProgressDelegate progressCallback)
        {
            return new Cli.RebaseCommand(Context, this).SkipRebase(progressCallback);
        }

        public void RemoveUpstream(IBranch branch)
        {
            new Cli.BranchCommand(Context, this).RemoveUpstream(branch);
        }

        public void RenameBranch(IBranchName sourceBranchName, string newBranchName)
        {
            new Cli.BranchCommand(Context, this).Rename(sourceBranchName, newBranchName);
        }

        public void RenameBranch(string sourceBranchName, string newBranchName)
            => RenameBranch(new BranchName(sourceBranchName), newBranchName);

        public void RenameRemote(string oldName, string newName)
        {
            new Cli.RemoteCommand(Context, this).Rename(oldName, newName);
        }

        public void RemoveRemote(string name)
        {
            new Cli.RemoteCommand(Context, this).Remove(name);
        }

        public void ResetHard(IRevision revision, OperationProgressDelegate progressCallback)
        {
            new Cli.ResetCommand(Context, this).ResetHard(revision, progressCallback);
        }

        public bool ResetKeep(IRevision revision)
        {
            return new Cli.ResetCommand(Context, this).ResetKeep(revision);
        }

        public bool ResetMerge(IRevision revision)
        {
            return new Cli.ResetCommand(Context, this).ResetMerge(revision);
        }

        public void ResetMixed(IRevision revision)
        {
            new Cli.ResetCommand(Context, this).ResetMixed(revision);
        }

        public void ResetPaths(ICollection<string> paths)
        {
            new Cli.ResetCommand(Context, this).ResetPaths(paths);
        }

        public void ResetSoft(IRevision revision)
        {
            new Cli.ResetCommand(Context, this).ResetSoft(revision);
        }

        public RevertResult Revert(IRevision revision, RevertOptions options)
        {
            return new Cli.RevertCommand(Context, this).Begin(revision, options);
        }

        public RevertResult RevertAbort(OperationProgressDelegate progressCallback)
        {
            return new Cli.RevertCommand(Context, this).Abort(progressCallback);
        }

        public RevertResult RevertContinue(OperationProgressDelegate progressCallback)
        {
            return new Cli.RevertCommand(Context, this).Continue(progressCallback);
        }

        public void SetRemoteFetchUrl(string name, string url)
        {
            new Cli.RemoteCommand(Context, this).SetFetchUrl(name, url);
        }

        public void SetRemotePushUrl(string name, string url)
        {
            new Cli.RemoteCommand(Context, this).SetPushUrl(name, url);
        }

        public void SetUpstream(IBranch branch, IBranch upstream)
        {
            new Cli.BranchCommand(Context, this).SetUpstream(branch, upstream);
        }

        public void SubmoduleUpdate(SubmoduleUpdateOptions options)
        {
            new Cli.SubmoduleUpdateCommand(Context, this).Update(options);
        }

        public override string ToString()
        {
            return WorkingDirectory;
        }

        internal StringUtf8 Intern(StringUtf8 value)
        {
            // I hit an exception here during testing, and I think it is because serialization can happen after the repository is disposed.
            // The only thing that objects get from the repository is the IStringCache, so this admittedly awkward fix is still somewhat safe.
            // The medium term plan is to decouple this cache and the Repository, which is obviously even better.
            return _stringCache == null ? value : _stringCache.Intern(value);
        }

        internal string Intern(string value)
        {
            // I hit an exception here during testing, and I think it is because serialization can happen after the repository is disposed.
            // The only thing that objects get from the repository is the IStringCache, so this admittedly awkward fix is still somewhat safe.
            // The medium term plan is to decouple this cache and the Repository, which is obviously even better.
            return _stringCache == null ? value : _stringCache.Intern(value);
        }

        internal void ResetDetails()
        {
            lock (_syncpoint)
            {
                _repositoryDetails = null;
            }
        }

        StringUtf8 IStringCache.Intern(StringUtf8 value)
            => Intern(value);

        string IStringCache.Intern(string value)
            => Intern(value);
    }
}