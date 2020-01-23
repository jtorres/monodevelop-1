//*************************************************************************************************
// PushRejectedException.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class PushRejectedException : PushException
    {
        internal PushRejectedException(string reason, string localRefName, string remoteRefName)
            : base(reason)
        {
            LocalRefName = localRefName;
            RemoteRefName = remoteRefName;
        }

        public readonly string LocalRefName;
        public readonly string RemoteRefName;
    }
}
