//*************************************************************************************************
// OperationError.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    public struct OperationError
    {
        internal OperationError(string message, OperationErrorType type)
        {
            if (ReferenceEquals(message, null))
                throw new ArgumentNullException(nameof(message));

            Message = message;
            Type = type;
        }

        /// <summary>
        /// The message returned with the error.
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// The type of error.
        /// </summary>
        public readonly OperationErrorType Type;

        public override string ToString()
        {
            return $"{Type}: {Message}";
        }
    }
}
