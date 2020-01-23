//*************************************************************************************************
// UpdateOptionFlags.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Flags related to `<seealso cref="UpdateOptions"/>`.
    /// </summary>
    [Flags]
    public enum UpdateOptionFlags
    {
        None = 0,

        /// <summary>
        /// If a specified file isn't in the index already then it's added.
        /// <para/>
        /// Default behavior is to ignore new files.
        /// </summary>
        Add = 1 << 0,

        /// <summary>
        /// If a specified file is in the index but is missing then it's removed.
        /// <para/>
        /// Default behavior is to ignore removed file.
        /// </summary>
        Remove = 1 << 1,

        /// <summary>
        /// Remove the file from the index even when the working directory still has such a file.
        /// <para/>
        /// Implies `<seealso cref="Remove"/>`.
        /// </summary>
        ForceRemove = 1 << 2,

        /// <summary>
        /// By default, when a file path exists in the index, `<see cref="IRepository.Add(System.Collections.Generic.IEnumerable{string}, UpdateOptions)"/>` refuses an attempt to add path.
        /// <para/>
        /// Similarly if a file path/file exists, a file path cannot be added.
        /// <para/>
        /// With `<seealso cref="Replace"/>`, existing entries that conflict with the entry being added are automatically removed with warning messages.
        /// </summary>
        Replace = 1 << 3,

        /// <summary>
        /// Restores the unmerged or needs updating state of a file during a merge if it was cleared by accident.
        /// </summary>
        Unresolve = 1 << 4,

        Default = Add
                | Remove,
    }
}
