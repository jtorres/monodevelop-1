//*************************************************************************************************
// ILoggable.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System.Text;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    /// <summary>
    /// Exposes the ability for an object to log information about itself.
    /// </summary>
    internal interface ILoggable
    {
        void Log(ExecutionContext context, StringBuilder log, int indent);
    }
}
