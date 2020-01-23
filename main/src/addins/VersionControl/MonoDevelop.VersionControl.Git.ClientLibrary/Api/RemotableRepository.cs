//*************************************************************************************************
// RemotableRepository.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class RemotableRepository : Base, IEquatable<RemotableRepository>, IRepository, IStringCache
    {
        public static readonly StringComparer PathComparer = StringComparer.InvariantCulture;

        internal RemotableRepository(IExecutionContext context, string workingDirectory, object userData = null)
            : base()
        {
            SetContext(context);

            if (workingDirectory == null)
                throw new ArgumentNullException(nameof(workingDirectory));

            _stringCache = new StringCache();
            _syncpoint = new object();
            _initialDirectory = workingDirectory;
            _userData = userData;

            EnsureRepositoryService();
        }

        internal RemotableRepository(IExecutionContext context, IRepositoryDetails details, object userData = null)
            : base()
        {
            SetContext(context);

            if (details == null)
                throw new ArgumentNullException(nameof(details));

            _stringCache = new StringCache();
            _syncpoint = new object();
            _initialDirectory = details.WorkingDirectory;
            _repositoryDetails = details;
            _userData = userData;

            EnsureRepositoryService();
        }

        private Environment _environment;
        private string _initialDirectory;
        private IRepositoryDetails _repositoryDetails;
        private StringCache _stringCache;
        private readonly object _syncpoint;
        private object _userData;

        public static IRepositoryService RepositoryService { get; set; }

        private void EnsureRepositoryService()
        {
            if (RepositoryService == null)
            {
                Context.Broker.InvalidateProxies += Broker_InvalidateProxies;
                RepositoryService = ExecuteTaskSynchronously(() => Context.Broker.GetProxyAsync<IRepositoryService, RepositoryService>("GitApi.RepositoryService"));
            }
        }

        private static void Broker_InvalidateProxies(object sender, EventArgs e)
        {
            (RepositoryService as IDisposable)?.Dispose();
            RepositoryService = null;

            // TODO do I have to unsubscribe to the event?
        }

        private void ExecuteTaskSynchronously(Func<Task> asyncMethod, [CallerMemberName]string memberName = "")
        {
            Context.Broker.ExecuteTaskSynchronously(asyncMethod);
        }

        private T ExecuteTaskSynchronously<T>(Func<Task<T>> asyncMethod, [CallerMemberName]string memberName = "")
        {
            return Context.Broker.ExecuteTaskSynchronously(asyncMethod);
        }

        private static void ExecuteTaskSynchronously(IExecutionContext context, Func<Task> asyncMethod, [CallerMemberName]string memberName = "")
        {
            context.Broker.ExecuteTaskSynchronously(asyncMethod);
        }

        private static T ExecuteTaskSynchronously<T>(IExecutionContext context, Func<Task<T>> asyncMethod, [CallerMemberName]string memberName = "")
        {
            return context.Broker.ExecuteTaskSynchronously(asyncMethod);
        }

        public Environment Environment
        {
            get
            {
                lock (_syncpoint)
                {
                    if (ReferenceEquals(_environment, null))
                    {
                        _environment = ExecuteTaskSynchronously(() => RepositoryService.GetEnvironmentAsync(this.SharedWorkingDirectory, CancellationToken.None));
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
                        _repositoryDetails = ExecuteTaskSynchronously(() => RepositoryService.GetRepositoryDetailsAsync(this.SharedWorkingDirectory, CancellationToken.None));
                    }
                    return _repositoryDetails;
                }
            }
        }

        public string SharedIndexFile
        {
            get { return RepositoryDetails.SharedIndexFile; }
        }

        // TODO: What is this thing
        public object UserData
        {
            get { return Volatile.Read(ref _userData); }
            set { Volatile.Write(ref _userData, value); }
        }

        public string WorkingDirectory
        {
            get { return _repositoryDetails?.WorkingDirectory ?? _initialDirectory; }
        }

        // TODO cache this
        private string SharedWorkingDirectoryInternal
        {
            get { return this.Context.Broker.TranslateToSharedPathIfNecessary(this.WorkingDirectory, isDirectory: true); }
        }

        public IReadOnlyCollection<IUpdatedIndexEntry> Add(IEnumerable<string> paths, UpdateOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.AddAsync(this.SharedWorkingDirectoryInternal, paths, options, CancellationToken.None));
        }

        public void AddRemote(string url, string name, RemoteTagOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.AddRemoteAsync(this.SharedWorkingDirectoryInternal, url, name, options, CancellationToken.None));
        }

        public void BranchSetUpstream(IBranchName branch, IBranchName upstreamBranch)
        {
            ExecuteTaskSynchronously(() => RepositoryService.BranchSetUpstreamAsync(this.SharedWorkingDirectoryInternal, branch, upstreamBranch, CancellationToken.None));
        }

        public void BranchSetUpstream(IBranchName branch, IRemoteName upstreamRemote, IBranchName upstreamBranch)
        {
            ExecuteTaskSynchronously(() => RepositoryService.BranchSetUpstreamAsync(this.SharedWorkingDirectoryInternal, branch, upstreamRemote, upstreamBranch, CancellationToken.None));
        }

        public void BranchUnsetUpstream(IBranchName branch)
        {
            ExecuteTaskSynchronously(() => RepositoryService.BranchUnsetUpstreamAsync(this.SharedWorkingDirectoryInternal, branch, CancellationToken.None));
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

            ExecuteTaskSynchronously(context, () => RepositoryService.ConfigurationSetGlobalValueAsync(name, value, userData, CancellationToken.None));
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
            ExecuteTaskSynchronously(() => RepositoryService.ConfigurationSetValueAsync(this.SharedWorkingDirectoryInternal, name, value, level, CancellationToken.None));
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

            ExecuteTaskSynchronously(context, () => RepositoryService.ConfigurationUnsetGlobalValueAsync(name, userData, CancellationToken.None));
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
            ExecuteTaskSynchronously(() => RepositoryService.ConfigurationUnsetValueAsync(this.SharedWorkingDirectoryInternal, name, level, CancellationToken.None));
        }

        public void PushStash(StashPushOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.PushStashAsync(this.SharedWorkingDirectoryInternal, options, CancellationToken.None));
        }

        public IReadOnlyList<StashUpdatedFile> ApplyStash(int stashRevision, StashApplyOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ApplyStashAsync(this.SharedWorkingDirectoryInternal, stashRevision, options, CancellationToken.None));
        }

        public IReadOnlyList<StashUpdatedFile> PopStash(int stashRevision, StashApplyOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.PopStashAsync(this.SharedWorkingDirectoryInternal, stashRevision, options, CancellationToken.None));
        }

        public void DropStash(int stashRevision)
        {
            ExecuteTaskSynchronously(() => RepositoryService.DropStashAsync(this.SharedWorkingDirectoryInternal, stashRevision, CancellationToken.None));
        }

        public void ClearStash()
        {
            ExecuteTaskSynchronously(() => RepositoryService.ClearStashAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
        }

        public void Checkout(IRevision revision, CheckoutOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.CheckoutAsync(this.SharedWorkingDirectoryInternal, revision, options, CancellationToken.None));
        }

        public void CheckoutIndex(CheckoutIndexOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.CheckoutIndexAsync(this.SharedWorkingDirectoryInternal, options, CancellationToken.None));
        }

        public void CheckoutIndex(IEnumerable<string> paths, CheckoutIndexOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.CheckoutIndexAsync(this.SharedWorkingDirectoryInternal, paths, options, CancellationToken.None));
        }

        public IEnumerable<IUpdatedWorktreeEntry> Clean(CleanOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.CleanAsync(this.SharedWorkingDirectoryInternal, options, CancellationToken.None));
        }

        public void CherryPick(IRevision revision, CherryPickOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.CherryPickAsync(this.SharedWorkingDirectoryInternal, revision, options, CancellationToken.None));
        }

        public void CherryPick(InclusiveRange range, CherryPickOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.CherryPickAsync(this.SharedWorkingDirectoryInternal, range, options, CancellationToken.None));
        }

        public void CherryPickContinue()
        {
            ExecuteTaskSynchronously(() => RepositoryService.CherryPickContinueAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
        }

        public void CherryPickAbort()
        {
            ExecuteTaskSynchronously(() => RepositoryService.CherryPickAbortAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
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
            ExecuteTaskSynchronously(context, () => RepositoryService.CloneAsync(remoteUrl, workingDirectory, options, userData, CancellationToken.None));

            return new RemotableRepository(context, workingDirectory, userData);
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
            return ExecuteTaskSynchronously(() => RepositoryService.CommitAsync(this.SharedWorkingDirectoryInternal, message, options, CancellationToken.None));
        }

        public void CreateBranch(string branchName, IRevision createAt)
        {
            ExecuteTaskSynchronously(() => RepositoryService.CreateBranchAsync(this.SharedWorkingDirectoryInternal, branchName, createAt, CancellationToken.None));
        }

        public void CreateTag(IRevision revision, string name, string message, TagOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.CreateTagAsync(this.SharedWorkingDirectoryInternal, revision, name, message, options, CancellationToken.None));
        }

        public void DeleteBranch(string branchCanonicalName, DeleteBranchOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.DeleteBranchAsync(this.SharedWorkingDirectoryInternal, branchCanonicalName, options, CancellationToken.None));
        }

        public void DeleteBranch(IBranch branch, DeleteBranchOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.DeleteBranchAsync(this.SharedWorkingDirectoryInternal, branch, options, CancellationToken.None));
        }

        public void DeleteRemoteBranch(IRevision remoteBranch, IRemote remote, PushOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.DeleteRemoteBranchAsync(this.SharedWorkingDirectoryInternal, remoteBranch, remote, options, CancellationToken.None));
        }

        public void DeleteTag(ITagName tag)
        {
            ExecuteTaskSynchronously(() => RepositoryService.DeleteTagAsync(this.SharedWorkingDirectoryInternal, tag, CancellationToken.None));
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
            return ExecuteTaskSynchronously(() => RepositoryService.EnumerateCommitsAsync(this.SharedWorkingDirectoryInternal, revision, options, CancellationToken.None));
        }

        public IEnumerable<ICommit> EnumerateCommits(IRevision revision)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.EnumerateCommitsAsync(this.SharedWorkingDirectoryInternal, revision, CancellationToken.None));
        }

        public IEnumerable<ICommit> EnumerateCommits(IRevision since, IRevision until, HistoryOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.EnumerateCommitsAsync(this.SharedWorkingDirectoryInternal, since, until, options, CancellationToken.None));
        }

        public IEnumerable<ICommit> EnumerateCommits(IRevision since, IEnumerable<IRevision> until, HistoryOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.EnumerateCommitsAsync(this.SharedWorkingDirectoryInternal, since, until.ToArray(), options, CancellationToken.None));
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
            => RemotableRepository.Equals(this as IRepository, other);

        public bool Equals(RemotableRepository other)
            => RemotableRepository.Equals(this as IRepository, other as IRepository);

        public override bool Equals(object obj)
            => RemotableRepository.Equals(this as IRepository, obj as IRepository);

        public void Fetch(IRemote remote, FetchOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.FetchAsync(this.SharedWorkingDirectoryInternal, remote, options, CancellationToken.None));
        }

        public IReadOnlyList<string> GetCanonicalPaths(string path)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.GetCanonicalPathsAsync(this.SharedWorkingDirectoryInternal, path, CancellationToken.None));
        }

        public override int GetHashCode()
        {
            return PathComparer.GetHashCode(GitDirectory);
        }

        public ObjectId FindMergeBase(IRevision revisionA, IRevision revisionB)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.FindMergeBaseAsync(this.SharedWorkingDirectoryInternal, revisionA, revisionB, CancellationToken.None));
        }

        public bool HasCommits()
        {
            return ExecuteTaskSynchronously(() => RepositoryService.HasCommitsAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
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
            ExecuteTaskSynchronously(context, () => RepositoryService.InitializeAsync(workingDirectory, options, userData, CancellationToken.None));

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
            return ExecuteTaskSynchronously(() => RepositoryService.IsAncestorAsync(this.SharedWorkingDirectoryInternal, revisionA, revisionB, CancellationToken.None));
        }

        public MergeCommandResult Merge(IRevision revision, MergeOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.MergeAsync(this.SharedWorkingDirectoryInternal, revision, options, CancellationToken.None));
        }

        public void MergeAbort()
        {
            ExecuteTaskSynchronously(() => RepositoryService.MergeAbortAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
        }

        /// <summary>
        /// Opens an existing repository and returns an <see cref="IRepository"/> reference to it.
        /// </summary>
        /// <param name="context">The execution context settings for this instance.</param>
        /// <param name="workingDirectory">Path to the worktree of the repository.</param>
        /// <param name="userData">Optional data attached to any trace messages sent from this command to listeners.</param>
        public static IRepository Open(IExecutionContext context, string workingDirectory, object userData)
        {
            return new RemotableRepository(context, workingDirectory, userData);
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
            return new RemotableRepository(context, details, userData);
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
            return ExecuteTaskSynchronously(() => RepositoryService.OpenBlobFromRevisionAsync(this.SharedWorkingDirectoryInternal, revision, path, CancellationToken.None));
        }

        public Stream OpenBlob(ObjectId blobId, string path)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.OpenBlobFromObjectIdAsync(this.SharedWorkingDirectoryInternal, blobId, path, CancellationToken.None));
        }

        public void CopyBlobToStream(Stream stream, ObjectId blobId, string path)
        {
            // TODO: Cannot remote streams yet, and do not want to lose performance in the regular case.
            // For now we will serialize the entire file in remote cases, and call the faster method in local cases.
            if (RepositoryService is RepositoryService)
            {
                // not a remote proxy
                ExecuteTaskSynchronously(() => RepositoryService.CopyBlobFromObjectIdToStreamAsync(this.SharedWorkingDirectoryInternal, stream, blobId, path, CancellationToken.None));
            }
            else
            {
                ExecuteTaskSynchronously(async () =>
                {
                    var contents = await RepositoryService.GetBlobContentsFromObjectIdWorkaroundAsync(this.SharedWorkingDirectoryInternal, blobId, path, CancellationToken.None);
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        foreach (string line in contents)
                        {
                            await writer.WriteLineAsync(line);
                        }
                    }
                });
            }
        }

        public Stream OpenBlob(IndexVersion index, string path)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.OpenBlobFromIndexVersionAsync(this.SharedWorkingDirectoryInternal, index, path, CancellationToken.None));
        }

        public IDifferenceEngine OpenDifferenceEngine(DifferenceOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.OpenDifferenceEngineAsync(this.SharedWorkingDirectoryInternal, options, CancellationToken.None));
        }

        public IObjectDatabase OpenObjectDatabase()
        {
            return new RemotableObjectDatabase(this);
        }

        public PullCommandResult Pull(IRemote remote, PullOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.PullAsync(this.SharedWorkingDirectoryInternal, remote, options, CancellationToken.None));
        }

        public void Push(IBranch localBranch, IRemote remote, PushOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.PushAsync(this.SharedWorkingDirectoryInternal, localBranch, remote, options, CancellationToken.None));
        }

        public void Push(IBranch localBranch, IBranch remoteBranch, PushOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.PushAsync(this.SharedWorkingDirectoryInternal, localBranch, remoteBranch, options, CancellationToken.None));
        }

        public void Push(IBranchName localBranch, IRemoteName remote, IBranchName remoteBranch, PushOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.PushAsync(this.SharedWorkingDirectoryInternal, localBranch, remote, remoteBranch, options, CancellationToken.None));
        }

        public void Push(ITagName localTag, IRemote remote, PushOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.PushAsync(this.SharedWorkingDirectoryInternal, localTag, remote, options, CancellationToken.None));
        }

        public void PushAllTags(IRemote remote, PushOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.PushAllTagsAsync(this.SharedWorkingDirectoryInternal, remote, options, CancellationToken.None));
        }

        public long ReadCommitCount(InclusiveRange range, HistoryOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadCommitCountAsync(this.SharedWorkingDirectoryInternal, range, options, CancellationToken.None));
        }

        public ITreeDifference ReadCommitTreeDifference(ICommit commit, DifferenceOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadCommitTreeDifferenceAsync(this.SharedWorkingDirectoryInternal, commit, options, CancellationToken.None));
        }

        public string ReadCurrentBranchName(bool shortName, out HeadType headType)
        {
            var result = ExecuteTaskSynchronously(() => RepositoryService.ReadCurrentBranchNameAsync(this.SharedWorkingDirectoryInternal, shortName, CancellationToken.None));
            headType = result.Item2;
            return result.Item1;
        }

        public ObjectId ReadCurrentHeadId()
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadCurrentHeadIdAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
        }

        public string ReadCurrentHeadValue()
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadCurrentHeadValueAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
        }

        public IReadOnlyList<ConfigurationEntry> ReadConfigList()
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadConfigListAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
        }

        public static IReadOnlyList<ConfigurationEntry> ReadGlobalConfigList(IExecutionContext context, object userData)
        {
            return ExecuteTaskSynchronously(context, () => RepositoryService.ReadGlobalConfigListAsync(userData, CancellationToken.None));
        }

        public static IReadOnlyList<ConfigurationEntry> ReadGlobalConfigList(object userData)
            => ReadGlobalConfigList(ExecutionContext.Current, userData);

        public static IReadOnlyList<ConfigurationEntry> ReadGlobalConfigList()
            => ReadGlobalConfigList(ExecutionContext.Current, null);

        public string ReadCurrentMergeMessage()
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadCurrentMergeMessageAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
        }

        public RepositoryCurrentOperation ReadCurrentOperation()
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadCurrentOperationAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
        }

        public IReadOnlyDictionary<string, ITreeDifferenceDetail> ReadFileInfo(string path)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadFileInfoAsync(this.SharedWorkingDirectoryInternal, path, CancellationToken.None));
        }

        public IHead ReadHead()
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadHeadAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
        }

        public ObjectHeader ReadHeader(ObjectId objectId, string path)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadHeaderAsync(this.SharedWorkingDirectoryInternal, objectId, path, CancellationToken.None));
        }

        public bool ReadIsIndexLocked()
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadIsIndexLockedAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
        }

        public ITreeDifference ReadIndexTreeDifference(DifferenceOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadIndexTreeDifferenceAsync(this.SharedWorkingDirectoryInternal, options, CancellationToken.None));
        }

        public T ReadObject<T>(ObjectId objectId)
            where T : class, IObject
        {
            if (typeof(IBlob).IsAssignableFrom(typeof(T)))
            {
                return ExecuteTaskSynchronously(() => RepositoryService.ReadBlobAsync(this.SharedWorkingDirectoryInternal, objectId, CancellationToken.None)) as T;
            }
            else if (typeof(ICommit).IsAssignableFrom(typeof(T)))
            {
                return ExecuteTaskSynchronously(() => RepositoryService.ReadCommitAsync(this.SharedWorkingDirectoryInternal, objectId, CancellationToken.None)) as T;
            }
            else if (typeof(ITree).IsAssignableFrom(typeof(T)))
            {
                return ExecuteTaskSynchronously(() => RepositoryService.ReadTreeAsync(this.SharedWorkingDirectoryInternal, objectId, CancellationToken.None)) as T;
            }
            else
            {
                throw new ArgumentException("Unexpected type requested from ReadObject");
            }
        }

        public IReferenceCollection ReadReferences(ReferenceOptions options)
        {
            // TODO this wrapping/unwrapping is unfortunate; can it be fixed?
            var result = ExecuteTaskSynchronously(() => RepositoryService.ReadReferencesAsync(this.SharedWorkingDirectoryInternal, options, CancellationToken.None));
            var collection = new ReferenceCollection(options.Flags);
            foreach (IReference reference in result.Item1)
            {
                collection.Add(reference);
            }
            collection.Head = result.Item2;
            return collection;
        }

        public IRemoteCollection ReadRemotes()
        {
            // TODO this wrapping/unwrapping is unfortunate; can it be fixed?
            var result = ExecuteTaskSynchronously(() => RepositoryService.ReadRemotesAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
            var collection = new RemoteCollection();
            foreach(IRemote remote in result)
            {
                collection.Add(remote);
            }
            return collection;
        }

        public ObjectDatabaseDetails ReadObjectDatabaseDetails()
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadObjectDatabaseDetailsAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
        }

        public ICommit ReadRevision(IRevision revision)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadRevisionAsync(this.SharedWorkingDirectoryInternal, revision, CancellationToken.None));
        }

        public IReadOnlyList<IStash> ReadStashList(int maxCount)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadStashListAsync(this.SharedWorkingDirectoryInternal, maxCount, CancellationToken.None));
        }

        public IReadOnlyList<IStash> ReadStashList()
            => ReadStashList(-1);

        public IStatusSnapshot ReadStatus(StatusOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadStatusAsync(this.SharedWorkingDirectoryInternal, options, CancellationToken.None));
        }

        public ITreeDifference ReadTreeDifference(ICommit source, ICommit target, DifferenceOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadTreeDifferenceAsync(this.SharedWorkingDirectoryInternal, source, target, options, CancellationToken.None));
        }

        public ITreeDifference ReadTreeDifference(ITree source, ITree target, DifferenceOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadTreeDifferenceAsync(this.SharedWorkingDirectoryInternal, source, target, options, CancellationToken.None));
        }

        public ITreeDifference ReadTreeDifference(IRevision source, IRevision target, DifferenceOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadTreeDifferenceAsync(this.SharedWorkingDirectoryInternal, source, target, options, CancellationToken.None));
        }

        public ITreeDifference ReadStatusIndex(DifferenceOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadStatusIndexAsync(this.SharedWorkingDirectoryInternal, options, CancellationToken.None));
        }

        public ITreeDifference ReadStatusWorktree(DifferenceOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ReadStatusWorktreeAsync(this.SharedWorkingDirectoryInternal, options, CancellationToken.None));
        }

        public RebaseResult RebaseAbort(OperationProgressDelegate progressCallback)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.RebaseAbortAsync(this.SharedWorkingDirectoryInternal, progressCallback, CancellationToken.None));
        }

        public RebaseResult RebaseBegin(IRevision upstream, RebaseOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.RebaseBeginAsync(this.SharedWorkingDirectoryInternal, upstream, options, CancellationToken.None));
        }

        public RebaseResult RebaseContinue(OperationProgressDelegate progressCallback)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.RebaseContinueAsync(this.SharedWorkingDirectoryInternal, progressCallback, CancellationToken.None));
        }

        public RebaseResult RebaseSkip(OperationProgressDelegate progressCallback)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.RebaseSkipAsync(this.SharedWorkingDirectoryInternal, progressCallback, CancellationToken.None));
        }

        public void RemoveUpstream(IBranch branch)
        {
            ExecuteTaskSynchronously(() => RepositoryService.RemoveUpstreamAsync(this.SharedWorkingDirectoryInternal, branch, CancellationToken.None));
        }

        public void RenameBranch(IBranchName sourceBranchName, string newBranchName)
        {
            ExecuteTaskSynchronously(() => RepositoryService.RenameBranchAsync(this.SharedWorkingDirectoryInternal, sourceBranchName, newBranchName, CancellationToken.None));
        }

        public void RenameBranch(string sourceBranchName, string newBranchName)
            => RenameBranch(new BranchName(sourceBranchName), newBranchName);

        public void RenameRemote(string oldName, string newName)
        {
            ExecuteTaskSynchronously(() => RepositoryService.RenameRemoteAsync(this.SharedWorkingDirectoryInternal, oldName, newName, CancellationToken.None));
            new Cli.RemoteCommand(Context, this).Rename(oldName, newName);
        }

        public void RemoveRemote(string name)
        {
            ExecuteTaskSynchronously(() => RepositoryService.RemoveRemoteAsync(this.SharedWorkingDirectoryInternal, name, CancellationToken.None));
        }

        public void ResetHard(IRevision revision, OperationProgressDelegate progressCallback)
        {
            ExecuteTaskSynchronously(() => RepositoryService.ResetHardAsync(this.SharedWorkingDirectoryInternal, revision, progressCallback, CancellationToken.None));
        }

        public bool ResetKeep(IRevision revision)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ResetKeepAsync(this.SharedWorkingDirectoryInternal, revision, CancellationToken.None));
        }

        public bool ResetMerge(IRevision revision)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.ResetMergeAsync(this.SharedWorkingDirectoryInternal, revision, CancellationToken.None));
        }

        public void ResetMixed(IRevision revision)
        {
            ExecuteTaskSynchronously(() => RepositoryService.ResetMixedAsync(this.SharedWorkingDirectoryInternal, revision, CancellationToken.None));
        }

        public void ResetPaths(ICollection<string> paths)
        {
            ExecuteTaskSynchronously(() => RepositoryService.ResetPathsAsync(this.SharedWorkingDirectoryInternal, paths, CancellationToken.None));
        }

        public void ResetSoft(IRevision revision)
        {
            ExecuteTaskSynchronously(() => RepositoryService.ResetSoftAsync(this.SharedWorkingDirectoryInternal, revision, CancellationToken.None));
        }

        public RevertResult Revert(IRevision revision, RevertOptions options)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.RevertAsync(this.SharedWorkingDirectoryInternal, revision, options, CancellationToken.None));
        }

        public RevertResult RevertAbort(OperationProgressDelegate progressCallback)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.RevertAbortAsync(this.SharedWorkingDirectoryInternal, progressCallback, CancellationToken.None));
        }

        public RevertResult RevertContinue(OperationProgressDelegate progressCallback)
        {
            return ExecuteTaskSynchronously(() => RepositoryService.RevertContinueAsync(this.SharedWorkingDirectoryInternal, progressCallback, CancellationToken.None));
        }

        public void SetRemoteFetchUrl(string name, string url)
        {
            ExecuteTaskSynchronously(() => RepositoryService.SetRemoteFetchUrlAsync(this.SharedWorkingDirectoryInternal, name, url, CancellationToken.None));
        }

        public void SetRemotePushUrl(string name, string url)
        {
            ExecuteTaskSynchronously(() => RepositoryService.SetRemotePushUrlAsync(this.SharedWorkingDirectoryInternal, name, url, CancellationToken.None));
        }

        public void SetUpstream(IBranch branch, IBranch upstream)
        {
            ExecuteTaskSynchronously(() => RepositoryService.SetUpstreamAsync(this.SharedWorkingDirectoryInternal, branch, upstream, CancellationToken.None));
        }

        public void SubmoduleUpdate(SubmoduleUpdateOptions options)
        {
            ExecuteTaskSynchronously(() => RepositoryService.SubmoduleUpdateAsync(this.SharedWorkingDirectoryInternal, options, CancellationToken.None));
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

        // TODO: This won't stand up to multi repo scenarios, etc. Revisit later.
        public string SharedWorkingDirectory
        {
            get
            {
                return ExecuteTaskSynchronously(() => RepositoryService.GetSharedWorkingDirectoryAsync(this.SharedWorkingDirectoryInternal, CancellationToken.None));
            }
        }

        public string LocalWorkingDirectory
        {
            get
            {
                return Context.Broker.TranslateFromSharedPathIfNecessary(SharedWorkingDirectory, false);
            }
        }
    }

    // TODO: Not every single one of these params/return values is remotable
    public interface IRepositoryService
    {
        Task<Environment> GetEnvironmentAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<object> GetUserDataAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<IRepositoryDetails> GetRepositoryDetailsAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<IUpdatedIndexEntry>> AddAsync(string repositoryPath, IEnumerable<string> paths, UpdateOptions options, CancellationToken cancellationToken);
        Task AddRemoteAsync(string repositoryPath, string url, string name, RemoteTagOptions options, CancellationToken cancellationToken);
        Task BranchSetUpstreamAsync(string repositoryPath, IBranchName branch, IBranchName upstreamBranch, CancellationToken cancellationToken);
        Task BranchSetUpstreamAsync(string repositoryPath, IBranchName branch, IRemoteName upstreamRemote, IBranchName upstreamBranch, CancellationToken cancellationToken);
        Task BranchUnsetUpstreamAsync(string repositoryPath, IBranchName branch, CancellationToken cancellationToken);
        Task RemoveRemoteAsync(string repositoryPath, string name, CancellationToken cancellationToken);
        Task RenameRemoteAsync(string repositoryPath, string oldName, string newName, CancellationToken cancellationToken);
        Task SetRemoteFetchUrlAsync(string repositoryPath, string name, string url, CancellationToken cancellationToken);
        Task SetRemotePushUrlAsync(string repositoryPath, string name, string url, CancellationToken cancellationToken);
        Task CheckoutAsync(string repositoryPath, IRevision revision, CheckoutOptions options, CancellationToken cancellationToken);
        Task CheckoutIndexAsync(string repositoryPath, CheckoutIndexOptions options, CancellationToken cancellationToken);
        Task CheckoutIndexAsync(string repositoryPath, IEnumerable<string> paths, CheckoutIndexOptions options, CancellationToken cancellationToken);
        Task CherryPickAsync(string repositoryPath, IRevision revision, CherryPickOptions options, CancellationToken cancellationToken);
        Task CherryPickAsync(string repositoryPath, InclusiveRange range, CherryPickOptions options, CancellationToken cancellationToken);
        Task CherryPickContinueAsync(string repositoryPath, CancellationToken cancellationToken);
        Task CherryPickAbortAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<IEnumerable<IUpdatedWorktreeEntry>> CleanAsync(string repositoryPath, CleanOptions options, CancellationToken cancellationToken);
        Task<ObjectId> CommitAsync(string repositoryPath, string message, CommitOptions options, CancellationToken cancellationToken);
        Task CreateBranchAsync(string repositoryPath, string branchName, IRevision createAt, CancellationToken cancellationToken);
        Task CreateTagAsync(string repositoryPath, IRevision revision, string name, string message, TagOptions options, CancellationToken cancellationToken);
        Task DeleteBranchAsync(string repositoryPath, string branchName, DeleteBranchOptions options, CancellationToken cancellationToken);
        Task DeleteBranchAsync(string repositoryPath, IBranch branch, DeleteBranchOptions options, CancellationToken cancellationToken);
        Task DeleteRemoteBranchAsync(string repositoryPath, IRevision remoteBranch, IRemote remote, PushOptions options, CancellationToken cancellationToken);
        Task DeleteTagAsync(string repositoryPath, ITagName tag, CancellationToken cancellationToken);
        // Note: Changed from IEnumerable<ICommit>. Perf issue?
        Task<ICommit[]> EnumerateCommitsAsync(string repositoryPath, IRevision revision, HistoryOptions options, CancellationToken cancellationToken);
        Task<ICommit[]> EnumerateCommitsAsync(string repositoryPath, IRevision revision, CancellationToken cancellationToken);
        Task<ICommit[]> EnumerateCommitsAsync(string repositoryPath, IRevision since, IRevision until, HistoryOptions options, CancellationToken cancellationToken);
        Task<ICommit[]> EnumerateCommitsAsync(string repositoryPath, IRevision since, IRevision[] untilArray, HistoryOptions options, CancellationToken cancellationToken);
        Task FetchAsync(string repositoryPath, IRemote remote, FetchOptions options, CancellationToken cancellationToken);
        Task<ObjectId> FindMergeBaseAsync(string repositoryPath, IRevision revisionA, IRevision revisionB, CancellationToken cancellationToken);
        Task<bool> HasCommitsAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<bool> IsAncestorAsync(string repositoryPath, IRevision revisionA, IRevision revisionB, CancellationToken cancellationToken);
        Task<IReadOnlyList<string>> GetCanonicalPathsAsync(string repositoryPath, string path, CancellationToken cancellationToken);
        Task<IReadOnlyDictionary<string, ITreeDifferenceDetail>> ReadFileInfoAsync(string repositoryPath, string path, CancellationToken cancellationToken);
        Task<MergeCommandResult> MergeAsync(string repositoryPath, IRevision revision, MergeOptions options, CancellationToken cancellationToken);
        Task MergeAbortAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<Stream> OpenBlobFromRevisionAsync(string repositoryPath, IRevision revision, string path, CancellationToken cancellationToken);
        Task<Stream> OpenBlobFromObjectIdAsync(string repositoryPath, ObjectId blobId, string path, CancellationToken cancellationToken);
        Task<string[]> GetBlobContentsFromObjectIdWorkaroundAsync(string repositoryPath, ObjectId blobId, string path, CancellationToken cancellationToken);
        Task<Stream> OpenBlobFromIndexVersionAsync(string repositoryPath, IndexVersion index, string path, CancellationToken cancellationToken);
        Task CopyBlobFromObjectIdToStreamAsync(string repositoryPath, Stream stream, ObjectId blobId, string path, CancellationToken cancellationToken);
        Task<IDifferenceEngine> OpenDifferenceEngineAsync(string repositoryPath, DifferenceOptions options, CancellationToken cancellationToken);
        Task<PullCommandResult> PullAsync(string repositoryPath, IRemote remote, PullOptions options, CancellationToken cancellationToken);
        Task PushAsync(string repositoryPath, IBranch localBranch, IRemote remote, PushOptions options, CancellationToken cancellationToken);
        Task PushAsync(string repositoryPath, IBranch localBranch, IBranch remoteBranch, PushOptions options, CancellationToken cancellationToken);
        Task PushAsync(string repositoryPath, IBranchName localBranch, IRemoteName remote, IBranchName remoteBranch, PushOptions options, CancellationToken cancellationToken);
        Task PushAsync(string repositoryPath, ITagName localTag, IRemote remote, PushOptions options, CancellationToken cancellationToken);
        Task PushAllTagsAsync(string repositoryPath, IRemote remote, PushOptions options, CancellationToken cancellationToken);
        Task<long> ReadCommitCountAsync(string repositoryPath, InclusiveRange range, HistoryOptions options, CancellationToken cancellationToken);
        Task<ITreeDifference> ReadCommitTreeDifferenceAsync(string repositoryPath, ICommit commit, DifferenceOptions options, CancellationToken cancellationToken);
        Task<Tuple<string, HeadType>> ReadCurrentBranchNameAsync(string repositoryPath, bool shortName, CancellationToken cancellationToken);
        Task<ObjectId> ReadCurrentHeadIdAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<string> ReadCurrentHeadValueAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<IReadOnlyList<ConfigurationEntry>> ReadConfigListAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<string> ReadCurrentMergeMessageAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<RepositoryCurrentOperation> ReadCurrentOperationAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<IReadOnlyList<ConfigurationEntry>> ReadGlobalConfigListAsync(object userData, CancellationToken cancellationToken);
        Task<IHead> ReadHeadAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<ObjectHeader> ReadHeaderAsync(string repositoryPath, ObjectId objectId, string path, CancellationToken cancellationToken);
        Task<bool> ReadIsIndexLockedAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<ITreeDifference> ReadIndexTreeDifferenceAsync(string repositoryPath, DifferenceOptions options, CancellationToken cancellationToken);
        // TODO: Can't have generics
        Task<IBlob> ReadBlobAsync(string repositoryPath, ObjectId objectId, CancellationToken cancellationToken);
        Task<ICommit> ReadCommitAsync(string repositoryPath, ObjectId objectId, CancellationToken cancellationToken);
        Task<ITree> ReadTreeAsync(string repositoryPath, ObjectId objectId, CancellationToken cancellationToken);
        Task<Tuple<IReference[], IHead>> ReadReferencesAsync(string repositoryPath, ReferenceOptions options, CancellationToken cancellationToken);
        Task<IRemote[]> ReadRemotesAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<ObjectDatabaseDetails> ReadObjectDatabaseDetailsAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<ICommit> ReadRevisionAsync(string repositoryPath, IRevision revision, CancellationToken cancellationToken);
        Task<IReadOnlyList<IStash>> ReadStashListAsync(string repositoryPath, int maxCount, CancellationToken cancellationToken);
        Task<IReadOnlyList<IStash>> ReadStashListAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<IStatusSnapshot> ReadStatusAsync(string repositoryPath, StatusOptions options, CancellationToken cancellationToken);
        Task<ITreeDifference> ReadStatusIndexAsync(string repositoryPath, DifferenceOptions options, CancellationToken cancellationToken);
        Task<ITreeDifference> ReadTreeDifferenceAsync(string repositoryPath, ICommit source, ICommit target, DifferenceOptions options, CancellationToken cancellationToken);
        Task<ITreeDifference> ReadTreeDifferenceAsync(string repositoryPath, ITree source, ITree target, DifferenceOptions options, CancellationToken cancellationToken);
        Task<ITreeDifference> ReadTreeDifferenceAsync(string repositoryPath, IRevision source, IRevision target, DifferenceOptions options, CancellationToken cancellationToken);
        Task<ITreeDifference> ReadStatusWorktreeAsync(string repositoryPath, DifferenceOptions options, CancellationToken cancellationToken);
        // TODO: Can't have this type of callback
        Task<RebaseResult> RebaseAbortAsync(string repositoryPath, OperationProgressDelegate progressCallback, CancellationToken cancellationToken);
        Task<RebaseResult> RebaseBeginAsync(string repositoryPath, IRevision upstream, RebaseOptions options, CancellationToken cancellationToken);
        // TODO: Can't have this type of callback
        Task<RebaseResult> RebaseContinueAsync(string repositoryPath, OperationProgressDelegate progressCallback, CancellationToken cancellationToken);
        // TODO: Can't have this type of callback
        Task<RebaseResult> RebaseSkipAsync(string repositoryPath, OperationProgressDelegate progressCallback, CancellationToken cancellationToken);
        Task RemoveUpstreamAsync(string repositoryPath, IBranch branch, CancellationToken cancellationToken);
        Task RenameBranchAsync(string repositoryPath, IBranchName sourceBranchName, string newBranchName, CancellationToken cancellationToken);
        Task RenameBranchAsync(string repositoryPath, string sourceBranchName, string newBranchName, CancellationToken cancellationToken);
        // TODO: Can't have this type of callback
        Task ResetHardAsync(string repositoryPath, IRevision revision, OperationProgressDelegate progressCallback, CancellationToken cancellationToken);
        Task<bool> ResetKeepAsync(string repositoryPath, IRevision revision, CancellationToken cancellationToken);
        Task<bool> ResetMergeAsync(string repositoryPath, IRevision revision, CancellationToken cancellationToken);
        Task ResetMixedAsync(string repositoryPath, IRevision revision, CancellationToken cancellationToken);
        Task ResetPathsAsync(string repositoryPath, ICollection<string> paths, CancellationToken cancellationToken);
        Task ResetSoftAsync(string repositoryPath, IRevision revision, CancellationToken cancellationToken);
        Task<RevertResult> RevertAsync(string repositoryPath, IRevision revision, RevertOptions options, CancellationToken cancellationToken);
        // TODO: Can't have this type of callback
        Task<RevertResult> RevertAbortAsync(string repositoryPath, OperationProgressDelegate progressCallback, CancellationToken cancellationToken);
        // TODO: Can't have this type of callback
        Task<RevertResult> RevertContinueAsync(string repositoryPath, OperationProgressDelegate progressCallback, CancellationToken cancellationToken);
        Task ConfigurationSetGlobalValueAsync(string name, string value, object userData, CancellationToken cancellationToken);
        Task ConfigurationSetValueAsync(string repositoryPath, string name, string value, ConfigurationLevel level, CancellationToken cancellationToken);
        Task CloneAsync(string remoteUrl, string workingDirectory, CloneOptions options, object userData, CancellationToken cancellationToken);
        Task InitializeAsync(string workingDirectory, InitializationOptions options, object userData, CancellationToken cancellationToken);
        Task SetUpstreamAsync(string repositoryPath, IBranch branch, IBranch upstream, CancellationToken cancellationToken);
        Task SubmoduleUpdateAsync(string repositoryPath, SubmoduleUpdateOptions options, CancellationToken cancellationToken);
        Task ConfigurationUnsetGlobalValueAsync(string name, object userData, CancellationToken cancellationToken);
        Task ConfigurationUnsetValueAsync(string repositoryPath, string name, ConfigurationLevel level, CancellationToken cancellationToken);
        Task PushStashAsync(string repositoryPath, StashPushOptions options, CancellationToken cancellationToken);
        Task<IReadOnlyList<StashUpdatedFile>> ApplyStashAsync(string repositoryPath, int stashRevision, StashApplyOptions options, CancellationToken cancellationToken);
        Task<IReadOnlyList<StashUpdatedFile>> PopStashAsync(string repositoryPath, int stashRevision, StashApplyOptions options, CancellationToken cancellationToken);
        Task DropStashAsync(string repositoryPath, int stashRevision, CancellationToken cancellationToken);
        Task ClearStashAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<string> GetSharedWorkingDirectoryAsync(string repositoryPath, CancellationToken cancellationToken);
    }

    public sealed class RepositoryService : IRepositoryService
    {
        private static Dictionary<string, Repository> repositoryLookup = new Dictionary<string, Repository>();

        internal static Repository GetRepository(string repositoryPath)
        {
            repositoryPath = ExecutionContext.Current.Broker.TranslateFromSharedPathIfNecessary(repositoryPath, isDirectory: true);

            if (!repositoryLookup.TryGetValue(repositoryPath, out Repository repositoryServiceImpl))
            {
                repositoryServiceImpl = new Repository(ExecutionContext.Current, repositoryPath);
                //repositoryLookup[repositoryPath] = repositoryServiceImpl;
            }

            // TODO: update access time for keep-alive

            return repositoryServiceImpl;
        }

        public Task<IReadOnlyCollection<IUpdatedIndexEntry>> AddAsync(string repositoryPath, IEnumerable<string> paths, UpdateOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.Add(paths, options);
            return Task.FromResult(result);
        }

        public Task AddRemoteAsync(string repositoryPath, string url, string name, RemoteTagOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.AddRemote(url, name, options);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<StashUpdatedFile>> ApplyStashAsync(string repositoryPath, int stashRevision, StashApplyOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ApplyStash(stashRevision, options);
            return Task.FromResult(result);
        }

        public Task BranchSetUpstreamAsync(string repositoryPath, IBranchName branch, IBranchName upstreamBranch, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.BranchSetUpstream(branch, upstreamBranch);
            return Task.CompletedTask;
        }

        public Task BranchSetUpstreamAsync(string repositoryPath, IBranchName branch, IRemoteName upstreamRemote, IBranchName upstreamBranch, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.BranchSetUpstream(branch, upstreamRemote, upstreamBranch);
            return Task.CompletedTask;
        }

        public Task BranchUnsetUpstreamAsync(string repositoryPath, IBranchName branch, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.BranchUnsetUpstream(branch);
            return Task.CompletedTask;
        }

        public Task CheckoutAsync(string repositoryPath, IRevision revision, CheckoutOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.Checkout(revision, options);
            return Task.CompletedTask;
        }

        public Task CheckoutIndexAsync(string repositoryPath, CheckoutIndexOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.CheckoutIndex(options);
            return Task.CompletedTask;
        }

        public Task CheckoutIndexAsync(string repositoryPath, IEnumerable<string> paths, CheckoutIndexOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.CheckoutIndex(paths, options);
            return Task.CompletedTask;
        }

        public Task CherryPickAbortAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.CherryPickAbort();
            return Task.CompletedTask;
        }

        public Task CherryPickAsync(string repositoryPath, IRevision revision, CherryPickOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.CherryPick(revision, options);
            return Task.CompletedTask;
        }

        public Task CherryPickAsync(string repositoryPath, InclusiveRange range, CherryPickOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.CherryPick(range, options);
            return Task.CompletedTask;
        }

        public Task CherryPickContinueAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.CherryPickContinue();
            return Task.CompletedTask;
        }

        public Task<IEnumerable<IUpdatedWorktreeEntry>> CleanAsync(string repositoryPath, CleanOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.Clean(options);
            return Task.FromResult(result);
        }

        public Task ClearStashAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.ClearStash();
            return Task.CompletedTask;
        }

        public Task CloneAsync(string remoteUrl, string workingDirectory, CloneOptions options, object userData, CancellationToken cancellationToken)
        {
            Repository.Clone(remoteUrl, workingDirectory, options, userData);
            return Task.CompletedTask;
        }

        public Task<ObjectId> CommitAsync(string repositoryPath, string message, CommitOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.Commit(message, options);
            return Task.FromResult(result);
        }

        public Task ConfigurationSetGlobalValueAsync(string name, string value, object userData, CancellationToken cancellationToken)
        {
            Repository.ConfigurationSetGlobalValue(name, value, userData);
            return Task.CompletedTask;
        }

        public Task ConfigurationSetValueAsync(string repositoryPath, string name, string value, ConfigurationLevel level, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.ConfigurationSetValue(name, value, level);
            return Task.CompletedTask;
        }

        public Task ConfigurationUnsetGlobalValueAsync(string name, object userData, CancellationToken cancellationToken)
        {
            Repository.ConfigurationUnsetGlobalValue(name, userData);
            return Task.CompletedTask;
        }

        public Task ConfigurationUnsetValueAsync(string repositoryPath, string name, ConfigurationLevel level, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.ConfigurationUnsetValue(name, level);
            return Task.CompletedTask;
        }

        public Task CreateBranchAsync(string repositoryPath, string branchName, IRevision createAt, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.CreateBranch(branchName, createAt);
            return Task.CompletedTask;
        }

        public Task CreateTagAsync(string repositoryPath, IRevision revision, string name, string message, TagOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.CreateTag(revision, name, message, options);
            return Task.CompletedTask;
        }

        public Task DeleteBranchAsync(string repositoryPath, string branchName, DeleteBranchOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.DeleteBranch(branchName, options);
            return Task.CompletedTask;
        }

        public Task DeleteBranchAsync(string repositoryPath, IBranch branch, DeleteBranchOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.DeleteBranch(branch, options);
            return Task.CompletedTask;
        }

        public Task DeleteRemoteBranchAsync(string repositoryPath, IRevision remoteBranch, IRemote remote, PushOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.DeleteRemoteBranch(remoteBranch, remote, options);
            return Task.CompletedTask;
        }

        public Task DeleteTagAsync(string repositoryPath, ITagName tag, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.DeleteTag(tag);
            return Task.CompletedTask;
        }

        public Task DropStashAsync(string repositoryPath, int stashRevision, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.DropStash(stashRevision);
            return Task.CompletedTask;
        }

        public Task<ICommit[]> EnumerateCommitsAsync(string repositoryPath, IRevision revision, HistoryOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.EnumerateCommits(revision, options);
            return Task.FromResult(result.ToArray());
        }

        public Task<ICommit[]> EnumerateCommitsAsync(string repositoryPath, IRevision revision, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.EnumerateCommits(revision);
            return Task.FromResult(result.ToArray());
        }

        public Task<ICommit[]> EnumerateCommitsAsync(string repositoryPath, IRevision since, IRevision until, HistoryOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.EnumerateCommits(since, until, options);
            return Task.FromResult(result.ToArray());
        }

        public Task<ICommit[]> EnumerateCommitsAsync(string repositoryPath, IRevision since, IRevision[] untilArray, HistoryOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.EnumerateCommits(since, untilArray, options);
            return Task.FromResult(result.ToArray());
        }

        public Task FetchAsync(string repositoryPath, IRemote remote, FetchOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.Fetch(remote, options);
            return Task.CompletedTask;
        }

        public Task<ObjectId> FindMergeBaseAsync(string repositoryPath, IRevision revisionA, IRevision revisionB, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.FindMergeBase(revisionA, revisionB);
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<string>> GetCanonicalPathsAsync(string repositoryPath, string path, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.GetCanonicalPaths(path);
            return Task.FromResult(result);
        }

        public Task<Environment> GetEnvironmentAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.Environment;
            return Task.FromResult(result);
        }

        public Task<IRepositoryDetails> GetRepositoryDetailsAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.RepositoryDetails;

            if (repository.Context.Broker.TranslateToSharedPathIfNecessary(result.WorkingDirectory, isDirectory: true) != result.WorkingDirectory)
            {
                // untranslated paths are relative
                result = new RepositoryDetails(result.CommonDirectory,
                    result.DescriptionFile,
                    repository.Context.Broker.TranslateToSharedPathIfNecessary(result.GitDirectory, isDirectory: true),
                    result.HooksDirectory,
                    result.IndexFile,
                    result.InfoDirectory,
                    result.LogsDirectory,
                    result.ObjectsDirectory,
                    result.SharedIndexFile,
                    repository.Context.Broker.TranslateToSharedPathIfNecessary(result.WorkingDirectory, isDirectory: true),
                    result.IsBareRepository);
            }

            return Task.FromResult(result);
        }

        public Task<object> GetUserDataAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.UserData;
            return Task.FromResult(result);
        }

        public Task<bool> HasCommitsAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.HasCommits();
            return Task.FromResult(result);
        }

        public Task InitializeAsync(string workingDirectory, InitializationOptions options, object userData, CancellationToken cancellationToken)
        {
            Repository.Initialize(workingDirectory, options, userData);
            return Task.CompletedTask;
        }

        public Task<bool> IsAncestorAsync(string repositoryPath, IRevision revisionA, IRevision revisionB, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.IsAncestor(revisionA, revisionB);
            return Task.FromResult(result);
        }

        public Task MergeAbortAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.MergeAbort();
            return Task.CompletedTask;
        }

        public Task<MergeCommandResult> MergeAsync(string repositoryPath, IRevision revision, MergeOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.Merge(revision, options);
            return Task.FromResult(result);
        }

        public Task<Stream> OpenBlobFromRevisionAsync(string repositoryPath, IRevision revision, string path, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.OpenBlob(revision, path);
            return Task.FromResult(result);
        }

        public Task<Stream> OpenBlobFromObjectIdAsync(string repositoryPath, ObjectId blobId, string path, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.OpenBlob(blobId, path);
            return Task.FromResult(result);
        }

        public Task<string[]> GetBlobContentsFromObjectIdWorkaroundAsync(string repositoryPath, ObjectId blobId, string path, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var stream = repository.OpenBlob(blobId, path);
            List<string> lines = new List<string>();

            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }
            }

            return Task.FromResult(lines.ToArray());
        }

        public Task<Stream> OpenBlobFromIndexVersionAsync(string repositoryPath, IndexVersion index, string path, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.OpenBlob(index, path);
            return Task.FromResult(result);
        }

        public Task CopyBlobFromObjectIdToStreamAsync(string repositoryPath, Stream stream, ObjectId blobId, string path, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.CopyBlobToStream(stream, blobId, path);
            return Task.CompletedTask;
        }

        public Task<IDifferenceEngine> OpenDifferenceEngineAsync(string repositoryPath, DifferenceOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.OpenDifferenceEngine(options);
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<StashUpdatedFile>> PopStashAsync(string repositoryPath, int stashRevision, StashApplyOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.PopStash(stashRevision, options);
            return Task.FromResult(result);
        }

        public Task<PullCommandResult> PullAsync(string repositoryPath, IRemote remote, PullOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.Pull(remote, options);
            return Task.FromResult(result);
        }

        public Task PushAllTagsAsync(string repositoryPath, IRemote remote, PushOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.PushAllTags(remote, options);
            return Task.CompletedTask;
        }

        public Task PushAsync(string repositoryPath, IBranch localBranch, IRemote remote, PushOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.Push(localBranch, remote, options);
            return Task.CompletedTask;
        }

        public Task PushAsync(string repositoryPath, IBranch localBranch, IBranch remoteBranch, PushOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.Push(localBranch, remoteBranch, options);
            return Task.CompletedTask;
        }

        public Task PushAsync(string repositoryPath, IBranchName localBranch, IRemoteName remote, IBranchName remoteBranch, PushOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.Push(localBranch, remote, remoteBranch, options);
            return Task.CompletedTask;
        }

        public Task PushAsync(string repositoryPath, ITagName localTag, IRemote remote, PushOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.Push(localTag, remote, options);
            return Task.CompletedTask;
        }

        public Task PushStashAsync(string repositoryPath, StashPushOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.PushStash(options);
            return Task.CompletedTask;
        }

        public Task<long> ReadCommitCountAsync(string repositoryPath, InclusiveRange range, HistoryOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadCommitCount(range, options);
            return Task.FromResult(result);
        }

        public Task<ITreeDifference> ReadCommitTreeDifferenceAsync(string repositoryPath, ICommit commit, DifferenceOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadCommitTreeDifference(commit, options);
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<ConfigurationEntry>> ReadConfigListAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadConfigList();
            return Task.FromResult(result);
        }

        public Task<Tuple<string, HeadType>> ReadCurrentBranchNameAsync(string repositoryPath, bool shortName, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            HeadType headType;
            var result = repository.ReadCurrentBranchName(shortName, out headType);
            return Task.FromResult(new Tuple<string, HeadType>(result, headType));
        }

        public Task<ObjectId> ReadCurrentHeadIdAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadCurrentHeadId();
            return Task.FromResult(result);
        }

        public Task<string> ReadCurrentHeadValueAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadCurrentHeadValue();
            return Task.FromResult(result);
        }

        public Task<string> ReadCurrentMergeMessageAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadCurrentMergeMessage();
            return Task.FromResult(result);
        }

        public Task<RepositoryCurrentOperation> ReadCurrentOperationAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadCurrentOperation();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyDictionary<string, ITreeDifferenceDetail>> ReadFileInfoAsync(string repositoryPath, string path, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadFileInfo(path);
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<ConfigurationEntry>> ReadGlobalConfigListAsync(object userData, CancellationToken cancellationToken)
        {
            var result = Repository.ReadGlobalConfigList(userData);
            return Task.FromResult(result);
        }

        public Task<IHead> ReadHeadAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadHead();
            return Task.FromResult(result);
        }

        public Task<ObjectHeader> ReadHeaderAsync(string repositoryPath, ObjectId objectId, string path, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadHeader(objectId, path);
            return Task.FromResult(result);
        }

        public Task<ITreeDifference> ReadIndexTreeDifferenceAsync(string repositoryPath, DifferenceOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadIndexTreeDifference(options);
            return Task.FromResult(result);
        }

        public Task<bool> ReadIsIndexLockedAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadIsIndexLocked();
            return Task.FromResult(result);
        }

        public Task<ObjectDatabaseDetails> ReadObjectDatabaseDetailsAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadObjectDatabaseDetails();
            return Task.FromResult(result);
        }

        public Task<Tuple<IReference[], IHead>> ReadReferencesAsync(string repositoryPath, ReferenceOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadReferences(options);
            return Task.FromResult(new Tuple<IReference[], IHead>(result.ToArray(), result.Head));
        }

        public Task<IRemote[]> ReadRemotesAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadRemotes();
            return Task.FromResult(result.ToArray());
        }

        public Task<ICommit> ReadRevisionAsync(string repositoryPath, IRevision revision, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadRevision(revision);
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<IStash>> ReadStashListAsync(string repositoryPath, int maxCount, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadStashList(maxCount);
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<IStash>> ReadStashListAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadStashList();
            return Task.FromResult(result);
        }

        public Task<IStatusSnapshot> ReadStatusAsync(string repositoryPath, StatusOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadStatus(options);
            return Task.FromResult(result);
        }

        public Task<ITreeDifference> ReadStatusIndexAsync(string repositoryPath, DifferenceOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadStatusIndex(options);
            return Task.FromResult(result);
        }

        public Task<ITreeDifference> ReadStatusWorktreeAsync(string repositoryPath, DifferenceOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadStatusWorktree(options);
            return Task.FromResult(result);
        }

        public Task<ITreeDifference> ReadTreeDifferenceAsync(string repositoryPath, ICommit source, ICommit target, DifferenceOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadTreeDifference(source, target, options);
            return Task.FromResult(result);
        }

        public Task<ITreeDifference> ReadTreeDifferenceAsync(string repositoryPath, ITree source, ITree target, DifferenceOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadTreeDifference(source, target, options);
            return Task.FromResult(result);
        }

        public Task<ITreeDifference> ReadTreeDifferenceAsync(string repositoryPath, IRevision source, IRevision target, DifferenceOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadTreeDifference(source, target, options);
            return Task.FromResult(result);
        }

        public Task<RebaseResult> RebaseAbortAsync(string repositoryPath, OperationProgressDelegate progressCallback, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.RebaseAbort(progressCallback);
            return Task.FromResult(result);
        }

        public Task<RebaseResult> RebaseBeginAsync(string repositoryPath, IRevision upstream, RebaseOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.RebaseBegin(upstream, options);
            return Task.FromResult(result);
        }

        public Task<RebaseResult> RebaseContinueAsync(string repositoryPath, OperationProgressDelegate progressCallback, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.RebaseContinue(progressCallback);
            return Task.FromResult(result);
        }

        public Task<RebaseResult> RebaseSkipAsync(string repositoryPath, OperationProgressDelegate progressCallback, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.RebaseSkip(progressCallback);
            return Task.FromResult(result);
        }

        public Task RemoveRemoteAsync(string repositoryPath, string name, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.RemoveRemote(name);
            return Task.CompletedTask;
        }

        public Task RemoveUpstreamAsync(string repositoryPath, IBranch branch, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.RemoveUpstream(branch);
            return Task.CompletedTask;
        }

        public Task RenameBranchAsync(string repositoryPath, IBranchName sourceBranchName, string newBranchName, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.RenameBranch(sourceBranchName, newBranchName);
            return Task.CompletedTask;
        }

        public Task RenameBranchAsync(string repositoryPath, string sourceBranchName, string newBranchName, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.RenameBranch(sourceBranchName, newBranchName);
            return Task.CompletedTask;
        }

        public Task RenameRemoteAsync(string repositoryPath, string oldName, string newName, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.RenameRemote(oldName, newName);
            return Task.CompletedTask;
        }

        public Task ResetHardAsync(string repositoryPath, IRevision revision, OperationProgressDelegate progressCallback, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.ResetHard(revision, progressCallback);
            return Task.CompletedTask;
        }

        public Task<bool> ResetKeepAsync(string repositoryPath, IRevision revision, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ResetKeep(revision);
            return Task.FromResult(result);
        }

        public Task<bool> ResetMergeAsync(string repositoryPath, IRevision revision, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ResetMerge(revision);
            return Task.FromResult(result);
        }

        public Task ResetMixedAsync(string repositoryPath, IRevision revision, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.ResetMixed(revision);
            return Task.CompletedTask;
        }

        public Task ResetPathsAsync(string repositoryPath, ICollection<string> paths, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.ResetPaths(paths);
            return Task.CompletedTask;
        }

        public Task ResetSoftAsync(string repositoryPath, IRevision revision, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.ResetSoft(revision);
            return Task.CompletedTask;
        }

        public Task<RevertResult> RevertAbortAsync(string repositoryPath, OperationProgressDelegate progressCallback, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.RevertAbort(progressCallback);
            return Task.FromResult(result);
        }

        public Task<RevertResult> RevertAsync(string repositoryPath, IRevision revision, RevertOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.Revert(revision, options);
            return Task.FromResult(result);
        }

        public Task<RevertResult> RevertContinueAsync(string repositoryPath, OperationProgressDelegate progressCallback, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.RevertContinue(progressCallback);
            return Task.FromResult(result);
        }

        public Task SetRemoteFetchUrlAsync(string repositoryPath, string name, string url, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.SetRemoteFetchUrl(name, url);
            return Task.CompletedTask;
        }

        public Task SetRemotePushUrlAsync(string repositoryPath, string name, string url, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.SetRemotePushUrl(name, url);
            return Task.CompletedTask;
        }

        public Task SetUpstreamAsync(string repositoryPath, IBranch branch, IBranch upstream, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.SetUpstream(branch, upstream);
            return Task.CompletedTask;
        }

        public Task SubmoduleUpdateAsync(string repositoryPath, SubmoduleUpdateOptions options, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            repository.SubmoduleUpdate(options);
            return Task.CompletedTask;
        }

        Task<IBlob> IRepositoryService.ReadBlobAsync(string repositoryPath, ObjectId objectId, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadObject<IBlob>(objectId);
            return Task.FromResult(result);
        }

        Task<ICommit> IRepositoryService.ReadCommitAsync(string repositoryPath, ObjectId objectId, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadObject<ICommit>(objectId);
            return Task.FromResult(result);
        }

        Task<ITree> IRepositoryService.ReadTreeAsync(string repositoryPath, ObjectId objectId, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            var result = repository.ReadObject<ITree>(objectId);
            return Task.FromResult(result);
        }

        public Task<string> GetSharedWorkingDirectoryAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repository = GetRepository(repositoryPath);
            string cleanedWorkingDirectory = repository.WorkingDirectory.Replace("/", "\\");
            if (!cleanedWorkingDirectory.EndsWith("\\"))
            {
                cleanedWorkingDirectory += '\\';
            }
            return Task.FromResult(repository.Context.Broker.TranslateToSharedPathIfNecessary(cleanedWorkingDirectory, false));
        }
    }
}