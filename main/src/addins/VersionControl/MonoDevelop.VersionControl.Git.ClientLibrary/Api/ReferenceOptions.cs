//*************************************************************************************************
// ReferenceOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.ReadReferences(ReferenceOptions)"/>`.
    /// </summary>
    public struct ReferenceOptions
    {
        public static readonly ReferenceOptions Default = new ReferenceOptions
        {
            Contains = null,
            Excludes = null,
            Flags = ReferenceOptionFlags.Default,
            Includes = null,
            ResultLimit = -1,
        };

        /// <summary>
        /// Only list tags which contain the specified revision.
        /// </summary>
        public IRevision Contains;

        /// <summary>
        /// Only list refs whose tips are not reachable from the specified revision.
        /// <para/>
        /// Invalid if `<see cref="Includes"/>` is not `<see langword="null"/>`.
        /// </summary>
        public IRevision Excludes;

        /// <summary>
        /// Set of reference types to query and return.
        /// <para/>
        /// Optionally supports populating tips of returned references.
        /// </summary>
        public ReferenceOptionFlags Flags;

        /// <summary>
        /// Only list refs whose tips are reachable from the specified revision.
        /// <para/>
        /// Invalid if `<see cref="Excludes"/>` is not `<see langword="null"/>`.
        /// </summary>
        public IRevision Includes;

        /// <summary>
        /// By default the command shows all refs that match `<see cref="Flags"/>`.
        /// <para/>
        /// This option makes it stop after showing that many refs.
        /// </summary>
        public int ResultLimit;
    }
}
