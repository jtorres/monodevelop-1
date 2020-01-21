//*************************************************************************************************
// PullOptionsFlags.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum PullOptionFlags
    {
        None = 0,

        /// <summary>
        /// See "git pull --force"
        /// </summary>
        Force = 1 << 0,

    }

    public enum PullOptionCommitFlags
    {
        Default = 0,

        /// <summary>
        /// See "git pull --commit"
        /// </summary>
        CommitAfterMerge,

        /// <summary>
        /// See "git pull --no-commit"
        /// </summary>
        NoCommitAfterMerge,
    }

    public enum PullOptionFastForwardFlags
    {
        Default = 0,

        /// <summary>
        /// See "git pull --ff"
        /// </summary>
        FastForwardOrMerge = 1,

        /// <summary>
        /// See "git pull --no-ff"
        /// </summary>
        NoFastForward = 2,

        /// <summary>
        /// See "git pull --ff-only"
        /// </summary>
        FastForwardOnly = 3,
    }

    public enum PullOptionSquashFlags
    {
        Default = 0,

        /// <summary>
        /// See "git pull --squash"
        /// </summary>
        Squash = 1,

        /// <summary>
        /// See "git pull --no-squash"
        /// </summary>
        NoSquash = 2,
    }

}
