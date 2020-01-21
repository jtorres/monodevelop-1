//*************************************************************************************************
// StringBuffer.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    /// <summary>
    /// Represents a mutable string of characters, which may or may not be cached for future reuse.
    /// </summary>
    internal sealed class StringBuffer : IDisposable
    {
        static StringBuffer()
        {
            _cache = new System.Collections.Concurrent.ConcurrentBag<StringBuilder>();
        }

        /// <summary>
        /// Initializes a new or returns an available cached instance of the <see cref="StringBuffer"/> class.
        /// </summary>
        public StringBuffer()
        {
            if (!_cache.TryTake(out _builder))
            {
                _builder = new StringBuilder();
            }
        }

        /// <summary>
        /// Initializes a new or returns an available caches instance of the <see cref="StringBuffer"/> class
        /// with an initial value of `<paramref name="value"/>`.
        /// </summary>
        /// <param name="value"></param>
        public StringBuffer(string value)
            : this()
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            builder.Append(value);
        }

        /// <summary>
        /// Gets or sets the character at the specified character position in this instance.
        /// </summary>
        /// <param name="index">The position of the character.</param>
        /// <returns>The Unicode character at position index.</returns>
        public char this[int index]
        {
            get
            {
                var builder = Volatile.Read(ref _builder);
                if (builder == null)
                    throw new ObjectDisposedException(typeof(StringBuffer).FullName);

                return builder[index];
            }
            set
            {
                var builder = Volatile.Read(ref _builder);
                if (builder == null)
                    throw new ObjectDisposedException(typeof(StringBuffer).FullName);

                builder[index] = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of characters that can be contained in the memory allocated by the current instance.
        /// </summary>
        public int Capacity
        {
            get
            {
                var builder = Volatile.Read(ref _builder);
                if (builder == null)
                    throw new ObjectDisposedException(typeof(StringBuffer).FullName);

                return builder.Capacity;
            }
        }

        /// <summary>
        /// Gets the number of characters currently held by this instance.
        /// </summary>
        public int Length
        {
            get
            {
                var builder = Volatile.Read(ref _builder);
                if (builder == null)
                    throw new ObjectDisposedException(typeof(StringBuffer).FullName);

                return builder.Length;
            }
        }

        /// <summary>
        /// Gets the maximum number of characters this instance can hold.
        /// </summary>
        public int MaxCapacity
        {
            get
            {
                var builder = Volatile.Read(ref _builder);
                if (builder == null)
                    throw new ObjectDisposedException(typeof(StringBuffer).FullName);

                return builder.MaxCapacity;
            }
        }

        private StringBuilder _builder;
        private static readonly System.Collections.Concurrent.ConcurrentBag<StringBuilder> _cache;

        /// <summary>
        /// Appends the string representation of a specified Unicode character to this instance
        /// </summary>
        /// <param name="value">The Unicode character to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public StringBuffer Append(char value)
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            builder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of the Unicode characters in a specified array to this instance.
        /// </summary>
        /// <param name="value">The array of characters to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public StringBuffer Append(char[] value)
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            builder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified subarray of Unicode characters to this instance.
        /// </summary>
        /// <param name="value">A character array.</param>
        /// <param name="index">The starting position in value.</param>
        /// <param name="count">The number of characters to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public StringBuffer Append(char[] value, int index, int count)
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            builder.Append(value, index, count);
            return this;
        }

        public StringBuffer Append(int value)
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            builder.Append(value);
            return this;
        }

        public StringBuffer Append(int? value)
        {
            if (value.HasValue)
            {
                var builder = Volatile.Read(ref _builder);
                if (builder == null)
                    throw new ObjectDisposedException(typeof(StringBuffer).FullName);

                builder.Append(value.Value);
            }
            return this;
        }

        /// <summary>
        /// Appends a copy of the specified string to this instance.
        /// </summary>
        /// <param name="value">The string to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public StringBuffer Append(string value)
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            builder.Append(value);
            return this;
        }

        /// <summary>
        /// <para>Appends a copy of the specified string to this instance.</para>
        /// <para>Escapes all unescaped instances of `<paramref name="characters"/>`, in
        /// `<paramref name="value"/>`, with `<paramref name="escape"/>`.</para>
        /// </summary>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public StringBuffer AppendEscaped(string value, IReadOnlyList<char> characters, char escape)
        {
            if (ReferenceEquals(value, null))
                throw new NullReferenceException(nameof(value));
            if (ReferenceEquals(characters, null))
                throw new NullReferenceException(nameof(characters));

            // no work to be done it there is nothing to escape
            if (value.Length == 0)
                return this;
            // no work to be done if there are no characters to escape
            if (characters.Count == 0)
                return this;

            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            // track the current position, and how many characters to escape
            int start = Length;
            int count = value.Length;

            // append the new value
            Append(value);
            // escape the appended value
            Escape(characters, escape, start, count);

            return this;
        }

        /// <summary>
        /// Appends the default line terminator to this instance.
        /// </summary>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public StringBuffer AppendLine()
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            builder.AppendLine();
            return this;
        }

        /// <summary>
        /// Removes all characters from the current <see cref="StringBuffer"/> instance.
        /// </summary>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public StringBuffer Clear()
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            builder.Clear();
            return this;
        }

        /// <summary>
        /// Copies the characters from a specified segment of this instance to a specified  segment of a destination <see cref="char"/>[].
        /// </summary>
        /// <param name="sourceIndex">The zero-based position in this instance where characters will be copied from.</param>
        /// <param name="destination">The array where characters will be copied.</param>
        /// <param name="index">The zero-based positio in destination where characters will be copied.</param>
        /// <param name="count">The number of characters to be copied.</param>
        public void CopyTo(int sourceIndex, char[] destination, int index, int count)
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            builder.CopyTo(sourceIndex, destination, index, count);
        }

        /// <summary>
        /// Releases all resources held by this instance.
        /// </summary>
        public void Dispose()
        {
            StringBuilder builder;
            if ((builder = Interlocked.Exchange(ref _builder, null)) == null)
                return;

            builder.Clear();
            _cache?.Add(builder);
        }

        /// <summary>
        /// Escapes all unescaped instances of `<paramref name="character"/>`, within a portion of the
        /// current <see cref="StringBuffer"/>, with `<paramref name="escape"/>`.
        /// </summary>
        /// <param name="characters">The characters to escape.</param>
        /// <param name="escape">The character to escape them with.</param>
        /// <param name="index">The first character to check.</param>
        /// <param name="count">The count of characters to check.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public StringBuffer Escape(char character, char escape, int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            if (checked(index + count) > builder.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (int i = 0; i < count; i += 1)
            {
                int idx = index + i;

                // if we find an unescaped double-quote character in the path,
                // we need to escape it
                if (builder[idx] == character
                    && builder[idx - 1] != escape)
                {
                    // insert an escape character
                    builder.Insert(idx, escape);
                    // move beyond the escaped character
                    i += 1;
                    // increase count because we've added an extra character
                    count += 1;
                }
            }

            return this;
        }

        /// <summary>
        /// Escapes all unescaped instances of `<paramref name="characters"/>`, within a portion
        /// of the current <see cref="StringBuffer"/>, with `<paramref name="escape"/>`.
        /// </summary>
        /// <param name="characters">The characters to escape.</param>
        /// <param name="escape">The character to escape them with.</param>
        /// <param name="index">The first character to check.</param>
        /// <param name="count">The count of characters to check.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public StringBuffer Escape(IReadOnlyList<char> characters, char escape, int index, int count)
        {
            if (ReferenceEquals(characters, null))
                throw new ArgumentNullException(nameof(characters));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            // no work to be done if there are no characters to escape
            if (characters.Count == 0)
                return this;

            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            if (checked(index + count) > builder.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (int i = 0; i < characters.Count; i += 1)
            {
                Escape(characters[i], escape, index, count);
            }

            return this;
        }
        /// <summary>
        /// Escapes all unescaped instances of `<paramref name="characters"/>`, in the current
        /// <see cref="StringBuffer"/> instance, with `<paramref name="escape"/>`.
        /// </summary>
        /// <param name="characters">The characters to escape.</param>
        /// <param name="escape">The character to escape them with.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public StringBuffer Escape(IReadOnlyList<char> characters, char escape)
            => Escape(characters, escape, 0, Length);

        public int FirstIndexOf(char value, int start, int count)
        {
            if (start < 0)
                return -1;
            if (count < 0)
                return -1;

            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            if (start > builder.Length)
                return -1;

            int cnt = Math.Min(count, builder.Length - start);

            for (int i = 0; i < cnt; i += 1)
            {
                if (builder[start + i] == value)
                    return start + i;
            }

            return -1;
        }

        public StringBuffer Insert(int index, char value)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            if (index >= builder.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            builder.Insert(index, value);

            return this;
        }

        public bool IsWhiteSpace(int start, int count)
        {
            if (start < 1 || count < 1)
                return false;

            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            if (start > builder.Length)
                return false;

            int cnt = Math.Min(count, builder.Length - start);

            for (int i = 0; i < cnt; i += 1)
            {
                if (!Char.IsWhiteSpace(builder[start + i]))
                    return false;
            }

            return true;
        }

        public int LastIndexOf(char value, int start, int count)
        {
            if (start < 0 || count < 0)
                return -1;

            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            if (start > builder.Length)
                return -1;

            int cnt = Math.Min(count, builder.Length - start);

            for (int i = 0; i < cnt; i += 1)
            {
                int index = builder.Length - start - i;

                if (builder[index] == value)
                    return index;
            }

            return -1;
        }

        public StringBuffer Prepend(char value)
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            builder.Insert(0, value);
            return this;
        }

        public StringBuffer Prepend(string value)
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            builder.Insert(0, value);
            return this;
        }

        public void ReadStreamReader(StreamReader reader)
        {
            char[] buffer = new char[(4096 / sizeof(char)) - 32];

            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                Append(buffer, 0, read);
            }
        }

        public StringBuffer Replace(char oldChar, char newChar)
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            builder.Replace(oldChar, newChar);
            return this;
        }

        public bool StartsWith(string value, int start, int count)
        {
            if (start < 0 || count < 0)
                return false;
            if (count < value.Length)
                return false;

            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            if (start > builder.Length)
                return false;

            int cnt = Math.Min(count, builder.Length - start);

            if (value.Length > cnt)
                return false;

            for (int i = 0; i < cnt; i += 1)
            {
                if (builder[start + i] != value[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Removes the specified range of characters from this instance.
        /// </summary>
        /// <param name="start">The zero-based position in this instance where removal begins.</param>
        /// <param name="count">The number of characters to remove.</param>
        /// <returns>A reference to this instance after the excise operation has completed.</returns>
        public StringBuffer Remove(int start, int count)
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            builder.Remove(start, count);

            return this;
        }

        /// <summary>
        /// Creates a string from a portion of the buffer from this instance.
        /// </summary>
        /// <param name="start">The zero-based position in this instance where copying begins.</param>
        /// <param name="length">The number of characters to copy.</param>
        /// <param name="result">A reference to the newly created string if successful.</param>
        /// <returns>True if successful; false otherwise.</returns>
        public bool TrySubstring(int start, int length, out string result)
        {
            result = null;

            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            if (start >= 0 && length >= 0 && start + length < builder.Length)
            {
                result = builder.ToString(start, length);
            }

            return result != null;
        }

        /// <summary>
        /// Creates a string from the whole of the buffer from this instance.
        /// </summary>
        /// <returns>A string whose value is the same as this instance.</returns>
        public override string ToString()
        {
            var builder = Volatile.Read(ref _builder);
            if (builder == null)
                throw new ObjectDisposedException(typeof(StringBuffer).FullName);

            return builder.ToString();
        }

        public static implicit operator string(StringBuffer buffer)
        {
            if (ReferenceEquals(buffer, null))
                return null;

            return buffer.ToString();
        }

        public static implicit operator StringBuilder(StringBuffer buffer)
        {
            if (ReferenceEquals(buffer, null))
                return null;

            return buffer._builder;
        }
    }
}
