//*************************************************************************************************
// SafeWindowsHandle.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Runtime.InteropServices;

namespace Microsoft.TeamFoundation.GitApi.Internal.Win32
{
    internal abstract class SafeWindowsHandle : SafeHandle
    {
        public SafeWindowsHandle()
            : base(IntPtr.Zero, true)
        { }
        public SafeWindowsHandle(IntPtr handle, bool ownsHandle)
            : base(Kernel32.InvalidHandleValue, ownsHandle)
        {
            SetHandle(handle);
        }

        public override bool IsInvalid { get { return IsClosed || handle == IntPtr.Zero; } }

        protected override bool ReleaseHandle()
        {
            bool closed;
            if (closed = Win32.Kernel32.CloseHandle(handle))
            {
                Close();
                SetHandleAsInvalid();
            }
            return closed;
        }
    }
}
