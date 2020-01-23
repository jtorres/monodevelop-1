//*************************************************************************************************
// TreeEntryDetailMode.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Mode (file type) of a `<see cref="ITreeDifferenceDetail"/>` of `<see cref="ITreeDifferenceEntry"/>`.
    /// </summary>
    public enum TreeEntryDetailMode
    {
        /// <summary>
        /// The entry does not exist or is unreadable.
        /// </summary>
        Nonexistent = 0,

        /// <summary>
        /// File system directory, or folder.
        /// </summary>
        Directory = 040000,

        /// <summary>
        /// File system file object.
        /// </summary>
        NormalFile = 0100644,

        /// <summary>
        /// Group-writable file on POSIX, normal file on Windows systems.
        /// <para>Mode 664 files are deprecated and no longer generated in new commits, but we must continue to honor them in ancient commits.</para>
        /// </summary>
        GroupWritableFile = 0100664,

        /// <summary>
        /// Executable file on POSIX systems, normal file on Windows systems.
        /// </summary>
        ExecutableFile = 0100755,

        /// <summary>
        /// File system symbolic-link.
        /// </summary>
        Symlink = 0120000,

        /// <summary>
        /// Git formatted faux-link file and/or `<see cref="Submodule"/>`.
        /// </summary>
        GitLink = 0160000,

        /// <summary>
        /// Git-submodule and/or `<see cref="GitLink"/>`.
        /// </summary>
        Submodule = GitLink,
    }
}
