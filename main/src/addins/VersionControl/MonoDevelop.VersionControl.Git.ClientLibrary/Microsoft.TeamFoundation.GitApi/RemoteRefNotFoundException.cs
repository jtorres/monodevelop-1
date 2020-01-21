using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Exception raised when a request is made for a reference that was not found in the remote
    /// </summary>
    public sealed class RemoteRefNotFoundException : Exception
    {
        internal RemoteRefNotFoundException(string message)
            : base(message)
        { }

        internal RemoteRefNotFoundException(string message, System.Exception innerException)
            : base(message, innerException)
        { }

        internal RemoteRefNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}
