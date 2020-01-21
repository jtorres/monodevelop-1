//*************************************************************************************************
// ParseResult.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    internal enum ParseResult
    {
        MatchComplete,
        MatchIncomplete,
        MatchMaybeComplete,
        NoMatch,
    }
}
