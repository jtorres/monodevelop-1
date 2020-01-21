//*************************************************************************************************
// ObjectDatabase.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git object-database.
    /// <para/>
    /// Designed to facilitate scenarios where frequent object-database reads are necissary.
    /// </summary>
    public interface IObjectDatabase : IDisposable
    {
        IRepository Repository { get; }

        /// <summary>
        /// Reads an object from the repository database.
        /// <para/>
        /// Only a single object can be read from the database at a time per instance of `<see cref="ObjectDatabase"/>`; thus all concurrent calls block until they can be serviced.
        /// <para/>
        /// Returns a reference to an object.
        /// </summary>
        /// <param name="objectId">The identity of the object to read.</param>
        T ReadObject<T>(ObjectId objectId)
            where T : class, IObject;

        /// <summary>
        /// Reads an object header from the respository database.
        /// <para/>
        /// Only a single object can be read from the database at a time per instance of `<see cref="ObjectDatabase"/>`; thus all concurrent calls block until they can be serviced.
        /// <summary>
        /// <param name="objectId">The identity of a commit which contains the object.</param>
        /// <param name="path">(optional) The path to the tree, relative to the root of the repository.</param>
        ObjectHeader ReadObjectHeader(ObjectId objectId, string path);

        /// <summary>
        /// Reads the tree at the specified path from the repository database.
        /// <para/>
        /// Only a single object can be read from the database at a time per instance of `<see cref="ObjectDatabase"/>`; thus all concurrent calls block until they can be serviced.
        /// <para/>
        /// Returns a reference to the tree if successful; otherwise `<see langword="null"/>`.
        /// </summary>
        /// <param name="treeId">The identity of the tree to read.</param>
        /// <param name="filter">(optional) The filter used to filter the children of the tree.</param>
        ITree ReadFilteredTree(ObjectId objectId, INamedObjectFilter filter);

        /// <summary>
        /// Reads the tree at the specified `<paramref name="path"/>` from the repository database.
        /// <para/>
        /// Only a single object can be read from the database at a time per instance of `<see cref="ObjectDatabase"/>`; thus all concurrent calls block until they can be serviced.
        /// <para/>
        /// Returns a reference to the tree if successful; otherwise `<see langword="null"/>`.
        /// </summary>
        /// <param name="commitId">The identity of a commit which contains the tree.</param>
        /// <param name="path">(optional) The path to the tree, relative to the root of the repository.</param>
        ITree ReadFilteredTree(ObjectId commitId, string path);

        /// <summary>
        /// Reads the tree at the specified `<paramref name="path"/>` from the repository database.
        /// <para/>
        /// Only a single object can be read from the database at a time per instance of `<see cref="ObjectDatabase"/>`; thus all concurrent calls block until they can be serviced.
        /// <para/>
        /// Returns a reference to the tree if successful; otherwise `<see langword="null"/>`.
        /// </summary>
        /// <param name="commitId">The identity of a commit which contains the tree.</param>
        /// <param name="path">(optional) The path to the tree, relative to the root of the repository.</param>
        /// <param name="filter">(optional) The filter used to filter the children of the tree.</param>
        ITree ReadFilteredTree(ObjectId commitId, string path, INamedObjectFilter filter);

        /// <summary>
        /// Begins reading a blob object from the repository database.
        /// <para/>
        /// The call is responsible for reading the blob contents from the database via `<see cref="IBlob.ReadContent(byte[], int, int)"/>` and reading until the end of the stream.
        /// <para/>
        /// The `<see cref="ObjectDatabase"/>` is considered busy until all the blob data has been read; and any concurrent calls into the database will either fail or block until the returned blob's data has been fully read from the database.
        /// <para/>
        /// Returns a reference to a blob object.
        /// </summary>
        /// <param name="objectId">The identity of the blob object to read.</param>
        IBlob StreamBlob(ObjectId blobId);
    }

    internal class ObjectDatabase : Base, IObjectDatabase
    {
        public const long BlobSizeMaximum = Cli.CatFileCommand.ObjectSizeMaximum;

        public ObjectDatabase(ExecutionContext context, Backend backend)
            : base()
        {
            SetContext(context);

            if (ReferenceEquals(backend, null))
                throw new ArgumentNullException(nameof(backend));

            _backend = backend;
            _semaphore = new SemaphoreSlim(1, 1);
            _syncpoint = new object();
        }

        public IRepository Repository
            => _backend?.Repository;

        private Backend _backend;
        // frustratingly we have to have a semaphore here to allow for the cross thread lock/unlock
        // required to support streaming, the monitor syncpoint is only for simple re-entrancy
        private SemaphoreSlim _semaphore;
        // the monitor syncpoint is for cheap atmoic access to values (such as checking if the
        // instance has been disposed already or not) and protecting the `Dispose` method.
        private readonly object _syncpoint;

        public void Dispose()
        {
            lock (_syncpoint)
            {
                _backend?.Dispose();
                _backend = null;

                _semaphore?.Dispose();
                _semaphore = null;
            }
        }

        public T ReadObject<T>(ObjectId objectId)
            where T : class, IObject
        {
            if (objectId == ObjectId.Zero)
                return null;

            lock (_syncpoint)
            {
                if (_backend == null)
                    throw new ObjectDisposedException(typeof(ObjectDatabase).FullName);

                try
                {
                    _semaphore.Wait();

                    using (var buffer = new ByteBuffer())
                    {
                        var type = typeof(T);

                        if (type == typeof(ITree) || type == typeof(Tree))
                        {
                            return _backend.ReadTree(buffer, objectId) as T;
                        }
                        else if (type == typeof(IBlob) || type == typeof(Blob))
                        {
                            return _backend.ReadBlob(buffer, objectId) as T;
                        }
                        else if (type == typeof(ICommit) || type == typeof(Commit))
                        {
                            return _backend.ReadCommit(buffer, objectId) as T;
                        }
                        else if (type == typeof(ITagAnnotation) || type == typeof(TagAnnotation))
                        {
                            return _backend.ReadTagAnnotation(buffer, objectId) as T;
                        }
                        else
                            throw new ObjectTypeException(typeof(T).FullName);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(ObjectDatabase)}.{nameof(ReadObject)}", exception, objectId, typeof(T).Name))
                {
                    // not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        public ITree ReadFilteredTree(ObjectId treeId, INamedObjectFilter filter)
        {
            if (treeId == ObjectId.Zero)
                return null;

            lock (_syncpoint)
            {
                if (_backend == null)
                    throw new ObjectDisposedException(typeof(ObjectDatabase).FullName);

                try
                {
                    _semaphore.Wait();

                    return ReadFilteredTreeInternal(treeId, null, filter);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(ObjectDatabase)}.{nameof(ReadFilteredTree)}", exception, treeId, filter.Name))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        public ITree ReadFilteredTree(ObjectId commitId, string path)
        {
            if (commitId == ObjectId.Zero)
                return null;

            lock (_syncpoint)
            {
                if (_backend == null)
                    throw new ObjectDisposedException(typeof(ObjectDatabase).FullName);

                try
                {
                    _semaphore.Wait();

                    return ReadFilteredTreeInternal(commitId, path, null);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(ObjectDatabase)}.{nameof(ReadFilteredTree)}", exception, commitId, path))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        public ITree ReadFilteredTree(ObjectId commitId, string path, INamedObjectFilter filter)
        {
            if (commitId == ObjectId.Zero)
                return null;

            lock (_syncpoint)
            {
                if (_backend == null)
                    throw new ObjectDisposedException(typeof(ObjectDatabase).FullName);

                try
                {
                    _semaphore.Wait();

                    return ReadFilteredTreeInternal(commitId, path, filter);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(ObjectDatabase)}.{nameof(ReadFilteredTree)}", exception, commitId, path))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        public ObjectHeader ReadObjectHeader(ObjectId objectId, string path)
        {

            lock (_syncpoint)
            {
                if (_backend == null)
                    throw new ObjectDisposedException(typeof(ObjectDatabase).FullName);

                try
                {
                    _semaphore.Wait();

                    return _backend.ReadHeader(objectId, path);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(ObjectDatabase)}.{nameof(ReadObjectHeader)}", exception, objectId, path))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        public IBlob StreamBlob(ObjectId objectId)
        {
            lock (_syncpoint)
            {
                if (_backend == null)
                    throw new ObjectDisposedException(typeof(ObjectDatabase).FullName);

                try
                {
                    _semaphore.Wait();

                    using (var buffer = new ByteBuffer())
                    {
                        return _backend.ReadBlobStream(buffer, objectId, Release);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(ObjectDatabase)}.{nameof(StreamBlob)}", exception, objectId))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
                finally
                {
                    // nothing to do here as the `_semaphore.Release()` will be taken care of by the
                    // `Release` delegate passed into `ReadBlobStream` when the stream read completes.
                }
            }
        }

        internal ITree ReadFilteredTreeInternal(ObjectId objectId, string path, INamedObjectFilter filter)
        {
            Debug.Assert(Monitor.IsEntered(_syncpoint), "Expected lock not held.");

            using (var buffer = new ByteBuffer())
            {
                return _backend.ReadFilteredTree(buffer, objectId, path, filter);
            }
        }

        private void Release(ObjectId objectId)
        {
            _semaphore.Release();
        }

        internal sealed class Backend : IDisposable
        {
            public Backend(IRepository repository,
                           CreateCatFileProcessDelegate batchProcessCallback,
                           CreateCatFileProcessDelegate checkProcessCallback,
                           ReadObjectDelegate<IBlob> readBlobCallback,
                           ReadObjectStreamDelegate<IBlob> readBlobStreamCallback,
                           ReadObjectDelegate<ICommit> readCommitCallback,
                           ReadHeaderDelegate readHeaderCallback,
                           ReadObjectDelegate<ITagAnnotation> readTagAnnotation,
                           ReadObjectDelegate<ITree> readTreeCallback)
            {
                if (repository == null)
                    throw new ArgumentNullException(nameof(repository));
                if (batchProcessCallback == null)
                    throw new ArgumentNullException(nameof(batchProcessCallback));
                if (checkProcessCallback == null)
                    throw new ArgumentNullException(nameof(checkProcessCallback));
                if (readBlobCallback == null)
                    throw new ArgumentNullException(nameof(readBlobCallback));
                if (readBlobStreamCallback == null)
                    throw new ArgumentNullException(nameof(readBlobStreamCallback));
                if (readCommitCallback == null)
                    throw new ArgumentNullException(nameof(readCommitCallback));
                if (readHeaderCallback == null)
                    throw new ArgumentNullException(nameof(readHeaderCallback));
                if (readTagAnnotation == null)
                    throw new ArgumentNullException(nameof(readTagAnnotation));
                if (readTreeCallback == null)
                    throw new ArgumentNullException(nameof(readTreeCallback));

                Repository = repository;

                _batchProcess = new Lazy<IProcess>(() => batchProcessCallback());
                _checkProcess = new Lazy<IProcess>(() => checkProcessCallback());
                _readBlob = readBlobCallback;
                _readBlobStream = readBlobStreamCallback;
                _readCommit = readCommitCallback;
                _readHeader = readHeaderCallback;
                _readTagAnnotation = readTagAnnotation;
                _readTree = readTreeCallback;
            }

            public readonly IRepository Repository;

            private Lazy<IProcess> _batchProcess;
            private Lazy<IProcess> _checkProcess;

            private ReadObjectDelegate<IBlob> _readBlob;
            private ReadObjectStreamDelegate<IBlob> _readBlobStream;
            private ReadObjectDelegate<ICommit> _readCommit;
            private ReadHeaderDelegate _readHeader;
            private ReadObjectDelegate<ITagAnnotation> _readTagAnnotation;
            private ReadObjectDelegate<ITree> _readTree;

            public void Dispose()
            {
                Lazy<IProcess> process;

                _readBlob = null;
                _readBlobStream = null;
                _readCommit = null;
                _readHeader = null;
                _readTree = null;

                if ((process = Interlocked.Exchange(ref _batchProcess, null)) != null
                    && process.IsValueCreated)
                {
                    try
                    {
                        // Close the process' standard input handle, this prevents the it from
                        // get stuck attempting to read data that is never coming. Since it may
                        // have previously been closed and NetFx, for reasons unknown, throws
                        // if that's the case we need to guard against that.
                        process.Value.StdIn.Close();
                    }
                    catch { /* squelch */ }

                    process.Value.WaitForExit();

                    // Kill the process and release all related resources.
                    process.Value.Dispose();
                }

                if ((process = Interlocked.Exchange(ref _checkProcess, null)) != null
                    && process.IsValueCreated)
                {
                    try
                    {
                        // Close the process' standard input handle, this prevents the it from
                        // get stuck attempting to read data that is never coming. Since it may
                        // have previously been closed and NetFx, for reasons unknown, throws
                        // if that's the case we need to guard against that.
                        process.Value.StdIn.Close();
                    }
                    catch { /* squelch */ }

                    process.Value.WaitForExit();

                    // Kill the process and release all related resources.
                    process.Value.Dispose();
                }
            }

            public IBlob ReadBlob(ByteBuffer buffer, ObjectId objectId)
                => _readBlob(_batchProcess?.Value, buffer, objectId, null, null);

            public ICommit ReadCommit(ByteBuffer buffer, ObjectId objectId)
                => _readCommit(_batchProcess?.Value, buffer, objectId, null, null);

            public IBlob ReadBlobStream(ByteBuffer buffer, ObjectId objectId, ReadStreamCompletedDelegate completedCallback)
                => _readBlobStream(_batchProcess?.Value, buffer, objectId, completedCallback);

            public ITree ReadFilteredTree(ByteBuffer buffer, ObjectId objectId, string path, INamedObjectFilter filter)
                => _readTree(_batchProcess?.Value, buffer, objectId, path, filter);

            // use 'git-cat-file --batch-check' for `ReadHeader` to avoid the overhead of streaming back the entire contents of the blob
            // which would require de-deltafication, inflation, and a copy across standard pipes just to be thrown away.
            public ObjectHeader ReadHeader(ObjectId objectId, string path)
                => _readHeader(_checkProcess?.Value, objectId, path);

            public ITagAnnotation ReadTagAnnotation(ByteBuffer buffer, ObjectId objectId)
                => _readTagAnnotation(_batchProcess?.Value, buffer, objectId, null, null);

            public ITree ReadTree(ByteBuffer buffer, ObjectId objectId)
                => _readTree(_batchProcess?.Value, buffer, objectId, null, null);
        }

        internal delegate IProcess CreateCatFileProcessDelegate();
        internal delegate ObjectHeader ReadHeaderDelegate(IProcess checkProcess, ObjectId objectId, string path);
        internal delegate T ReadObjectDelegate<T>(IProcess batchProcess, ByteBuffer buffer, ObjectId objectId, string path, INamedObjectFilter filter)
            where T : IObject;
        internal delegate T ReadObjectStreamDelegate<T>(IProcess batchProcess, ByteBuffer buffer, ObjectId objectId, ReadStreamCompletedDelegate completedCallback)
            where T : IObject;
        internal delegate void ReadStreamCompletedDelegate(ObjectId objectId);
    }
}
