//*************************************************************************************************
// DetachedHeadException.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************
using System.Runtime.Serialization;

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class DetachedHeadException : ExceptionBase
    {
        internal const string ErrorPrefix = "You are not currently on a branch";

        internal DetachedHeadException(string message)
            : base(message)
        { }

        internal DetachedHeadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
