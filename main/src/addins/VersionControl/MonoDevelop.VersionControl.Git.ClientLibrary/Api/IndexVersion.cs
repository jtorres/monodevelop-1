//*************************************************************************************************
// IndexVersion.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum IndexVersion : int
    {
        Default = 0,

        /// <summary>
        /// The default index for Git.
        /// </summary>
        Version2 = 2,

        /// <summary>
        /// Similar to `<see cref="Version2"/>` with extra features.
        /// </summary>
        Version3 = 3,

        /// <summary>
        /// Performs a simple pathname compression that reduces index size by 30%-50% on large repositories, which results in faster load time
        /// <para/>
        /// Version 4 is relatively young (first released in 1.8.0 in October 2012).
        /// <para/>
        /// Other Git implementations such as JGit and libgit2 may not support it yet.
        /// </summary>
        Version4 = 4,
    }
}
