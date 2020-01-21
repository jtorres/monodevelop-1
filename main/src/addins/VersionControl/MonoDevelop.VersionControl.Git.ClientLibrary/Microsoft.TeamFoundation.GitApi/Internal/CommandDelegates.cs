//*************************************************************************************************
// CommandDelegates.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal delegate IProcess StartProcessDelegate(string command, bool redirect);
    internal delegate string CreateApplicationNameDelegate(Environment environment);
    internal delegate string CreateProcessCommandLineDelegate(string command, Environment environment);
}
