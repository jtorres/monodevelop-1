//
// SvnVersionStatus.cs
//
// Author:
//       Mike Krüger <mikkrg@microsoft.com>
//
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using Xwt.Drawing;

namespace MonoDevelop.VersionControl.Subversion
{
	class SvnVersionStatus : VersionStatus
	{
		public override Image OverlayIcon => throw new NotImplementedException ();

		public override Image Icon => throw new NotImplementedException ();

		public override string Label => throw new NotImplementedException ();

		public override bool HasConflicts => throw new NotImplementedException ();

		public override bool IsTracked => throw new NotImplementedException ();

		public override bool IsModified => throw new NotImplementedException ();

		public override bool IsRemoteModified => throw new NotImplementedException ();

		public override bool IsScheduledAdd => throw new NotImplementedException ();

		public override bool IsScheduledDelete => throw new NotImplementedException ();

		public override bool IsScheduledReplace => throw new NotImplementedException ();

		public override bool IsConflicted => throw new NotImplementedException ();

		public override bool IsLocked => throw new NotImplementedException ();

		public override bool IsIgnored => throw new NotImplementedException ();

		public override string Color => throw new NotImplementedException ();

		public override bool CommitDefault => throw new NotImplementedException ();

		public override bool IsScheduled => throw new NotImplementedException ();

		internal static void SetLocked (VersionStatus status)
		{
			var customStatus = status as CustomVersionStatus;
			customStatus.Data.OverlayIcon = overlay_locked;
			customStatus.Data.IsLocked = true;
		}

		internal static void SetLockOwned (VersionStatus status)
		{
			var customStatus = status as CustomVersionStatus;
			customStatus.Data.OverlayIcon = overlay_unlocked;
			customStatus.Data.IsLocked = false;
		}

		internal static void SetLockRequired (VersionStatus status)
		{
			var customStatus = status as CustomVersionStatus;
			customStatus.Data.OverlayIcon = overlay_protected;
			customStatus.Data.IsLocked = false;
		}
	}
}
