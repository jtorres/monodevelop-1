//*************************************************************************************************
// CaseInsensitiveUtf8Comparer.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Text;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    /// <summary>
    /// Represents a string comparison operation that uses case-insensitive and ordinal comparison rules.
    /// </summary>
    public sealed class StringUtf8ComparerOrdinalIgnoreCase : StringUtf8Comparer
    {
        internal StringUtf8ComparerOrdinalIgnoreCase()
        { }

        /// <summary>
        /// Compares two strings and returns an indication of their relative sort order.
        /// </summary>
        /// <param name="left">Value to compare to <paramref name="right"/>.</param>
        /// <param name="right">Value to compare to <paramref name="left"/>.</param>
        /// <returns>A signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</returns>
        public override int Compare(StringUtf8 left, StringUtf8 right)
        {
            return StringUtf8.Compare(left, right, true);
        }

        /// <summary>
        /// Tests whether two strings are equal.
        /// </summary>
        /// <param name="left">Value to compare to <paramref name="right"/>.</param>
        /// <param name="right">Value to compare to <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if equal; otherwise <see langword="false"/>.</returns>
        public override bool Equals(StringUtf8 left, StringUtf8 right)
        {
            return StringUtf8.Equals(left, right, true);
        }

        /// <summary>
        /// Gets the hash code value of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The string to be hashed.</param>
        /// <returns>A 32-bit signed hash code calculated from the value of <paramref name="value"/>.</returns>
        public override int GetHashCode(StringUtf8 value)
        {
            if (ReferenceEquals(value, null))
                return 0;

            return StringComparer.OrdinalIgnoreCase.GetHashCode((string)value);
        }

        internal sealed override int Compare(byte[] leftUtf8, int leftIndex, int leftCount, byte[] rightUtf8, int rightIndex, int rightCount)
        {
            int count = Math.Min(leftCount, rightCount);
            int compareCount = 0;

            unsafe
            {
                fixed (byte* lptr = leftUtf8)
                fixed (byte* rptr = rightUtf8)
                {
                    byte* lBytes = lptr + leftIndex;
                    byte* rBytes = rptr + rightIndex;

                    char* lChars = stackalloc char[BlockSize];
                    char* rChars = stackalloc char[BlockSize];

                    while (compareCount < count)
                    {
                        int byteCount = Math.Min(count - compareCount, BlockSize);

                        int lCount = Encoding.UTF8.GetChars(lBytes + compareCount, byteCount, lChars, BlockSize);
                        int rCount = Encoding.UTF8.GetChars(rBytes + compareCount, byteCount, rChars, BlockSize);

                        int charCount = Math.Min(lCount, rCount);

                        for (int i = 0; i < charCount; i += 1)
                        {
                            lChars[i] = char.ToLowerInvariant(lChars[i]);
                            rChars[i] = char.ToLowerInvariant(rChars[i]);

                            int compare = lChars[i] - rChars[i];

                            if (compare != 0)
                                return compare;
                        }

                        byteCount = Math.Min(Encoding.UTF8.GetByteCount(lChars, lCount),
                                             Encoding.UTF8.GetByteCount(rChars, rCount));

                        compareCount += byteCount;
                    }
                }
            }

            return leftCount - rightCount;
        }

        internal sealed override bool Equals(byte[] leftUtf8, int leftIndex, int leftCount, byte[] rightUtf8, int rightIndex, int rightCount)
        {
            int count = Math.Min(leftCount, rightCount);
            int compareCount = 0;

            unsafe
            {
                fixed (byte* lptr = leftUtf8)
                fixed (byte* rptr = rightUtf8)
                {
                    byte* lBytes = lptr + leftIndex;
                    byte* rBytes = rptr + rightIndex;

                    char* lChars = stackalloc char[BlockSize];
                    char* rChars = stackalloc char[BlockSize];

                    while (compareCount < count)
                    {
                        int byteCount = Math.Min(count - compareCount, BlockSize);

                        int lCount = Encoding.UTF8.GetChars(lBytes + compareCount, byteCount, lChars, BlockSize);
                        int rCount = Encoding.UTF8.GetChars(rBytes + compareCount, byteCount, rChars, BlockSize);

                        int charCount = Math.Min(lCount, rCount);

                        for (int i = 0; i < charCount; i += 1)
                        {
                            lChars[i] = char.ToLowerInvariant(lChars[i]);
                            rChars[i] = char.ToLowerInvariant(rChars[i]);

                            if (lChars[i] != rChars[i])
                                return false;
                        }

                        byteCount = Math.Min(Encoding.UTF8.GetByteCount(lChars, lCount),
                                             Encoding.UTF8.GetByteCount(rChars, rCount));

                        compareCount += byteCount;
                    }
                }
            }

            return true;
        }

        internal sealed override int GetHashCode(byte[] utf8, int index, int count)
        {
            uint hash = 0;

            unsafe
            {
                fixed (byte* ptr = utf8)
                {
                    byte* bytes = ptr + index;
                    char* chars = stackalloc char[BlockSize];

                    int hashedCount = 0;

                    while (hashedCount < count)
                    {
                        int byteCount = Math.Min(count - hashedCount, BlockSize);
                        int charCount = Encoding.UTF8.GetChars(bytes + hashedCount, byteCount, chars, BlockSize);

                        for (int i = 0; i < charCount; i += 1)
                        {
                            chars[i] = char.ToLowerInvariant(chars[i]);
                        }

                        Internal.Extensions.Murmur3((byte*)chars, charCount * sizeof(char), ref hash);

                        byteCount = Encoding.UTF8.GetByteCount(chars, charCount);

                        hashedCount += byteCount;
                    }
                }
            }

            return unchecked((int)hash);
        }
    }
}
