//*************************************************************************************************
// RevertResult.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum RevertResult
    {
        /// <summary>
        /// The revert operation has been aborted.
        /// </summary>
        Aborted,
        /// <summary>
        /// The revert operation has been completed
        /// </summary>
        Completed,
        /// <summary>
        /// The revert operation has been stopped due to conflicts
        /// </summary>
        Conflicts,
    }
}
