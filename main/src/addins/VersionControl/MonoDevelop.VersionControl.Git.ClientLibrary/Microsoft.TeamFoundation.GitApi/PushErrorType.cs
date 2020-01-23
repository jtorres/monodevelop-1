
namespace Microsoft.TeamFoundation.GitApi
{
    public enum PushErrorType
    {
        /// <summary>
        /// Rejected by remote
        /// </summary>
        Rejected = 0,

        /// <summary>
        /// Updates were rejected because the tip of your current branch is behind its remote
        /// counterpart. Integrate the remote changes before pushing again.
        /// </summary>
        CurrentBehindRemote = 1,

        /// <summary>
        /// You cannot update a remote ref that points at a non-commit object, or update a remote ref
        /// to make it point at a non-commit object, without using `<see cref="PushOptionsFlags.Force"/>`.
        /// </summary>
        RefNeedsForce = 2,

        /// <summary>
        /// Updates were rejected because a pushed branch tip is behind its remote counterpart. Check
        /// out this branch and integrate the remote changes before pushing again.
        /// </summary>
        CheckoutPullPush = 3,

        /// <summary>
        /// Updates were rejected because the remote contains work that you do not have locally. This
        /// is usually caused by another repository pushing to the same ref. You may want to first
        /// integrate the remote changes before pushing again.
        /// </summary>
        RefFetchFirst = 4,

        /// <summary>
        /// Updates were rejected because the tag already exists in the remote.
        /// </summary>
        RefAlreadyExists = 5,
    }
}
