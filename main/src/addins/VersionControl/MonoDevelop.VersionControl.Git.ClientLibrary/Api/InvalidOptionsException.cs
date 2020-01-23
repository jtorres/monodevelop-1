//*************************************************************************************************
// InvalidOptionsException.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class InvalidOptionsException : ExceptionBase
    {
        internal InvalidOptionsException(string message)
            : base(message)
        { }

        internal InvalidOptionsException(string message, System.Exception innerException)
            : base(message, innerException)
        { }

        internal InvalidOptionsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}
