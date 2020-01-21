
namespace Microsoft.TeamFoundation.GitApi
{
    public abstract class GitVersionException : ExceptionBase
    {
        public const string ErrorMessage = "git version: ";

        internal GitVersionException(string message)
            : base(ErrorMessage + message)
        { }

        internal GitVersionException(string message, System.Exception innerException)
            : base(ErrorMessage + message, innerException)
        { }

        internal GitVersionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}
