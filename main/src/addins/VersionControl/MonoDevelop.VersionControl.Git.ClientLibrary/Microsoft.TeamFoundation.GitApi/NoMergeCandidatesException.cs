//*************************************************************************************************
// NoMergeCandidatesException.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************
using System.Runtime.Serialization;

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class NoMergeCandidatesException : ExceptionBase
    {
        internal NoMergeCandidatesException(string message)
            : base(message)
        { }

        internal NoMergeCandidatesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
