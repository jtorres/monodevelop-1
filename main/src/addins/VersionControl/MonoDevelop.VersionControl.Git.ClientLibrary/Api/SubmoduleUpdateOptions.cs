// SubmoduleUpdateOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System.Collections.Generic;

namespace Microsoft.TeamFoundation.GitApi
{

    public struct SubmoduleUpdateOptions
    {
        public static readonly SubmoduleUpdateOptions Default = new SubmoduleUpdateOptions
        {
            Flags = SubmoduleUpdateOptionsFlags.None,
            Method = SubmoduleUpdateOptionsMethods.Unspecified,
            Reference = null,
            Depth = null,
            Paths = null,
            ProgressCallback = null,
        };

        public SubmoduleUpdateOptionsFlags Flags;
        public SubmoduleUpdateOptionsMethods Method;

        /// <summary>
        /// See "git submodule update --reference".
        /// </summary>
        public IRepository Reference;
        
        /// <summary>
        /// See "git submodule update --depth".
        /// </summary>
        public int? Depth;

        /// <summary>
        /// See "git submodule update -- [&lt;path&gt; ...]".
        /// </summary>
        public IEnumerable<string> Paths;

        public OperationProgressDelegate ProgressCallback;
    }
}
