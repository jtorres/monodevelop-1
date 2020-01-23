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
    public class ReferenceException : ExceptionBase
    {
        internal ReferenceException(string message)
            : base(message)
        {
            _type = ReferenceType.Unknown;
        }

        internal ReferenceException(string message, Exception innerException)
            : base(message, innerException)
        {
            _type = ReferenceType.Unknown;
        }

        internal ReferenceException(string message, ReferenceType type)
            : base(message)
        {
            _type = type;
        }

        internal ReferenceException(string message, ReferenceType type, Exception innerException)
            : base(message, innerException)
        {
            _type = type;
        }

        internal ReferenceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info.GetValue(nameof(ReferenceType), typeof(ReferenceType)) is ReferenceType type)
            {
                _type = type;
            }
            else
            {
                _type = ReferenceType.Unknown;
            }
        }

        private ReferenceType _type;

        public ReferenceType ReferenceType
        {
            get { return _type; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(ReferenceType), _type);
        }
    }

    [Serializable]
    public class AmbiguousReferenceException: ReferenceException
    {
        internal AmbiguousReferenceException(string message)
            : base(message)
        { }

        internal AmbiguousReferenceException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal AmbiguousReferenceException(string message, ReferenceType type)
            : base(message, type)
        { }

        internal AmbiguousReferenceException(string message, ReferenceType type, Exception innerException)
            : base(message, type, innerException)
        { }

        internal AmbiguousReferenceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class InvalidReferenceException : ReferenceException
    {
        internal InvalidReferenceException(string message)
            : base(message)
        { }

        internal InvalidReferenceException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal InvalidReferenceException(string message, ReferenceType type)
            : base(message, type)
        { }

        internal InvalidReferenceException(string message, ReferenceType type, Exception innerException)
            : base(message, type, innerException)
        { }

        internal InvalidReferenceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// Exception raised when a request is made for a reference that was not found in the repository
    /// </summary>
    [Serializable]
    public class ReferenceNotFoundException : ReferenceException
    {
        internal ReferenceNotFoundException(string message)
            : base(message)
        { }

        internal ReferenceNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal ReferenceNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class ReferenceReadException : ReferenceException
    {
        public const string DefaultMessage = "failed to read reference data from Git";

        internal ReferenceReadException()
            : base(DefaultMessage)
        { }

        internal ReferenceReadException(string message)
            : base(message)
        { }

        internal ReferenceReadException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal ReferenceReadException(Exception innerException)
            : base(DefaultMessage, innerException)
        { }

        internal ReferenceReadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// Exception raised when an invalid or unexpected reference type is encountered and cannot be handled gracefully.
    /// </summary>
    [Serializable]
    public class ReferenceTypeMismatchException : ReferenceException
    {
        public const string DefaultMessage = "invalid or unexpected reference type";

        internal ReferenceTypeMismatchException()
            : base(DefaultMessage)
        { }

        internal ReferenceTypeMismatchException(string message)
            : base(message)
        { }

        internal ReferenceTypeMismatchException(Exception innerException)
            : base(DefaultMessage, innerException)
        { }

        internal ReferenceTypeMismatchException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal ReferenceTypeMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        internal static ReferenceTypeMismatchException Create(ReferenceType expected, ReferenceType actual)
        {
            string expectedName = Reference.TypeToName(expected);
            string actualName = Reference.TypeToName(actual);
            string message = Invariant($"Unexpected reference type; expected '{expectedName}', found '{actualName}'.");

            return new ReferenceTypeMismatchException(message);
        }
    }
}
