//*************************************************************************************************
// StashPushOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.PushStash(StashPushOptions)"/>`.
    /// </summary>
    public struct StashPushOptions
    {
        public static readonly StashPushOptions Default = new StashPushOptions
        {
            Flags = StashPushFlags.None,
        };

        /// <summary>
        /// Extended options related to a Git stash Push operation.
        /// </summary>
        public StashPushFlags Flags;

        /// <summary>
        /// Optional stash message.
        /// </summary>
        public string Message;
    }
}
