//*************************************************************************************************
// PushProgress.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class PushErrorException : PushException
    {
        internal PushErrorException(string reason, PushErrorType type)
            : base(reason)
        { }

        internal PushErrorException(PushErrorType type)
            : this(TypeToString(type), type)
        { }

        internal PushErrorException(string reason, PushErrorType type, Exception innerException)
            : base(reason, innerException)
        { }

        internal PushErrorException(PushErrorType type, Exception innerException)
            : this(TypeToString(type), type, innerException)
        { }

        internal PushErrorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        internal static string TypeToString(PushErrorType type)
        {
            Debug.Assert(Enum.IsDefined(typeof(PushErrorType), type), $"The `{nameof(type)}` parameter is undefined.");

            // these string are all define in the Git source code. See 'push.c' `static const char []` declarations.

            switch (type)
            {
                case PushErrorType.CurrentBehindRemote:
                    return "Updates were rejected because the tip of your current branch is behind its remote counterpart. Integrate the remote changes  before pushing again.";

                case PushErrorType.CheckoutPullPush:
                    return "Updates were rejected because a pushed branch tip is behind its remote counterpart. Check out this branch and integrate the remote changes before pushing again.";

                case PushErrorType.RefNeedsForce:
                    return "You cannot update a remote ref that points at a non-commit object, or update a remote ref to make it point at a non-commit object, without using forced push.";

                case PushErrorType.RefAlreadyExists:
                    return "Updates were rejected because the tag already exists in the remote.";

                case PushErrorType.RefFetchFirst:
                    return "Updates were rejected because the remote contains work that you do not have locally. This is usually caused by another repository pushing to the same ref. You may want to first integrate the remote changes before pushing again.";

                case PushErrorType.Rejected:
                default:
                    return "Updates were rejected by the remote.";
            }
        }
    }
}
