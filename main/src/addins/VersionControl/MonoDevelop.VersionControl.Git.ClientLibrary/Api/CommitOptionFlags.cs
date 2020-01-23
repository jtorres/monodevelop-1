//*************************************************************************************************
// CommitOptionFlags.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum CommitOptionFlags
    {
        None = 0,

        /// <summary>
        /// Usually recording a commit that has the exact same tree as its sole parent commit is a mistake, and the command prevents you from making such a commit.
        /// <para/>
        /// This option bypasses the safety, and is primarily for use by foreign SCM interface scripts.
        /// </summary>
        AllowEmpty = 1 << 0,

        /// <summary>
        /// Like `<see cref="AllowEmpty"/>` this command is primarily for use by foreign SCM interface scripts.
        /// <para/>
        /// It allows you to create a commit with an empty commit message without using plumbing commands like git-commit-tree.
        /// </summary>
        AllowEmptyMessage = 1 << 1,

        /// <summary>
        /// Replace the tip of the current branch by creating a new commit.
        /// <para/>
        /// The recorded tree is prepared as usual, and the message from the original commit is used as the starting point, instead of an empty message, when no other message is specified from the command line.
        /// <para/>
        /// The new commit has the same parents and author as the current one.
        /// </summary>
        Amend = 1 << 2,

        /// <summary>
        /// Make a commit by taking the updated working tree contents of the paths specified, disregarding any contents that have been staged for other paths.
        /// <para/>
        /// This is the default mode of operation of git commit if any paths are given on the command line, in which case this option can be omitted.
        /// <para/>
        /// If this option is specified together with `<seealso cref="Amend"/>`, then no paths need to be specified, which can be used to amend the last commit without committing changes that have already been staged.
        /// </summary>
        Only = 1 << 3,
    }
}
