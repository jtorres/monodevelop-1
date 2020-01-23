//*************************************************************************************************
// CheckoutOptions.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System.Collections.Generic;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<seealso cref="IRepository.Checkout(IRevision, CheckoutOptions)"/>`.
    /// </summary>
    public struct CheckoutOptions
    {
        public static readonly CheckoutOptions Default = new CheckoutOptions
        {
            Flags = CheckoutOptionFlags.None,
            Paths = null,
            ProgressCallback = null,
        };

        /// <summary>
        /// Extended options related to a Git checkout operation.
        /// </summary>
        public CheckoutOptionFlags Flags;

        /// <summary>
        /// 
        /// <summary/>
        public IEnumerable<string> Paths;

        /// <summary>
        /// 
        /// <summary/>
        public OperationProgressDelegate ProgressCallback;
    }
}
