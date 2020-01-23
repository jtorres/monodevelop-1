//*************************************************************************************************
// PushOptionsFlags.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum PushOptionsFlags
    {
        None = 0,

        /// <summary>
        /// Usually, the command refuses to update a remote ref that is not an ancestor of the local ref used to overwrite it.
        /// <para/>
        /// This flag disables these checks, and can cause the remote repository to lose commits; use it with care.
        /// </summary>
        Force = 1 << 0,

        /// <summary>
        /// Usually, the command refuses to update a remote ref that is not an ancestor of the local ref used to overwrite it.
        /// <para/>
        /// This option overrides this restriction if the current value of the remote ref is the expected value; "git push" fails otherwise.
        /// <para/>
        /// Imagine that you have to rebase what you have already published. You will have to bypass the "must fast-forward" rule in order to replace the history you originally published with the rebased history.
        /// <para/>
        /// If somebody else built on top of your original history while you are rebasing, the tip of the branch at the remote may advance with her commit, and blindly pushing with `<seealso cref="Force"/>` will lose her work.
        /// <para/>
        /// This option allows you to say that you expect the history you are updating is what you rebased and want to replace.
        /// <para/>
        /// If the remote ref still points at the commit you specified, you can be sure that no other people did anything to the ref.
        /// <para/>
        /// It is like taking a "lease" on the ref without explicitly locking it, and the remote ref is updated only if the "lease" is still valid.
        /// </summary>
        ForceWithLease = 1 << 1,

        /// <summary>
        /// Add upstream (tracking) reference.
        /// </summary>
        SetUpstream = 1 << 2,

        /// <summary>
        /// All refs under refs/tags are pushed, in addition to refspecs explicitly listed on the command line.
        /// </summary>
        Tags = 1 << 3,
    }
}
