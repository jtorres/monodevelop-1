//*************************************************************************************************
// OperationException.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    public sealed class OperationException : ExceptionBase
    {
        internal OperationException(OperationError operationError)
            : base(operationError.ToString())
        {
            OperationError = operationError;
        }

        internal OperationException(OperationError operationError, System.Exception innerException)
            : base(operationError.ToString(), innerException)
        {
            OperationError = operationError;
        }

        internal OperationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public readonly OperationError OperationError;
    }
}
