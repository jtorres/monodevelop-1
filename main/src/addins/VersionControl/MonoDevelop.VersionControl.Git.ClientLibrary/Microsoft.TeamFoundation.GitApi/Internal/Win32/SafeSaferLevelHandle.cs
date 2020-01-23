//*************************************************************************************************
// SafeSaferLevelHandle.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Runtime.InteropServices;

namespace Microsoft.TeamFoundation.GitApi.Internal.Win32
{
    internal sealed class SafeSaferLevelHandle : SafeHandle
    {
        public SafeSaferLevelHandle(IntPtr invalidHandleValue, bool ownsHandle)
            : base(invalidHandleValue, ownsHandle)
        { }
        public SafeSaferLevelHandle()
            : this(Advapi32.InvalidHandleValue, true)
        {
            SetHandle(handle);
        }

        public override bool IsInvalid
        {
            get
            {
                return IsClosed
                  || handle == IntPtr.Zero
                  || handle == Advapi32.InvalidHandleValue;
            }
        }

        protected override bool ReleaseHandle()
        {
            bool closed;
            if (closed = Win32.Advapi32.SaferCloseLevel(handle))
            {
                Close();
                SetHandleAsInvalid();
            }
            return closed;
        }
    }
}
