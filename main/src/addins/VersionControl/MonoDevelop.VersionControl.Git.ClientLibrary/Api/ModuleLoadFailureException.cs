//*************************************************************************************************
// ModuleLoadFailureException.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class ModuleLoadFailureException : ExceptionBase
    {
        public const string DefaultMessage = "Process failed to start correctly because required libraries could not be loaded.";

        internal ModuleLoadFailureException()
            : base(DefaultMessage)
        { }

        internal ModuleLoadFailureException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}
