//*************************************************************************************************
// RemotableObjectDatabase.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.TeamFoundation.GitApi
{
    internal class RemotableObjectDatabase : Base, IObjectDatabase
    {
        public RemotableObjectDatabase(RemotableRepository repository)
            : base()
        {
            if (ReferenceEquals(repository, null))
                throw new ArgumentNullException(nameof(repository));

            SetContext(repository.Context);

            Repository = repository;
            RemoteObjectDatabase = ExecuteTaskSynchronously(() => Context.Broker.GetProxyAsync<IRemoteObjectDatabase, RemoteObjectDatabase>("GitApi.ObjectDatabase"));
            ExecuteTaskSynchronously(() => RemoteObjectDatabase.InitializeAsync(repository.WorkingDirectory, CancellationToken.None));
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

        public IRepository Repository { get; }

        private IRemoteObjectDatabase RemoteObjectDatabase { get; set; }

        public void Dispose()
        {
            (RemoteObjectDatabase as IDisposable)?.Dispose();
            RemoteObjectDatabase = null;
        }

        public T ReadObject<T>(ObjectId objectId)
            where T : class, IObject
        {
            if (objectId == ObjectId.Zero)
                return null;

            if (typeof(IBlob).IsAssignableFrom(typeof(T)))
            {
                return ExecuteTaskSynchronously(() => RemoteObjectDatabase.ReadBlobAsync(objectId, CancellationToken.None)) as T;
            }
            else if (typeof(ICommit).IsAssignableFrom(typeof(T)))
            {
                return ExecuteTaskSynchronously(() => RemoteObjectDatabase.ReadCommitAsync(objectId, CancellationToken.None)) as T;
            }
            else if (typeof(ITree).IsAssignableFrom(typeof(T)))
            {
                return ExecuteTaskSynchronously(() => RemoteObjectDatabase.ReadTreeAsync(objectId, CancellationToken.None)) as T;
            }
            else
            {
                throw new ArgumentException("Unexpected type requested from ReadObject");
            }
        }

        public ITree ReadFilteredTree(ObjectId treeId, INamedObjectFilter filter)
        {
            if (treeId == ObjectId.Zero)
                return null;

            return ExecuteTaskSynchronously(() => RemoteObjectDatabase.ReadFilteredTreeAsync(treeId, filter, CancellationToken.None));
        }

        public ITree ReadFilteredTree(ObjectId commitId, string path)
        {
            if (commitId == ObjectId.Zero)
                return null;

            return ExecuteTaskSynchronously(() => RemoteObjectDatabase.ReadFilteredTreeAsync(commitId, path, CancellationToken.None));
        }

        public ITree ReadFilteredTree(ObjectId commitId, string path, INamedObjectFilter filter)
        {
            if (commitId == ObjectId.Zero)
                return null;

            return ExecuteTaskSynchronously(() => RemoteObjectDatabase.ReadFilteredTreeAsync(commitId, path, filter, CancellationToken.None));
        }

        public ObjectHeader ReadObjectHeader(ObjectId objectId, string path)
        {
            return ExecuteTaskSynchronously(() => RemoteObjectDatabase.ReadObjectHeaderAsync(objectId, path, CancellationToken.None));
        }

        public IBlob StreamBlob(ObjectId objectId)
        {
            return ExecuteTaskSynchronously(() => RemoteObjectDatabase.StreamBlobAsync(objectId, CancellationToken.None));
        }
    }

    public interface IRemoteObjectDatabase
    {
        Task InitializeAsync(string repositoryPath, CancellationToken cancellationToken);
        Task<IBlob> ReadBlobAsync(ObjectId objectId, CancellationToken cancellationToken);
        Task<ICommit> ReadCommitAsync(ObjectId objectId, CancellationToken cancellationToken);
        Task<ITree> ReadTreeAsync(ObjectId objectId, CancellationToken cancellationToken);
        Task<ObjectHeader> ReadObjectHeaderAsync(ObjectId objectId, string path, CancellationToken cancellationToken);
        Task<ITree> ReadFilteredTreeAsync(ObjectId objectId, INamedObjectFilter filter, CancellationToken cancellationToken);
        Task<ITree> ReadFilteredTreeAsync(ObjectId commitId, string path, CancellationToken cancellationToken);
        Task<ITree> ReadFilteredTreeAsync(ObjectId commitId, string path, INamedObjectFilter filter, CancellationToken cancellationToken);
        Task<IBlob> StreamBlobAsync(ObjectId blobId, CancellationToken cancellationToken);
    }

    public class RemoteObjectDatabase : IRemoteObjectDatabase, IDisposable
    {
        private IObjectDatabase objectDatabaseImpl;

        public RemoteObjectDatabase()
        {
        }

        public void Dispose()
        {
            this.objectDatabaseImpl.Dispose();
            this.objectDatabaseImpl = null;
        }

        public Task InitializeAsync(string repositoryPath, CancellationToken cancellationToken)
        {
            var repo = RepositoryService.GetRepository(repositoryPath);
            this.objectDatabaseImpl = repo.OpenObjectDatabase();

            return Task.CompletedTask;
        }

        public Task<IBlob> ReadBlobAsync(ObjectId objectId, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.objectDatabaseImpl.ReadObject<IBlob>(objectId));
        }

        public Task<ICommit> ReadCommitAsync(ObjectId objectId, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.objectDatabaseImpl.ReadObject<ICommit>(objectId));
        }

        public Task<ITree> ReadFilteredTreeAsync(ObjectId objectId, INamedObjectFilter filter, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.objectDatabaseImpl.ReadFilteredTree(objectId, filter));
        }

        public Task<ITree> ReadFilteredTreeAsync(ObjectId commitId, string path, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.objectDatabaseImpl.ReadFilteredTree(commitId, path));
        }

        public Task<ITree> ReadFilteredTreeAsync(ObjectId commitId, string path, INamedObjectFilter filter, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.objectDatabaseImpl.ReadFilteredTree(commitId, path, filter));
        }

        public Task<ObjectHeader> ReadObjectHeaderAsync(ObjectId objectId, string path, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.objectDatabaseImpl.ReadObjectHeader(objectId, path));
        }

        public Task<ITree> ReadTreeAsync(ObjectId objectId, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.objectDatabaseImpl.ReadObject<ITree>(objectId));
        }

        public Task<IBlob> StreamBlobAsync(ObjectId blobId, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.objectDatabaseImpl.StreamBlob(blobId));
        }
    }
}
