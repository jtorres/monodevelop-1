//*************************************************************************************************
// ResizableBuffer.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    interface IBuffer : IWritable
    {
        int Capacity { get; }
    }

    /// <summary>
    /// Thread-safe, exandable, in-memory buffer class build on top of <see cref="ByteBuffer"/>.
    /// </summary>
    internal sealed class ResizableBuffer : IBuffer, IDisposable
    {
        public const int DefaultSize = ByteBuffer.InitialSize;

        public ResizableBuffer(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentException(nameof(capacity));

            _buffer = new ByteBuffer(ByteBuffer.InitialSize);
        }

        public ResizableBuffer()
            : this(DefaultSize)
        { }

        /// <summary>
        /// Gets the number of bytes available to be read
        /// </summary>
        public int Available
        {
            get { return _set - _get; }
        }
        /// <summary>
        /// Gets the current capacity of the buffer in bytes
        /// </summary>
        public int Capacity
        {
            get
            {
                lock (_syncpoint)
                {
                    if (_buffer == null)
                        throw new ObjectDisposedException(typeof(ResizableBuffer).FullName);

                    return _buffer.Length;
                }
            }
        }
        /// <summary>
        /// Gets if the buffer has been closed.
        /// </summary>
        public bool Closed
        {
            get { lock (_syncpoint) return _closed; }
        }
        /// <summary>
        /// Gets the number of bytes written into the buffer.
        /// </summary>
        public int Length
        {
            get { lock (_syncpoint) return _set; }
        }

        private readonly object _syncpoint = new object();
        private ByteBuffer _buffer;
        private bool _closed;
        private int _get;
        private int _set;

        public void Close()
        {
            lock (_syncpoint)
            {
                if (_buffer == null)
                    throw new ObjectDisposedException(typeof(ResizableBuffer).FullName);

                _closed = true;
                Monitor.PulseAll(_syncpoint);
            }
        }

        public void Dispose()
        {
            lock (_syncpoint)
            {
                if (_buffer != null)
                {
                    _buffer.Dispose();
                    _buffer = null;
                    _set = -1;
                    _get = -1;
                }
            }
        }

        public int Read(byte[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (count + index > buffer.Length)
                throw new ArgumentOutOfRangeException($"`{nameof(index)} + {nameof(count)} > {nameof(buffer)}.Length`");

            lock (_syncpoint)
            {
                if (_buffer == null)
                    throw new ObjectDisposedException(typeof(ResizableBuffer).FullName);

                while (_get == _set - 1)
                {
                    if (_closed)
                        return 0;

                    Monitor.Wait(_syncpoint);
                }

                int length = _set - _get;
                int copy = Math.Min(count, length);

                Array.Copy(_buffer, _get, buffer, index, copy);
                _get += copy;

                return copy;
            }
        }

        public void Write(byte[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(count + index) > buffer.Length)
                throw new IndexOutOfRangeException(nameof(count));

            lock (_syncpoint)
            {
                if (_buffer == null)
                    throw new ObjectDisposedException(typeof(ResizableBuffer).FullName);
                if (_closed)
                    throw new InvalidOperationException($"`{typeof(ResizableBuffer).Name}.{nameof(Close)} == {true}`.");

                while (_set + count > _buffer.Length)
                {
                    Expand();
                }

                Array.Copy(buffer, index, _buffer, _set, count);
                _set += count;

                Monitor.PulseAll(_syncpoint);
            }
        }

        void IWritable.Write(byte[] buffer, int index, int count)
            => Write(buffer, index, count);

        private void Expand()
        {
            Debug.Assert(Monitor.IsEntered(_syncpoint), "Expected lock is not held");

            _buffer.Grow();
        }
    }
}
