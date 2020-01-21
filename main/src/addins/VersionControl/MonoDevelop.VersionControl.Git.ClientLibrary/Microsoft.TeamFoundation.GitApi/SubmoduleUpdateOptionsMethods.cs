//*************************************************************************************************
// SubmoduleUpdateOptionsModes.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum SubmoduleUpdateOptionsMethods
    {
        /// <summary>
        /// See "git submodule update".  Use the default update method or
        /// the method specified in "submodule.&lt;name&gt;.update" config
        /// setting.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// See "git submodule update --checkout".
        /// </summary>
        Checkout = 1,

        /// <summary>
        /// See "git submodule update --merge".
        /// </summary>
        Merge = 2,

        /// <summary>
        /// See "git submodule update --rebase".
        /// </summary>
        Rebase = 3,
    }
}
