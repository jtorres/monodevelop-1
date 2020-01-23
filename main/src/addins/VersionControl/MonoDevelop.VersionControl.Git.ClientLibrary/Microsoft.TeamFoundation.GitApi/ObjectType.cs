//*************************************************************************************************
// ObjectType.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model abstraction defining the Git object type.
    /// </summary>
    public enum ObjectType
    {
        /// <summary>
        /// Type is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// `<see cref="IBlob"/>`.
        /// </summary>
        Blob,

        /// <summary>
        /// `<see cref="ICommit"/>`.
        /// </summary>
        Commit,

        /// <summary>
        /// `<see cref="ITagAnnotation"/>`.
        /// </summary>
        Tag,

        /// <summary>
        /// `<see cref="ITree"/>`.
        /// </summary>
        Tree,

        /// <summary>
        /// Git sub-module.
        /// </summary>
        Submodule,
    }
}
