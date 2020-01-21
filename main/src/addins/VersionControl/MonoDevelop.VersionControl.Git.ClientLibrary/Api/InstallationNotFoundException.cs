//*************************************************************************************************
// InstallationNotFoundException.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace Microsoft.TeamFoundation.GitApi
{
    [Serializable]
    public abstract class InstallationException : ExceptionBase
    {
        internal InstallationException(string message)
            : base(message)
        { }

        internal InstallationException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal InstallationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class InstallationInvalidException : ExceptionBase
    {
        internal InstallationInvalidException(string message)
            : base(message)
        { }

        internal InstallationInvalidException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal InstallationInvalidException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class InstallationNotFoundException : ExceptionBase
    {
        internal InstallationNotFoundException(string message)
            : base(message)
        { }

        internal InstallationNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal InstallationNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
