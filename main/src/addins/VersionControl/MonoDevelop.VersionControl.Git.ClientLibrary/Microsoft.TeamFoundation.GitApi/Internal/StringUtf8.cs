//*************************************************************************************************
// StringUtf8.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    /// <summary>
    /// Multi-byte string class, specifically for handling UTF-8 encoded characters.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
    public unsafe class StringUtf8 : IComparable<StringUtf8>, IEnumerable<char>, IEquatable<StringUtf8>
    {
        public static readonly StringUtf8 Empty = new StringUtf8(string.Empty);
        public static readonly System.Text.Encoding Encoding = System.Text.Encoding.UTF8;

        /// <summary>
        /// Creates a new instance of <see cref="StringUtf8"/> from a non-null <see cref="string"/>.
        /// </summary>
        /// <param name="original">The <see cref="string"/> to create the instance from.</param>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="original"/> is null.</exception>
        public StringUtf8(string original)
        {
            if (ReferenceEquals(original, null))
                throw new ArgumentNullException(nameof(original));

            if (original.Length == 0)
            {
                _bytes = Array.Empty<byte>();
                _count = 0;
                _first = 0;
            }
            else
            {
                int count = Encoding.GetByteCount(original);
                count = GetBufferAllocationSize(count);

                _bytes = new byte[count];
                _first = 0;

                unsafe
                {
                    fixed (byte* bytes = _bytes)
                    fixed (char* chars = original)
                    {
                        _count = Encoding.GetBytes(chars, original.Length, bytes, count);
                    }
                }
            }
        }

        public StringUtf8 (string original, int index, int count)
        {
            if (ReferenceEquals (original, null))
                throw new ArgumentNullException (nameof (original));
            original = original.Substring (index, count);
            if (original.Length == 0) {
                _bytes = Array.Empty<byte> ();
                _count = 0;
                _first = 0;
            } else {
                int cnt = Encoding.GetByteCount (original);
                cnt = GetBufferAllocationSize (cnt);

                _bytes = new byte[cnt];
                _first = 0;

                unsafe {
                    fixed (byte* bytes = _bytes)
                    fixed (char* chars = original) {
                        _count = Encoding.GetBytes (chars, original.Length, bytes, cnt);
                    }
                }
            }
        }

        internal StringUtf8(byte[] bytes)
        {
            // pad the allocation by four bytes to prevent AV issues when using
            // optimizations like *((uint*)_bytes) comparisons.
            int allocationSize = GetBufferAllocationSize(bytes.Length);

            _bytes = new byte[allocationSize];
            _count = bytes.Length;
            _first = 0;

            Buffer.BlockCopy(bytes, 0, _bytes, _first, _count);
        }

        internal StringUtf8(byte[] bytes, int index, int count)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index + count > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            int allocationSize = GetBufferAllocationSize(count);

            _bytes = new byte[allocationSize];
            _count = count;
            _first = 0;

            Buffer.BlockCopy(bytes, index, _bytes, 0, count);
        }

        internal StringUtf8(char c, int repeated)
        {
            if (repeated < 0)
                throw new ArgumentOutOfRangeException(nameof(repeated));

            byte* uchar = stackalloc byte[sizeof(uint)];
            int size = Encoding.GetBytes(&c, 1, uchar, sizeof(uint));

            int count = (size * repeated);

            byte[] bytes = new byte[(count)];
            fixed (byte* b = bytes)
            {
                for (int i = 0; i < repeated; i += 1)
                {
                    int offset = i * size;
                    Buffer.MemoryCopy(uchar, b + offset, count - offset, size);
                }
            }

            _bytes = bytes;
            _count = count;
            _first = 0;
        }

        private StringUtf8(byte[] bytes, int count)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            _bytes = bytes;
            _count = count;
            _first = 0;
        }

        private StringUtf8(StringUtf8 original, int index, int count)
        {
            if (original == null)
                throw new ArgumentNullException(nameof(original));
            if (index < original._first)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || count > original._count)
                throw new ArgumentOutOfRangeException(nameof(count));

            _bytes = original._bytes;
            _count = count;
            _first = index;
        }

        /// <summary>
        /// The length, in bytes, of the string; not the count of characters in the string.
        /// </summary>
        public int Length
            => _count;

        internal byte this[int index]
        {
            get
            {
                if (index < 0 || checked(_first + index) > _count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _bytes[_first + index];
            }
        }

        private readonly byte[] _bytes;
        private readonly int _first;
        private readonly int _count;

        /// <summary>
        /// Compares two multi-byte strings by evaluating the numeric values of the corresponding <see cref="byte"/> values in each.
        /// <para>Case-insensitive comparisons incur <see cref="string"/> allocations.</para>
        /// </summary>
        /// <param name="left">The first string to compare.</param>
        /// <param name="right">The second string to compare.</param>
        /// <param name="ignoreCase"><see langword="true"/> if case-insensitive comparison should be used; <see langword="false"/> otherwise.</param>
        /// <returns>An integer that indicates the lexical relationship between the two comparands.</returns>
        public static int Compare(StringUtf8 left, StringUtf8 right, bool ignoreCase)
        {
            if (ReferenceEquals(left, right))
                return 0;
            if (ReferenceEquals(left, null))
                return 1;
            if (ReferenceEquals(right, null))
                return -1;

            StringUtf8Comparer comparer = (ignoreCase)
                ? StringUtf8Comparer.OrdinalIgnoreCase as StringUtf8Comparer
                : StringUtf8Comparer.Ordinal as StringUtf8Comparer;

            return comparer.Compare(left._bytes, left._first, left._count,
                                    right._bytes, right._first, right._count);
        }
        /// <summary>
        /// Compares two multi-byte strings by evaluating the numeric values of the corresponding <see cref="byte"/> values in each.
        /// <para>Case-insensitive comparisons incur <see cref="string"/> allocations.</para>
        /// </summary>
        /// <param name="left">The first string to compare.</param>
        /// <param name="right">The second string to compare.</param>
        /// <returns>An integer that indicates the lexical relationship between the two comparands.</returns>
        public static int Compare(StringUtf8 left, StringUtf8 right)
            => StringUtf8.Compare(left, right, false);
        /// <summary>
        /// Compares this multi-byte string to another by evaluating the numeric values of the corresponding <see cref="byte"/> values in each.
        /// <para>Case-insensitive comparisons incur <see cref="string"/> allocations.</para>
        /// </summary>
        /// <param name="other">The other string to compare to this instance.</param>
        /// <returns>An integer that indicates the lexical relationship between the two comparands.</returns>
        public int CompareTo(StringUtf8 other)
            => StringUtf8.Compare(this, other);

        /// <summary>
        /// Concatenates two specified instances of <see cref="StringUtf8"/> into a new instance.
        /// </summary>
        /// <param name="left">The first string to concatenate.</param>
        /// <param name="right">The second string to concatenate.</param>
        /// <returns>A new instance of <see cref="StringUtf8"/> containing the contents of
        /// <paramref name="left"/> and <paramref name="right"/>.</returns>
        public static StringUtf8 Concat(StringUtf8 left, StringUtf8 right)
        {
            if (ReferenceEquals(left, null))
                throw new ArgumentNullException(nameof(left));
            if (ReferenceEquals(null, right))
                throw new ArgumentNullException(nameof(right));

            int count = left.Length + right.Length;
            int allocationSize = GetBufferAllocationSize(count);
            // pad the allocation by four bytes to prevent AV issues when using
            // optimizations like *((uint*)_bytes) comparisons.
            byte[] bytes = new byte[allocationSize];

            Buffer.BlockCopy(left._bytes, left._first, bytes, 0, left._count);
            Buffer.BlockCopy(right._bytes, right._first, bytes, left._count, right._count);

            return new StringUtf8(bytes, count);
        }

        /// <summary>
        /// Copies one instance of <see cref="StringUtf8"/> into a new instance, without using slices.
        /// </summary>
        /// <param name="original">The instance to copy.</param>
        /// <returns>The copied instance.</returns>
        public static StringUtf8 Copy(StringUtf8 original)
        {
            if (ReferenceEquals(original, null))
                throw new ArgumentNullException(nameof(original));

            return new StringUtf8(original._bytes, original._first, original._count);
        }
        /// <summary>
        /// Copies this instance of <see cref="StringUtf8"/> into a new instance without using slices.
        /// </summary>
        /// <returns>The copied instance.</returns>
        public StringUtf8 Copy()
            => StringUtf8.Copy(this);

        /// <summary>
        /// Copies upto `<paramref name="count"/>` <see cref="byte"/>s, of this instance of
        /// <see cref="StringUtf8"/>, starting from `<paramref name="first"/>`, into
        /// `<paramref name="array"/>`, starting at `<paramref name="index"/>`.
        /// </summary>
        /// <param name="first">The zero-base offset of the first <see cref="byte"/> of this instance of <see cref="StringUtf8"/> to copy.</param>
        /// <param name="array">The array to copy the bytes from this instance of <see cref="StringUtf8"/> into.</param>
        /// <param name="index">The zero-based offset of `<paramref name="array"/>` to copy this instance of <see cref="StringUtf8"/> into.</param>
        /// <param name="count">The number of bytes of this instance of <see cref="StringUtf8"/> to copy.</param>
        /// <returns>The number of bytes copied.</returns>
        public int CopyTo(int first, byte[] array, int index, int count)
        {
            if (first < 0)
                throw new ArgumentOutOfRangeException(nameof(first));
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 && checked(index + count) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            // Calculate how much to copy, while not allowing the copy to exceed `_count` or `_byte.Length`.
            int copylen = Math.Min(_count - first, count);

            Buffer.BlockCopy(_bytes, _first + first, array, index, copylen);

            return copylen;
        }

        /// <summary>
        /// Copies upto `<paramref name="count"/>` <see cref="byte"/>s, of this instance of
        /// <see cref="StringUtf8"/>, starting from `<paramref name="first"/>`, into
        /// `<paramref name="array"/>`.</summary>
        /// <param name="first">The zero-base offset of the first <see cref="byte"/> of this instance of <see cref="StringUtf8"/> to copy.</param>
        /// <param name="array">The array to copy the bytes from this instance of <see cref="StringUtf8"/> into.</param>
        /// <param name="count">The number of bytes of this instance of <see cref="StringUtf8"/> to copy.</param>
        /// <returns>The number of bytes copied.</returns>
        public int CopyTo(int first, byte[] array, int count)
            => CopyTo(first, array, 0, count);

        /// <summary>
        /// Copies this instance of <see cref="StringUtf8"/> into `<paramref name="array"/>`.
        /// </summary>
        /// <param name="array">The array to copy the bytes from this instance of <see cref="StringUtf8"/> into.</param>
        /// <returns>The number of bytes copied.</returns>
        public int CopyTo(byte[] array)
            => CopyTo(0, array, 0, _count);

        /// <summary>
        /// Determines whether the end of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The value to match to the end of this instance.</param>
        /// <param name="index">The index to finish matching against.</param>
        /// <param name="count">The maximum number of characters to match.</param>
        /// <param name="ignoreCase"><see langword="true"/> if the case of the character should be ignored;
        /// otherwise <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool EndsWith(StringUtf8 value, int index, int count, bool ignoreCase)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            // if the length to be matched is longer than the possible match length, there's no possile match
            if (count < value.Length)
                return false;

            // by definition a string ends with itself, unless match is constrained
            if (ReferenceEquals(this, value))
                return true;
            // if they are slices of the same backing data, if they end on the same position then their ends match
            if (ReferenceEquals(_bytes, value._bytes) && _first + _count == value._first + value._count)
                return true;

            // if there are not enough characters in the slice, there's no possile match
            if (_count - count < 0)
                return false;
            // if there are not enough characters in the slice, there's no possile match
            if (index + count > _count)
                return false;
            // if there are not enough characters in the slice, there's no possile match
            if (_first + index > _count)
                return false;

            // no need to match more than the length of the value string
            count = value.Length;
            index = Math.Max(index, _count - count);

            StringUtf8Comparer comparer = (ignoreCase)
                ? StringUtf8Comparer.OrdinalIgnoreCase as StringUtf8Comparer
                : StringUtf8Comparer.Ordinal as StringUtf8Comparer;

            StringUtf8 substring;
            if (TrySubstring(this, index, count, out substring))
                return comparer.Equals(substring, value);

            return false;
        }
        /// <summary>
        /// Determines whether the end of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The value to match to the end of this instance.</param>
        /// <param name="index">The index to finish matching against.</param>
        /// <param name="count">The maximum number of characters to match.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool EndsWith(StringUtf8 value, int index, int count)
            => EndsWith(value, index, count, false);
        /// <summary>
        /// Determines whether the end of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The value to match to the end of this instance.</param>
        /// <param name="index">The index to finish matching against.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool EndsWith(StringUtf8 value, int index)
            => EndsWith(value, index, _count - index, false);
        /// <summary>
        /// Determines whether the end of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The value to match to the end of this instance.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool EndsWith(StringUtf8 value)
            => EndsWith(value, 0, _count, false);

        /// <summary>
        /// Determines whether the end of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The value to match to the end of this instance.</param>
        /// <param name="index">The index to finish matching against.</param>
        /// <param name="count">The maximum number of characters to match.</param>
        /// <param name="ignoreCase"><see langword="true"/> if the case of the character should be ignored;
        /// otherwise <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool EndsWith(string value, int index, int count, bool ignoreCase)
            => EndsWith((StringUtf8)value, index, count, ignoreCase);
        /// <summary>
        /// Determines whether the end of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The value to match to the end of this instance.</param>
        /// <param name="index">The index to finish matching against.</param>
        /// <param name="count">The maximum number of characters to match.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool EndsWith(string value, int index, int count)
            => EndsWith((StringUtf8)value, index, count, false);
        /// <summary>
        /// Determines whether the end of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The value to match to the end of this instance.</param>
        /// <param name="index">The index to finish matching against.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool EndsWith(string value, int index)
            => EndsWith((StringUtf8)value, index, _count - index, false);
        /// <summary>
        /// Determines whether the end of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The value to match to the end of this instance.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool EndsWith(string value)
            => EndsWith((StringUtf8)value, 0, _count, false);

        /// <summary>
        /// Determines this instance of <see cref="StringUtf8"/> and an other have the same value.
        /// </summary>
        /// <param name="left">The first string to compare, or <see langword="null"/>.</param>
        /// <param name="right">The first string to compare, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the value of a is the same as the value of b; otherwise,
        /// <see langword="false"/>. If both a and b are null, the method returns <see langword="true"/>.</returns>
        public override bool Equals(object obj)
            => StringUtf8.Equals(this, obj as StringUtf8);

        /// <summary>
        /// Determines this instance of <see cref="StringUtf8"/> and an <paramref name="other"/> have the same value.
        /// </summary>
        /// <param name="other">A string to compare against this instance.</param>
        /// <returns><see langword="true"/> if the value of a is the same as the value of <paramref name="other"/>; otherwise,
        /// <see langword="false"/>. If both a and b are null, the method returns <see langword="true"/>.</returns>
        public bool Equals(StringUtf8 other)
            => StringUtf8.Equals(this, other);

        /// <summary>
        /// <para>Determines whether two specified <see cref="StringUtf8"/> objects have the same value.</para>
        /// <para>Case-insensitive comparisons incur <see cref="string"/> allocations.</para>
        /// </summary>
        /// <param name="left">The first string to compare, or <see langword="null"/>.</param>
        /// <param name="right">The first string to compare, or <see langword="null"/>.</param>
        /// <param name="ignoreCase"><see langword="true"/> if the case of the character should be ignored;
        /// otherwise <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if the value of a is the same as the value of b; otherwise,
        /// <see langword="false"/>. If both a and b are null, the method returns <see langword="true"/>.</returns>
        public static bool Equals(StringUtf8 left, StringUtf8 right, bool ignoreCase)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(null, right))
                return false;
            if (left._count != right._count)
                return false;

            StringUtf8Comparer comparer = (ignoreCase)
                ? StringUtf8Comparer.OrdinalIgnoreCase as StringUtf8Comparer
                : StringUtf8Comparer.Ordinal as StringUtf8Comparer;

            return comparer.Equals(left._bytes, left._first, left._count,
                                   right._bytes, right._first, right._count);
        }
        /// <summary>
        /// Determines whether two specified <see cref="StringUtf8"/> objects have the same value.
        /// </summary>
        /// <param name="left">The first string to compare, or <see langword="null"/>.</param>
        /// <param name="right">The first string to compare, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the value of a is the same as the value of b; otherwise,
        /// <see langword="false"/>. If both a and b are null, the method returns <see langword="true"/>.</returns>
        public static bool Equals(StringUtf8 left, StringUtf8 right)
            => StringUtf8.Equals(left, right, false);

        public bool EqualOrSibling(StringUtf8 other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(_bytes, other._bytes))
                return true;

            return StringUtf8.Equals(this, other);
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified Unicode character in this
        /// instance. The search starts at a specified byte position and examines a specified
        /// number of bytes.
        /// </summary>
        /// <param name="value">A Unicode character to seek.</param>
        /// <param name="index">The search starting position.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <returns>The zero-based index position of value from the start of the string if that
        /// character is found, or -1 if it is not.</returns>
        public int FirstIndexOf(char value, int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || checked(index + count) > _count)
                throw new ArgumentOutOfRangeException(nameof(count));

            byte* uchar = stackalloc byte[sizeof(uint)];
            int size = Encoding.GetBytes(&value, 1, uchar, sizeof(uint));
            uint mask = 0;

            for (int i = 0; i < size; i += 1)
            {
                mask <<= 8;
                mask += 0xFF;
            }

            fixed (byte* bytes = _bytes)
            {
                byte* s = bytes + _first;
                byte* b = s + index;
                byte* e = b + count;

                while (b != e)
                {
                    if ((*((uint*)b) & mask) == (*((uint*)uchar) & mask))
                        return (int)(b - s);

                    b += 1;
                }
            }

            return -1;
        }
        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified Unicode character
        /// in this string. The search starts at a specified byte position.
        /// </summary>
        /// <param name="value">A Unicode character to seek.</param>
        /// <param name="index">The search starting position.</param>
        /// <returns>The zero-based index position of value from the start of the string if that
        /// character is found, or -1 if it is not.</returns>
        public int FirstIndexOf(char value, int index)
            => FirstIndexOf(value, index, _count - index);
        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified Unicode character in this
        /// instance.
        /// </summary>
        /// <param name="value">A Unicode character to seek.</param>
        /// <returns>The zero-based index position of value from the start of the string if that
        /// character is found, or -1 if it is not.</returns>
        public int FirstIndexOf(char value)
            => FirstIndexOf(value, 0, _count);

        /// <summary>
        /// Retrieves an object that can iterate through the individual characters in this string.
        /// </summary>
        public IEnumerator<char> GetEnumerator()
        {
            // allocate two chars to handle the surrogate pairs edge case
            char[] chars = new char[2];

            int end = _first + _count;

            for (int i = _first; i < end; i += 1)
            {
                if (i < 0x80)
                {
                    yield return (char)_bytes[i];
                }
                else if (i < 0xE0)
                {
                    // UTF8  `0xC0 < byte[0] < 0xE0` are 2-byte values
                    int count = Encoding.GetChars(_bytes, i, 2, chars, 0);

                    if (count > 0)
                        yield return chars[0];

                    if (count > 1)
                        yield return chars[1];

                    i += 1;
                }
                else if (i < 0xF0)
                {
                    // UTF8  `0xE0 < byte[0] < 0xF0` are 3-byte values
                    int count = Encoding.GetChars(_bytes, i, 3, chars, 0);

                    if (count > 0)
                        yield return chars[0];

                    if (count > 1)
                        yield return chars[1];

                    i += 2;
                }
                else
                {
                    // UTF8  `0xF0 < byte[0]` are 4-byte values
                    int count = Encoding.GetChars(_bytes, i, 4, chars, 0);

                    if (count > 0)
                        yield return chars[0];

                    if (count > 1)
                        yield return chars[1];

                    i += 3;
                }
            }

            yield break;
        }

        /// <summary>
        /// Hashes the instance of <see cref="StringUtf8"/>, optionally ignoring case.
        /// </summary>
        /// <param name="ignoreCase"><see langword="true"/> if the case of the character should be ignored;
        /// otherwise <see langword="false"/>.</param>
        /// <returns>A hash code for the current instance.</returns>
        public int GetHashCode(bool ignoreCase)
        {
            StringUtf8Comparer comparer = ignoreCase
                ? StringUtf8Comparer.OrdinalIgnoreCase as StringUtf8Comparer
                : StringUtf8Comparer.Ordinal as StringUtf8Comparer;

            return comparer.GetHashCode(_bytes, _first, _count);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode()
            => GetHashCode(false);

        /// <summary>
        /// Indicates whether a specified string is empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="index">The index to begin checking this instance for white-space.</param>
        /// <param name="count">The maximum number of characters to check.</param>
        /// <returns><see langword="true"/> if only white-space; otherwise <see langword="false"/></returns>
        public bool IsWhiteSpace(int index, int count)
        {
            const int BlockSize = 64;

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || checked(index + count) > _count)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0)
                return true;

            unsafe
            {
                fixed (byte* ptr = _bytes)
                {
                    byte* bytes = ptr + _first + index;
                    char* chars = stackalloc char[64];

                    int tested = 0;

                    while (tested < count)
                    {
                        int byteCount = Math.Min(count - tested, BlockSize);
                        int charCount = Encoding.GetChars(bytes + tested, byteCount, chars, BlockSize);

                        for (int i = 0; i < charCount; i += 1)
                        {
                            if (!char.IsWhiteSpace(chars[i]))
                                return false;
                        }

                        tested += byteCount;
                    }
                }
            }

            return true;
        }
        /// <summary>
        /// Indicates whether a specified string is empty, or consists only of white-space characters.
        /// </summary>
        /// <returns><see langword="true"/> if only white-space; otherwise <see langword="false"/></returns>
        public bool IsWhiteSpace()
            => IsWhiteSpace(0, _count);

        /// <summary>
        /// Reports the zero-based index position of the last occurrence of the specified Unicode character in a substring within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.
        /// </summary>
        /// <param name="value">The Unicode character to seek.</param>
        /// <param name="index">The starting position of the search. The search proceeds from startIndex toward the beginning of this instance.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <returns>The zero-based index position of value if that character is found, or -1 if it is not found or if the current instance equals <see cref="Empty"/>.</returns>
        public int LastIndexOf(char value, int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || checked(index) + count > _count)
                throw new ArgumentOutOfRangeException(nameof(count));

            uint uchar = 0;
            int size = Encoding.GetBytes(&value, 1, (byte*)&uchar, sizeof(uint));
            uint mask = 0;

            for (int i = 0; i < size; i += 1)
            {
                mask <<= 8;
                mask += 0xFF;
            }

            fixed (byte* bytes = _bytes)
            {
                byte* s = bytes + _first;
                byte* e = s + index;
                byte* b = e + count;


                while (b != e)
                {
                    if ((*((uint*)b) & mask) == (*(&uchar) & mask))
                        return (int)(b - s);

                    b -= 1;
                }
            }

            return -1;
        }
        /// <summary>
        /// Reports the zero-based index position of the last occurrence of a specified string within
        /// this instance. The search starts at a specified byte position and proceeds backward
        /// toward the beginning of the string for a specified number of byte positions.
        /// </summary>
        /// <param name="value">The Unicode character to seek.</param>
        /// <param name="index">The search starting position. The search proceeds from startIndex
        /// toward the beginning of this instance.</param>
        /// <returns>The zero-based starting index position of value if that string is found, or -1 if
        /// it is not found or if the current instance equals <see cref="Empty"/>. If value is
        /// empty, the return value is the smaller of <paramref name="index"/> and the last index
        /// position in this instance.</returns>
        public int LastIndexOf(char value, int index)
            => LastIndexOf(value, index, _count - index);
        /// <summary>
        /// Reports the zero-based index position of the last occurrence of a specified string within
        /// this instance. The search starts at a specified byte position and proceeds backward
        /// toward the beginning of the string for a specified number of byte positions.
        /// </summary>
        /// <param name="value">The Unicode character to seek.</param>
        /// <returns>The zero-based starting index position of value if that string is found, or -1 if
        /// it is not found or if the current instance equals <see cref="Empty"/>.</returns>
        public int LastIndexOf(char value)
            => LastIndexOf(value, _first, _count - _first);

        /// <summary>
        /// Returns a new string in which all occurrences of a specified Unicode character in this instance are replaced with another specified Unicode character.
        /// <para>A string that is equivalent to this instance except that all instances of <paramref name="oldValue"/> are replaced with <paramref name="newValue"/>.</para>
        /// <para>If <paramref name="oldValue"/> is not found in the current instance, the method returns the current instance unchanged.</para>
        /// </summary>
        /// <param name="oldValue">The ASCII to be replaced.</param>
        /// <param name="newValue">The ASCII character to replace all occurrences of oldChar.</param>
        /// <param name="index">The earliest point in the string where replacement will take place.</param>
        /// <param name="count">The number of characters to comparer and potential replace.</param>
        public StringUtf8 Replace(byte oldValue, byte newValue, int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || checked(index + count) > _count)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (oldValue >= 0x80)
                throw new ArgumentException("Invalid UTF-8 character.", nameof(oldValue));
            if (newValue >= 0x80)
                throw new ArgumentException("Invalid UTF-8 character.", nameof(newValue));

            byte[] array = null;
            int length = index + count;

            for (int i = _first + index; i < length; i += 1)
            {
                if (_bytes[i] == oldValue)
                {
                    if (ReferenceEquals(array, null))
                    {
                        array = new byte[_count];
                        Buffer.BlockCopy(_bytes, _first, array, 0, _count);
                    }

                    array[i - index - _first] = newValue;
                }
            }

            return ReferenceEquals(array, null)
                ? this
                : new StringUtf8(array);
        }
        /// <summary>
        /// Returns a new string in which all occurrences of a specified Unicode character in this instance are replaced with another specified Unicode character.
        /// <para>A string that is equivalent to this instance except that all instances of <paramref name="oldValue"/> are replaced with <paramref name="newValue"/>.</para>
        /// <para>If <paramref name="oldValue"/> is not found in the current instance, the method returns the current instance unchanged.</para>
        /// </summary>
        /// <param name="oldValue">The ASCII to be replaced.</param>
        /// <param name="newValue">The ASCII character to replace all occurrences of oldChar.</param>
        public StringUtf8 Replace(byte oldValue, byte newValue)
            => Replace(oldValue, newValue, 0, _count);

        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The string that this instance starts with.</param>
        /// <param name="index">The index to begin matching.</param>
        /// <param name="count">The maximum number of characters to match.</param>
        /// <param name="ignoreCase"><see langword="true"/> if the case of the character should be ignored;
        /// otherwise <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool StartsWith(StringUtf8 value, int index, int count, bool ignoreCase)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || checked(index + count) > _count)
                throw new ArgumentOutOfRangeException(nameof(count));

            StringUtf8Comparer comparer = (ignoreCase)
                ? StringUtf8Comparer.OrdinalIgnoreCase as StringUtf8Comparer
                : StringUtf8Comparer.Ordinal as StringUtf8Comparer;

            count = Math.Min(count, value.Length);

            // create a substring of the slice to be compared
            StringUtf8 substring;
            if (TrySubstring(this, index, count, out substring))
                return comparer.Equals(substring, value);

            return false;
        }
        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The string that this instance starts with.</param>
        /// <param name="index">The index to begin matching.</param>
        /// <param name="count">The maximum number of characters to match.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool StartsWith(StringUtf8 value, int index, int count)
            => StartsWith(value, index, count, false);
        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The string that this instance starts with.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool StartsWith(StringUtf8 value)
            => StartsWith(value, 0, _count, false);

        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The string that this instance starts with.</param>
        /// <param name="index">The index to begin matching.</param>
        /// <param name="count">The maximum number of characters to match.</param>
        /// <param name="ignoreCase"><see langword="true"/> if the case of the character should be ignored;
        /// otherwise <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool StartsWith(string value, int index, int count, bool ignoreCase)
            => StartsWith((StringUtf8)value, index, count, ignoreCase);
        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The string that this instance starts with.</param>
        /// <param name="index">The index to begin matching.</param>
        /// <param name="count">The maximum number of characters to match.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool StartsWith(string value, int index, int count)
            => StartsWith((StringUtf8)value, index, count, false);
        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string.
        /// </summary>
        /// <param name="value">The string that this instance starts with.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
        public bool StartsWith(string value)
            => StartsWith((StringUtf8)value, 0, _count, false);

        /// <summary>
        /// <para>Retrieves a substring from this instance. The substring starts at a specified
        /// byte position and has a specified length.</para>
        /// <para>The underlying bytes are not duplicated during substring creation, instead both
        /// <see cref="StringUtf8"/> instances reference the same underlying allocation.</para>
        /// </summary>
        /// <param name="index">The zero-based starting byte position of a substring
        /// in this instance.</param>
        /// <param name="count">The number of bytes in the substring.</param>
        /// <returns>A new instance of <see cref="StringUtf8"/> that is equivalent to the
        /// substring of length <paramref name="count"/> that begins at <paramref name="index"/>
        /// in this instance, or <see cref="Empty"/> if startIndex is equal to the length of
        /// this instance and length is zero.</returns>
        public StringUtf8 Substring(int index, int count)
        {
            if (index < 0 || index > _count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || checked(index + count) > _count)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0)
                return Empty;

            StringUtf8 substring;
            if (!TrySubstring(this, index, count, out substring))
            {
                // Throw when substring has fatally failed
                if (substring == null)
                    throw new InvalidOperationException($"Failed to slice \"{this}\".");
            }

            return substring; ;
        }
        /// <summary>
        /// <para>Retrieves a substring from this instance. The substring starts at a specified
        /// byte position and continues to the end of the string.</para>
        /// <para>The underlying bytes are not duplicated during substring creation, instead both
        /// <see cref="StringUtf8"/> instances reference the same underlying allocation.</para>
        /// </summary>
        /// <param name="index">The zero-based starting byte position of a substring
        /// in this instance.</param>
        /// <returns>A new instance of <see cref="StringUtf8"/> that is equivalent to the substring
        /// that begins at <paramref name="index"/> in this instance, or <see cref="Empty"/> if
        /// <paramref name="index"/> is equal to the length of this instance.</returns>
        public StringUtf8 Substring(int index)
            => Substring(index, _count - index);

        /// <summary>
        /// Converts the multi-byte string into a wide character string.
        /// </summary>
        /// <returns>Wide character (aka <see langword="char"/> string.</returns>
        public override string ToString()
        {
            return Encoding.GetString(_bytes, _first, _count);
        }

        /// <summary>
        /// <para>Trims the left side of a block of text.</para>
        /// <para>The trimming is only of a single "indent" usually a tab character, or a series of
        /// spaces.</para>
        /// </summary>
        /// <param name="tabSize">
        /// <para>The number of spaces which are considered equivalent to a tab.</para>
        /// <returns>A new instance of <see cref="StringUtf8"/> with the white-space trimmed of the
        /// beginning of each line.</returns>
        public StringUtf8 TrimLeftTab(int tabSize)
        {
            const int OverSizeLimit = 1024;

            if (tabSize < 0)
                throw new ArgumentOutOfRangeException(nameof(tabSize));

            if (tabSize == 0)
                return this;

            // allocate a new buffer because this instance's buffer could be shared with
            // "substring" objects, also "strings are immutable" or something like that
            // we only need a buffer the size of this sub-string (ala _count)
            byte[] bytes = new byte[_count];
            int skipCounter = 0;
            int length = 0; // length of the new string

            for (int i = 0; i < _count;)
            {
                byte c = _bytes[_first + i];

                // Skip tabSize number of characters, throwing if non-whitespace is encountered
                if (c == ' ' && skipCounter < tabSize)
                {
                    skipCounter += 1;
                    i += 1;

                    continue;
                }

                // find the end of the current line
                int end = FirstIndexOf('\n', i);
                if (end < 0)
                {
                    // it is very likely we won't find an new line in the message
                    // in that case just set `end` to the end of the buffer.
                    end = _count;
                }
                else
                {
                    // Include the \n in the copy
                    end += 1;
                }

                // calculate where to start copying and how much to copy
                int copyStart = _first + i;
                int copyLength = end - copyStart;

                Buffer.BlockCopy(_bytes, copyStart, bytes, length, copyLength);

                // compound the length of the new string
                length += copyLength;
                // move the read pointer past the end of line character
                i += copyLength;
                // reset the indent counter
                skipCounter = 0;
            }

            // if we're wasting `OverSizeLimit` bytes or more, resize the allocation
            if (length < bytes.Length - OverSizeLimit)
            {
                // calculate the new size
                int bufferSize = StringUtf8.GetBufferAllocationSize(length);
                // resize the array allocation
                Array.Resize(ref bytes, bufferSize);
            }

            return new StringUtf8(bytes, length);
        }

        /// <summary>
        /// Converts the string representation of a number to its 32-bit signed integer equivalent. A
        /// return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="result">The 32-bit signed integer.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/></returns>
        public static bool TryParse(StringUtf8 value, out int result)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return Internal.Extensions.TryParse(value._bytes, value._first, value._count, out result);
        }
        public bool TryParse(out int result)
            => StringUtf8.TryParse(this, out result);

