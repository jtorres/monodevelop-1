//*************************************************************************************************
// FetchOptionsFlags.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum FetchOptionsFlags
    {
        None = 0,

        /// <summary>
        /// Fetch all tags from the remote (i.e., fetch remote tags refs/tags/* into local
        /// tags with the same name), in addition to whatever else would otherwise be fetched.
        ///<para/>
        /// Using this option alone does not subject tags to pruning, even if `<see cref="Prune"/>`
        /// is used (though tags may be pruned anyway if they are also the destination of an explicit
        /// refspec; see `<see cref="Prune"/>`).
        /// <para/>
        /// Cannot be used with `<see cref="NoTags"/>`.
        /// </summary>
        AllTags = 1 << 0,

        /// <summary>
        /// When git fetch is used with {rbranch}:{lbranch} refspec, it refuses to update the local
        /// branch {lbranch} unless the remote branch {rbranch} it fetches is a descendant of {lbranch}.
        /// <para/>
        /// This option overrides that check.
        /// </summary>
        Force = 1 << 1,

        /// <summary>
        /// After fetching, remove any remote-tracking references that no longer exist on the remote.
        /// <para/>
        /// Tags are not subject to pruning if they are fetched only because of the default tag
        /// auto-following or due to a `<see cref="AllTags"/>` option.
        /// <para/>
        /// However, if tags are fetched due to an explicit refspec (either on the command line or in
        /// the remote configuration, for example if the remote was cloned with the --mirror option),
        /// then they are also subject to pruning.
        /// </summary>
        Prune = 1 << 2,

        /// <summary>
        /// By default, tags that point at objects that are downloaded from the remote repository are
        /// fetched and stored locally.
        /// <para/>
        /// This option disables this automatic tag following.
        /// <para/>
        /// The default behavior for a remote may be specified with the remote.{name}.tagOpt setting.
        /// </summary>
        NoTags = 1 << 3,
    }
}
