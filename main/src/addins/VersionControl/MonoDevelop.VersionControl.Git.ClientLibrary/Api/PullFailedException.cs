//*************************************************************************************************
// PullFailedException.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class PullFailedException : ExceptionBase
    {
        private const string DefaultMessage = "Pull operation failed.";

        internal PullFailedException(string message)
            : base(string.IsNullOrEmpty(message) ? DefaultMessage : message)
        { }

        internal PullFailedException(OperationError error)
            : base(error.Message)
        { }

        internal PullFailedException(string message, System.Exception innerException)
            : base(message, innerException)
        { }

        internal PullFailedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}
