﻿/**** Git Process Management Library ****
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
    partial class TypeComparer : IEqualityComparer<INamedObject>
    {
        public static readonly IEqualityComparer<INamedObject> NamedObjectComparer = new NamedObjectComparer();

        public bool Equals(INamedObject left, INamedObject right)
            => NamedObjectComparer.Equals(left, right);

        public int GetHashCode(INamedObject value)
            => NamedObjectComparer.GetHashCode(value);
    }

    namespace Internal
    {
        internal class NamedObjectComparer : IEqualityComparer<INamedObject>
        {
            public bool Equals(INamedObject left, INamedObject right)
            {
                if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                    return false;

                return ObjectId.Comparer.Equals(left.ObjectId, right.ObjectId);
            }

            public bool Equals<T>(NamedObject<T> left, NamedObject<T> right)
                where T : IObject
            {
                return ObjectId.Comparer.Equals(left.ObjectId, right.ObjectId);
            }

            public int GetHashCode(INamedObject value)
            {
                return ReferenceEquals(value, null)
                     ? 0
                     : ObjectId.Comparer.GetHashCode(value.ObjectId);
            }

            public int GetHashCode<T>(NamedObject<T> value)
                where T : IObject
            {
                return ObjectId.Comparer.GetHashCode(value.ObjectId);
            }
        }
    }
}
