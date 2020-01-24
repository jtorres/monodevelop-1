//
// HistoryTests.cs
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
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MonoDevelop.VersionControl.Git.Tests
{
	[TestFixture]
	class HistoryTests : AbstractGitTests
	{
		[Test]
		public async Task TestByFileHistoryAsync ()
		{
			await AddFileAsync ("testfile", "1", true, true);
			await AddFileAsync ("testfile2", "2", true, true);
			await AddFileAsync ("testfile3", "3", true, true);

			await AddFileAsync ("testfile", "new 1", true, true);
			await AddFileAsync ("testfile3", "new 1", true, true);
			
			var history = await Repo.GetHistoryAsync ("testfile", null);
			Assert.AreEqual (2, history.Length);
			Assert.AreEqual ("Commit #3\n", history [0].Message);
			Assert.AreEqual ("Commit #0\n", history [1].Message);

			history = await Repo.GetHistoryAsync ("testfile2", null);
			Assert.AreEqual (1, history.Length);
			Assert.AreEqual ("Commit #1\n", history [0].Message);

			history = await Repo.GetHistoryAsync ("testfile3", null);
			Assert.AreEqual (2, history.Length);
			Assert.AreEqual ("Commit #4\n", history [0].Message);
			Assert.AreEqual ("Commit #2\n", history [1].Message);
		}
	}
}
