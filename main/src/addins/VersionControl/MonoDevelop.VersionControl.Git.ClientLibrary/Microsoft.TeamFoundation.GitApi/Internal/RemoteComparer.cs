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
    partial class TypeComparer : ITypeComparer<IRemote>
    {
        public readonly ITypeComparer<IRemote> RemoteComparer = new RemoteComparer();

        /// <summary>
        /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
        /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
        /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
        /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
        /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
        /// </summary>
        public int Compare(IRemote left, IRemote right)
            => RemoteComparer.Compare(left, right);

        /// <summary>
        /// <para>Determines whether the specified objects are equal.</para>
        /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
        /// </summary>
        public bool Equals(IRemote left, IRemote right)
            => RemoteComparer.Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public int GetHashCode(IRemote value)
            => RemoteComparer.GetHashCode(value);
    }

    namespace Internal
    {
        internal sealed class RemoteComparer : ITypeComparer<IRemote>
        {
            public RemoteComparer()
            { }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(IRemote left, IRemote right)
            {
                if (ReferenceEquals(left, right))
                    return 0;

                var leftRemote = left as Remote;
                var rightRemote = right as Remote;
                int cmp;

                if (!ReferenceEquals(leftRemote, null) && !ReferenceEquals(rightRemote, null))
                {
                    if ((cmp = Remote.NameStringUtf8Comparer.Compare(leftRemote?.NameUtf8, rightRemote?.NameUtf8)) != 0)
                        return cmp;

                    return Remote.UrlStringUtf8Comparer.Compare(leftRemote?.FetchUrlUtf8, rightRemote?.FetchUrlUtf8);
                }
                else
                {
                    if ((cmp = RemoteName.StringComparer.Compare(left?.Name, right?.Name)) != 0)
                        return cmp;

                    return Remote.UrlStringComparer.Compare(left?.FetchUrl, right?.FetchUrl);
                }
            }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(IRemote left, RemoteName right)
            {
                if (ReferenceEquals(left, right))
                    return 0;

                var leftRemote = left as Remote;

                if (!ReferenceEquals(leftRemote, null))
                    return Remote.NameStringUtf8Comparer.Compare(leftRemote.NameUtf8, right.NameUtf8);

                return RemoteName.StringComparer.Compare(left?.Name, right?.Name);
            }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(IRemote left, IRemoteName right)
            {
                if (ReferenceEquals(left, right))
                    return 0;

                var leftRemote = left as Remote;

                if (!ReferenceEquals(leftRemote, null))
                {
                    var rightRemote = right as Remote;

                    if (!ReferenceEquals(rightRemote, null))
                    {
                        int cmp = Remote.NameStringUtf8Comparer.Compare(leftRemote.NameUtf8, rightRemote.NameUtf8);
                        if (cmp != 0)
                            return cmp;

                        return Remote.UrlStringUtf8Comparer.Compare(leftRemote.FetchUrlUtf8, rightRemote.FetchUrlUtf8);
                    }

                    var rightName = right as RemoteName;

                    if (!ReferenceEquals(leftRemote, null) && !ReferenceEquals(rightName, null))
                        return Remote.NameStringUtf8Comparer.Compare(leftRemote.NameUtf8, rightName.NameUtf8);
                }

                return RemoteName.StringComparer.Compare(left?.Name, right?.Name);
            }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(IRemote left, string right)
            {
                return RemoteName.StringComparer.Compare(left?.Name, right);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IRemote left, IRemote right)
            {
                if (ReferenceEquals(left, right))
                    return true;

                var leftRemote = left as Remote;
                var rightRemote = right as Remote;

                if (!ReferenceEquals(leftRemote, null) && !ReferenceEquals(rightRemote, null))
                {
                    return Remote.UrlStringUtf8Comparer.Equals(leftRemote.FetchUrlUtf8, rightRemote.FetchUrlUtf8)
                        && Remote.NameStringUtf8Comparer.Equals(leftRemote.NameUtf8, rightRemote.NameUtf8);
                }
                else
                {
                    return Remote.UrlStringComparer.Equals(left?.FetchUrl, right?.FetchUrl)
                        && Remote.NameStringComparer.Equals(left?.Name, right?.Name);
                }
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IRemote left, RemoteName right)
            {
                if (ReferenceEquals(left, right))
                    return true;

                var leftRemote = left as Remote;

                if (!ReferenceEquals(leftRemote, null))
                    return Remote.NameStringUtf8Comparer.Equals(leftRemote.NameUtf8, right?.NameUtf8);

                return Remote.NameStringComparer.Equals(left?.Name, right?.Name);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IRemote left, IRemoteName right)
            {
                if (ReferenceEquals(left, right))
                    return true;

                var leftRemote = left as Remote;

                if (!ReferenceEquals(leftRemote, null))
                {
                    var rightRemote = right as Remote;

                    if (!ReferenceEquals(rightRemote, null))
                        return Remote.UrlStringUtf8Comparer.Equals(leftRemote.FetchUrlUtf8, rightRemote.FetchUrlUtf8)
                            && Remote.NameStringUtf8Comparer.Equals(leftRemote.NameUtf8, rightRemote.NameUtf8);

                    var rightName = right as RemoteName;

                    if (!ReferenceEquals(rightName, null))
                        return Remote.NameStringUtf8Comparer.Equals(leftRemote.NameUtf8, rightName.NameUtf8);
                }

                return Remote.NameStringComparer.Equals(left?.Name, right?.Name);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(IRemote left, string right)
            {
                return Remote.NameStringComparer.Equals(left?.Name, right);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(IRemote value)
            {
                var remote = value as Remote;

                if (!ReferenceEquals(remote, null))
                    return Remote.UrlStringUtf8Comparer.GetHashCode(remote.FetchUrlUtf8);

                return Remote.UrlStringComparer.GetHashCode(value);
            }
        }
    }
}
