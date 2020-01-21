//*************************************************************************************************
// ObjectDatabaseBusyException.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class ObjectDatabaseBusyException : ExceptionBase
    {
        private const string DefaultMessage = "The object database is busy";

        internal ObjectDatabaseBusyException()
            : base(DefaultMessage)
        { }

        internal ObjectDatabaseBusyException(System.Exception innerException)
            : base(DefaultMessage, innerException)
        { }

        internal ObjectDatabaseBusyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}
