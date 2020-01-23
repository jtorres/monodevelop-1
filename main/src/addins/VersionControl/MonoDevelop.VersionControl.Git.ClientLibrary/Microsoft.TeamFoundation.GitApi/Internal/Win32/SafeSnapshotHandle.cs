//*************************************************************************************************
// SafeSnapshotHandle.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Runtime.InteropServices;

namespace Microsoft.TeamFoundation.GitApi.Internal.Win32
{
    internal sealed class SafeSnapshotHandle : SafeHandle
    {
        public SafeSnapshotHandle()
            : base(IntPtr.Zero, true)
        { }
        public SafeSnapshotHandle(IntPtr handle, bool ownsHandle)
            : base(Kernel32.InvalidHandleValue, ownsHandle)
        {
            SetHandle(handle);
        }

        public override bool IsInvalid
        {
            get
            {
                return IsClosed
                    || handle == IntPtr.Zero
                    || handle == Kernel32.InvalidHandleValue;
            }
        }

        protected override bool ReleaseHandle()
        {
            bool closed;
            if (closed = Kernel32.CloseHandle(handle))
            {
                Close();
                SetHandleAsInvalid();
            }
            return closed;
        }
    }
}
