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
    public class BranchException : ReferenceException
    {
        internal BranchException(string message)
            : base(message, ReferenceType.Heads)
        { }

        internal BranchException(string message, string name)
            : base(message, ReferenceType.Heads)
        {
            _name = name;
        }

        internal BranchException(string message, Exception innerException)
            : base(message, ReferenceType.Heads, innerException)
        { }

        internal BranchException(string message, string name, Exception innerException)
            : base(message, ReferenceType.Heads, innerException)
        {
            _name = name;
        }

        internal BranchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _name = info.GetString(nameof(BranchName));
        }

        private readonly string _name;

        public string BranchName
        {
            get { return _name; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(BranchName), _name);

            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class BranchCreationException : BranchException
    {
        internal BranchCreationException(string message)
            : base(message)
        { }

        internal BranchCreationException(string message, string name)
            : base(message, name)
        { }

        internal BranchCreationException(string message, Exception innerException)
            : base(message)
        { }

        internal BranchCreationException(string message, string name, Exception innerException)
            : base(message, name)
        { }

        internal BranchCreationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class BranchDeletionException : BranchException
    {
        internal BranchDeletionException(string message)
            : base(message)
        { }

        internal BranchDeletionException(string message, string name)
            : base(message, name)
        { }

        internal BranchDeletionException(string message, Exception innerException)
            : base(message)
        { }

        internal BranchDeletionException(string message, string name, Exception innerException)
            : base(message, name)
        { }

        internal BranchDeletionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class BranchExistsException : BranchException
    {
        internal BranchExistsException(string message)
            : base(message)
        { }

        internal BranchExistsException(string message, string name)
            : base(message, name)
        { }

        internal BranchExistsException(string message, Exception innerException)
            : base(message)
        { }

        internal BranchExistsException(string message, string name, Exception innerException)
            : base(message, name)
        { }

        internal BranchExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class BranchModificationException : BranchException
    {
        internal BranchModificationException(string message)
            : base(message)
        { }

        internal BranchModificationException(string message, string name)
            : base(message, name)
        { }

        internal BranchModificationException(string message, Exception innerException)
            : base(message)
        { }

        internal BranchModificationException(string message, string name, Exception innerException)
            : base(message, name)
        { }

        internal BranchModificationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
