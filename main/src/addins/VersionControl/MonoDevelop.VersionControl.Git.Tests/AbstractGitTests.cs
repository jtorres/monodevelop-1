//
// GitStatusTests.cs
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

using MonoDevelop.Core;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonoDevelop.VersionControl.Git.Tests
{
	abstract class AbstractGitTests
	{
		protected GitRepository Repo;

		public FilePath LocalPath { get; private set; }

		[SetUp]
		public void Setup ()
		{
			// Generate directories and a svn util.
			LocalPath = new FilePath (FileService.CreateTempDirectory ());

			LibGit2Sharp.Repository.Init (LocalPath);
			var url = "file://" + LocalPath;
			Repo = GetRepo (LocalPath, url);
			Repo.SetUserInfo ("author", "email@service.domain");
		}

		[TearDown]
		public void TearDown ()
		{
			if (Repo != null) {
				Repo.Dispose ();
				Repo = null;
			}
			DeleteDirectory (LocalPath);
		}

		#region Utils

		internal static GitRepository GetRepo (string path, string url)
		{
			return new GitRepository (VersionControlService.GetVersionControlSystems ().First (id => id.Name == "Git"), path, url);
		}

		internal static void DeleteDirectory (string path)
		{
			string [] files = Directory.GetFiles (path);
			string [] dirs = Directory.GetDirectories (path);

			foreach (var file in files) {
				File.SetAttributes (file, FileAttributes.Normal);
				File.Delete (file);
			}

			foreach (var dir in dirs) {
				DeleteDirectory (dir);
			}

			Directory.Delete (path, true);
		}

		protected async Task AddFileAsync (string relativePath, string contents, bool toVcs, bool commit)
		{
			var monitor = new ProgressMonitor ();
			string added = Path.Combine (LocalPath, relativePath);
			if (contents == null)
				Directory.CreateDirectory (added);
			else
				File.WriteAllText (added, contents);

			if (toVcs)
				await Repo.AddAsync (new FilePath [] { added }, false, monitor);

			if (commit)
				await CommitFileAsync (added);
		}

		int CommitNumber;
		protected async Task CommitFileAsync (string path)
		{
			var monitor = new ProgressMonitor ();
			ChangeSet changes = Repo.CreateChangeSet (Repo.RootPath);

			// [Git] Needed by build bots.
			changes.ExtendedProperties.Add ("Git.AuthorName", "author");
			changes.ExtendedProperties.Add ("Git.AuthorEmail", "email@service.domain");

			changes.AddFile (await Repo.GetVersionInfoAsync (path, VersionInfoQueryFlags.IgnoreCache));
			changes.GlobalComment = string.Format ("Commit #{0}", CommitNumber++);
			await Repo.CommitAsync (changes, monitor);
		}
		#endregion
	}
}