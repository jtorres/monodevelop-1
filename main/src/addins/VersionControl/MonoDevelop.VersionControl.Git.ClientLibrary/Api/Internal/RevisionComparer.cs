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
    partial class TypeComparer : ITypeComparer<IRevision>
    {
        public readonly ITypeComparer<IRevision> RevisionComparer = new RevisionComparer();

        /// <summary>
        /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
        /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
        /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
        /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
        /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
        /// </summary>
        public int Compare(IRevision left, IRevision right)
            => RevisionComparer.Compare(left, right);

        /// <summary>
        /// <para>Determines whether the specified objects are equal.</para>
        /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
        /// </summary>
        public bool Equals(IRevision left, IRevision right)
            => RevisionComparer.Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public int GetHashCode(IRevision value)
            => RevisionComparer.GetHashCode(value);
    }

    namespace Internal
    {
        internal class RevisionComparer : ITypeComparer<IRevision>
        {
            public RevisionComparer()
            { }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(IRevision left, IRevision right)
            {
                if (ReferenceEquals(left, right))
                    return 0;

                var leftRevision = left as Revision;
                var rightRevision = right as Revision;

                if (!ReferenceEquals(leftRevision, null) && !ReferenceEquals(rightRevision, null))
                    return Revision.StringUtf8Comparer.Compare(leftRevision.RevisionTextUtf8, rightRevision.RevisionTextUtf8);

                return Revision.StringComparer.Compare(left?.RevisionText, right?.RevisionText);
            }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(IRevision left, string right)
            {
                return Revision.StringComparer.Compare(left?.RevisionText, right);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IRevision left, IRevision right)
            {
                if (ReferenceEquals(left, right))
                    return true;

                var leftRevision = left as Revision;
                var rightRevision = right as Revision;

                if (!ReferenceEquals(leftRevision, null) && !ReferenceEquals(rightRevision, null))
                    return Revision.StringUtf8Comparer.Equals(leftRevision.RevisionTextUtf8, rightRevision.RevisionTextUtf8);

                return Revision.StringComparer.Equals(left.RevisionText, right?.RevisionText);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IRevision left, string right)
            {
                return Revision.StringComparer.Equals(left?.RevisionText, right);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(IRevision value)
            {
                var revision = value as Revision;

                if (!ReferenceEquals(value, null))
                    return Revision.StringUtf8Comparer.GetHashCode(revision.RevisionTextUtf8);

                return Revision.StringComparer.GetHashCode(value.RevisionText);
            }
        }
    }
}
