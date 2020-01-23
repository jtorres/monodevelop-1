//*************************************************************************************************
// MergeInProgressException.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************
using System.Runtime.Serialization;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Exception indicating that a merge operation is currently in progress with unmerged conflicts that 
    /// need to be resolved before continuing.
    /// </summary>
    public sealed class MergeInProgressException : GitException
    {
        internal const string ErrorSuffix = " is not possible because you have unmerged files.";

        internal MergeInProgressException(string errorText, int exitCode)
            : base(errorText, exitCode)
        { }

        internal MergeInProgressException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
