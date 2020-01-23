using System;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using Xwt.Drawing;

namespace MonoDevelop.VersionControl
{
	public abstract class VersionStatus
	{
		protected static Xwt.Drawing.Image overlay_modified;
		protected static Xwt.Drawing.Image overlay_removed;
		protected static Xwt.Drawing.Image overlay_renamed;
		protected static Xwt.Drawing.Image overlay_conflicted;
		protected static Xwt.Drawing.Image overlay_added;
		internal static Xwt.Drawing.Image overlay_controled;
		protected static Xwt.Drawing.Image overlay_unversioned;
		protected static Xwt.Drawing.Image overlay_protected;
		protected static Xwt.Drawing.Image overlay_locked;
		protected static Xwt.Drawing.Image overlay_unlocked;
		protected static Xwt.Drawing.Image overlay_ignored;

		protected static Xwt.Drawing.Image icon_modified;
		protected static Xwt.Drawing.Image icon_removed;
		protected static Xwt.Drawing.Image icon_conflicted;
		protected static Xwt.Drawing.Image icon_added;
		protected static Xwt.Drawing.Image icon_controled;

		static VersionStatus ()
		{
			try {
				overlay_modified = Xwt.Drawing.Image.FromResource ("modified-overlay-16.png");
				overlay_removed = Xwt.Drawing.Image.FromResource ("removed-overlay-16.png");
				overlay_renamed = Xwt.Drawing.Image.FromResource ("renamed-overlay-16.png");
				overlay_conflicted = Xwt.Drawing.Image.FromResource ("conflict-overlay-16.png");
				overlay_added = Xwt.Drawing.Image.FromResource ("added-overlay-16.png");
				overlay_controled = Xwt.Drawing.Image.FromResource ("versioned-overlay-16.png");
				overlay_unversioned = Xwt.Drawing.Image.FromResource ("unversioned-overlay-16.png");
				overlay_protected = Xwt.Drawing.Image.FromResource ("lock-required-overlay-16.png");
				overlay_unlocked = Xwt.Drawing.Image.FromResource ("unlocked-overlay-16.png");
				overlay_locked = Xwt.Drawing.Image.FromResource ("locked-overlay-16.png");
				overlay_ignored = Xwt.Drawing.Image.FromResource ("ignored-overlay-16.png");

				icon_modified = ImageService.GetIcon ("vc-file-modified", Gtk.IconSize.Menu);
				icon_removed = ImageService.GetIcon ("vc-file-removed", Gtk.IconSize.Menu);
				icon_conflicted = ImageService.GetIcon ("vc-file-conflicted", Gtk.IconSize.Menu);
				icon_added = ImageService.GetIcon ("vc-file-added", Gtk.IconSize.Menu);
				icon_controled = Xwt.Drawing.Image.FromResource ("versioned-overlay-16.png");

				Unversioned = new CustomVersionStatus (new VersionStatusData {
					Label = GettextCatalog.GetString ("Unversioned"),
					OverlayIcon = overlay_unversioned,
					IsTracked = false
				});

				Versioned = new CustomVersionStatus (new VersionStatusData {
					Label = GettextCatalog.GetString ("Versioned"),
					IsTracked = true
				});

				Ignored = new CustomVersionStatus (new VersionStatusData {
					Label = GettextCatalog.GetString ("Ignored"),
					OverlayIcon = overlay_ignored,
					IsIgnored = true,
					IsTracked = false
				});

				Modified = new CustomVersionStatus (new VersionStatusData {
					Label = GettextCatalog.GetString ("Modified"),
					IsModified = true,
					IsTracked = true,
					Icon = icon_modified,
					OverlayIcon = overlay_modified
				});

				ScheduledReplace = new CustomVersionStatus (new VersionStatusData {
					Label = GettextCatalog.GetString ("Renamed"),
					IsModified = true,
					IsTracked = true,
					IsScheduledReplace = true,
					IsScheduled = true,
					Icon = icon_modified,
					OverlayIcon = overlay_renamed
				});

				ScheduledAdd = new CustomVersionStatus (new VersionStatusData {
					Label = GettextCatalog.GetString ("Add"),
					IsModified = true,
					IsTracked = true,
					IsScheduledAdd = true,
					IsScheduled = true,
					Icon = icon_added,
					OverlayIcon = overlay_added
				});

				ScheduledDelete = new CustomVersionStatus (new VersionStatusData {
					Label = GettextCatalog.GetString ("Delete"),
					IsModified = true,
					IsTracked = true,
					IsScheduledDelete = true,
					IsScheduled = true,
					Icon = icon_removed,
					OverlayIcon = overlay_removed
				});

				Conflicted = new CustomVersionStatus (new VersionStatusData {
					Label = GettextCatalog.GetString ("Conflict"),
					IsModified = true,
					IsTracked = true,
					IsConflicted = true,
					Icon = icon_conflicted,
					OverlayIcon = overlay_conflicted
				});

				Missing = new CustomVersionStatus (new VersionStatusData {
					Label = GettextCatalog.GetString ("Missing"),
					IsTracked = true,
					Icon = icon_removed,
					OverlayIcon = overlay_removed
				});

			} catch (Exception e) {
				LoggingService.LogError ("Error while loading icons.", e);
			}
		}

