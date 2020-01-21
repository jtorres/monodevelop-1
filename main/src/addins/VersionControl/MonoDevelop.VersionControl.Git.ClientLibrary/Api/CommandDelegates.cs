//*************************************************************************************************
// CommandDelegates.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    public delegate bool TerminateProcessDelegate(IProcess process, TimeSpan timeout);
}
