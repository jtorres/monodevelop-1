//*************************************************************************************************
// ReferenceOptionFlags.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Flags related to `<seealso cref="ReferenceOptions"/>`.
    /// </summary>
    [Flags]
    public enum ReferenceOptionFlags
    {
        None = 0,

        /// <summary>
        /// Default set of references to query.
        /// </summary>
        Default = RefsHeads
                | RefsRemotes
                | RefsTags,

        /// <summary>
        /// Queries all references in refs/heads.
        /// </summary>
        RefsHeads = 1 << 0,

        /// <summary>
        /// Queries all references in refs/notes.
        /// </summary>
        RefsNotes = 1 << 2,

        /// <summary>
        /// Queries all references in refs/remotes.
        /// </summary>
        RefsRemotes = 1 << 4,

        /// <summary>
        /// Queries all references in refs/stash.
        /// </summary>
        RefsStash = 1 << 6,

        /// <summary>
        /// Queries all references in refs/tags.
        /// </summary>
        RefsTags = 1 << 8,

        /// <summary>
        /// Queries all references in refs.
        /// </summary>
        RefsAll = RefsHeads
                | RefsNotes
                | RefsRemotes
                | RefsStash
                | RefsTags,

        /// <summary>
        /// Populates the commits of references in refs/heads
        /// <para/>
        /// Only valid when combined with `<see cref="RefsHeads"/>`
        /// </summary>
        TipsHeads = 1 << 12,

        /// <summary>
        /// Populates the commits of references in refs/notes
        /// <para/>
        /// Only valid when combined with `<see cref="RefsNotes"/>`
        /// </summary>
        TipsNotes = 1 << 14,

        /// <summary>
        /// Populates the commits of references in refs/remotes
        /// <para/>
        /// Only valid when combined with `<see cref="RefsRemotes"/>`
        /// </summary>
        TipsRemotes = 1 << 16,

        /// <summary>
        /// Populates the commits of references in refs/stash
        /// <para/>
        /// Only valid when combined with `<see cref="RefsStash"/>`
        /// </summary>
        TipsStash = 1 << 18,

        /// <summary>
        /// Populates the commits of references in refs/tags
        /// <para/>
        /// Only valid when combined with `<see cref="RefsTags"/>`
        /// </summary>
        TipsTags = 1 << 20,

        /// <summary>
        /// Populates the commits of references in refs.
        /// <para/>
        /// Only valid when combined with the corresponding `Refs*` flag.
        /// </summary>
        TipsAll = TipsHeads
                | TipsNotes
                | TipsRemotes
                | TipsStash
                | TipsTags,

        /// <summary>
        /// Populates tag annotations
        /// <para/>
        /// Only valid when combined with `<see cref="RefsTags"/>`
        /// </summary>
        TagAnnotations = 1 << 22,

        /// <summary>
        /// Ensures that the repository HEAD will be read.
        /// </summary>
        ReadHead = 1 << 23,

        /// <summary>
        /// Queries all references in refs, and ensures that the repository HEAD will be read.
        /// </summary>
        RefsAllAndHead = RefsAll
                       | ReadHead,

        /// <summary>
        /// Queries all references in refs/heads, refs/remotes, and ensures the repository
        /// HEAD will be read..
        /// </summary>
        BranchesAndHead = RefsHeads
                        | RefsRemotes
                        | ReadHead,

        /// <summary>
        /// Queries all references in refs/tags and populates tag annotations.
        /// </summary>
        TagsWithAnnotations = RefsTags
                            | TagAnnotations,

        /// <summary>
        /// Queries all references in refs, populates commits, ensures the
        /// repository HEAD will be read, and populates tag annotations.
        /// </summary>
        Everything = RefsAll
                   | TipsAll
                   | ReadHead
                   | TagAnnotations,
    }
}