#pragma warning disable 3001 // Compiler Warning (level 1) CS3001: Argument type 'type' is not CLS-compliant
        /// <summary>
        /// Converts the string representation of a number to its 32-bit unsigned integer equivalent. A
        /// return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="result">The 32-bit unsigned integer.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/></returns>
        public static bool TryParse(StringUtf8 value, out uint result)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return Internal.Extensions.TryParse(value._bytes, value._first, value._count, out result);
        }
        public bool TryParse(out uint result)
            => StringUtf8.TryParse(this, out result);
#pragma warning restore 3001

        /// <summary>
        /// Converts the string representation of a number to its 64-bit signed integer equivalent. A
        /// return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="result">The 64-bit signed integer.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/></returns>
        public static bool TryParse(StringUtf8 value, out long result)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return Internal.Extensions.TryParse(value._bytes, value._first, value._count, out result);
        }
        public bool TryParse(out long result)
            => StringUtf8.TryParse(this, out result);

#pragma warning disable 3001 // Compiler Warning (level 1) CS3001: Argument type 'type' is not CLS-compliant
        /// <summary>
        /// Converts the string representation of a number to its 64-bit unsigned integer equivalent. A
        /// return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="result">The 64-bit unsigned integer.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/></returns>
        public static bool TryParse(StringUtf8 value, out ulong result)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return Internal.Extensions.TryParse(value._bytes, value._first, value._count, out result);
        }
        public bool TryParse(out ulong result)
            => StringUtf8.TryParse(this, out result);
