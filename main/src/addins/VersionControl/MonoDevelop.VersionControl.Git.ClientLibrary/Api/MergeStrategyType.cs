//*************************************************************************************************
// MergeStrategyType.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum MergeStrategyType
    {
        Default = 0,

        /// <summary>
        /// Indicates a `<see cref="MergeStrategy"/>` reference can be safely cast to a `<see cref="MergeStrategyOctopus"/>` reference.
        /// </summary>
        Octopus,

        /// <summary>
        /// Indicates a `<see cref="MergeStrategy"/>` reference can be safely cast to a `<see cref="MergeStrategyOurs"/>` reference.
        /// </summary>
        Ours,

        /// <summary>
        /// Indicates a `<see cref="MergeStrategy"/>` reference can be safely cast to a `<see cref="MergeStrategyRecursive"/>` reference.
        /// </summary>
        Recursive,

        /// <summary>
        /// Indicates a `<see cref="MergeStrategy"/>` reference can be safely cast to a `<see cref="MergeStrategyResolve"/>` reference.
        /// </summary>
        Resolve,
    }
}
