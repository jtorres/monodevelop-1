//*************************************************************************************************
// TagOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.CreateTag(IRevision, string, string, TagOptions)"/>`.
    /// </summary>
    public struct TagOptions
    {
        public static readonly TagOptions Default = new TagOptions
        {
            Flags = TagOptionFlags.None,
        };

        /// <summary>
        /// Extended options related to a Git tag operation.
        /// </summary>
        public TagOptionFlags Flags;
    }
}