#pragma warning restore 3001

        public static bool TrySubstring(StringUtf8 value, int index, int count, out StringUtf8 substring)
        {
            if (ReferenceEquals(value, null))
            {
                substring = null;
                return false;
            }

            byte[] _bytes = value._bytes;
            int _count = value._count;
            int _first = value._first;

            if ((index < 0 || index > _count)
                || (count < 0 || checked(index + count) > _count))
            {
                substring = null;
                return false;
            }

            // Assume success, but verify.
            bool success = true;

            fixed (byte* b = _bytes)
            {
                byte* s = b + value._first + index;
                byte* e = s + count - 1;

                // make sure the first byte conforms to UTF-8 standard: https://en.wikipedia.org/wiki/UTF-8#Description
                if ((*s & 0xC0) == 0x80) // first byte is a continuation
                {
                    success = false;
                }

                // make sure the last byte is a boundary
                if ((*e & 0x80) != 0x00           // last byte is not ASCII
                    && index + count + 1 < _count // AND it is not the end of the string
                    && (*(e + 1) & 0xC0) == 0x80) // AND the next byte is a continuation
                {
                    success = false;
                }
            }

            substring = new StringUtf8(value, _first + index, count);
            return success;
        }

        /// <summary>
        /// Returns the byte value of <paramref name="value"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="value">The instance of <see cref="StringUtf8"/> to read from.</param>
        /// <param name="index">The index of the value to be returned.</param>
        /// <returns>The value of <paramref name="value"/> at <paramref name="index"/>.</returns>
        public static byte ValueAt(StringUtf8 value, int index)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (index < 0 || index >= value.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return value._bytes[index];
        }

        /// <summary>
        /// Returns the value of <paramref name="index"/> of this instance of <see cref="StringUtf8"/>.
        /// </summary>
        /// <param name="index">The index of the value to be returned.</param>
        /// <returns>The value of this instance at <paramref name="index"/>.</returns>
        public byte ValueAt(int index)
            => StringUtf8.ValueAt(this, index);

        /// <summary>
        /// Converts a portion of this instance of <see cref="StringUtf8"/>, <see cref="ObjectId.Length"/> characters long,
        /// starting with the <paramref name="index"/> character.
        /// </summary>
        /// <param name="index">The first character in the hexadecimal identity representation.</param>
        /// <returns><see cref="ObjectId"/> from the UTF-8 hexadecimal identity representation.</returns>
        internal ObjectId ToObjectId(int index)
        {
            if (index < 0 || index + ObjectId.Length > _count)
                throw new ArgumentOutOfRangeException(nameof(index));

            ObjectId objectId = new ObjectId { };

            try
            {
                fixed (byte* ptr = _bytes)
                {
                    byte* src = ptr + _first + index;

                    ObjectId* dst = &objectId;

                    for (int i = 0, j = 0; i < ObjectId.Size; i += 1, j += 2)
                    {
                        char ca = (char)src[j + 1];
                        char cb = (char)src[j + 0];

                        int ia = ObjectId.HexToDec(ca);
                        ia <<= ObjectId.NibbleBits0;
                        ia &= ObjectId.MaskNibble0;

                        int ib = ObjectId.HexToDec(cb);
                        ib <<= ObjectId.NibbleBits1;
                        ib &= ObjectId.MaskNibble1;

                        ((byte*)dst)[i] = (byte)(ia | ib);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ObjectIdParseException(ex.Message, this, index);
            }

            return objectId;
        }

        /// <summary>
        /// Converts a portion of this instance of <see cref="StringUtf8"/>, <see cref="ObjectId.Length"/> characters long.
        /// </summary>
        /// <returns><see cref="ObjectId"/> from the UTF-8 hexadecimal identity representation.</returns>
        internal ObjectId ToObjectId()
            => ToObjectId(0);

        /// <summary>
        /// Converts a portion of this instance of <see cref="StringUtf8"/>, <see cref="ObjectId.Length"/> characters long,
        /// starting with the <paramref name="index"/> character.
        /// </summary>
        /// <param name="index">The first character in the packed byte identity representation.</param>
        /// <returns><see cref="ObjectId"/> from the packed byte identity representation.</returns>
        internal ObjectId ToObjectIdFromBytes(int index)
        {
            if (index < 0 || index + ObjectId.Length / 2 > _count)
                throw new ArgumentOutOfRangeException(nameof(index));

            ObjectId objectId = new ObjectId { };

            fixed (byte* ptr = _bytes)
            {
                byte* src = ptr + _first + index;

                ObjectId* dst = &objectId;

                Buffer.MemoryCopy(src, dst, sizeof(ObjectId), sizeof(ObjectId));
            }

            return objectId;
        }

        /// <summary>
        /// Converts a portion of this instance of <see cref="StringUtf8"/>, <see cref="ObjectId.Length"/> characters long.
        /// </summary>
        /// <returns><see cref="ObjectId"/> from the packed byte identity representation.</returns>
        internal ObjectId ToObjectIdFromBytes()
            => ToObjectIdFromBytes(0);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int GetBufferAllocationSize(int size)
        {
            return size + sizeof(uint) - (size & (sizeof(uint) - 1));
        }

        public static bool operator ==(StringUtf8 left, StringUtf8 right)
            => StringUtf8Comparer.Ordinal.Equals(left, right);
        public static bool operator !=(StringUtf8 left, StringUtf8 right)
            => !StringUtf8Comparer.Ordinal.Equals(left, right);

        public static StringUtf8 operator +(StringUtf8 left, StringUtf8 right)
            => Concat(left, right);

        public static bool operator <(StringUtf8 left, StringUtf8 right)
            => StringUtf8Comparer.Ordinal.Compare(left, right) < 0;
        public static bool operator >(StringUtf8 left, StringUtf8 right)
            => StringUtf8Comparer.Ordinal.Compare(left, right) > 0;

        public static bool operator <=(StringUtf8 left, StringUtf8 right)
            => StringUtf8Comparer.Ordinal.Compare(left, right) <= 0;
        public static bool operator >=(StringUtf8 left, StringUtf8 right)
            => StringUtf8Comparer.Ordinal.Compare(left, right) >= 0;

        public static explicit operator string(StringUtf8 value)
        {
            if (value is null)
                return null;

            if (value.Length == 0)
                return string.Empty;

            fixed (byte* bytes = value._bytes)
            {
                int count = value._count;
                int first = value._first;

                byte* s = bytes + first;

                // Make sure the first byte is a boundary
                while ((*s & 0xC0) == 0x80 // Make sure the first byte conforms to UTF-8 standard: https://en.wikipedia.org/wiki/UTF-8#Description
                    && first + 1 - value._first <= value._count) // First byte is a continuation
                {
                    first += 1;

                    s = bytes + first;
                }

                byte* e = s + count - 1;

                // Make sure the last byte is a boundary
                while (*e == 0
                    || ((*e & 0x80) != 0x00            // Last byte is not ascii
                        && count + 1 > 0               // AND count is at least zero
                        && count + 1 < value._count    // AND it is not the end of the string
                        && (*(e + 1) & 0xC0) == 0x80)) // AND the next byte is a continuation
                {
                    count -= 1;

                    e = s + count - 1;
                }

                return Encoding.GetString(s, count);
            }
        }

        public static explicit operator StringUtf8(string value)
        {
            if (ReferenceEquals(value, null))
                return null;

            if (value.Length == 0)
            {
                return new StringUtf8(Array.Empty<byte>());
            }
            else
            {
                return new StringUtf8(value);
            }
        }
    }
}
