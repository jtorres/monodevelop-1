//*************************************************************************************************
// RevertException.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class RevertException : ExceptionBase
    {
        internal RevertException(string message)
            : base(message)
        { }

        internal RevertException(string message, System.Exception innerException)
            : base(message, innerException)
        { }

        internal RevertException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}
