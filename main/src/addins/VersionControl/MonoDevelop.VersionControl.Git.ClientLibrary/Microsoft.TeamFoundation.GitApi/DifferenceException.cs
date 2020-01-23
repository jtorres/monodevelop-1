//*************************************************************************************************
// DifferenceException.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class DifferenceException : ExceptionBase
    {
        public const string DefaultMessage = "unable to parse difference output.";

        internal DifferenceException()
            : base(DefaultMessage)
        { }

        internal DifferenceException(string message = DefaultMessage)
            : base(message)
        { }

        internal DifferenceException(System.Exception innerException)
            : base(DefaultMessage, innerException)
        { }

        internal DifferenceException(string message, System.Exception innerException)
            : base(message, innerException)
        { }

        internal DifferenceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}
