//*************************************************************************************************
// Blob.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.IO;
using System.Threading;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model wrapper for a Git blob (file content).
    /// </summary>
    public interface IBlob : IDisposable, IObject, IEquatable<IBlob>
    {
        /// <summary>
        /// Gets `<see langword="true"/>` if the blob contains readable data; otherwise `<see langword="false"/>`.
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// The tree which enumerates this blob.
        /// </summary>
        ITree Parent { get; }

        /// <summary>
        /// Reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and returns number of bytes read into the buffer.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes.
        /// <para/>
        /// When this method returns, `<paramref name="buffer"/>` contains the specified byte array with the values between `<paramref name="offset"/>` and (`<paramref name="offset"/>` + `<paramref name="count"/>` - 1) replaced by the bytes read from the current source.
        /// </param>
        /// <param name="offset">The zero-based byte offset in `<paramref name="buffer"/>` at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        int ReadContent(byte[] buffer, int offset, int count);

        /// <summary>
        /// Reads the bytes from the blob, if it is readable, and writes them to `<paramref name="writableStream"/>`.
        /// </summary>
        /// <param name="writableStream">The stream to which the contents of the current stream will be copied.</param>
        void ToStream(Stream writableStream);
    }

    /// <summary>
    /// Object model wrapper for a Git blob (file content).
    /// </summary>
    internal class Blob : ObjectBase, IBlob, IEquatable<Blob>
    {
        public Blob(ITree parent, ObjectHeader header, IReadable readable)
            : base(parent, header, ObjectType.Blob)
        {
            _parent = parent;
            _readable = readable;
        }

        public Blob(ITree parent, ObjectHeader header, IWritable writable)
            : this(parent, header, writable as IReadable)
        {
            _writable = writable;
        }

        public Blob(ObjectHeader header, IReadable readable)
            : this(null, header, readable)
        { }

        public Blob(ObjectHeader header, IWritable writable)
            : this(null, header, writable)
        { }

        public Blob(ObjectHeader header)
            : this(null, header, null)
        { }

        private readonly ITree _parent;
        private IReadable _readable;
        private IWritable _writable;

        public bool CanRead
        {
            get { return _readable != null; }
        }

        public bool CanWrite
        {
            get { return _writable != null; }
        }

        public ITree Parent
        {
            get { return _parent; }
        }

        public void Dispose()
        {
            (Interlocked.Exchange(ref _readable, null) as IDisposable)?.Dispose();
            (Interlocked.Exchange(ref _writable, null) as IDisposable)?.Dispose();
        }

        public bool Equals(Blob other)
            => Comparer.Equals(this, other);

        public bool Equals(IBlob other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            return Equals(obj as Blob)
                || Equals(obj as IBlob)
                || base.Equals(obj);
        }

        public override int GetHashCode()
            => base.GetHashCode();

        public int ReadContent(byte[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            var readable = System.Threading.Volatile.Read(ref _readable);
            if (readable == null)
                throw new InvalidOperationException();

            return readable.Read(buffer, index, count);
        }

        public void ToStream(Stream writableStream)
        {
            if (writableStream == null)
                throw new ArgumentNullException(nameof(writableStream));
            if (!writableStream.CanWrite)
                throw new ArgumentException(nameof(writableStream));

            var readable = System.Threading.Volatile.Read(ref _readable);
            if (readable == null)
                throw new InvalidOperationException();

            using (var buffer = new ByteBuffer())
            {
                int read;
                while ((read = readable.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writableStream.Write(buffer, 0, read);
                }
            }
        }

        public override string ToString()
            => base.ToString();

        internal void FromStream(Stream readableStream)
        {
            if (readableStream == null)
                throw new ArgumentNullException(nameof(readableStream));
            if (!readableStream.CanRead)
                throw new ArgumentException(nameof(readableStream));

            var writable = System.Threading.Volatile.Read(ref _writable);
            if (writable == null)
                throw new InvalidOperationException();

            using (var buffer = new ByteBuffer())
            {
                int read;
                while ((read = readableStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writable.Write(buffer, 0, read);
                }
            }
        }

        internal override void ParseData(ByteBuffer buffer, ref int index, int count, int skipPrefix, INamedObjectFilter filter)
        {
            throw new NotSupportedException();
        }

        internal void WriteContent(byte[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            var writable = System.Threading.Volatile.Read(ref _writable);
            if (writable == null)
                throw new InvalidOperationException();

            writable.Write(buffer, index, count);
        }
    }
}
