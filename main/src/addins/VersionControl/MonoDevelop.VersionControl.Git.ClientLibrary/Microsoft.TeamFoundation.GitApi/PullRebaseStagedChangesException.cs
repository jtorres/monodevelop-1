namespace Microsoft.TeamFoundation.GitApi
{
    internal class PullRebaseStagedChangesException : WorkingDirectoryLocalChangesException
    {
        internal const string StagedChangesPrefix = "error: cannot pull with rebase: Your index contains uncommitted changes.";

        internal PullRebaseStagedChangesException(string message)
            : base(message)
        { }
    }
}
