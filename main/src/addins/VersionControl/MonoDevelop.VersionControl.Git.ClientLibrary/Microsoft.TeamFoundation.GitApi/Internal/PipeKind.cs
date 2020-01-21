//*************************************************************************************************
// PipeKind.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal enum PipeKind
    {
        GenericInput,
        GenericOutput,
        StandardInput,
        StandardOutput,
        StandardError,
    }
}
