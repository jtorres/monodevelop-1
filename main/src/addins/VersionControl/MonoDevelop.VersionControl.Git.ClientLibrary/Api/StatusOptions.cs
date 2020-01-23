//*************************************************************************************************
// StatusOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.ReadStatus(StatusOptions)"/>`.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct StatusOptions
    {
        public static readonly StatusOptions Default = new StatusOptions
        {
            AheadBehind = StatusAheadBehind.Default,
            Ignored = StatusIgnored.Default,
            IgnoreSubmodules = StatusIgnoreSubmodule.None,
            Path = null,
            UntrackedFiles = StatusUntrackedFiles.Normal,
        };

        /// <summary>
        /// Display or do not display detailed ahead/behind counts for the branch relative to its upstream branch.
        /// Defaults to true.
        /// </summary>
        public StatusAheadBehind AheadBehind;

        /// <summary>
        /// Ignored files are reported when `<see langword="true"/>`; otherwise ignored files are omitted.
        /// </summary>
        public StatusIgnored Ignored;

        /// <summary>
        /// Specifies how submodule changes are reported or omitted.
        /// </summary>
        public StatusIgnoreSubmodule IgnoreSubmodules;

        /// <summary>
        /// When set, used as a filter (or pathspec) to limit the subtree for which status is reported.
        /// <para/>
        /// No path based filtering occurs when `<see langword="null"/>`.
        /// </summary>
        public string Path;

        /// <summary>
        /// Options related to the inclusion of untracked file information.
        /// </summary>
        public StatusUntrackedFiles UntrackedFiles;

    }
}
