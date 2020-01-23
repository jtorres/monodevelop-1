//*************************************************************************************************
// UpdateOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.Add(System.Collections.Generic.IEnumerable{string}, UpdateOptions)"/>`.
    /// </summary>
    public struct UpdateOptions
    {
        public static readonly UpdateOptions Default = new UpdateOptions { Flags = UpdateOptionFlags.Default };

        /// <summary>
        /// Extended options related to a Git update-index operation.
        /// </summary>
        public UpdateOptionFlags Flags;
    }
}
