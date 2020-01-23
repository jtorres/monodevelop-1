//*************************************************************************************************
// PushException.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public abstract class PushException : ExceptionBase
    {
        internal PushException(string reason)
            : base($"rejected {reason}")
        {
            Reason = reason;
        }

        internal PushException(string reason, System.Exception innerException)
            : base($"rejected {reason}", innerException)
        {
            Reason = reason;
        }

        internal PushException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        private readonly string Reason;
    }
}
