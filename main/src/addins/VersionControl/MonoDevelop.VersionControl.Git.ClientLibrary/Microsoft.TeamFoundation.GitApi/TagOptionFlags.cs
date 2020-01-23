//*************************************************************************************************
// TagOptionFlags.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum TagOptionFlags
    {
        None = 0,

        /// <summary>
        /// Replace an existing tag, if it already exists, with the given name.
        /// </summary>
        Force = 1 << 0,
    }
}
