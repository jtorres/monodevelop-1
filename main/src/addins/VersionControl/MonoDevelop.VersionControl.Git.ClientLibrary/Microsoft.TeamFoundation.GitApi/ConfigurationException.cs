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
    public class ConfigurationException : ExceptionBase
    {
        internal ConfigurationException(string message)
            : base(message)
        { }

        internal ConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal ConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class ConfigurationLockedException : ConfigurationException
    {
        internal ConfigurationLockedException(string message)
            : base(message)
        { }

        internal ConfigurationLockedException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal ConfigurationLockedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class ConfigurationNotFoundException : ConfigurationException
    {
        internal ConfigurationNotFoundException(string message)
            : base(message)
        { }

        internal ConfigurationNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal ConfigurationNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class ConfigurationReadException : ExceptionBase
    {
        private const string ErrorPrefix = "git config: ";

        internal ConfigurationReadException(string message)
            : base(ErrorPrefix + message)
        { }

        internal ConfigurationReadException(string errorText, int exitCode)
            : base($"{ErrorPrefix}{errorText} 0x{exitCode.ToString("X8")}")
        { }

        internal ConfigurationReadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
