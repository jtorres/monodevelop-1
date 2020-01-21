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

using System.Collections.Generic;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    partial class TypeComparer : IEqualityComparer<Installation>
    {
        public static readonly IEqualityComparer<Installation> InstallationComparer = new InstallationComparer();

        /// <summary>
        /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
        /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
        /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
        /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
        /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
        /// </summary>
        public bool Equals(Installation left, Installation right)
            => InstallationComparer.Equals(left, right);

        public int GetHashCode(Installation value)
            => InstallationComparer.GetHashCode(value);
    }

    namespace Internal
    {
        internal class InstallationComparer : IEqualityComparer<Installation>
        {
            private static readonly PathComparer PathComparer = new PathComparer();

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public bool Equals(Installation left, Installation right)
            {
                if (ReferenceEquals(left, right))
                    return true;
                if (ReferenceEquals(left, null) || ReferenceEquals(null, right))
                    return false;

                return PathComparer.Equals(left.Path, right.Path);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(Installation value)
            {
                return PathComparer.GetHashCode(value?.Path);
            }
        }
    }
}
