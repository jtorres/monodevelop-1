//*************************************************************************************************
// CircularBuffer.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static System.FormattableString;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    /// <summary>
    /// Pipe like, thread-safe, in-memory, circular buffer.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay, nq")]
    internal sealed class CircularBuffer : IBuffer, IDisposable
    {
        public const int DefaultCapacity = 16 * 1024;
        public const int MaximumCapacity = 64 * 1024 * 1024;
        private const int BufferSize = 16 * 1024;

        public CircularBuffer(int capacity)
        {
            Debug.Assert(capacity > 0, $"The `{nameof(capacity)}` parameter is less than zero.");
            Debug.Assert(capacity <= MaximumCapacity, $"The `{nameof(capacity)}` parameter is greater than `{nameof(CircularBuffer)}.{nameof(MaximumCapacity)}`.");

            _buffer = new byte[capacity];

            int bufferSize = Math.Min(capacity, BufferSize);

            _closed = false;
            _get = 0;
            _put = 0;
            _syncpoint = new object();
            _totalRead = 0;
            _totalWritten = 0;
        }

        public CircularBuffer()
            : this(DefaultCapacity)
        { }

        /// <summary>
        /// Gets the maximum capacity, in bytes, of the channel.
        /// </summary>
        public int Capacity
        {
            get { lock (_syncpoint) return _buffer.Length; }
        }

        /// <summary>
        /// Gets true if the channel is closed; otherwise false.
        /// </summary>
        public bool Closed
        {
            get { return Volatile.Read(ref _closed); }
        }

        /// <summary>
        /// Gets the number of bytes written into the channel and not read back out yet.
        /// </summary>
        public long Length
        {
            get { lock (_syncpoint) return InternalLength; }
        }

        /// <summary>
        /// Gets the total number of bytes read from the channel.
        /// </summary>
        public long TotalBytesRead
        {
            get { return Volatile.Read(ref _totalRead); }
        }

        /// <summary>
        /// Gets the total number of bytes written into the channel.
        /// </summary>
        public long TotalBytesWritten
        {
            get { return Volatile.Read(ref _totalWritten); }
        }

        internal long InternalLength
        {
            get
            {
                Debug.Assert(Monitor.IsEntered(_syncpoint), "Expected lock not held during read.");
                Debug.Assert(_get >= 0 && _get < _buffer.Length, $"The `{nameof(_get)}` value is outside the bounds of `{nameof(_buffer)}`.");
                Debug.Assert(_put >= 0 && _put < _buffer.Length, $"The `{nameof(_put)}` value is outside the bounds of `{nameof(_buffer)}`.");

                return _totalWritten - _totalRead;
            }
        }

        int IReadable.Available
            => (int)Length;

        bool IReadable.Closed
            => Closed;

        int IReadable.Length
            => (int)Length;

        private string DebuggerDisplay
        {
            get { return Invariant($"{nameof(CircularBuffer)}: {TotalBytesRead}/{TotalBytesWritten}"); }
        }

        private readonly byte[] _buffer;

        private bool _closed;

        private int _get;
        private int _put;

        private readonly object _syncpoint;

        private long _totalRead;
        private long _totalWritten;

        /// <summary>
        /// Closes the <see cref="CircularBuffer"/>, notifying the other end of the channel that it has been closed.
        /// </summary>
        public void Close()
        {
            lock (_syncpoint)
            {
                _closed = true;

                // signal all waiters that the channel is closed
                Monitor.PulseAll(_syncpoint);
            }
        }

        /// <summary>
        /// Releases all resources associated with this instance.
        /// </summary>
        public void Dispose()
        {
            // `Dispose` means close
            Close();
        }

        /// <summary>
        /// Writes all bytes in the channel to a stream until the <see cref="CircularBuffer"/> is closed.
        /// </summary>
        /// <param name="output">Writable stream.</param>
        /// <returns>The number of bytes successfully written into the stream.</returns>
        public long FlushToStream(Stream output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (!output.CanWrite)
                throw new ArgumentException($"{typeof(Stream)}.{nameof(Stream.CanWrite)} == {false}", nameof(output));

            int bufferSize = Math.Min(_buffer.Length, BufferSize);
            byte[] buffer = new byte[bufferSize];
            long written = 0;

            int read = 0;
            while (!Closed && (read = InternalRead(buffer, 0, buffer.Length, true)) > 0)
            {
                output.Write(buffer, 0, read);
                written += read;
            }

            return written;
        }

        /// <summary>
        /// <para>Writes all available bytes in a stream into the channel.</para>
        /// <para>Blocks the current thread so long as the channel remains open the stream returns bytes.</para>
        /// </summary>
        /// <param name="input">Readable stream.</param>
        /// <returns>The number of bytes written into the channel.</returns>
        public long FromStream(Stream input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (!input.CanRead)
                throw new ArgumentException($"{typeof(Stream).Name}.{nameof(Stream.CanRead)} == {false}", nameof(input));

            int bufferSize = Math.Min(_buffer.Length, BufferSize);
            byte[] buffer = new byte[bufferSize];
            long written = 0;

            int read = 0;
            while (!Closed && (read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                written += InternalWrite(buffer, 0, read, true);
            }

            return written;
        }

        /// <summary>
        /// <para>Reads bytes from the channel.</para>
        /// <para>Blocks until there is at least one byte to read.</para>
        /// </summary>
        /// <param name="buffer">The array of bytes tto copy bytes from the channel into.</param>
        /// <param name="offset">The offset into the array of where to begin copying.</param>
        /// <param name="count">The number of bytes to copy into the array.</param>
        /// <returns>The number of bytes successfully read from the channel.</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || checked(offset + count) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            return InternalRead(buffer, offset, count, false);
        }

        /// <summary>
        /// <para>Reads bytes from the channel.</para>
        /// <para>Blocks the current thread until all bytes have been ready from the channel or the channel is closed.</para>
        /// </summary>
        /// <param name="buffer">The array of bytes to copy bytes from the channel into.</param>
        /// <param name="offset">The offset into the array of where to begin copying.</param>
        /// <param name="count">The number of bytes to copy into the array.</param>
        /// <returns>The number of bytes successfully read from the buffer.</returns>
        public int ReadWait(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > buffer.Length)
                throw new IndexOutOfRangeException(nameof(count));

            return InternalRead(buffer, offset, count, true);
        }

        /// <summary>
        /// <para>Writes bytes into the channel.</para>
        /// <para>Blocks until at least one byte can be written.</para>
        /// </summary>
        /// <param name="buffer">The array of bytes to be copied into the buffer.</param>
        /// <param name="offset">The offset into the array of where to begin copying.</param>
        /// <param name="count">The number of bytes from the array to copy.</param>
        /// <returns>The number of bytes successfully written into the buffer.</returns>
        public int Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > buffer.Length)
                throw new IndexOutOfRangeException(nameof(count));

            return InternalWrite(buffer, offset, count, false);
        }

        /// <summary>
        /// <para>Writes bytes into the channel.</para>
        ///
        /// <para>Blocks the current thread until all bytes have been written into the channel or the channel is closed.</para>
        /// </summary>
        /// <param name="buffer">The array of bytes to be copied into the buffer.</param>
        /// <param name="offset">The offset into the array of where to begin copying.</param>
        /// <param name="count">The number of bytes from the array to copy.</param>
        /// <returns>The number of bytes successfully written into the buffer.</returns>
        public int WriteWait(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > buffer.Length)
                throw new IndexOutOfRangeException(nameof(count));

            return InternalWrite(buffer, offset, count, true);
        }

        /// <summary>
        /// Writes all available bytes in the channel to a stream.
        /// </summary>
        /// <param name="output">Writable stream.</param>
        /// <param name="autoFlush"><see langword="true"/> if <see cref="CircularBuffer"/> will flush after
        /// every write to <paramref name="output"/>; otherwise <see langword="false"/>.</param>
        /// <returns>The number of bytes successfully written into the stream.</returns>
        public long ToStream(Stream output, bool autoFlush)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (!output.CanWrite)
                throw new InvalidOperationException($"{typeof(Stream)}.{nameof(Stream.CanWrite)} == {false}");

            int bufferSize = Math.Min(_buffer.Length, BufferSize);
            byte[] buffer = new byte[bufferSize];
            long written = 0;

            int read = 0;
            while ((read = InternalRead(buffer, 0, buffer.Length, false)) > 0)
            {
                output.Write(buffer, 0, read);
                written += read;

                if (autoFlush)
                {
                    output.Flush();
                }
            }

            return written;
        }
        /// <summary>
        /// Writes all available bytes in the channel to a stream.
        /// </summary>
        /// <param name="output">Writable stream.</param>
        /// <returns>The number of bytes successfully written into the stream.</returns>
        public long ToStream(Stream output)
            => ToStream(output, false);

        internal int InternalRead(byte[] buffer, int offset, int count, bool waitForCompletion)
        {
            Debug.Assert(buffer != null, $"The `{nameof(buffer)}` parameter is null.");
            Debug.Assert(count >= 0 && count <= buffer.Length, $"The `{nameof(count)}` parameter is outside the bounds of `{nameof(buffer)}`.");
            Debug.Assert(offset >= 0 && offset <= buffer.Length, $"The `{nameof(offset)}` parameter is outside the bounds of `{nameof(buffer)}`.");
            Debug.Assert(checked(count + offset) <= buffer.Length, $"The `{nameof(count)}` + `{nameof(offset)}` parameters exeed the size of `{nameof(buffer)}`.");

            lock (_syncpoint)
            {
                int read = 0;

                while (read < count)
                {
                    while (InternalLength == 0)
                    {
                        if (_closed)
                            return read;
                        if (!waitForCompletion && read > 0)
                            return read;

                        Monitor.Wait(_syncpoint);
                    }

                    Debug.Assert(_get >= 0 && _get < _buffer.Length, $"The `{nameof(_get)}` value is outside the bounds of `{nameof(_buffer)}`.");
                    Debug.Assert(_put >= 0 && _put < _buffer.Length, $"The `{nameof(_put)}` value is outside the bounds of `{nameof(_buffer)}`.");

                    int avail = (_get < _put)
                        ? _put - _get
                        : _buffer.Length - _get;

                    Debug.Assert(avail >= 0, $"The `{nameof(avail)}` value is less than zero.");

                    int take = Math.Min(avail, count - read);

                    Debug.Assert(take > 0, $"The `{nameof(take)}` value is less than or equal to zero.");

                    System.Buffer.BlockCopy(_buffer, _get, buffer, offset + read, take);

                    _get += take;
                    _get %= _buffer.Length;

                    read += take;
                    _totalRead += take;

                    Monitor.PulseAll(_syncpoint);
                }

                return read;
            }
        }

        internal int InternalWrite(byte[] buffer, int offset, int count, bool waitForCompletion)
        {
            Debug.Assert(buffer != null, $"The `{nameof(buffer)}` parameter is null.");
            Debug.Assert(count >= 0 && count <= buffer.Length, $"The `{nameof(count)}` parameter is outside the bounds of `{nameof(buffer)}`.");
            Debug.Assert(offset >= 0 && offset <= buffer.Length, $"The `{nameof(offset)}` parameter is outside the bounds of `{nameof(buffer)}`.");
            Debug.Assert(checked(count + offset) <= buffer.Length, $"The `{nameof(count)}` + `{nameof(offset)}` parameters exeed the size of `{nameof(buffer)}`.");

            lock (_syncpoint)
            {
                int written = 0;

                while (written < count)
                {
                    while (InternalLength == Capacity)
                    {
                        if (_closed)
                            return written;
                        if (!waitForCompletion && written > 0)
                            return written;

                        Monitor.Wait(_syncpoint);
                    }

                    Debug.Assert(_get >= 0 && _get < _buffer.Length, $"The `{nameof(_get)}` value is outside the bounds of `{nameof(_buffer)}`.");
                    Debug.Assert(_put >= 0 && _put < _buffer.Length, $"The `{nameof(_put)}` value is outside the bounds of `{nameof(_buffer)}`.");

                    int avail = (_put >= _get)
                        ? _buffer.Length - _put
                        : _get - _put;

                    Debug.Assert(avail >= 0, $"The `{nameof(avail)}` value is less than zero.");

                    int give = Math.Min(avail, count - written);

                    Debug.Assert(give > 0, $"The `{nameof(give)}` value is less than or equal to zero.");

                    System.Buffer.BlockCopy(buffer, offset + written, _buffer, _put, give);

                    _put += give;
                    _put %= _buffer.Length;

                    written += give;
                    _totalWritten += give;

                    Monitor.PulseAll(_syncpoint);
                }

                return written;
            }
        }

        void IReadable.Close()
            => Close();

        int IReadable.Read(byte[] buffer, int index, int count)
            => Read(buffer, index, count);

        void IWritable.Write(byte[] buffer, int index, int count)
            => WriteWait(buffer, index, count);
    }
}
