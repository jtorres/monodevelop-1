//*************************************************************************************************
// CheckoutIndexOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public struct CheckoutIndexOptions
    {
        public static readonly CheckoutIndexOptions Default = new CheckoutIndexOptions
        {
            Flags = CheckoutIndexOptionFlags.None,
            Stage = CheckoutIndexOptionStage.Default,
        };

        public CheckoutIndexOptionFlags Flags;
        public CheckoutIndexOptionStage Stage;
    }
}
