//*************************************************************************************************
// UpdatedWorktreeFileType.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum UpdatedWorktreeEntryType
    {
        Unknown = 0,

        /// <summary>
        /// The operation failed.
        /// </summary>
        Failed,

        /// <summary>
        /// The file system entry was removed.
        /// </summary>
        Removed,

        /// <summary>
        /// The file system entry could not be removed.
        /// </summary>
        RemoveFailed,

        /// <summary>
        /// The operation on the file system entry was skipped because it is a repository.
        /// </summary>
        SkippedRepository,

        /// <summary>
        /// The operation would have removed the file system entry, had the operation been actual.
        /// </summary>
        WouldRemove,

        /// <summary>
        /// The operation would have skipped the file system entry due to being a repository, had it been actual.
        /// </summary>
        WouldSkipRepository,
    }
}
