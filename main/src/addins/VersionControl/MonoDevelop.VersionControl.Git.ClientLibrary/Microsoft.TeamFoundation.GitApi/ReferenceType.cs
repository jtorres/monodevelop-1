//*************************************************************************************************
// ReferenceType.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum ReferenceType
    {
        Unknown,

        /// <summary>
        /// Heads or "local branches", usually found in the refs/heads/ folder of the .git/ directory.
        /// </summary>
        Heads,

        /// <summary>
        /// "Remote branches", usually found in the refs/remotes/ folder of the .git/ directory.
        /// </summary>
        Remotes,

        /// <summary>
        /// The HEAD reference contained in the .git/HEAD file.
        /// </summary>
        Head,

        /// <summary>
        /// Note references, usually found in the refs/notes/ folder of the .git/ directory.
        /// </summary>
        Notes,

        /// <summary>
        /// Stash reference, usually found in the refs/stash file in the .git/ directory.
        /// </summary>
        Stash,

        /// <summary>
        /// Tags, usually found in the refs/tags/ folder of the .git/ directory.
        /// </summary>
        Tags,
    }
}
