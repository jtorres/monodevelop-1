//*************************************************************************************************
// SafeProcessHandle.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Runtime.InteropServices;

namespace Microsoft.TeamFoundation.GitApi.Internal.Win32
{
    internal class SafeProcessHandle : SafeHandle, IEquatable<SafeProcessHandle>
    {
        public static readonly SafeProcessHandle CurrentProcessHandle = Kernel32.GetCurrentProcess();
        public static readonly SafeProcessHandle Null = new SafeProcessHandle(IntPtr.Zero, false);

        public SafeProcessHandle()
            : base(IntPtr.Zero, true)
        { }

        public SafeProcessHandle(IntPtr handle)
            : base(IntPtr.Zero, true)
        {
            SetHandle(handle);
        }

        public SafeProcessHandle(IntPtr handle, bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle)
        {
            SetHandle(handle);
        }

        public bool IsCurrentProcess { get { return this == CurrentProcessHandle; } }

        public override bool IsInvalid { get { return IsClosed || handle == IntPtr.Zero; } }

        public override bool Equals(object obj)
            => Equals(obj as SafeProcessHandle);

        public bool Equals(SafeProcessHandle other)
            => this == other;

        public override int GetHashCode()
            => handle.GetHashCode();

        protected override bool ReleaseHandle()
            => Kernel32.CloseHandle(handle);
    }
}
