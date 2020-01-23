//*************************************************************************************************
// FetchSubmoduleOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options, specifically related Git submodules, for `<seealso cref="FetchOptions"/>` and `<seealso cref="PullOptions"/>..
    /// </summary>
    public enum FetchSubmoduleOptions
    {
        Default,

        /// <summary>
        /// Unconditionally recurse into all populated submodules during fetch.
        /// </summary>
        Yes,

        /// <summary>
        /// Only recurse into a populated submodule when the super project retrieves a commit that
        /// updates the submodule's reference to a commit that isn't already in the local submodule clone.
        /// </summary>
        OnDemand,

        /// <summary>
        /// Completely disables submodule recursion during fetch.
        /// </summary>
        No,
    }
}
