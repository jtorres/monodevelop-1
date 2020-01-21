//*************************************************************************************************
// RebaseResult.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum RebaseResult
    {
        /// <summary>
        /// The rebase operation has been aborted.
        /// </summary>
        Aborted,
        /// <summary>
        /// The rebase operation has been completed
        /// </summary>
        Completed,
        /// <summary>
        /// The rebase operation has been stopped due to conflicts
        /// </summary>
        Conflicts,
    }
}
