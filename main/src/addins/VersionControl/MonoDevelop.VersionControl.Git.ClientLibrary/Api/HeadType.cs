//*************************************************************************************************
// HeadState.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum HeadType
    {
        Unknown,

        /// <summary>
        /// This is the "normal" state for a Git repository.
        /// <para/>
        /// Mechanically, '$GIT_DIR/HEAD' contains a reference to a branch in '$GIT_DIR/refs/heads/' or in '$GIT_DIR/packed-refs' and that branch contains a oid to a commit.
        /// <para/>
        /// Branch and commit details are available.
        /// </summary>
        Normal,

        /// <summary>
        /// The repository is "checked out" to a specific commit.
        /// <para/>
        /// Mechanically, '$GIT_DIR/HEAD' contains an oid to commit but no reference information.
        /// <para/>
        /// Branch details are not available, but commit details are.
        /// </summary>
        Detached,

        /// <summary>
        /// The repository is "checked out" to a non-existent branch.
        /// <para/>
        /// Mechanically, '$GIT_DIR/HEAD' contains a reference to a branch which does not exist '$GIT_DIR/refs/heads' nor in '.git/packed-refs'.
        /// <para/>
        ///
        /// Neither branch nor commit details are available, however the expected branch name is.
        ///
        /// </summary>
        Unborn,

        /// <summary>
        /// The repository is in an undefined state.
        /// <para/>
        /// Mechanically, '$GIT_DIR/HEAD' is missing or contains "garbage" data.
        /// <para/>
        /// The references is useless.
        /// </summary>
        Malformed,
    }
}
