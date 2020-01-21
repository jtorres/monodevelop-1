//
// GitBranchTests.cs
//
// Author:
//       Mike Krüger <mikkrg@microsoft.com>
//
// Copyright (c) 2020 Microsoft Corporation. All rights reserved.
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
using ICSharpCode.Decompiler.IL;

namespace MonoDevelop.VersionControl.Git.Tests
{
	[TestFixture]
	class GitBranchTests : AbstractGitTests
	{
		[Test]
		public async Task TestAddTagAsync ()
		{
			var repo2 = (GitRepository)Repo;
			await AddFileAsync ("file1", "text", true, true);
			var head = new GitRevision (Repo, repo2.RootPath, repo2.RootRepository.Head.Tip); ;
			await repo2.AddTagAsync ("tag1", head, "my-tag");
			var tags = await repo2.GetTagsAsync ();
			Assert.AreEqual (1, tags.Count);
		}

		[Test]
		public async Task TestRemoveTagAsync ()
		{
			var repo2 = (GitRepository)Repo;
			await AddFileAsync ("file1", "text", true, true);
			var head = new GitRevision (Repo, repo2.RootPath, repo2.RootRepository.Head.Tip); ;
			await repo2.AddTagAsync ("tag1", head, "my-tag");
			var tags = await repo2.GetTagsAsync ();
			Assert.AreEqual (1, tags.Count);
			await repo2.RemoveTagAsync ("tag1");
			Assert.AreEqual (0, (await repo2.GetTagsAsync ()).Count);
		}

		[Test]
		public async Task TestGetTagsAsync ()
		{
			var repo2 = (GitRepository)Repo;
			await AddFileAsync ("file1", "text", true, true);
			var head = new GitRevision (Repo, repo2.RootPath, repo2.RootRepository.Head.Tip); ;
			await repo2.AddTagAsync ("tag1", head, "my-tag");
			await repo2.AddTagAsync ("tag2", head, "my-tag");
			await repo2.AddTagAsync ("tag3", head, "my-tag");
			var tags = await repo2.GetTagsAsync ();
			Assert.AreEqual (3, tags.Count);
			Assert.AreEqual ("tag1", tags [0]);
			Assert.AreEqual ("tag2", tags [1]);
			Assert.AreEqual ("tag3", tags [2]);
		}

		[Test]
		public async Task TestGetCurrentBranch ()
		{
			var repo = (GitRepository)Repo;

			await AddFileAsync ("file1", "text", true, true);
			await repo.CreateBranchAsync ("myBranch", null, null);
			await repo.SwitchToBranchAsync (new ProgressMonitor (), "myBranch");

			Assert.AreEqual ("myBranch", repo.GetCurrentBranch ());
		}

		[Test]
		public async Task TestGetCurrentBranchAsync ()
		{
			var repo = (GitRepository)Repo;

			await AddFileAsync ("file1", "text", true, true);
			await repo.CreateBranchAsync ("myBranch", null, null);
			await repo.SwitchToBranchAsync (new ProgressMonitor (), "myBranch");

			Assert.AreEqual ("myBranch", await repo.GetCurrentBranchAsync ());
		}

		[Test]
		public async Task TestGetLocalBranchNamesAsync ()
		{
			var repo = (GitRepository)Repo;

			await AddFileAsync ("file1", "text", true, true);
			await repo.CreateBranchAsync ("myBranch1", null, null);
			await repo.CreateBranchAsync ("myBranch2", null, null);
			await repo.CreateBranchAsync ("myBranch3", null, null);
			await repo.CreateBranchAsync ("myBranch4", null, null);

			var branchNames = await repo.GetLocalBranchNamesAsync ();

			Assert.AreEqual (5, branchNames.Count);
			Assert.IsTrue (branchNames.Contains ("master"));
			Assert.IsTrue (branchNames.Contains ("myBranch1"));
			Assert.IsTrue (branchNames.Contains ("myBranch2"));
			Assert.IsTrue (branchNames.Contains ("myBranch3"));
			Assert.IsTrue (branchNames.Contains ("myBranch4"));
		}
	}
}
