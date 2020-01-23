//*************************************************************************************************
// SafeJobObjectHandle.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Runtime.InteropServices;

namespace Microsoft.TeamFoundation.GitApi.Internal.Win32
{
    internal sealed class SafeJobObjectHandle : SafeHandle
    {
        public SafeJobObjectHandle()
            : base(IntPtr.Zero, true)
        { }

        public SafeJobObjectHandle(IntPtr handle, bool ownsHandle)
            : base(Kernel32.InvalidHandleValue, ownsHandle)
        {
            SetHandle(handle);
        }

        public static readonly SafeJobObjectHandle Null = new SafeJobObjectHandle(IntPtr.Zero, ownsHandle: false);

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
            if (closed = Win32.Kernel32.CloseHandle(handle))
            {
                Close();
                SetHandleAsInvalid();
            }
            return closed;
        }
    }
}
