//*************************************************************************************************
// LsDirectoryOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum LsDirectoryOptions
    {
        Default = 0,

        /// <summary>
        /// If a whole directory is classified as "other", show just its name (with a trailing slash) and not its whole contents.
        /// </summary>
        CollapseOtherDirectories,

        /// <summary>
        /// Do not list empty directories. Implies `<see cref="CollapseOtherDirectories"/>`.
        /// </summary>
        IgnoreEmptyDirectories,
    }
}
