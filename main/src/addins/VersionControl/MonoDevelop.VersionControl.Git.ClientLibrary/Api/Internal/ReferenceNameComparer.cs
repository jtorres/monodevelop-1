/**** Git Process Management Library ****
 *
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the ""Software""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    public partial class TypeComparer : ITypeComparer<IReferenceName>
    {
        public readonly ITypeComparer<IReferenceName> ReferenceNameComparer = new ReferenceNameComparer();

        /// <summary>
        /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
        /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
        /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
        /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
        /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
        /// </summary>
        public int Compare(IReferenceName left, IReferenceName right)
            => ReferenceNameComparer.Compare(left, right);

        /// <summary>
        /// <para>Determines whether the specified objects are equal.</para>
        /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
        /// </summary>
        public bool Equals(IReferenceName left, IReferenceName right)
            => ReferenceNameComparer.Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public int GetHashCode(IReferenceName value)
            => ReferenceNameComparer.GetHashCode(value);
    }

    namespace Internal
    {
        internal class ReferenceNameComparer : ITypeComparer<IReferenceName>
        {
            public ReferenceNameComparer()
            { }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(IReferenceName left, IReferenceName right)
            {
                if (ReferenceEquals(left, right))
                    return 0;

                var leftName = left as ReferenceName;
                var rightName = right as ReferenceName;

                if (!ReferenceEquals(leftName, null) && !ReferenceEquals(rightName, null))
                {
                    return StringUtf8Comparer.Ordinal.Compare(leftName.CanonicalNameUtf8, rightName.CanonicalNameUtf8);
                }
                else
                {
                    return StringComparer.Ordinal.Compare(left?.CanonicalName, right?.CanonicalName);
                }
            }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(IReferenceName left, IRevision right)
            {
                if (ReferenceEquals(left, right))
                    return 0;

                var leftName = left as ReferenceName;
                var rightName = right as ReferenceName;

                if (!ReferenceEquals(leftName, null) && !ReferenceEquals(rightName, null))
                {
                    return StringUtf8Comparer.Ordinal.Compare(leftName.CanonicalNameUtf8, rightName.CanonicalNameUtf8);
                }
                else
                {
                    return StringComparer.Ordinal.Compare(left?.CanonicalName, right?.RevisionText);
                }
            }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(IReferenceName left, string right)
            {
                return StringComparer.Ordinal.Compare(left?.CanonicalName, right);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IReferenceName left, IReferenceName right)
            {
                if (ReferenceEquals(left, right))
                    return true;

                var leftName = left as ReferenceName;
                var rightName = right as ReferenceName;

                if (!ReferenceEquals(leftName, null) && !ReferenceEquals(rightName, null))
                {
                    return StringUtf8Comparer.Ordinal.Equals(leftName.CanonicalNameUtf8, rightName.CanonicalNameUtf8);
                }
                else
                {
                    return StringComparer.Ordinal.Equals(left?.CanonicalName, right?.CanonicalName);
                }
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IReferenceName left, IRevision right)
            {
                if (ReferenceEquals(left, right))
                    return true;

                var leftName = left as ReferenceName;
                var rightName = right as ReferenceName;

                if (!ReferenceEquals(leftName, null) && !ReferenceEquals(rightName, null))
                {
                    return StringUtf8Comparer.Ordinal.Equals(leftName.CanonicalNameUtf8, rightName.CanonicalNameUtf8);
                }
                else
                {
                    return StringComparer.Ordinal.Equals(left?.CanonicalName, right?.RevisionText)
                        || StringComparer.Ordinal.Equals(left?.FriendlyName, right?.RevisionText);
                }
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IReferenceName left, string right)
            {
                return StringComparer.Ordinal.Equals(left?.CanonicalName, right)
                    || StringComparer.Ordinal.Equals(left?.FriendlyName, right);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(IReferenceName value)
            {
                var name = value as ReferenceName;

                if (!ReferenceEquals(name, null))
                    return ReferenceName.StringUtf8Comparer.GetHashCode(name.CanonicalNameUtf8);

                return ReferenceName.StringComparer.GetHashCode(value?.CanonicalName);
            }
        }
    }
}
