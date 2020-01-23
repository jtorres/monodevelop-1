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
    public class CherryPickExcpetion : GitException
    {
        internal CherryPickExcpetion(string message)
            : base(message)
        { }

        internal CherryPickExcpetion(string message, string errorText)
            : base(message, errorText)
        { }

        internal CherryPickExcpetion(string message, int exitCode)
            : base(message, exitCode)
        { }

        internal CherryPickExcpetion(string message, int exitCode, string errorText)
            : base(message, errorText, exitCode)
        { }

        internal CherryPickExcpetion(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal CherryPickExcpetion(string message, string errorText, Exception innerException)
            : base(message, errorText, innerException)
        { }

        internal CherryPickExcpetion(string message, int exitCode, Exception innerException)
            : base(message, exitCode, innerException)
        { }

        internal CherryPickExcpetion(string message, int exitCode, string errorText, Exception innerException)
            : base(message, exitCode, errorText, innerException)
        { }

        internal CherryPickExcpetion(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// Exception raised when an attempt is made to cherry-pick a merge commit without specifying the parent number.
    /// </summary>
    [Serializable]
    public class AmbiguousCherryPickOfMergeException : CherryPickExcpetion
    {
        internal AmbiguousCherryPickOfMergeException(string errorText, int exitCode)
            : base(errorText, exitCode)
        { }

        internal AmbiguousCherryPickOfMergeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class CherryPickConflictException : CherryPickExcpetion
    {
        internal CherryPickConflictException(string errorText, int exitCode)
            : base(errorText, exitCode)
        { }

        internal CherryPickConflictException(string errorText, int exitCode, Exception innerException)
            : base(errorText, exitCode, innerException)
        { }

        internal CherryPickConflictException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class CherryPickEmptyException : CherryPickExcpetion
    {
        internal CherryPickEmptyException(string errorText, int exitCode)
            : base(errorText)
        { }

        internal CherryPickEmptyException(string errorText, int exitCode, Exception innerException)
            : base(errorText, exitCode, innerException)
        { }

        internal CherryPickEmptyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
