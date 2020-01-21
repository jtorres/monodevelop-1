//*************************************************************************************************
// PushOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************


namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.Push(IBranchName, IRemoteName, IBranchName, PushOptions)"/>`.
    /// </summary>
    public struct PushOptions
    {
        public static readonly PushOptions Default = new PushOptions
        {
            Flags = PushOptionsFlags.None,
            ProgressCallback = null,
        };

        /// <summary>
        /// Extended options related to a Git push operation.
        /// </summary>
        public PushOptionsFlags Flags;

        /// <summary>
        /// Callback delegate to receive progress updates during an operation.
        /// </summary>
        public OperationCallback ProgressCallback;
    }
}