		public static VersionStatus Unversioned;
		public static VersionStatus Versioned;
		public static VersionStatus Ignored;
		public static VersionStatus Modified;
		public static VersionStatus ScheduledReplace;
		public static VersionStatus ScheduledAdd;
		public static VersionStatus ScheduledDelete;
		public static VersionStatus Conflicted;
		public static VersionStatus Missing;

		public abstract Xwt.Drawing.Image OverlayIcon { get; }

		public abstract Xwt.Drawing.Image Icon { get; }

		public abstract string Label { get; }

		public abstract bool HasConflicts { get; }
		public abstract bool IsTracked { get; }
		public abstract bool IsModified { get; }
		public abstract bool IsRemoteModified { get; }
		public abstract bool IsScheduledAdd { get; }
		public abstract bool IsScheduledDelete { get; }
		public abstract bool IsScheduledReplace { get; }
		public abstract bool IsScheduled { get; }
		public abstract bool IsConflicted { get; }
		public abstract bool IsLocked { get; }
		public abstract bool IsIgnored { get; }

		public abstract string Color { get; }

		public abstract bool CommitDefault { get; }

		protected class VersionStatusData
		{
			public Image OverlayIcon { get; set; }

			public Image Icon { get; set; }

			public string Label { get; set; }

			public bool HasConflicts { get; set; }

			public bool IsTracked { get; set; }

			public bool IsModified { get; set; }

			public bool IsRemoteModified { get; set; }

			public bool IsScheduledAdd { get; set; }

			public bool IsScheduledDelete { get; set; }

			public bool IsLocked { get; set; }

			public bool IsIgnored { get; set; }

			public bool IsScheduledReplace { get; set; }

			public bool IsScheduled { get; set; }

			public bool IsConflicted { get; set; }

			public string Color { get; set; }

			public bool CommitDefault { get; set; } = true;
		}

		protected class CustomVersionStatus : VersionStatus
		{
			public VersionStatusData Data { get; private set; }

			public CustomVersionStatus (VersionStatusData data)
			{
				this.Data = data ?? throw new System.ArgumentNullException (nameof (data));
			}

			public override Image OverlayIcon { get => Data.OverlayIcon; }

			public override Image Icon { get => Data.Icon; }

			public override string Label { get => Data.Label; }

			public override bool HasConflicts { get => Data.HasConflicts; }

			public override bool IsTracked { get => Data.IsTracked; }

			public override bool IsModified { get => Data.IsModified; }

			public override bool IsRemoteModified { get => Data.IsRemoteModified; }

			public override bool IsScheduledAdd { get => Data.IsScheduledAdd; }

			public override bool IsScheduledDelete { get => Data.IsScheduledDelete; }

			public override bool IsScheduledReplace { get => Data.IsScheduledReplace; }

			public override bool IsScheduled { get => Data.IsScheduled; }

			public override bool IsLocked { get => Data.IsLocked; }

			public override bool IsIgnored { get => Data.IsIgnored; }

			public override bool IsConflicted { get => Data.IsConflicted; }

			public override string Color { get => Data.Color; }

			public override bool CommitDefault { get => Data.CommitDefault; }

			public override string ToString ()
			{
				return $"[CustomVersionStatus: Label={Label}]";
			}
		}

		/*
		public static Xwt.Drawing.Image LoadOverlayIconForStatus (VersionStatus status)
		{
			if ((status & VersionStatus.Ignored) != 0)
				return overlay_ignored;

			if ((status & VersionStatus.Versioned) == 0)
				return overlay_unversioned;

			switch (status & VersionStatus.LocalChangesMask) {
			case VersionStatus.Modified:
			case VersionStatus.ScheduledIgnore:
				return overlay_modified;
			case VersionStatus.ScheduledReplace:
				return overlay_renamed;
			case VersionStatus.Conflicted:
				return overlay_conflicted;
			case VersionStatus.ScheduledAdd:
				return overlay_added;
			case VersionStatus.Missing:
			case VersionStatus.ScheduledDelete:
				return overlay_removed;
			}

			if ((status & VersionStatus.LockOwned) != 0)
				return overlay_unlocked;

			if ((status & VersionStatus.Locked) != 0)
				return overlay_locked;

			if ((status & VersionStatus.LockRequired) != 0)
				return overlay_protected;

			return null;
		}

		public static Xwt.Drawing.Image LoadIconForStatus (VersionStatus status)
		{
			switch (status & VersionStatus.LocalChangesMask) {
			case VersionStatus.Modified:
			case VersionStatus.ScheduledReplace:
				return icon_modified;
			case VersionStatus.Conflicted:
				return icon_conflicted;
			case VersionStatus.ScheduledAdd:
				return icon_added;
			case VersionStatus.Missing:
			case VersionStatus.ScheduledDelete:
				return icon_removed;
			}
			return null;
		}

		public static string GetStatusLabel (VersionStatus status)
		{
			if ((status & VersionStatus.Versioned) == 0)
				return GettextCatalog.GetString ("Unversioned");

			switch (status & VersionStatus.LocalChangesMask) {
			case VersionStatus.ScheduledReplace:
				return GettextCatalog.GetString ("Renamed");
			case VersionStatus.Conflicted:
				return GettextCatalog.GetString ("Conflict");
			case VersionStatus.ScheduledAdd:
				return GettextCatalog.GetString ("Add");
			case VersionStatus.ScheduledDelete:
				return GettextCatalog.GetString ("Delete");
			case VersionStatus.Missing:
				return GettextCatalog.GetString ("Missing");
			}
			return String.Empty;
		}
		*/

	}
}
