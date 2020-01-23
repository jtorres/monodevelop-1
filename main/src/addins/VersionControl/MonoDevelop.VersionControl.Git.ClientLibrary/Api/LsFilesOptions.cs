//*************************************************************************************************
// LsFilesOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to "IRepository.ReadLsFiles".
    /// </summary>
    public struct LsFilesOptions
    {
        public static readonly LsFilesOptions Default = new LsFilesOptions
        {
            DirectoryOptions = LsDirectoryOptions.Default,
            FileOptions = LsFileTypes.Default,
        };

        /// <summary>
        /// Options related to directory enumeration.
        /// </summary>
        public LsDirectoryOptions DirectoryOptions;

        /// <summary>
        /// Options related to file enumeration.
        /// </summary>
        public LsFileTypes FileOptions;
    }
}
