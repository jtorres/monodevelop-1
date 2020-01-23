//*************************************************************************************************
// MergeWhitespaceOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Treats lines with the indicated type of whitespace change as unchanged for the sake of a three-way merge.
    /// <para/>
    /// Whitespace changes mixed with other changes to a line are not ignored.
    /// <para/>
    /// If their version only introduces whitespace changes to a line, our version is used;
    /// <para/>
    /// If our version introduces whitespace changes but their version includes a substantial change, their version is used;
    /// <para/>
    /// Otherwise, the merge proceeds in the usual way.
    /// </summary>
    public enum MergeRecursiveWhitespace
    {
        None,

        IgnoreWhitespaceChanges,

        IgnoreAllSpaces,

        IgnoreSpaceAtEndOfLine,
    }
}
