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
using System.Runtime.Serialization;

namespace Microsoft.TeamFoundation.GitApi
{
    [Serializable]
    public class CheckoutException : ExceptionBase
    {
        private const char PathSeparatorChar = ';';
        private const string PathSerapatorString = ";";

        internal CheckoutException(string message)
            : base(message)
        {
            _paths = new List<CheckoutConflict>();
        }

        internal CheckoutException(string message, Exception innerException)
            : base(message, innerException)
        {
            _paths = new List<CheckoutConflict>();
        }

        internal CheckoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var serialized = info.GetString(nameof(Conflicts));
            var datas = serialized.Split(PathSeparatorChar);

            _paths = new List<CheckoutConflict>();

            foreach (var data in datas)
            {
                _paths.Add(CheckoutConflict.FromSerialized(data));
            }
        }

        /// <summary>
        /// Gets the list of paths in conflict.
        /// </summary>
        public IReadOnlyList<ICheckoutConflict> Conflicts
        {
            get { return _paths; }
        }

        private readonly List<CheckoutConflict> _paths;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            var datas = new List<string>(_paths.Count);

            foreach (var path in _paths)
            {
                string data = path.ToSerialized();
                datas.Add(data);
            }

            string serialized = string.Join(PathSerapatorString, datas);

            info.AddValue(nameof(Conflicts), serialized);
        }

        internal void AddConflictingPaths(ICollection<CheckoutConflict> conflicts)
        {
            _paths.AddRange(conflicts);
        }
    }

    [Serializable]
    public class CheckoutConflictException : CheckoutException
    {
        internal CheckoutConflictException(string message)
            : base(message)
        { }

        internal CheckoutConflictException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class CheckoutUntrackedException : CheckoutException
    {
        internal CheckoutUntrackedException(string message)
            : base(message)
        { }

        internal CheckoutUntrackedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
