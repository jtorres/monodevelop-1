//*************************************************************************************************
// TreeDifferenceDetailComparer.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using Microsoft.TeamFoundation.GitApi.Internal;
using System.Collections.Generic;

namespace Microsoft.TeamFoundation.GitApi
{
    partial class TypeComparer : IEqualityComparer<ITreeDifferenceEntry>
    {
        public readonly IEqualityComparer<ITreeDifferenceDetail> TreeDifferenceDetailComparer = TreeDifferenceDetail.Comparer;

        /// <summary>
        /// <para>Determines whether the specified objects are equal.</para>
        /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
        /// </summary>
        public bool Equals(ITreeDifferenceDetail left, ITreeDifferenceDetail right)
            => TreeDifferenceDetailComparer.Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public int GetHashCode(ITreeDifferenceDetail value)
            => TreeDifferenceDetailComparer.GetHashCode(value);
    }

    namespace Internal
    {
        internal class TreeDifferenceDetailComparer : IEqualityComparer<ITreeDifferenceDetail>
        {
            private static readonly TypeComparer Comparer = new TypeComparer();

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public bool Equals(ITreeDifferenceDetail left, ITreeDifferenceDetail right)
            {
                if (ReferenceEquals(left, right))
                    return true;
                if (left is null || right is null)
                    return false;

                return Comparer.Equals(left.ObjectId, right.ObjectId)
                    && left.Mode == right.Mode
                    && left.Type == right.Type;
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(ITreeDifferenceDetail value)
            {
                return value.ObjectId.GetHashCode();
            }
        }
    }
}
