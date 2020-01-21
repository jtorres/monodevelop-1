//*************************************************************************************************
// StringUtf8Comparer.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System.Collections.Generic;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    /// <summary>
    /// Represents a string comparison operation that uses specific case and ordinal comparison rules.
    /// </summary>
    public abstract class StringUtf8Comparer : IComparer<StringUtf8>, IEqualityComparer<StringUtf8>
    {
        public static readonly StringUtf8ComparerOrdinal Ordinal = new StringUtf8ComparerOrdinal();
        public static readonly StringUtf8ComparerOrdinalIgnoreCase OrdinalIgnoreCase = new StringUtf8ComparerOrdinalIgnoreCase();

        protected const int BlockSize = 64;

        /// <summary>
        /// Compares two strings and returns an indication of their relative sort order.
        /// </summary>
        /// <param name="left">Value to compare to <paramref name="right"/>.</param>
        /// <param name="right">Value to compare to <paramref name="left"/>.</param>
        /// <returns>A signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</returns>
        public abstract int Compare(StringUtf8 left, StringUtf8 right);

        /// <summary>
        /// Tests whether two strings are equal.
        /// </summary>
        /// <param name="left">Value to compare to <paramref name="right"/>.</param>
        /// <param name="right">Value to compare to <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if equal; otherwise <see langword="false"/>.</returns>
        public abstract bool Equals(StringUtf8 left, StringUtf8 right);

        public abstract int GetHashCode(StringUtf8 value);

        internal abstract int Compare(byte[] leftUtf8, int leftIndex, int leftCount, byte[] rightUtf8, int rightIndex, int rightCount);

        internal abstract bool Equals(byte[] leftUtf8, int leftIndex, int leftCount, byte[] rightUtf8, int rightIndex, int rightCount);

        internal abstract int GetHashCode(byte[] utf8, int index, int count);
    }
}
