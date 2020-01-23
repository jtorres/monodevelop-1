//*************************************************************************************************
// DifferenceFormat.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************
using System;

namespace Microsoft.TeamFoundation.GitApi
{
    public enum DifferenceFormat
    {
        /// <summary>
        /// Default difference format.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Generate the difference using the raw format.
        /// </summary>
        Raw = 1,

        /// <summary>
        /// Show only names and status of changed files.
        /// </summary>
        NameStatus = 2,
    }
}
