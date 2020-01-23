//*************************************************************************************************
// RevertOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.Revert(IRevision, RevertOptions)"/>`.
    /// </summary>
    public struct RevertOptions
    {
        public static readonly RevertOptions Default = new RevertOptions { };

        public OperationCallback ProgressCallback;
    }
}
