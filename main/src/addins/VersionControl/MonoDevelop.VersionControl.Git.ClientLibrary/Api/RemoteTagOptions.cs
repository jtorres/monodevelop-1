//*************************************************************************************************
// RemoteTagOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum RemoteTagOptions
    {
        None,

        /// <summary>
        /// All tags should be affected by the operation.
        /// </summary>
        AllTags,

        /// <summary>
        /// No tags should be affected by the operation.langword
        /// </summary>
        NoTags
    }
}
