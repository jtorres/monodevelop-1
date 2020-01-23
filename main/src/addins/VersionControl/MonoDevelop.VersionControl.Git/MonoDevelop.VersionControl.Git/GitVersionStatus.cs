//
// GitVersionStatus.cs
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
using Minimatch;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using Xwt.Drawing;

namespace MonoDevelop.VersionControl.Git
{
	sealed class GitVersionStatus : VersionStatus
	{
		public static VersionStatus NewFile;
		public static VersionStatus Copied;
		public static VersionStatus Deleted;
		public static VersionStatus Modified_Staged;
		public static VersionStatus Modified_Unstaged;
		public static VersionStatus Renamed;
		public static VersionStatus TypeChanged;
		public static VersionStatus Unmerged;
		public static VersionStatus Untracked;

		static GitVersionStatus ()
		{
			string green = IdeApp.Preferences.UserInterfaceTheme == Theme.Light ? "#9CC0EA" : "#4e9a06";
			string red = IdeApp.Preferences.UserInterfaceTheme == Theme.Light ? "#E896A0" : "#cc0000";

			NewFile = new CustomVersionStatus (new VersionStatusData {
				Label = GettextCatalog.GetString ("New File"),
				IsModified = true,
				IsTracked = true,
				IsScheduledAdd = true,
				IsScheduled = true,
				Icon = icon_added,
				OverlayIcon = overlay_added,
				Color = green
			});

			Copied = new CustomVersionStatus (new VersionStatusData {
				Label = GettextCatalog.GetString ("Copied"),
				IsModified = true,
				IsTracked = true,
				IsScheduledReplace = true,
				IsScheduled = true,
				Icon = icon_modified,
				OverlayIcon = overlay_renamed,
				Color = green
			});

			Deleted = new CustomVersionStatus (new VersionStatusData {
				Label = GettextCatalog.GetString ("Deleted"),
				IsModified = true,
				IsTracked = true,
				IsScheduledDelete = true,
				IsScheduled = true,
				Icon = icon_removed,
				OverlayIcon = overlay_removed,
				Color = green
			});

			Modified_Staged = new CustomVersionStatus (new VersionStatusData {
				Label = GettextCatalog.GetString ("Modified"),
				IsModified = true,
				IsTracked = true,
				IsScheduled = true,
				Icon = icon_modified,
				OverlayIcon = overlay_modified,
				Color = green
			});

			Modified_Unstaged = new CustomVersionStatus (new VersionStatusData {
				Label = GettextCatalog.GetString ("Modified"),
				IsModified = true,
				IsTracked = true,
				Icon = icon_modified,
				OverlayIcon = overlay_modified,
				CommitDefault = false,
				Color = red
			});

			Renamed = new CustomVersionStatus (new VersionStatusData {
				Label = GettextCatalog.GetString ("Renamed"),
				IsModified = true,
				IsTracked = true,
				IsScheduledReplace = true,
				IsScheduled = true,
				Icon = icon_modified,
				OverlayIcon = overlay_renamed,
				Color = green
			});

			TypeChanged = new CustomVersionStatus (new VersionStatusData {
				Label = GettextCatalog.GetString ("Typechange"),
				IsModified = true,
				IsTracked = true,
				IsScheduledReplace = true,
				IsScheduled = true,
				Icon = icon_modified,
				OverlayIcon = overlay_renamed,
				Color = green
			});

			Unmerged = new CustomVersionStatus (new VersionStatusData {
				Label = GettextCatalog.GetString ("Unmerged"),
				IsModified = true,
				IsTracked = true,
				IsConflicted = true,
				Icon = icon_conflicted,
				OverlayIcon = overlay_conflicted,
				CommitDefault = false,
				Color = red
			});

			Untracked = new CustomVersionStatus (new VersionStatusData {
				Label = GettextCatalog.GetString ("Untracked"),
				IsModified = false,
				IsTracked = false,
				CommitDefault = false,
				Color = red
			});
		}

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

		public override bool IsScheduled => throw new NotImplementedException ();

		public override bool IsConflicted => throw new NotImplementedException ();

		public override bool IsLocked => throw new NotImplementedException ();

		public override bool IsIgnored => throw new NotImplementedException ();

		public override string Color => throw new NotImplementedException ();

		public override bool CommitDefault => throw new NotImplementedException ();
	}
}
