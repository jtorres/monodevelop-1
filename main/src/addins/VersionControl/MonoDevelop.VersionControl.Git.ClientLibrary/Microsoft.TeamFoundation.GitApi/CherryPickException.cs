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
    public class CherryPickException : GitException
    {
        internal CherryPickException(ExecuteResult executeResult)
            : base(executeResult.ErrorText)
        { }

        internal CherryPickException(string message, ExecuteResult executeResult)
            : base(message, executeResult)
        { }

        internal CherryPickException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal CherryPickException(string message, string errorText, Exception innerException)
            : base(message, errorText, innerException)
        { }

        internal CherryPickException(ExecuteResult executeResult, Exception innerException)
            : base(executeResult.ErrorText, executeResult, innerException)
        { }

        internal CherryPickException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// Exception raised when an attempt is made to cherry-pick a merge commit without specifying the parent number.
    /// </summary>
    [Serializable]
    public class AmbiguousCherryPickOfMergeException : CherryPickException
    {
        internal AmbiguousCherryPickOfMergeException(ExecuteResult executeResult)
            : base(executeResult)
        { }

        internal AmbiguousCherryPickOfMergeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class CherryPickConflictException : CherryPickException
    {
        internal CherryPickConflictException(ExecuteResult executeResult)
            : base(executeResult)
        { }

        internal CherryPickConflictException(ExecuteResult executeResult, Exception innerException)
            : base(executeResult, innerException)
        { }

        internal CherryPickConflictException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class CherryPickEmptyException : CherryPickException
    {
        internal CherryPickEmptyException(ExecuteResult executeResult)
            : base(executeResult)
        { }

        internal CherryPickEmptyException(ExecuteResult executeResult, Exception innerException)
            : base(executeResult, innerException)
        { }

        internal CherryPickEmptyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
