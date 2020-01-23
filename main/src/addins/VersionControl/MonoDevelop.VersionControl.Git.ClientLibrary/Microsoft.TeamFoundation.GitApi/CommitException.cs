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
    public class CommitException : GitException
    {
        internal CommitException(string message)
            : base(message)
        { }

        internal CommitException(string message, string errorText)
            : base(message, errorText)
        { }

        internal CommitException(ExecuteResult executeResult)
            : base(executeResult)
        { }

        internal CommitException(string message, ExecuteResult executeResult)
            : base(message, executeResult)
        { }

        internal CommitException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal CommitException(string message, string errorText, Exception innerException)
            : base(message, errorText, innerException)
        { }

        internal CommitException(string message, ExecuteResult executeResult, Exception innerException)
            : base(message, executeResult, innerException)
        { }

        internal CommitException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// Exception indicating there were no changes to commit.
    /// </summary>
    [Serializable]
    public sealed class EmptyCommitException : CommitException
    {
        internal EmptyCommitException(string message)
            : base(message)
        { }

        internal EmptyCommitException(ExecuteResult executeResult)
            : base(executeResult)
        { }

        internal EmptyCommitException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// Exception indicating the commit message was empty. Can be raised by any command that performs a commit
    /// e.g. commit, cherry-pick, etc.
    /// </summary>
    [Serializable]
    public sealed class EmptyCommitMessageException : CommitException
    {
        internal const string MessagePrefix = "Aborting commit due to empty commit message";

        internal EmptyCommitMessageException(string message)
            : base(message)
        { }

        internal EmptyCommitMessageException(ExecuteResult executeResult)
            : base(executeResult)
        { }

        internal EmptyCommitMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
