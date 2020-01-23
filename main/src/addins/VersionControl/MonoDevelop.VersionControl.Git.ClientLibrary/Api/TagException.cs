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
using System.Runtime.Serialization;

namespace Microsoft.TeamFoundation.GitApi
{
    [Serializable]
    public class TagException : ReferenceException
    {
        internal TagException(string message)
            : base(message)
        { }

        internal TagException(string message, string name)
            : base(message)
        {
            _name = name;
        }

        internal TagException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal TagException(string message, string name, Exception innerException)
            : base(message, innerException)
        {
            _name = name;
        }

        internal TagException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _name = info.GetString(nameof(TagName));
        }

        private readonly string _name;

        public string TagName
        {
            get { return _name; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(TagName), _name);

            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class TagCreationException : TagException
    {
        internal TagCreationException(string message)
            : base(message)
        { }

        internal TagCreationException(string message, string name)
            : base(message, name)
        { }

        internal TagCreationException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal TagCreationException(string message, string name, Exception innerException)
            : base(message, name, innerException)
        { }

        internal TagCreationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class TagDeletionException : TagException
    {
        internal TagDeletionException(string message)
            : base(message)
        { }

        internal TagDeletionException(string message, string name)
            : base(message, name)
        { }

        internal TagDeletionException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal TagDeletionException(string message, string name, Exception innerException)
            : base(message, name, innerException)
        { }

        internal TagDeletionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class TagExistsException : TagException
    {
        internal TagExistsException(string message)
            : base(message)
        { }

        internal TagExistsException(string message, string name)
            : base(message, name)
        { }

        internal TagExistsException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal TagExistsException(string message, string name, Exception innerException)
            : base(message, name, innerException)
        { }

        internal TagExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class TagModificationException : TagException
    {
        internal TagModificationException(string message)
            : base(message)
        { }

        internal TagModificationException(string message, string name)
            : base(message, name)
        { }

        internal TagModificationException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal TagModificationException(string message, string name, Exception innerException)
            : base(message, name, innerException)
        { }

        internal TagModificationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
