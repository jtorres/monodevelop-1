//*************************************************************************************************
// ProcessException.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Thrown when <see cref="IProcess"/> creation / start failure occurs.
    /// </summary>
    public sealed class ProcessException : ExceptionBase
    {
        internal ProcessException(string message)
            : base(message)
        { }

        internal ProcessException(string message, System.Exception innerException)
            : base(message, innerException)
        { }

        internal ProcessException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}
