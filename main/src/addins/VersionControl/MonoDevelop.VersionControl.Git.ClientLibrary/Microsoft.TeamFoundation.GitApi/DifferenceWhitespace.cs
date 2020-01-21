//*************************************************************************************************
// DifferenceWhitespace.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Flags related `<seealso cref="DifferenceOptions"/>` and treatment of whitespace.
    /// </summary>
    [Flags]
    public enum DifferenceWhitespace
    {
        Default = 0,

        /// <summary>
        /// Ignore changes in amount of whitespace.
        /// <para/>
        /// This ignores whitespace at line end, and considers all other sequences of one or more whitespace characters to be equivalent.
        /// </summary>
        IgnoreWhitespaceChanges = 1 << 0,

        /// <summary>
        /// Ignore whitespace when comparing lines.
        /// <para/>
        /// This ignores differences even if one line has whitespace where the other line has none.
        /// </summary>
        IgnoreAllSpaces = 1 << 1,

        /// <summary>
        /// Ignore changes in whitespace at EOL.
        /// </summary>
        IgnoreSpaceAtEndOfLine = 1 << 2,

        /// <summary>
        /// Ignore changes whose lines are all blank.
        /// </summary>
        IgnoreBlankLines = 1 << 3,

        IgnoreAllWhitespace = IgnoreAllSpaces
                            | IgnoreBlankLines
                            | IgnoreSpaceAtEndOfLine
                            | IgnoreWhitespaceChanges,
    }
}
