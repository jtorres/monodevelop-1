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
using static System.FormattableString;

namespace Microsoft.TeamFoundation.GitApi
{
    [Serializable]
    public abstract class ReferenceNameException : ReferenceException
    {
        internal ReferenceNameException(string message, ReferenceType type)
            : base(message, type)
        { }

        internal ReferenceNameException(string message, ReferenceType type, Exception innerException)
            : base(message, type, innerException)
        { }

        internal ReferenceNameException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        protected static string FormatExceptionMessage(string name, ReferenceType type)
        {
            string typeName = Reference.TypeToName(type);

            return (name is null)
                ? Invariant($"fatal: invalid {typeName} name.")
                : Invariant($"fatal: '{name}' is not a valid {typeName} name.");
        }

        internal static ReferenceNameException Create(string name, ReferenceType type)
        {
            // Assume that the reference is a branch, unless it's specifically a tag
            // this is incomplete logic, but it covers the 99% use-case.
            return (type == ReferenceType.Tags)
                ? TagNameException.FromName(name) as ReferenceNameException
                : BranchNameException.FromName(name) as ReferenceNameException;
        }
    }

    [Serializable]
    public class BranchNameException : ReferenceNameException
    {
        internal BranchNameException(string message)
            : base(message, ReferenceType.Heads)
        { }

        internal BranchNameException(string message, Exception innerException)
            : base(message, ReferenceType.Heads, innerException)
        { }

        internal BranchNameException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        internal static BranchNameException FromName(string name, Exception innerException = null)
        {
            string message = FormatExceptionMessage(name, ReferenceType.Heads);
            return (innerException is null)
                ? new BranchNameException(message)
                : new BranchNameException(message, innerException);
        }
    }

    [Serializable]
    public class TagNameException : ReferenceNameException
    {
        internal TagNameException(string message)
            : base(message, ReferenceType.Tags)
        { }

        internal TagNameException(string message, Exception innerException)
            : base(message, ReferenceType.Tags, innerException)
        { }

        internal TagNameException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        internal static TagNameException FromName(string name, Exception innerException = null)
        {
            string message = FormatExceptionMessage(name, ReferenceType.Tags);
            return (innerException is null)
                ? new TagNameException(message)
                : new TagNameException(message, innerException);
        }
    }
}
