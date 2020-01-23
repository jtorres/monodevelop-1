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
    partial class TypeComparer
    {
        /// <summary>
        /// Compares or equates two strings, ignoring case, and separator ('/', '\') characters.
        /// </summary>
        public readonly ITypeComparer<string> PathComparer = new PathComparer();

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// <para>Returns a signed integer that indicates the relative values of `<paramref name="left"/>` and `<paramref name="right"/>`.</para>
        /// <para>Less than zero when `<paramref name="left"/>` is less than `<paramref name="right"/>`.</para>
        /// <para>Zero when `<paramref name="left"/>` equals `<paramref name="right"/>`.</para>
        /// <para>Greater than zero when `<paramref name="left"/>` is greater than `<paramref name="right"/>`.</para>
        /// </summary>
        public int PathCompare(string left, string right)
            => PathComparer.Compare(left, right);

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// <para>Returns `<see langword="true"/>` if the specified objects are equal; otherwise, `<see langword="false"/>`.</para>
        /// </summary>
        public bool PathEquals(string left, string right)
            => PathComparer.Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public int PathGetHashCode(string value)
            => PathComparer.GetHashCode(value);

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// <para>Returns a signed integer that indicates the relative values of `<paramref name="left"/>` and `<paramref name="right"/>`.</para>
        /// <para>Less than zero when `<paramref name="left"/>` is less than `<paramref name="right"/>`.</para>
        /// <para>Zero when `<paramref name="left"/>` equals `<paramref name="right"/>`.</para>
        /// <para>Greater than zero when `<paramref name="left"/>` is greater than `<paramref name="right"/>`.</para>
        /// </summary>
        internal int PathCompare(StringUtf8 left, StringUtf8 right)
            => (PathComparer as PathComparer).Compare(left, right);

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// <para>Returns `<see langword="true"/>` if the specified objects are equal; otherwise, `<see langword="false"/>`.</para>
        /// </summary>
        internal bool PathEquals(StringUtf8 left, StringUtf8 right)
            => (PathComparer as PathComparer).Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        internal int PathGetHashCode(StringUtf8 value)
            => (PathComparer as PathComparer).GetHashCode(value);
    }

    namespace Internal
    {
        internal class PathComparer : StringComparer, ITypeComparer<string>, ITypeComparer<StringUtf8>
        {
            public static readonly StringComparer StringComparer = OrdinalIgnoreCase;
            public static readonly StringComparison StringComparison = StringComparison.OrdinalIgnoreCase;
            internal static readonly StringUtf8Comparer StringUtf8Comparer = StringUtf8Comparer.OrdinalIgnoreCase;

            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// <para>Returns a signed integer that indicates the relative values of `<paramref name="left"/>` and `<paramref name="right"/>`.</para>
            /// <para>Less than zero when `<paramref name="left"/>` is less than `<paramref name="right"/>`.</para>
            /// <para>Zero when `<paramref name="left"/>` equals `<paramref name="right"/>`.</para>
            /// <para>Greater than zero when `<paramref name="left"/>` is greater than `<paramref name="right"/>`.</para>
            /// </summary>
            public override int Compare(string left, string right)
            {
                if (ReferenceEquals(left, right))
                    return 0;
                if (ReferenceEquals(left, null))
                    return -1;
                if (ReferenceEquals(null, right))
                    return 1;

                string normalLeft = NormalizePath(left);
                string normalRight = NormalizePath(right);

                return StringComparer.Compare(normalLeft, normalRight);
            }

            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// <para>Returns a signed integer that indicates the relative values of `<paramref name="left"/>` and `<paramref name="right"/>`.</para>
            /// <para>Less than zero when `<paramref name="left"/>` is less than `<paramref name="right"/>`.</para>
            /// <para>Zero when `<paramref name="left"/>` equals `<paramref name="right"/>`.</para>
            /// <para>Greater than zero when `<paramref name="left"/>` is greater than `<paramref name="right"/>`.</para>
            /// </summary>
            public int Compare(StringUtf8 left, StringUtf8 right)
            {
                if (ReferenceEquals(left, right))
                    return 0;
                if (ReferenceEquals(left, null))
                    return -1;
                if (ReferenceEquals(null, right))
                    return 1;

                StringUtf8 normalLeft = NormalizePath(left);
                StringUtf8 normalRight = NormalizePath(right);

                return StringUtf8Comparer.Compare(normalLeft, normalRight);
            }

            /// <summary>
            /// Determines whether the specified objects are equal.
            /// <para>Returns `<see langword="true"/>` if the specified objects are equal; otherwise, `<see langword="false"/>`.</para>
            /// </summary>
            public override bool Equals(string left, string right)
            {
                if (ReferenceEquals(left, right))
                    return true;
                if (ReferenceEquals(left, null) || ReferenceEquals(null, right))
                    return false;

                string normalLeft = NormalizePath(left);
                string normalRight = NormalizePath(right);

                return StringComparer.Equals(normalLeft, normalRight);
            }

            /// <summary>
            /// Determines whether the specified objects are equal.
            /// <para>Returns `<see langword="true"/>` if the specified objects are equal; otherwise, `<see langword="false"/>`.</para>
            /// </summary>
            public bool Equals(StringUtf8 left, StringUtf8 right)
            {
                if (ReferenceEquals(left, right))
                    return true;
                if (ReferenceEquals(left, null) || ReferenceEquals(null, right))
                    return false;

                StringUtf8 normalLeft = NormalizePath(left);
                StringUtf8 normalRight = NormalizePath(right);

                return StringUtf8Comparer.Equals(normalLeft, normalRight);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public override int GetHashCode(string value)
            {
                if (ReferenceEquals(value, null))
                    return 0;

                string normalValue = NormalizePath(value);

                return StringComparer.GetHashCode(normalValue);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(StringUtf8 value)
            {
                if (ReferenceEquals(value, null))
                    return 0;

                StringUtf8 normalValue = NormalizePath(value);

                return StringComparer.GetHashCode(normalValue);
            }

            private static string NormalizePath(string path)
            {
                return path.Replace('\\', '/').TrimEnd();
            }

            private static StringUtf8 NormalizePath(StringUtf8 path)
            {
                return path.Replace((byte)'\\', (byte)'/');
            }
        }
    }
}
