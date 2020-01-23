//*************************************************************************************************
// RenameDetection.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum RenameDetection
    {
        Default = 0,

        /// <summary>
        /// No rename detection should be used.
        /// </summary>
        NoRenameDetection,

        /// <summary>
        /// Rename detection should be used.
        /// </summary>
        FollowRenames,
    }
}
