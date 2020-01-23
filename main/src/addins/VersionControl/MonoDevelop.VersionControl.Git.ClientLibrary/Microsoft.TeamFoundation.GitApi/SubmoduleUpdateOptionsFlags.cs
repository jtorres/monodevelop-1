// SubmoduleUpdateOptionsFlags.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum SubmoduleUpdateOptionsFlags
    {
        None = 0,

        /// <summary>
        /// See "git submodule update --init".
        /// </summary>
        Init = 1 << 0,

        /// <summary>
        /// See "git submodule update --remote".
        /// </summary>
        Remote = 1 << 1,

        /// <summary>
        /// See "git submodule update --no-fetch".
        /// </summary>
        NoFetch = 1 << 2,

        /// <summary>
        /// See "git submodule update --force".
        /// </summary>
        Force = 1 << 3,

#if false
        // This is disabled for the moment.  Yes "git submodule update"
        // automatically handles recursive updating, but the suppressed
        // progress and error reporting make it difficult to use.  For
        // example, if there is an error with a sub-sub-module, it is
        // hard for us to tell what happened because we don't always
        // know entry/exit in each sub-sub and because relative paths
        // are reported relative the sub. And without looking at a
        // status or summary of the sub, we don't know if a sub-sub
        // error caused it to skip later sub-sub updates.
        //
        // All in all, it would be better if we handle the recursion
        // and iteration directly.
        //
        /// <summary>
        /// See "git submodule update --recursive".
        /// </summary>
        Recursive = 1 << 4,
#endif
    }
}
