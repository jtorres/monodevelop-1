//*************************************************************************************************
// StatusSubmoduleState.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Set of flags to classify the status of a submodule.
    /// </summary>
    [Flags]
    public enum StatusSubmoduleState
    {
        None = 0,

        /// <summary>
        /// The submodule's HEAD points to a different commit (than listed in the index in the super).
        /// </summary>
        NewCommit = 0x4,

        /// <summary>
        /// The submodule has staged changes in its index (staged changes within the submodule).
        /// </summary>
        Modified = 0x2,

        /// <summary>
        /// The submodule has untracked changes (unstaged changes and untracked files).
        /// </summary>
        UntrackedChanges = 0x1,
    };
}
