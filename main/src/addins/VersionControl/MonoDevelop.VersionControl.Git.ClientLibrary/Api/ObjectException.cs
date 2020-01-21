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
    public class ObjectException : GitException
    {
        internal ObjectException(string message)
            : base(message)
        { }

        internal ObjectException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal ObjectException(string errorText, int exitCode)
            : base(errorText, exitCode)
        { }

        internal ObjectException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class ObjectMissingException : ObjectException
    {
        private const string MessageFormat = "Object Not Found: {0}";

        internal ObjectMissingException(ObjectId objectId)
            : base(MissingMessage(objectId))
        {
            ObjectId = objectId;
        }

        internal ObjectMissingException(ObjectId objectId, Exception innerException)
            : base(MissingMessage(objectId), innerException)
        {
            ObjectId = objectId;
        }

        internal ObjectMissingException(string errorText, int exitCode)
            : base(errorText, exitCode)
        {
            ObjectId = ObjectId.Zero;
        }

        internal ObjectMissingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ObjectId = ObjectId.FromString(info.GetString(nameof(ObjectId)));
        }

        public readonly ObjectId ObjectId;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(ObjectId), ObjectId.ToString());
        }

        private static string MissingMessage(ObjectId objectId)
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, MessageFormat, objectId);
        }
    }

    [Serializable]
    public class ObjectTooLargeException : ObjectException
    {
        internal ObjectTooLargeException(ObjectId oid, long size)
            : base(FormatMessage(oid, size))
        {
            _oid = oid;
            _size = size;
        }

        internal ObjectTooLargeException(ObjectId oid, long size, Exception innerException)
            : base(FormatMessage(oid, size), innerException)
        {
            _oid = oid;
            _size = size;
        }

        internal ObjectTooLargeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _oid = ObjectId.FromString(info.GetString(nameof(ObjectId)));
            _size = info.GetInt64(nameof(Size));
        }

        public ObjectId ObjectId
            => _oid;

        private ObjectId _oid;

        public long Size
            => _size;

        private long _size;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(ObjectId), _oid.ToString());
            info.AddValue(nameof(Size), _size);
        }

        private static string FormatMessage(ObjectId oid, long size)
        {
            return FormattableString.Invariant($"Object {oid} is too large ({size:###,###})");
        }
    }

    [Serializable]
    public class ObjectTypeException : ObjectException
    {
        public const string DefaultMessage = "invalid or unsupported object type detected.";

        internal ObjectTypeException()
            : base(DefaultMessage)
        { }

        internal ObjectTypeException(string message)
            : base(message)
        { }

        internal ObjectTypeException(Exception innerException)
            : base(DefaultMessage, innerException)
        { }

        internal ObjectTypeException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal ObjectTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
