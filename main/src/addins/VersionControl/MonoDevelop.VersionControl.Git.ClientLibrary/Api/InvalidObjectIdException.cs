//*************************************************************************************************
// InvalidObjectIdException.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class InvalidObjectIdException : ExceptionBase
    {
        public const string ErrorMessage = "invalid object identity";

        internal InvalidObjectIdException(ObjectId objectId)
            : base(ErrorMessage)
        {
            ObjectId = objectId;
        }

        internal InvalidObjectIdException(ObjectId objectId, System.Exception innerException)
            : base(ErrorMessage, innerException)
        {
            ObjectId = objectId;
        }

        internal InvalidObjectIdException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public readonly ObjectId ObjectId;
    }
}
