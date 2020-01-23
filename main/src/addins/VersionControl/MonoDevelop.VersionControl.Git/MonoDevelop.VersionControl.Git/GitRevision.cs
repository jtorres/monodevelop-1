//
// GitRepository.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
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

//#define DEBUG_GIT

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.TeamFoundation.GitApi;
using MonoDevelop.Core;

namespace MonoDevelop.VersionControl.Git
{
	public class GitRevision : Revision
	{
		readonly string rev;

		internal FilePath FileForChanges { get; set; }

		public string GitRepository {
			get; private set;
		}

		public GitRevision (Repository repo, string gitRepository, Commit commit) : base (repo, commit?.Author.When.DateTime ?? DateTime.Now, commit?.Author.Name, commit?.Message)
		{
			GitRepository = gitRepository;
			rev = commit != null ? commit.Id.Sha : "";
		}

		public GitRevision (Repository repo, string gitRepository, Commit commit, DateTime time, string author, string message) : base (repo, time, author, message)
		{
			GitRepository = gitRepository;
			rev = commit != null ? commit.Id.Sha : "";
		}

		public GitRevision (Repository repo, string gitRepository, string sha, string message) : base (repo)
		{
			GitRepository = gitRepository;
			Message = message;
			rev = sha;
		}

		internal GitRevision (Repository repo, string gitRepository, ICommit commit) : base (repo, commit.Committer.Timestamp.DateTime, commit.Author.Username, commit.Message)
		{
			GitRepository = gitRepository;
			Email = commit.Author.Email;
			rev = commit.RevisionText;
			ShortMessage = commit.FirstLine;
			if (ShortMessage.Length > 50) 
				ShortMessage = ShortMessage.Substring (0, 50) + "…";
		}

		public override string ToString ()
		{
			return rev;
		}

		string shortName;
		public override string ShortName {
			get {
				if (shortName != null)
					return shortName;
				return shortName = rev.Length > 10 ? rev.Substring (0, 10) : rev;
			}
		}

		internal Commit GetCommit (LibGit2Sharp.Repository repository)
		{
			if (repository.Info.WorkingDirectory != GitRepository)
				throw new ArgumentException ("Commit does not belog to the repository", nameof (repository));
			return repository.Lookup<Commit> (rev);
		}

		public override Task<Revision> GetPreviousAsync (CancellationToken cancellationToken)
		{
			var repo = (GitRepository)Repository;
			return repo.RunOperationAsync (GitRepository, (repository, token) => GetPrevious (repository), cancellationToken: cancellationToken);
		}

		internal Revision GetPrevious (LibGit2Sharp.Repository repository)
		{
			if (repository.Info.WorkingDirectory != GitRepository)
				throw new ArgumentException ("Commit does not belog to the repository", nameof (repository));
			var id = repository.Lookup<Commit> (rev)?.Parents.FirstOrDefault ();
			return id == null ? null : new GitRevision (Repository, GitRepository, id);
		}
	}
}
