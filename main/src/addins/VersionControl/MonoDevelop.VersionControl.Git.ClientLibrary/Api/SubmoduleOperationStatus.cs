//*************************************************************************************************
// SubmoduleOperationStatus.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
#if false
    // TODO.GitApi: Confirm that this is still needed.
    public struct SubmoduleOperationStatus
    {
        internal SubmoduleOperationStatus(SubmoduleOperationStatus copy)
        {
            Name = copy.Name;
            Url = copy.Url;
            Path = copy.Path;
            Head = copy.Head;
        }

        public string Name;
        public string Url;
        public string Path;
        public Api.ObjectId Head;
    }
#endif
}
