//*************************************************************************************************
// CheckoutIndexOptionFlags.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    [Flags]
    public enum CheckoutIndexOptionFlags
    {
        None = 0,

        /// <summary>
        /// Forces overwrite of existing files.
        /// </summary>
        Force = 1 << 0,
    }
}
