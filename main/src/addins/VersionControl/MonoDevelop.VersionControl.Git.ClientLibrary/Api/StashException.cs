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

        internal StashException(string errorText, int exitCode) : base(errorText, exitCode)
        {
        }

        internal StashException(string message, string errorText, int exitCode) : base(message, errorText, exitCode)
        {
        }

        internal StashException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal StashException(string message, string errorText, Exception innerException) : base(message, errorText, innerException)
        {
        }

        internal StashException(string message, int exitCode, Exception innerException) : base(message, exitCode, innerException)
        {
        }

        internal StashException(string message, int exitCode, string errorText, Exception innerException) : base(message, exitCode, errorText, innerException)
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
