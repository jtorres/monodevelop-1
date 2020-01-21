//*************************************************************************************************
// CaseSensitiveUtf8Comparer.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    /// <summary>
    /// Represents a string comparison operation that uses case-sensitive and ordinal comparison rules.
    /// </summary>
    public sealed class StringUtf8ComparerOrdinal : StringUtf8Comparer
    {
        internal StringUtf8ComparerOrdinal()
        { }

        /// <summary>
        /// Compares two strings and returns an indication of their relative sort order.
        /// </summary>
        /// <param name="left">Value to compare to <paramref name="right"/>.</param>
        /// <param name="right">Value to compare to <paramref name="left"/>.</param>
        /// <returns>A signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</returns>
        public sealed override int Compare(StringUtf8 left, StringUtf8 right)
        {
            return StringUtf8.Compare(left, right, false);
        }

        /// <summary>
        /// Tests whether two strings are equal.
        /// </summary>
        /// <param name="left">Value to compare to <paramref name="right"/>.</param>
        /// <param name="right">Value to compare to <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if equal; otherwise <see langword="false"/>.</returns>
        public sealed override bool Equals(StringUtf8 left, StringUtf8 right)
        {
            return StringUtf8.Equals(left, right, false);
        }

        /// <summary>
        /// Gets the hash code value of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The string to be hashed.</param>
        /// <returns>A 32-bit signed hash code calculated from the value of <paramref name="value"/>.</returns>
        public sealed override int GetHashCode(StringUtf8 value)
        {
            if (ReferenceEquals(value, null))
                return 0;

            return value.GetHashCode();
        }

        internal sealed override int Compare(byte[] leftUtf8, int leftIndex, int leftCount, byte[] rightUtf8, int rightIndex, int rightCount)
        {
            int count = Math.Min(leftCount, rightCount);
            int compare = Internal.Extensions.Compare(leftUtf8, leftIndex, rightUtf8, rightIndex, count);

            if (compare == 0)
            {
                compare = leftCount - rightCount;
            }

            return compare;
        }

        internal sealed override bool Equals(byte[] leftUtf8, int leftIndex, int leftCount, byte[] rightUtf8, int rightIndex, int rightCount)
        {
            if (leftCount != rightCount)
                return false;

            return Internal.Extensions.Equals(leftUtf8, leftIndex, rightUtf8, rightIndex, leftCount);
        }

        internal sealed override int GetHashCode(byte[] utf8, int index, int count)
        {
            return Internal.Extensions.GetMurmur3(utf8, index, count);
        }
    }
}
