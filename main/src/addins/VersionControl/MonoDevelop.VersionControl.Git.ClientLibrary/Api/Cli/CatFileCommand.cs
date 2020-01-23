 //*************************************************************************************************
// CatFileCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    /// <summary>
    /// Execution manager for git-cat-file
    /// </summary>
    internal class CatFileCommand : GitCommand
    {
        public const string Command = "cat-file";
        /// <summary>
        /// <para>Limit to the size of an object that can be contiguously read into memory.</para>
        /// <para>512 KiB</para>
        /// </summary>
        public const long ObjectSizeMaximum = 512 * 1024;
        public const string ObjectTypeNameBlob = "blob";
        public const string ObjectTypeNameCommit = "commit";
        public const string ObjectTypeNameTag = "tag";
        public const string ObjectTypeNameTree = "tree";
        public const string ObjectTypeNameUnknown = "unknown";

        private const string HeaderFormat = "[%(objectname)][%(objecttype)][%(objectsize)]";
        private const string BatchCommand = Command + " --batch=" + HeaderFormat;
        private const string CheckCommand = Command + " --batch-check=" + HeaderFormat;
        private const string MissingError = "missing\n";

        private static readonly byte[] MissingErrorUtf8 = Encoding.UTF8.GetBytes(MissingError);

        public CatFileCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        /// <summary>
        /// Enumerates the names associated with the values of <see cref="ObjectType"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateObjectNames()
        {
            yield return ObjectTypeNameBlob;
            yield return ObjectTypeNameCommit;
            yield return ObjectTypeNameTag;
            yield return ObjectTypeNameTree;
            yield break;
        }

        /// <summary>
        /// Enumerates the values of <see cref="ObjectType"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ObjectType> EnumerateObjectTypes()
        {
            yield return ObjectType.Blob;
            yield return ObjectType.Commit;
            yield return ObjectType.Tag;
            yield return ObjectType.Tree;
            yield break;
        }

        /// <summary>
        /// Gets the <see cref="ObjectType"/> associated with <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name to decode.</param>
        /// <returns>The <see cref="ObjectType"/> associated with <paramref name="name"/>.</returns>
        public static ObjectType ObjectNameToType(string name)
        {
            if (ReferenceEquals(name, null))
                throw new NullReferenceException(nameof(name));

            switch (name)
            {
                case ObjectTypeNameBlob:
                    return ObjectType.Blob;

                case ObjectTypeNameCommit:
                    return ObjectType.Commit;

                case ObjectTypeNameTag:
                    return ObjectType.Tag;

                case ObjectTypeNameTree:
                    return ObjectType.Tree;

                default:
                    throw new ObjectTypeException(typeof(ObjectType).FullName);
            }
        }

        /// <summary>
        /// Gets the associated name of a <see cref="ObjectType"/>.
        /// </summary>
        /// <param name="type">The value of <see cref="ObjectType"/> to transform.</param>
        /// <returns>The textual name for the value of <paramref name="type"/>.</returns>
        public static string ObjectTypeToName(ObjectType type)
        {
            Debug.Assert(Enum.IsDefined(typeof(ObjectType), type), $"The `{nameof(type)}` parameter is undefined.");

            switch (type)
            {
                case ObjectType.Blob:
                    return ObjectTypeNameBlob;

                case ObjectType.Commit:
                    return ObjectTypeNameCommit;

                case ObjectType.Tag:
                    return ObjectTypeNameTag;

                case ObjectType.Tree:
                    return ObjectTypeNameTree;

                default:
                    throw new ObjectTypeException(typeof(ObjectType).FullName);
            }
        }

        /// <summary>
        /// <para>Opens a blob from the object database for streaming, and apply any necessary filters..</para>
        /// <para>Unlike other methods for reading from the object database, <see cref="OpenBlob"/>
        /// will apply any and all filters to the blob's content before returning bytes to the caller.</para>
        /// <para>The type of the object must be a blob, or result is indeterminate.</para>
        /// </summary>
        /// <param name="revision">The revision to use when reading from the object database.</param>
        /// <param name="path">
        /// <para>A path to the object on disk relative to the root of the repository.</para>
        /// <para>The path must be in "repository format" and be complete (pathish and/or fnmatch *not* supported)</para>
        /// </param>
        /// <returns>An <see cref="IReadable"/> stream of bytes from a blob stored in the
        /// object database.</returns>
        public Stream OpenBlob(IRevision revision, string path)
        {
            if (ReferenceEquals(revision, null))
                throw new ArgumentNullException(nameof(revision));
            if (ReferenceEquals(path, null))
                throw new ArgumentNullException(nameof(path));

            using (var command = new StringBuffer(Command))
            {
                string quotedPath = PathHelper.EscapePath(path);

                command.Append(" --filters ")
                       .Append(revision.RevisionText)
                       .Append(':')
                       .Append(quotedPath);

                try
                {
                    return OpenBlobInternal(command);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CatFileCommand)}.{nameof(OpenBlob)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        /// <summary>
        /// <para>Opens a blob from the object database for streaming, and apply any necessary filters..</para>
        /// <para>Unlike other methods for reading from the object database, <see cref="OpenBlob"/>
        /// will apply any and all filters to the blob's content before returning bytes to the caller.</para>
        /// <para>The type of the object must be a blob, or result is indeterminate.</para>
        /// </summary>
        /// <param name="blobId">The blob ID to use when reading from the object database.</param>
        /// <param name="path">
        /// <para>A path to the object on disk relative to the root of the repository.</para>
        /// <para>The path must be in "repository format" and be complete (pathish and/or fnmatch *not* supported)</para>
        /// </param>
        /// <returns>An <see cref="IReadable"/> stream of bytes from a blob stored in the
        /// object database.</returns>
        public Stream OpenBlob(ObjectId blobId, string path)
        {
            if (ReferenceEquals(path, null))
                throw new ArgumentNullException(nameof(path));

            using (var command = new StringBuffer(Command))
            {
                string quotedPath = PathHelper.EscapePath(path);

                command.Append(" --filters --path=")
                       .Append(quotedPath)
                       .Append(" ")
                       .Append(blobId.RevisionText);

                try
                {
                    return OpenBlobInternal(command);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CatFileCommand)}.{nameof(OpenBlob)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public Stream OpenBlob(ObjectId blobId)
        {
            using (var command = new StringBuffer(Command))
            {
                command.Append(" blob ")
                       .Append(blobId.RevisionText);

                try
                {
                    using (Tracer.TraceCommand($"{nameof(CatFileCommand)} {nameof(OpenBlob)}", command, userData: _userData))
                    {
                        return OpenBlobInternal(command);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CatFileCommand)}.{nameof(OpenBlob)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and
                    // `throw` here makes the compiler happy.
                    throw;
                }
            }
        }

        public void CopyBlobToStream(Stream stream, ObjectId blobId, string path)
        {
            using (var command = new StringBuffer(Command))
            {
                command.Append(" blob ")
                       .Append(blobId.RevisionText);

                try
                {
                    using (Tracer.TraceCommand($"{nameof(CatFileCommand)} {nameof(CopyBlobToStream)}", command, userData: _userData))
                    {
                        CopyBlobToStreamInternal(stream, command);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CatFileCommand)}.{nameof(CopyBlobToStream)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and
                    // `throw` here makes the compiler happy.
                    throw;
                }
            }
        }

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
        public Stream OpenBlob(IndexVersion index, string path)
        {
            if (ReferenceEquals(path, null))
                throw new ArgumentNullException(nameof(path));

            Debug.Assert(Enum.IsDefined(typeof(IndexVersion), index), $"The `{nameof(index)}` parameter is undefined.");

            using (var command = new StringBuffer(Command))
            {
                string quotedPath = PathHelper.EscapePath(path);

                command.Append(" --filters :")
                       .Append((int)index)
                       .Append(':')
                       .Append(quotedPath);

                try
                {
                    return OpenBlobInternal(command);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CatFileCommand)}.{nameof(OpenBlob)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public IObjectDatabase OpenObjectDatabase()
        {
            var backend = new ObjectDatabase.Backend(_repository,
                                                     GetBatchProcess,
                                                     GetCheckProcess,
                                                     ReadObjectInternal<IBlob>,
                                                     ReadObjectStreamInternal<IBlob>,
                                                     ReadObjectInternal<ICommit>,
                                                     ReadHeaderInternal,
                                                     ReadObjectInternal<ITagAnnotation>,
                                                     ReadObjectInternal<ITree>);

            var odb = new ObjectDatabase(Context, backend);

            return odb;
        }

        /// <summary>
        /// Reads an object header without blob content from the object database.
        /// </summary>
        /// <param name="objectId">The identity of the object to be read.</param>
        /// <returns>The object header related to the identity requested.</returns>
        public ObjectHeader ReadHeader(ObjectId objectId, string path)
        {
            if (objectId == ObjectId.Zero)
                return ObjectHeader.Unknown;

            string command = $"{Command} {objectId}";
            var header = new ObjectHeader { };

            try
            {
                using (Tracer.TraceCommand(Command, command, userData: _userData))
                using (IProcess process = GetCheckProcess())
                {
                    var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                    header = ReadHeaderInternal(process, objectId, path);

                    TestExitCode(process, command, stderrTask);
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CatFileCommand)}.{nameof(ReadHeader)}", exception, command))
            {
                // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                throw;
            }

            return header;
        }



        ///// <summary>
        ///// Reads an object header with blob content from the object database.
        ///// </summary>
        ///// <param name="objectId">The identity of the object to be read.</param>
        ///// <returns>The object header related to the identity requested.</returns>
        ///// <exception cref="ObjectTooLargeException">Throw if the object to be read is larger that <see cref="ObjectSizeMaximum"/>.</exception>
        //public T ReadObject<T>(ObjectId objectId)
        //    where T : class, IObject
        //{
        //    if (objectId == ObjectId.Zero)
        //        return default(T);

        //    string command = $"{Command} {objectId}";
        //    var result = default(T);

        //    using (Tracer.TraceCommand(Command, command, userData: _userData))
        //    using (var buffer = new ByteBuffer())
        //    using (IProcess process = GetBatchProcess())
        //    {
        //        try
        //        {
        //            var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

        //            result = ReadObjectInternal<T>(process, buffer, objectId, null, null);

        //            TestExitCode(process, command, stderrTask);
        //        }
        //        catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CatFileCommand)}.{nameof(ReadObject)}", exception, command))
        //        {
        //            // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
        //            throw;
        //        }
        //    }

        //    return result;
        //}

        /// <summary>
        /// <para>Reads the object at the specified path from the repository database.</para>
        /// <para>Only a single object can be read from the database at a time per instance of
        /// <see cref="ObjectDatabase"/>; thus all concurrent calls block until they can be serviced.</para>
        /// </summary>
        /// <param name="objectId">The identity of the tree to read.</param>
        /// <param name="childFilter">String used to filter the children of the tree.</param>
        /// <param name="childComparer">Comparer used to filter the children of the tree.</param>
        /// <returns>Reference to an object.</returns>
        public ITree ReadFilteredTree(ObjectId objectId, INamedObjectFilter filter)
        {
            if (objectId == ObjectId.Zero)
                return null;

            string command = $"{Command} {objectId} filter={filter?.Name}";
            ITree tree = null;

            using (Tracer.TraceCommand(Command, command, userData: _userData))
            using (var buffer = new ByteBuffer())
            using (IProcess process = GetBatchProcess())
            {
                try
                {
                    var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                    tree = ReadObjectInternal<ITree>(process, buffer, objectId, null, filter);

                    TestExitCode(process, command, stderrTask);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CatFileCommand)}.{nameof(ReadFilteredTree)}", exception, command))
                {
                    // should not be reached, but we'll re-throw just-in-case
                    throw new GitException(command, exception);
                }
                catch (Exception exception) when (!(exception is GitException))
                {
                    // wrap any non-`CliException` in a `CliException`
                    throw new GitException(command, exception);
                }
            }

            return tree;
        }

        public ITree ReadTreeByPath(ObjectId commitId, string path)
        {
            if (commitId == ObjectId.Zero)
                return null;
            string command = $"{Command} {commitId}:path={path}";
            ITree tree = null;

            using (Tracer.TraceCommand(Command, command, userData: _userData))
            using (var buffer = new ByteBuffer())
            using (IProcess process = GetBatchProcess())
            {
                try
                {
                    var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                    tree = ReadObjectInternal<ITree>(process, buffer, commitId, path, null);

                    TestExitCode(process, command, stderrTask);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CatFileCommand)} {nameof(ReadTreeByPath)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }

            return tree;
        }

        private Stream OpenBlobInternal(string command)
        {
            const int PipeBufferSize = 4 * 1024; // 4 KiB is the buffer size of a standard pipe

            object syncpoint = new object();

            // Allocate an anonymous pipe for communication between the two threads, do not make the
            // pipe inheritable as there's no need for child processes to inherit them; this could
            // lead to deadlock issues with open handles, etc.
            var writer = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.None, PipeBufferSize) { ReadMode = PipeTransmissionMode.Byte, };
            var reader = new AnonymousPipeClientStream(PipeDirection.In, writer.ClientSafePipeHandle) { ReadMode = PipeTransmissionMode.Byte, };

            // Acquire exclusive lock on the syncpoint
            lock (syncpoint)
            {
                var task = Task.Run(() =>
                {
                    // Acquire a lock on the syncpoint and signal the creator thread that work has started
                    lock (syncpoint)
                    {
                        Monitor.Pulse(syncpoint);
                    }

                    using (writer)
                    {
                        try
                        {
                            using (IProcess process = CreateCatFileProcess(command))
                            {
                                var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                                byte[] buffer = new byte[PipeBufferSize];

                                int read;
                                while ((read = process.StdOut.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    writer.Write(buffer, 0, read);
                                }

                                TestExitCode(process, command, stderrTask);
                            }
                        }
                        finally
                        {
                            // Close the channel to unblock any potential readers
                            writer.Close();
                        }
                    }
                });

                // Wait for the worker thread to signal that it has started before returning
                Monitor.Wait(syncpoint);
            }

            return reader;
        }

        private void CopyBlobToStreamInternal(Stream stream, string command)
        {
            object syncpoint = new object();

            // Acquire exclusive lock on the syncpoint
            lock (syncpoint)
            {
                var task = Task.Run(async () =>
                {

                    try
                    {
                        using (IProcess process = CreateCatFileProcess(command))
                        {
                            var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                            await process.StdOut.CopyToAsync(stream);

                            TestExitCode(process, command, stderrTask);
                        }
                    }
                    finally
                    {
                        // Close the channel to unblock any potential readers
                    }

                    // Acquire a lock on the syncpoint and signal the creator thread that work has finished (used to be started)
                    lock (syncpoint)
                    {
                        Monitor.Pulse(syncpoint);
                    }
                });

                // Wait for the worker thread to signal that it has started before returning
                Monitor.Wait(syncpoint);
            }
        }

        private ObjectHeader ReadHeaderInternal(IProcess process, ObjectId objectId, string path)
        {
            if (ReferenceEquals(process, null))
                throw new ArgumentNullException(nameof(process));

            var header = new ObjectHeader { };

            try
            {
                using (var buffer = new ByteBuffer())
                {
                    int idx = 0;
                    int read = 0;

                    // Compose the oid query and write it to stdin
                    string objIdStr = objectId.ToString();
                    if (path != null)
                    {
                        objIdStr += ":" + path;
                    }
                    objIdStr += '\n';

                    process.StandardInput.Write(objIdStr);

                    int r; bool success = false;
                    while (read < buffer.Length && (r = process.StdOut.Read(buffer, read, buffer.Length - read)) > 0)
                    {
                        read += r;

                        if (success = ReadObjectHeaderInternal(buffer, ref idx, read, out header))
                            break;
                        // Not successful, is it because git-cat-file reported the "{hex_sha_40}
                        // missing" message?
                        else if (Extensions.Equals(buffer, read - MissingErrorUtf8.Length, MissingErrorUtf8, 0, MissingErrorUtf8.Length))
                            throw new MissingObjectException(objectId);
                    }

                    if (!success)
                        throw new ObjectParseException("object-header", new StringUtf8(buffer, 0, read), idx);

                    return header;
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext(nameof(objectId), exception, objectId, path))
            {
                throw;
            }
        }

        private bool ReadObjectHeaderInternal(ByteBuffer buffer, ref int index, int count, out ObjectHeader header)
        {
            const char Eol = '\n';

            header = new ObjectHeader { };

            ObjectId objectId = ObjectId.Zero;
            long objectSize = -1;
            ObjectType objectType = ObjectType.Unknown;

            int eol = buffer.FirstIndexOf(Eol, index, count);
            if (eol < 0)
                return false;

            int i1 = 0,
                i2 = 0;

            i1 = buffer.FirstIndexOf('[', i2, count - i2);
            if (i1 < 0)
                return false;

            i2 = buffer.FirstIndexOf(']', i1, count - i1);
            if (i2 < 0)
                return false;

            Debug.Assert(i2 - i1 - 1 == ObjectId.Length);

            objectId = ObjectId.FromUtf8(buffer, i1 + 1);

            i1 = buffer.FirstIndexOf('[', i2, count - i2);
            if (i1 < 0)
                return false;

            i2 = buffer.FirstIndexOf(']', i1, count - i1);
            if (i2 < 0)
                return false;

            string parsedType = Encoding.ASCII.GetString(buffer, i1 + 1, i2 - i1 - 1);

            objectType = ObjectNameToType(parsedType);

            i1 = buffer.FirstIndexOf('[', i2, count - i2);
            if (i1 < 0)
                return false;

            i2 = buffer.FirstIndexOf(']', i1, count - i1);
            if (i2 < 0)
                return false;

            string parsedSize = Encoding.ASCII.GetString(buffer, i1 + 1, i2 - i1 - 1);

            if (!long.TryParse(parsedSize, NumberStyles.Number, CultureInfo.InvariantCulture, out objectSize))
                throw new ObjectParseException("object-size", new StringUtf8(buffer, 0, count), index);

            header = new ObjectHeader(objectId, objectType, objectSize);

            index = eol + 1;

            return true;
        }

        private T ReadObjectInternal<T>(IProcess process, ByteBuffer buffer, ObjectId objectId, string path, INamedObjectFilter filter)
            where T : class, IObject
        {
            var header = new ObjectHeader { };
            IObject result = null;

            int idx = 0;
            int read = 0;

            // write the object id to stdin, using committish:path format if path != null
            string objIdStr = objectId.ToString();
            if (path != null)
            {
                objIdStr += ":" + path;
            }
            objIdStr += '\n';
            process.StandardInput.Write(objIdStr);

            int r; bool success = false;
            while (read < buffer.Length && (r = process.StdOut.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += r;

                if (success = ReadObjectHeaderInternal(buffer, ref idx, read, out header))
                    break;
                // not successful, is it because git-cat-file reported the "{hex_sha_40} missing" message?
                else if (Extensions.Equals(buffer, read - MissingErrorUtf8.Length, MissingErrorUtf8, 0, MissingErrorUtf8.Length))
                    throw new MissingObjectException(objectId);
            }

            if (!success)
                throw new ObjectParseException("object-header", new StringUtf8(buffer, 0, read), idx);

            if (header.Size > ObjectSizeMaximum)
                throw new ObjectTooLargeException(objectId, header.Size);

            if (idx >= read || buffer.Length < header.Size)
            {
                MakeSpace(buffer, ref idx, ref read);
            }

            while (idx < read && buffer[idx] == '\0')
            {
                idx += 1;
            }

            while (read < header.Size + idx + 1)
            {
                while (buffer.Length < idx + header.Size)
                {
                    MakeSpace(buffer, ref idx, ref read);
                }

                if ((r = process.StdOut.Read(buffer, read, buffer.Length - read)) <= 0)
                    throw new ObjectParseException("object-data", new StringUtf8(buffer, 0, read), idx);

                read += r;
            }

            switch (header.Type)
            {
                case ObjectType.Blob:
                {
                    var content = new Internal.ResizableBuffer((int)header.Size);

                    content.Write(buffer, idx, (int)header.Size);

                    var blob = new Blob(header, content as IReadable);
                    blob.SetContextAndCache(Context, _repository as IStringCache);
                    result = blob;
                }
                break;

                case ObjectType.Commit:
                {
                    var commit = new Commit(header);
                    commit.SetContextAndCache(Context, _repository as IStringCache);
                    commit.ParseData(buffer, ref idx, read - idx, 0, filter);

                    result = commit;
                }
                break;

                case ObjectType.Submodule:
                    break;

                case ObjectType.Tag:
                {
                    var tagAnnotation = new TagAnnotation(header);
                    tagAnnotation.SetContextAndCache(Context, _repository as IStringCache);
                    tagAnnotation.ParseData(buffer, ref idx, read - idx, 0, filter);

                    result = tagAnnotation;
                }
                break;

                case ObjectType.Tree:
                {
                    var tree = new Tree(header);
                    tree.SetContextAndCache(Context, _repository as IStringCache);
                    tree.ParseData(buffer, ref idx, (int)header.Size, 0, filter);

                    result = tree;
                }
                break;
            }

            if (result is T value)
                return value;

            throw new ReferenceTypeMismatchException(FormattableString.Invariant($"{typeof(T).FullName} != {typeof(IObject).FullName}"));
        }

        private T ReadObjectStreamInternal<T>(IProcess process, ByteBuffer buffer, ObjectId objectId, ObjectDatabase.ReadStreamCompletedDelegate completedCallback)
            where T : class, IObject
        {
            var header = new ObjectHeader { };
            IObject result = null;

            int idx = 0;
            int read = 0;

            // write the object id to stdin
            string objIdStr = objectId.ToString() + '\n';
            process.StandardInput.Write(objIdStr);

            int r; bool success = false;
            while (read < buffer.Length && (r = process.StdOut.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += r;

                if (success = ReadObjectHeaderInternal(buffer, ref idx, read, out header))
                    break;
            }

            if (!success)
                throw new ObjectParseException("object-header", new StringUtf8(buffer, 0, read), idx);

            switch (header.Type)
            {
                case ObjectType.Blob:
                    {
                        var content = new CircularBuffer();
                        content.Write(buffer, idx, read - idx);

                        Task.Run(() =>
                        {
                            try
                            {
                                idx = 0;
                                read = 0;

                                using (var bytes = new ByteBuffer())
                                {
                                    while (read < header.Size && (r = process.StdOut.Read(buffer, 0, bytes.Length)) > 0)
                                    {
                                        read += r;

                                        content.Write(bytes, 0, r);
                                    }
                                }
                            }
                            finally
                            {
                                completedCallback?.Invoke(objectId);
                            }
                        });

                        var blob = new Blob(header, content as IReadable);
                        blob.SetContextAndCache(Context, _repository as IStringCache);
                        result = blob;
                    }
                    break;

                case ObjectType.Commit:
                    {
                        while (read - idx < header.Size)
                        {
                            if ((r = process.StdOut.Read(buffer, read, (int)header.Size - read)) <= 0)
                                throw new ObjectParseException("commit-data", new StringUtf8(buffer, 0, read), idx);

                            read += r;
                        }

                        var commit = new Commit(header);
                        commit.SetContextAndCache(Context, _repository as IStringCache);
                        commit.ParseData(buffer, ref idx, read - idx, 0, null);

                        result = commit;
                        completedCallback?.Invoke(objectId);
                    }
                    break;

                case ObjectType.Submodule:
                    break;

                case ObjectType.Tag:
                    {
                        while (read - idx < header.Size)
                        {
                            if ((r = process.StdOut.Read(buffer, read, (int)header.Size - read)) <= 0)
                                throw new ObjectParseException("tag-data", new StringUtf8(buffer, 0, read), idx);

                            read += r;
                        }

                        var tagAnnotation = new TagAnnotation(header);
                        tagAnnotation.SetContextAndCache(Context, _repository as IStringCache);
                        tagAnnotation.ParseData(buffer, ref idx, read - idx, 0, null);

                        result = tagAnnotation;
                        completedCallback?.Invoke(objectId);
                    }
                    break;

                case ObjectType.Tree:
                    {
                        while (read - idx < header.Size)
                        {
                            if ((r = process.StdOut.Read(buffer, read, (int)header.Size - read)) <= 0)
                                throw new ObjectParseException("tree-data", new StringUtf8(buffer, 0, read), idx);

                            read += r;
                        }

                        var tree = new Tree(header);
                        tree.SetContextAndCache(Context, _repository as IStringCache);
                        tree.ParseData(buffer, ref idx, read - idx, 0, null);

                        result = tree;
                        completedCallback?.Invoke(objectId);
                    }
                    break;
            }

            if (result is T value)
                return value;

            throw new ReferenceTypeMismatchException(FormattableString.Invariant($"{typeof(T).FullName} != {typeof(IObject).FullName}"));
        }

        private IProcess GetBatchProcess()
            => CreateCatFileProcess(BatchCommand);

        private IProcess GetCheckProcess()
            => CreateCatFileProcess(CheckCommand);

        private bool IsNullOrEmptyEnumeration(IEnumerable<ObjectId> objectIdEnumeration)
        {
            if (objectIdEnumeration == null)
                return true;

            var objectIdCollection = objectIdEnumeration as TypeCollection<ObjectId>;
            if (objectIdCollection != null)
                return objectIdCollection.Count <= 0;

            return false;
        }

        private IProcess CreateCatFileProcess(string command)
        {
            return CreateProcess(command);
        }
    }
}
