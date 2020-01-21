//*************************************************************************************************
// CommitOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.Commit(string, CommitOptions)"/>`.
    /// </summary>
    public struct CommitOptions
    {
        public static readonly CommitOptions Default = new CommitOptions()
        {
            AuthorEmail = null,
            AuthorName = null,
            Flags = CommitOptionFlags.None,
        };

        /// <summary>
        /// The name of the commit author.
        /// </summary>
        public string AuthorName;


        /// <summary>
        /// The email of the commit author;
        /// </summary>
        public string AuthorEmail;


        /// <summary>
        /// Extended options related to a Git commit operation.
        /// </summary>
        public CommitOptionFlags Flags;
    }
}
