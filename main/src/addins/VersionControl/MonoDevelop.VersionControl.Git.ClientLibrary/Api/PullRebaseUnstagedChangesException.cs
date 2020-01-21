namespace Microsoft.TeamFoundation.GitApi
{
    class PullRebaseUnstagedChangesException : WorkingDirectoryLocalChangesException
    {
        internal const string UnstagedChangesPrefix = "error: cannot pull with rebase: You have unstaged changes.";

        internal PullRebaseUnstagedChangesException(string message)
            : base(message)
        { }
    }
}
