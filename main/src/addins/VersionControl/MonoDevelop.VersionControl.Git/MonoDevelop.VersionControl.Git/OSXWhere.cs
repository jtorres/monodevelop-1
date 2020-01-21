//
// OSXWhere.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.TeamFoundation.GitApi;
using MonoDevelop.Core;
using MonoDevelop.Core.Text;
using MonoDevelop.Ide;
using ProgressMonitor = MonoDevelop.Core.ProgressMonitor;

namespace MonoDevelop.VersionControl.Git
{
	class OsXWhere : IWhere
	{
		public bool FindApplication (string name, out string path)
		{
			path = null;
			return false;
		}

		public bool FindGitAskpass (Installation installation, out string path)
		{
			path = Path.Combine (Path.GetDirectoryName (typeof (OsXWhere).Assembly.Location), "GitAskPass");
			return true;
		}

		public bool FindGitCredentialManager (Installation installation, out string path)
		{
			path = null;
			return false;
		}

		public bool FindGitInstallation (string path, KnownDistribution distro, out Installation installation)
		{
			installation = null;
			return false;
		}

		public bool FindGitInstallations (out List<Installation> installations)
		{
			installations = null;
			return false;
		}

		public bool FindHomePath (Microsoft.TeamFoundation.GitApi.Environment environment, out string homePath)
		{
			homePath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			return true;
		}

		public bool FindHomePath (out string homePath)
		{
			homePath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			return true;
		}

		public bool GitGlobalConfig (Microsoft.TeamFoundation.GitApi.Environment environment, out string path)
		{
			path = null;
			return false;
		}

		public bool GitGlobalConfig (out string path)
		{
			path = null;
			return false;
		}

		public bool GitLocalConfig (string startingDirectory, out string path)
		{
			path = null;
			return false;
		}

		public bool GitLocalConfig (out string path)
		{
			path = null;
			return false;
		}

		public bool GitPortableConfig (out string path)
		{
			path = null;
			return false;
		}

		public bool GitSystemConfig (out string path)
		{
			path = null;
			return false;
		}

		public bool GitSystemConfig (Installation installation, out string path)
		{
			path = null;
			return false;
		}

		public bool GitXdgConfig (out string path)
		{
			path = null;
			return false;
		}
	}
}
