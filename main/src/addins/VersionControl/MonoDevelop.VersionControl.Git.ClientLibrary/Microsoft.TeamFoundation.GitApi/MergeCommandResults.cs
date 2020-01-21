namespace Microsoft.TeamFoundation.GitApi
{
    public enum MergeCommandResult
    {
        Undefined = 0,

        /// <summary>
        /// The target branch is a child of the source branch.
        /// </summary>
        AlreadyUpToDate,

        /// <summary>
        /// The target branch is an ancestor of the source branch.
        /// </summary>
        FastForwardMerge,

        /// <summary>
        /// The target and source branches were merged.
        /// </summary>
        NonFastForwardMerge,

        /// <summary>
        /// Normal merge, but --no-commit requested.
        /// </summary>
        NonFastForwadMergeNoCommit,

        /// <summary>
        /// Merge stopped because of a conflict.
        /// </summary>
        Conflict,

        // Note that we DO NOT have an ERROR result
        // code because MergeCommand.Merge() throws
        // on errors.
    }

}
