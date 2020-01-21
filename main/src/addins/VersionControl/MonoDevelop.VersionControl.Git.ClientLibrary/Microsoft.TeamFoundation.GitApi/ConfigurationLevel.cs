//*************************************************************************************************
// ConfigLevel.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// The well-known Git configuration levels.
    /// <para/>
    /// Configuration entries with the same name and a greater level value override value of a lesser level value.
    /// </summary>
    [Flags]
    public enum ConfigurationLevel
    {
        None = 0,

        /// <summary>
        /// System wide configuration file.
        /// <para/>
        /// Windows only, support for Portable Git.
        /// </summary>
        Portable = 1 << 4,

        /// <summary>
        /// System wide configuration file.
        /// </summary>
        System = 1 << 8,

        /// <summary>
        /// X compatible configuration file.
        /// <para/>
        /// Potentially roaming configuration file on Windows.
        /// </summary>
        Xdg = 1 << 12,

        /// <summary>
        /// User configuration file.
        /// </summary>
        Global = 1 << 16,

        /// <summary>
        /// Repository configuration file.
        /// </summary>
        Local = 1 << 20,

        /// <summary>
        /// Command line argument.
        /// </summary>
        Command = 1 << 24,

        Any = Command | Local | Global | Xdg | System | Portable,

        Basic = Global | Xdg | Portable,
    }
}
