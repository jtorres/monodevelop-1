namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options to control behavior of deleting a branch via the Delete Branch Command.
    /// </summary>
    public struct DeleteBranchOptions
    {
        public static readonly DeleteBranchOptions Default = new DeleteBranchOptions
        {
            Force = false
        };

        /// <summary>
        /// Include --Force option when deleting a branch. Default is false.
        /// </summary>
        public bool Force { get; set; }
    }
}
