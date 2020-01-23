using System;
using System.Runtime.Serialization;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Base class for stash exceptions.
    /// </summary>
    [Serializable]
    public abstract class StashException : GitException
    {
        #region Constructors

        internal StashException(string message) : base(message)
        {
        }

        internal StashException(string message, string errorText) : base(message, errorText)
        {
        }

        internal StashException(ExecuteResult executeResult) : base(executeResult)
        {
        }

        internal StashException(string message, ExecuteResult executeResult) : base(message, executeResult)
        {
        }

        internal StashException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal StashException(string message, string errorText, Exception innerException) : base(message, errorText, innerException)
        {
        }

        internal StashException(string message, ExecuteResult executeResult, Exception innerException) : base(message, executeResult, innerException)
        {
        }

        internal StashException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #endregion
    }

    /// <summary>
    /// Stash pop or stash apply operation failed to restore the state of the index/staged files.
    /// </summary>
    [Serializable]
    public class StashRestoreIndexFailedException : StashException
    {
        internal StashRestoreIndexFailedException(string message)
            : base(message)
        { }

        internal StashRestoreIndexFailedException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal StashRestoreIndexFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
