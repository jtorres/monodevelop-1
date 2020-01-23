//*************************************************************************************************
// HttpExtraHeaderComparer.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    partial class TypeComparer : ITypeComparer<HttpExtraHeader>
    {
        public readonly ITypeComparer<HttpExtraHeader> HttpExtraHeaderComparer = new HttpExtraHeaderComparer();

        /// <summary>
        /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
        /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
        /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
        /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
        /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
        /// </summary>
        public int Compare(HttpExtraHeader left, HttpExtraHeader right)
            => HttpExtraHeaderComparer.Compare(left, right);

        /// <summary>
        /// <para>Determines whether the specified objects are equal.</para>
        /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
        /// </summary>
        public bool Equals(HttpExtraHeader left, HttpExtraHeader right)
            => HttpExtraHeaderComparer.Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public int GetHashCode(HttpExtraHeader value)
            => HttpExtraHeaderComparer.GetHashCode(value);
    }

    namespace Internal
    {
        internal class HttpExtraHeaderComparer : ITypeComparer<HttpExtraHeader>
        {
            public HttpExtraHeaderComparer()
            { }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(HttpExtraHeader left, HttpExtraHeader right)
            {
                return HttpExtraHeader.NameStringComparer.Compare(left.Name, right.Name);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(HttpExtraHeader left, HttpExtraHeader right)
            {
                return HttpExtraHeader.NameStringComparer.Equals(left.Name, right.Name)
                    && HttpExtraHeader.ValueStringComparer.Equals(right.Value, left.Value);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(HttpExtraHeader value)
            {
                return HttpExtraHeader.NameStringComparer.GetHashCode(value.Name);
            }
        }
    }
}
