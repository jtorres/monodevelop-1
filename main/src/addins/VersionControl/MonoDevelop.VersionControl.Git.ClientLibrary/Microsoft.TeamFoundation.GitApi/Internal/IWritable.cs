//*************************************************************************************************
// IWritable.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal interface IWritable : IReadable
    {
        void Write(byte[] buffer, int index, int count);
    }
}
