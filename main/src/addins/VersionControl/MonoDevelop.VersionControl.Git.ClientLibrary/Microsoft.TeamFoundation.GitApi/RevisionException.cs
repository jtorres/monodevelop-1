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

using System;
using System.Runtime.Serialization;

namespace Microsoft.TeamFoundation.GitApi
{
    [Serializable]
    public class RevisionException : GitException
    {
        internal RevisionException(string message)
            : base(message)
        { }

        internal RevisionException(string errorText, int exitCode)
            : base(errorText, exitCode)
        { }

        internal RevisionException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal RevisionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// Exception indicating that an invalid revision was passed to a command; or HEAD was referenced while unborn.
    /// </summary>
    [Serializable]
    public class BadRevisionException : RevisionException
    {
        internal BadRevisionException(string message)
            : base(message)
        { }

        internal BadRevisionException(string errorText, int exitCode)
            : base(errorText, exitCode)
        { }

        internal BadRevisionException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal BadRevisionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
