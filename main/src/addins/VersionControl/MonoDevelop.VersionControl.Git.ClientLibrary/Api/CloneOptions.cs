//*************************************************************************************************
// CloneOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="Repository.Clone(IExecutionContext, string, string, CloneOptions)"/>`.
    /// <para/>
    /// Options related to `<see cref="Repository.Clone(string, string, CloneOptions)"/>`.
    /// </summary>
    public struct CloneOptions
    {
        public static readonly CloneOptions Default = new CloneOptions
        {
            BranchName = null,
            Checkout = true,
            ProgressCallback = null,
            RecurseSubmodules = false,
            Reference = null,
            RemoteName = null,
            SeperateGitDir = null,
            SingleBranch = false,
        };

        /// <summary>
        /// If not `<see langword="null"/>`, instead of pointing the newly created HEAD to the branch pointed to by the cloned repository’s HEAD, point to {name} branch instead.
        /// <para/>
        /// In a non-bare repository, this is the branch that will be checked out.
        /// <para/>
        /// `<see cref="ReferenceName"/>` can also take tags and detaches the HEAD at that commit in the resulting repository.
        /// </summary>
        public string BranchName;
        /// <summary>
        /// `<see langword="true"/>` if the repository should be checked out after fetch is successful; otherwise `<see langword="false"/>`.
        /// <para/>
        /// Ignored when used in combination with `<see cref="Cli.CloneCommand.BareFromLocal(IRepository, string, CloneOptions)"/>` and `<see cref="Cli.CloneCommand.BareFromRemote(string, string, bool, CloneOptions)"/>`.
        /// </summary>
        public bool Checkout;

        public OperationProgressDelegate ProgressCallback;

        /// <summary>
        /// When `<see langword="true"/>`, after the clone is created, initialize and clone all submodules are initialized and cloned.
        /// <para/>
        /// Submodules are initialized and cloned using their default settings.
        /// <para/>
        /// The resulting clone has submodule.active set to "." (meaning all submodules).
        /// <para/>
        /// This is equivalent to running `<see cref="Cli.SubmoduleUpdateCommand.Update(SubmoduleUpdateOptions)"/>` with `<see cref="SubmoduleUpdateOptionsFlags.Init"/>` immediately after the clone is finished.
        /// </summary>
        public bool RecurseSubmodules;

        /// <summary>
        /// If not `<see langword="null"/>` and the reference repository is on the local machine, automatically setup .git/objects/info/alternates to obtain objects from the reference repository.
        /// <para/>
        /// Using an already existing repository as an alternate will require fewer objects to be copied from the repository being cloned, reducing network and local storage costs.
        /// <para/>
        /// A non-existing directory is skipped with a warning instead of aborting the clone.
        /// </summary>
        public IRepository Reference;

        /// <summary>
        /// If not `<see langword="null"/>`, instead of using "origin" to keep track of the upstream repository, use the value of `<see cref="RemoteName"/>`.
        /// </summary>
        public string RemoteName;

        /// <summary>
        /// When not `<see langword="null"/>`, instead of placing the cloned repository where it is supposed to be, place the cloned repository at the specified directory, then make a file-system-agnostic Git symbolic link to there.
        /// <para/>
        /// The result is Git repository can be separated from working tree.
        /// </summary>
        public string SeperateGitDir;
        /// <summary>
        /// Clone only the history leading to the tip of a single branch, either specified by the `<see cref="ReferenceName"/>` option or the primary branch remote’s HEAD points at.
        /// <para/>
        /// Further fetches into the resulting repository will only update the remote-tracking branch for the branch this option was used for the initial cloning.
        /// <para/>
        /// If the HEAD at the remote did not point at any branch when `<see cref="SingleBranch"/>` clone was made, no remote-tracking branch is created.
        /// </summary>
        public bool SingleBranch;
    }
}
