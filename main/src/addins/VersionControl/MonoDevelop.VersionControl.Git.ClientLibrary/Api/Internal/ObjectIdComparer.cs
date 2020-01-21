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
using System.Collections.Generic;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    partial class TypeComparer : IEqualityComparer<ObjectId>
    {
        public readonly IEqualityComparer<ObjectId> ObjectIdComparer = new ObjectIdComparer();

        /// <summary>
        /// <para>Determines whether the specified objects are equal.</para>
        /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
        /// </summary>
        public bool Equals(ObjectId left, ObjectId right)
            => ObjectIdComparer.Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public int GetHashCode(ObjectId value)
            => ObjectIdComparer.GetHashCode(value);
    }

    namespace Internal
    {
        internal class ObjectIdComparer : IEqualityComparer<ObjectId>
        {
            public ObjectIdComparer()
            { }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(ObjectId left, ObjectId right)
            {
                unsafe
                {
                    uint* pl = (uint*)&left;
                    uint* pr = (uint*)&right;

                    return pl[0] == pr[0]
                        && pl[1] == pr[1]
                        && pl[2] == pr[2]
                        && pl[3] == pr[3]
                        && pl[4] == pr[4];
                }
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(ObjectId left, IRevision right)
            {
                if (right is ObjectId)
                    return Equals(left, (ObjectId)right);

                return Revision.Comparer.Equals(left, right);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(ObjectId left, string right)
            {
                if (ReferenceEquals(left, null))
                    return false;

                try
                {
                    ObjectId rightOid = ObjectId.FromString(right);

                    return Equals(left, rightOid);
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public unsafe bool Equals(ObjectId objectId, byte[] bytes, int index)
            {
                if (ReferenceEquals(bytes, null))
                    return false;
                if (index < 0 || index + ObjectId.Length >= bytes.Length || index + ObjectId.Length < 0)
                    return false;

                fixed (byte* bptr = bytes)
                {
                    var lptr = (byte*)&objectId;
                    byte* rptr = bptr + index;
                    int len = Math.Min(ObjectId.Size, bytes.Length);

                    return Internal.Win32.Msvscrt.Memcmp(lptr, rptr, len) == 0;
                }
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(ObjectId value)
            {
                unsafe
                {
                    int* p = (int*)&value;
                    return p[2];
                }
            }
        }
    }
}
