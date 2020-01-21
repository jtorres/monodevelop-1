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

using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    partial class TypeComparer : ITypeComparer<IReference>
    {
        public readonly ITypeComparer<IReference> ReferenceComparer = new ReferenceComparer();

        /// <summary>
        /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
        /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
        /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
        /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
        /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
        /// </summary>
        public int Compare(IReference left, IReference right)
            => ReferenceComparer.Compare(left, right);

        /// <summary>
        /// <para>Determines whether the specified objects are equal.</para>
        /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
        /// </summary>
        public bool Equals(IReference left, IReference right)
            => ReferenceComparer.Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public int GetHashCode(IReference value)
            => ReferenceComparer.GetHashCode(value);
    }

    namespace Internal
    {
        internal sealed class ReferenceComparer : ITypeComparer<IReference>
        {
            public ReferenceComparer()
            { }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(IReference left, IReference right)
            {
                if (ReferenceEquals(left, right))
                    return 0;

                var leftReference = left as Reference;
                var rightReference = right as Reference;

                if (!ReferenceEquals(leftReference, null) && !ReferenceEquals(rightReference, null))
                    return Reference.NameStringUtf8Comparer.Compare(leftReference.CanonicalNameUtf8, rightReference.CanonicalNameUtf8);

                return Reference.NameStringComparer.Compare(left?.CanonicalName, right?.CanonicalName);
            }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(IReference left, ReferenceName right)
            {
                var leftReference = left as Reference;

                if (!ReferenceEquals(leftReference, null))
                {
                    return ReferenceName.StringUtf8Comparer.Compare(leftReference.CanonicalNameUtf8, right?.CanonicalNameUtf8);
                }

                return Reference.NameStringComparer.Compare(left?.CanonicalName, right?.CanonicalName);
            }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(IReference left, IReferenceName right)
            {
                if (ReferenceEquals(left, right))
                    return 0;

                var leftReference = left as Reference;

                if (!ReferenceEquals(leftReference, null))
                {
                    var rightReference = right as Reference;

                    if (!ReferenceEquals(rightReference, null))
                        return Reference.NameStringUtf8Comparer.Compare(leftReference.CanonicalNameUtf8, rightReference.CanonicalNameUtf8);

                    var referenceName = right as ReferenceName;

                    if (!ReferenceEquals(referenceName, null))
                        return Reference.NameStringUtf8Comparer.Compare(leftReference.CanonicalNameUtf8, referenceName.CanonicalNameUtf8);
                }

                return Reference.NameStringComparer.Compare(left?.CanonicalName, right?.CanonicalName);
            }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(IReference left, string right)
            {
                return Reference.NameStringComparer.Compare(left?.CanonicalName, right);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IReference left, IReference right)
            {
                if (ReferenceEquals(left, right))
                    return true;

                var leftReference = left as Reference;
                var rightReference = right as Reference;

                if (!ReferenceEquals(leftReference, null) && !ReferenceEquals(rightReference, null))
                    return leftReference.ReferenceType == rightReference.ReferenceType
                        && leftReference.ObjectId == rightReference.ObjectId
                        && Reference.NameStringUtf8Comparer.Equals(leftReference.CanonicalNameUtf8, rightReference.CanonicalNameUtf8);

                return left?.ObjectId == right?.ObjectId
                    && Reference.NameStringComparer.Equals(left.CanonicalName, right.CanonicalName);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IReference left, ReferenceName right)
            {
                if (ReferenceEquals(left, right))
                    return true;

                var leftReference = left as Reference;

                if (!ReferenceEquals(leftReference, null))
                    return Reference.NameStringUtf8Comparer.Equals(leftReference.CanonicalNameUtf8, right?.CanonicalNameUtf8);

                return Reference.NameStringComparer.Equals(left?.CanonicalName, right?.CanonicalName);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IReference left, IReferenceName right)
            {
                if (ReferenceEquals(left, right))
                    return true;

                var leftReference = left as Reference;

                if (!ReferenceEquals(leftReference, null))
                {
                    var rightReference = right as Reference;

                    if (!ReferenceEquals(rightReference, null))
                        return leftReference.ReferenceType == rightReference.ReferenceType
                            && leftReference.ObjectId == rightReference.ObjectId
                            && Reference.NameStringUtf8Comparer.Equals(leftReference.CanonicalNameUtf8, rightReference.CanonicalNameUtf8);

                    var referenceName = right as ReferenceName;

                    if (!ReferenceEquals(referenceName, null))
                        return Reference.NameStringUtf8Comparer.Equals(leftReference.CanonicalNameUtf8, referenceName.CanonicalNameUtf8);
                }

                return Reference.NameStringComparer.Equals(left?.CanonicalName, right?.CanonicalName);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IReference left, IRevision right)
            {
                if (ReferenceEquals(left, right))
                    return true;

                var leftReference = left as Reference;

                if (!ReferenceEquals(leftReference, null))
                {
                    var rightReference = right as Reference;

                    if (!ReferenceEquals(rightReference, null))
                        return leftReference.ReferenceType == rightReference.ReferenceType
                            && leftReference.ObjectId == rightReference.ObjectId
                            && Reference.NameStringUtf8Comparer.Equals(leftReference.CanonicalNameUtf8, rightReference.CanonicalNameUtf8);

                    var referenceName = right as ReferenceName;

                    if (!ReferenceEquals(referenceName, null))
                        return Reference.NameStringUtf8Comparer.Equals(leftReference.CanonicalNameUtf8, referenceName.CanonicalNameUtf8);
                }

                return Reference.NameStringComparer.Equals(left?.FriendlyName, right?.RevisionText)
                    || Reference.NameStringComparer.Equals(left?.CanonicalName, right?.RevisionText);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IReference left, string right)
            {
                return Reference.NameStringComparer.Equals(left?.CanonicalName, right)
                    || Reference.NameStringComparer.Equals(left?.FriendlyName, right);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(IReference value)
            {
                var reference = value as Reference;

                if (!ReferenceEquals(reference, null))
                    return Reference.NameStringUtf8Comparer.GetHashCode(reference.CanonicalNameUtf8);
                else
                    return Reference.NameStringComparer.GetHashCode(value?.CanonicalName);
            }
        }
    }
}
