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
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.VersionControl;
using MonoDevelop.VersionControl.Git;
using MonoDevelop.VersionControl.Tests;
using NUnit.Framework;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MonoDevelop.VersionControl.Git.Tests
{

	[TestFixture]
	class GitStatusTests : AbstractGitTests
	{
		[Test]
		public async Task TestFileIsVersionedAsync ()
		{
			const string fileName = "mytest.txt";
			var fullPath = LocalPath.Combine (fileName);
			await AddFileAsync (fileName, "content", true, true);
			var vi = await Repo.GetVersionInfoAsync (fullPath, VersionInfoQueryFlags.IncludeRemoteStatus | VersionInfoQueryFlags.IgnoreCache);
			Assert.AreEqual (GitVersionStatus.Versioned, vi.Status);
		}

		[Test]
		public async Task TestFileIsNewFileAsync ()
		{
			const string fileName = "mytest.txt";
			var fullPath = LocalPath.Combine (fileName);
			File.WriteAllText (fullPath, "content");
			var psi = new ProcessStartInfo ("git") {
				WorkingDirectory = LocalPath,
				Arguments = "add " + fileName
			};
			Process.Start (psi).WaitForExit ();
			var vi = await Repo.GetVersionInfoAsync (fullPath, VersionInfoQueryFlags.IncludeRemoteStatus | VersionInfoQueryFlags.IgnoreCache);
			Assert.AreEqual (GitVersionStatus.NewFile, vi.Status);
		}

		[Test]
		public async Task TestFileIsModified_UnstagedAsync ()
		{
			const string fileName = "mytest.txt";
			var fullPath = LocalPath.Combine (fileName);
			await AddFileAsync (fileName, "content", true, true);
			File.WriteAllText (fullPath, "new content");
			var vi = await Repo.GetVersionInfoAsync (fullPath, VersionInfoQueryFlags.IncludeRemoteStatus | VersionInfoQueryFlags.IgnoreCache);
			Assert.AreEqual (GitVersionStatus.Modified_Unstaged, vi.Status);
		}


		[Test]
		public async Task TestFileIsModified_StagedAsync ()
		{
			const string fileName = "mytest.txt";
			var fullPath = LocalPath.Combine (fileName);
			await AddFileAsync (fileName, "content", true, true);
			File.WriteAllText (fullPath, "new content");
			var psi = new ProcessStartInfo ("git") {
				WorkingDirectory = LocalPath,
				Arguments = "add " + fileName
			};
			Process.Start (psi).WaitForExit ();

			var vi = await Repo.GetVersionInfoAsync (fullPath, VersionInfoQueryFlags.IncludeRemoteStatus | VersionInfoQueryFlags.IgnoreCache);
			Assert.AreEqual (GitVersionStatus.Modified_Staged, vi.Status);
		}

		[Test]
		public async Task TestFileIsMissingAsync ()
		{
			const string fileName = "mytest.txt";
			var fullPath = LocalPath.Combine (fileName);
			await AddFileAsync (fileName, "content", true, true);
			File.Delete (fullPath);
			var vi = await Repo.GetVersionInfoAsync (fullPath, VersionInfoQueryFlags.IncludeRemoteStatus | VersionInfoQueryFlags.IgnoreCache);
			Assert.AreEqual (GitVersionStatus.Missing, vi.Status);
		}


		[Test]
		public async Task TestFileIsDeletedAsync ()
		{
			const string fileName = "mytest.txt";
			var fullPath = LocalPath.Combine (fileName);
			await AddFileAsync (fileName, "content", true, true);
			await Repo.DeleteFileAsync (fileName, true, new ProgressMonitor (), false);
			var vi = await Repo.GetVersionInfoAsync (fullPath, VersionInfoQueryFlags.IncludeRemoteStatus | VersionInfoQueryFlags.IgnoreCache);
			Assert.AreEqual (GitVersionStatus.Deleted, vi.Status);
		}


		[Test]
		[Ignore("Implement me!")]
		public async Task TestFileIsRenamedAsync ()
		{
			const string fileName = "mytest.txt";
			var fullPath = LocalPath.Combine (fileName);
			await AddFileAsync (fileName, "content", true, true);
			var fileName2 = "test2.txt";
			var path2 = LocalPath.Combine (fileName2);
			var psi = new ProcessStartInfo ("git") {
				WorkingDirectory = LocalPath,
				Arguments = "mv " + fileName + " " + fileName2
			};
			Process.Start (psi).WaitForExit ();
			var vi = await Repo.GetVersionInfoAsync (fileName2, VersionInfoQueryFlags.IncludeRemoteStatus | VersionInfoQueryFlags.IgnoreCache);
			Assert.AreEqual (GitVersionStatus.Renamed, vi.Status);
		}


		[Test]
		public async Task TestTypeChangeAsync ()
		{
			const string fileName = "mytest.txt";
			const string fileName2 = "mytest2.txt";
			var fullPath = LocalPath.Combine (fileName);
			var fullPath2 = LocalPath.Combine (fileName2);
			await AddFileAsync (fileName, "content", true, true);
			await AddFileAsync (fullPath2, "content", true, true);
			File.Delete (fullPath);
			var psi = new ProcessStartInfo ("ln") {
				WorkingDirectory = LocalPath,
				Arguments = "-s '" + fullPath2 + "' '" + fullPath +"'"
			};
			Process.Start (psi).WaitForExit ();
			psi = new ProcessStartInfo ("git") {
				WorkingDirectory = LocalPath,
				Arguments = "add " + fileName
			};
			Process.Start (psi).WaitForExit ();
			var vi = await Repo.GetVersionInfoAsync (fullPath, VersionInfoQueryFlags.IncludeRemoteStatus | VersionInfoQueryFlags.IgnoreCache);
			Assert.AreEqual (GitVersionStatus.TypeChanged, vi.Status);
		}

		
	}
}