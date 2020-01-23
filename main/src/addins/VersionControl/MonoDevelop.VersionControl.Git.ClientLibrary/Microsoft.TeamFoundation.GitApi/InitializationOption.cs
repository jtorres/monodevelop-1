//*************************************************************************************************
// InitializationOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="Repository.Initialize(IExecutionContext, string, InitializationOptions)"/>`.
    /// </summary>
    public sealed class InitializationOptions
    {
        public static readonly InitializationOptions Default = new InitializationOptions();

        public InitializationOptions(bool bare = false,
                                     string separateGitDirectory = null,
                                     string templateDirectory = null)
        {
            Bare = bare;
            SeparateGitDirectory = separateGitDirectory;
            TemplateDirectory = templateDirectory;
        }

        /// <summary>
        /// `<see langword="true"/>` if the repository should be created and initialized as "bare".
        /// <para/>
        /// "Bare" repositories do not have a working directory, and therefore no separate $GIT_DIR.
        /// <para/>
        /// Incompatible with `<see cref="SeparateGitDirectory"/>`.
        /// </summary>
        public readonly bool Bare;

        /// <summary>
        /// Specifies a path to where the ".git/" folder [$GIT_DIR], that Git uses to maintain repository state, should be located.
        /// <para/>
        /// By default, Git will place $GIT_DIR at the root of the repository's working directory.
        /// <para/>
        /// Incompatible with `<see cref="Bare"/>`.
        /// </summary>
        public readonly string SeparateGitDirectory;

        /// <summary>
        /// Specify the directory from which templates will be used.
        /// <para/>
        /// Files and directories in the template directory whose name do not start with a dot (".") will be copied to the $GIT_DIR after it is created.
        /// </summary>
        public readonly string TemplateDirectory;
    }
}
