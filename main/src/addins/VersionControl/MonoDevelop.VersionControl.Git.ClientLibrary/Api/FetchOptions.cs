//*************************************************************************************************
// FetchOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.Fetch(IRemote, FetchOptions)"/>`.
    /// </summary>
    public struct FetchOptions
    {
        public static readonly FetchOptions Default = new FetchOptions
        {
            Flags = FetchOptionsFlags.None,
            ProgressCallback = null,
            RemoteRevision = null,
            SubmoduleRecusion = FetchSubmoduleOptions.Default,
        };

        /// <summary>
        /// Extended options related to a Git fetch operation.
        /// </summary>
        public FetchOptionsFlags Flags;

        /// <summary>
        /// Callback delegate to receive progress updates during an operation.
        /// </summary>
        public OperationProgressDelegate ProgressCallback;

        /// <summary>
        /// The specific remote revision to fetch. If null, all branches.
        /// </summary>
        public IRevision RemoteRevision;

        /// <summary>
        /// Controls how submodules are processed during fetch operation.
        /// </summary>
        public FetchSubmoduleOptions SubmoduleRecusion;
    }
}
