//*************************************************************************************************
// MergeFailedException.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************
using System.Runtime.Serialization;

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class MergeFailedException : ExceptionBase
    {
        internal MergeFailedException(string message)
            : base(message)
        { }

        internal MergeFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
