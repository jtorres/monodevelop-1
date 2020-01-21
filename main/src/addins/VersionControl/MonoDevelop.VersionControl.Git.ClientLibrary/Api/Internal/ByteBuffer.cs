//*************************************************************************************************
// ByteBuffer.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal class ByteBuffer : IDisposable, IEquatable<ByteBuffer>
    {
        public const int InitialSize = 16 * 1024 / sizeof(byte); // 16,384 bytes or 16 KiB
        public const int MaximumSize = 64 * 1024 * 1024 / sizeof(byte); // 67,108,864 bytes or 64 MiB

        static ByteBuffer()
        {
            _cache = new ConcurrentBag<byte[]>();
        }

        /// <summary>
        /// Creates a new, or returned a cached `<see cref="ByteBuffer"/>` of `<paramref name="requestedSize"/>`.
        /// </summary>
        /// <param name="requestedSize"></param>
        public ByteBuffer(int requestedSize)
        {
            if (requestedSize < 0 || requestedSize > MaximumSize)
                throw new ArgumentOutOfRangeException(nameof(requestedSize));

            byte[] bytes = null;

            int maxTries = _cache.Count;
            int attempts = 0;

            while (bytes == null
                && attempts < maxTries
                && _cache.TryTake(out bytes)
                && bytes.Length < requestedSize)
            {
                _cache.Add(bytes);
                bytes = null;

                attempts += 1;
            }

            if (bytes == null)
            {
                int length = InitialSize;
                while (length < requestedSize)
                {
                    length *= 2;
                }

                bytes = new byte[length];
            }
            else
            {
                Array.Clear(bytes, 0, bytes.Length);
            }

            _bytes = bytes;
            _length = requestedSize;
            _threadId = Thread.CurrentThread.ManagedThreadId;
        }

        public ByteBuffer()
            : this(InitialSize)
        { }

        /// <summary>
        /// Gets the length of the buffer.
        /// </summary>
        public int Length
        {
            get { return _length; }
        }

        private static readonly ConcurrentBag<byte[]> _cache;

        private byte[] _bytes;
        private int _length;
        private readonly int _threadId;

        public byte this[int index]
        {
            get
            {
                Debug.Assert(_threadId == Thread.CurrentThread.ManagedThreadId);

                if (index < 0 || index >= _length)
                    throw new IndexOutOfRangeException(nameof(index));

                var bytes = _bytes;
                if (bytes == null)
                    throw new ObjectDisposedException(typeof(ByteBuffer).FullName);

                return bytes[index];
            }
            set
            {
                Debug.Assert(_threadId == Thread.CurrentThread.ManagedThreadId);

                if (index < 0 || index >= _length)
                    throw new IndexOutOfRangeException(nameof(index));

                var bytes = _bytes;
                if (bytes == null)
                    throw new ObjectDisposedException(typeof(ByteBuffer).FullName);

                bytes[index] = value;
            }
        }

        /// <summary>
        /// Releases all resources associated with this instance.
        /// </summary>
        public void Dispose()
        {
            byte[] bytes;
            if ((bytes = Interlocked.Exchange(ref _bytes, null)) == null)
                return;

            _length = -1;

            // only return initial sized buffers to the cache
            if (bytes.Length == InitialSize)
            {
                _cache.Add(bytes);
            }
        }

        public static bool Equals(ByteBuffer left, ByteBuffer right)
        {
            Debug.Assert(left._threadId == right._threadId);

            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(null, right))
                return false;
            if (ReferenceEquals(left._bytes, null) || ReferenceEquals(right._bytes, null))
                return false;
            if (left._length != right._length)
                return false;

            var lBytes = left._bytes;
            var rBytes = right._bytes;
            var length = left._length;

            return lBytes.Equals(0, rBytes, 0, length);
        }

        public override bool Equals(object obj)
            => ByteBuffer.Equals(this, obj as ByteBuffer);

        public bool Equals(ByteBuffer other)
            => ByteBuffer.Equals(this, other);

        public override int GetHashCode()
        {
            Debug.Assert(_threadId == Thread.CurrentThread.ManagedThreadId);

            var bytes = _bytes;
            return bytes == null
                ? 0
                : bytes.GetHashCode();
        }

        public void Grow(int increase)
        {
            if (increase < 0)
                throw new ArgumentOutOfRangeException(nameof(increase));

            Debug.Assert(_threadId == Thread.CurrentThread.ManagedThreadId);

            int want = checked(_length + increase);
            if (want > MaximumSize)
                throw new InvalidOperationException($"Cannot grow a buffer larger than {MaximumSize} bytes.");

            var bytes = _bytes;
            if (bytes == null)
                throw new ObjectDisposedException(typeof(ByteBuffer).FullName);

            int size = bytes.Length;

            while (size < want)
            {
                size = checked(size + InitialSize);
            }

            if (size > MaximumSize)
                throw new InvalidOperationException($"Cannot grow a buffer larger than {MaximumSize:###,###} bytes.");

            Array.Resize(ref bytes, size);

            _bytes = bytes;
            _length = bytes.Length;
        }
        public void Grow()
            => Grow(InitialSize);

        public void Shift(int index, int count)
        {
            Debug.Assert(_threadId == Thread.CurrentThread.ManagedThreadId);

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || checked(index + count) > _length)
                throw new ArgumentOutOfRangeException(nameof(count));

            System.Buffer.BlockCopy(_bytes, index, _bytes, 0, count);
        }

        public void Shrink(int decrease)
        {
            if (decrease < 0)
                throw new ArgumentOutOfRangeException(nameof(decrease));

            Debug.Assert(_threadId == Thread.CurrentThread.ManagedThreadId);

            int want = checked(_length - decrease);
            if (want < InitialSize)
                throw new InvalidOperationException($"Cannot shrink a buffer smaller than {InitialSize} bytes.");

            var bytes = _bytes;
            if (bytes == null)
                throw new ObjectDisposedException(typeof(ByteBuffer).FullName);

            int size = bytes.Length;

            while (size > want)
            {
                size = checked(size - InitialSize);
            }

            if (size < InitialSize)
                throw new InvalidOperationException($"Cannot shrink a buffer smaller than {InitialSize} bytes.");

            Array.Resize(ref bytes, size);

            _bytes = bytes;
            _length = bytes.Length;
        }
        public void Shrink()
            => Shrink(InitialSize);

        public int FirstIndexOf(char value, int start, int count)
        {
            Debug.Assert(_threadId == Thread.CurrentThread.ManagedThreadId);
            Debug.Assert(start >= 0, $"The `{nameof(start)}` parameter is less than zero.");
            Debug.Assert(count >= 0, $"The `{nameof(count)}` parameter is less than zero.");

            if (checked(start + count) > _length)
                throw new ArgumentOutOfRangeException(nameof(count));

            var bytes = _bytes;
            if (bytes == null)
                throw new ObjectDisposedException(typeof(ByteBuffer).FullName);

            return bytes.FirstIndexOf(start, count, (byte)value);
        }
        public int FirstIndexOf(char value, int count)
            => FirstIndexOf(value, 0, count);

        public int SecondIndexOf(char value, int start, int count)
        {
            Debug.Assert(_threadId == Thread.CurrentThread.ManagedThreadId);
            Debug.Assert(start >= 0, $"The `{nameof(start)}` parameter is less than zero.");
            Debug.Assert(count >= 0, $"The `{nameof(count)}` parameter is less than zero.");

            int firstIndex = FirstIndexOf(value, start, count);
            if (firstIndex >= 0 && firstIndex < _length - 1)
            {
                return FirstIndexOf(value, firstIndex + 1, count - (firstIndex - start + 1));
            }
            else
            {
                return -1;
            }
        }
        public int SecondIndexOf(char value, int count)
            => SecondIndexOf(value, 0, count);

        public bool IsWhiteSpace(int start, int count)
        {
            Debug.Assert(_threadId == Thread.CurrentThread.ManagedThreadId);
            Debug.Assert(start >= 0, $"The `{nameof(start)}` parameter is less than zero.");
            Debug.Assert(count >= 0 && start + count <= Length, $"The `{nameof(count)}` parameter is invalid.");

            if (count == 0)
                return true;

            var bytes = _bytes;
            if (bytes == null)
                throw new ObjectDisposedException(typeof(ByteBuffer).FullName);

            int end = checked(start + count);
            if (end > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    for (int i = start; i < end; i += 1)
                    {
                        char c = ((char*)ptr)[i];

                        Debug.Assert(!Char.IsSurrogate(c), $"Unexpected utf-16 surrogate character encountered; expected ascii value.");

                        if (!Char.IsWhiteSpace(c))
                            return false;
                    }
                }
            }

            return true;
        }
        public bool IsWhiteSpace()
            => IsWhiteSpace(0, _length);

        public int LastIndexOf(char value, int start, int count)
        {
            Debug.Assert(_threadId == Thread.CurrentThread.ManagedThreadId);
            Debug.Assert(start >= 0, $"The `{nameof(start)}` parameter is less than zero.");
            Debug.Assert(count >= 0, $"The `{nameof(count)}` parameter is less than zero.");

            if (checked(start + count) > _length)
                throw new ArgumentOutOfRangeException(nameof(count));

            var bytes = _bytes;
            if (bytes == null)
                throw new ObjectDisposedException(typeof(ByteBuffer).FullName);

            return bytes.LastIndexOf(start, count, (byte)value);
        }
        public int LastIndexOf(char value, int count)
            => LastIndexOf(value, 0, count);

        public bool StartsWith(string value, int start, int count)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Debug.Assert(_threadId == Thread.CurrentThread.ManagedThreadId);
            Debug.Assert(start >= 0, $"The `{nameof(start)}` parameter is less than zero.");
            Debug.Assert(count >= 0, $"The `{nameof(count)}` parameter is invalid.");

            if (start < 0)
                return false;
            if (count < 0)
                return false;
            if (count < value.Length)
                return false;

            int end = checked(start + count);
            if (end > _length)
                throw new ArgumentOutOfRangeException(nameof(count));

            var bytes = _bytes;
            if (bytes == null)
                throw new ObjectDisposedException(typeof(ByteBuffer).FullName);

            Debug.Assert(end <= _length, $"`{nameof(start)} + {nameof(count)} > {nameof(ByteBuffer.Length)}`.");

            if (end > _length || start + count < 0)
                return false;

            unsafe
            {
                fixed (char* pchars = value)
                fixed (byte* pbytes = bytes)
                {
                    for (int i = 0; i < value.Length; i += 1)
                    {
                        char a = pchars[i];
                        char b = (char)pbytes[i + start];

                        Debug.Assert(!Char.IsSurrogate(a), $"Unexpected utf-16 surrogate character encountered; expected ascii value.");
                        Debug.Assert(!Char.IsSurrogate(b), $"Unexpected utf-16 surrogate character encountered; expected ascii value.");

                        if (a != b)
                            return false;
                    }
                }
            }

            return true;
        }
        public bool StartsWith(string value, int count)
            => StartsWith(value, 0, count);
        public bool StartsWith(string value)
            => StartsWith(value, 0, _length);

        public string Substring(Encoding encoding, int start, int count)
        {
            Debug.Assert(_threadId == Thread.CurrentThread.ManagedThreadId);

            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (count < 0 || checked(start + count) > _length)
                throw new IndexOutOfRangeException(nameof(count));

            var bytes = _bytes;
            if (bytes == null)
                throw new ObjectDisposedException(typeof(ByteBuffer).FullName);

            int end = start + count;

            return encoding.GetString(bytes, start, count);
        }
        public string Substring(int start, int count)
            => Substring(Encoding.UTF8, start, count);
        public string Substring(int count)
            => Substring(Encoding.UTF8, 0, count);

        public static implicit operator byte[] (ByteBuffer buffer)
        {
            Debug.Assert(buffer._threadId == Thread.CurrentThread.ManagedThreadId);

            if (ReferenceEquals(buffer, null))
                return null;

            return buffer._bytes;
        }
    }
}
