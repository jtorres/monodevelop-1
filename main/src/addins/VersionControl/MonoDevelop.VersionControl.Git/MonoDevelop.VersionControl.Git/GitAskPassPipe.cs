//
// GitAskPassPipe.cs
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
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using LibGit2Sharp;
using MonoDevelop.Core;
using MonoDevelop.Ide;

namespace MonoDevelop.VersionControl.Git
{
	class GitAskPassPipe : IDisposable
	{
		NamedPipeServerStream server;
		private IAsyncResult waitConnection;
		private UsernamePasswordCredentials userNameCredentials;
		private Credentials sshCredentials;

		static string ENV_VAR = "MONODEVELOP_GIT_ASKPASS_PIPE";
		static string Pipe { get; set; }
		static string AskPassPath { get; set; }

		public string Url { get; private set; }

		static GitAskPassPipe ()
		{
			Pipe = "MD_GIT_Pipe_" + Process.GetCurrentProcess ().Id;
			AskPassPath = Path.Combine (Path.GetDirectoryName (typeof (OsXWhere).Assembly.Location), "GitAskPass");
		}

		public GitAskPassPipe (string url)
		{
			this.Url = url;
		}

		public static void SetupContext (Microsoft.TeamFoundation.GitApi.IExecutionContext ctx)
		{
			ctx.Git.EnvironmentValues.Add (new Microsoft.TeamFoundation.GitApi.Environment.Variable (ENV_VAR, Pipe));
			ctx.Git.EnvironmentValues.Add (new Microsoft.TeamFoundation.GitApi.Environment.Variable ("GIT_ASKPASS", AskPassPath));
			ctx.Git.EnvironmentValues.Add (new Microsoft.TeamFoundation.GitApi.Environment.Variable ("DISPLAY", "localhost:0.0"));
			ctx.Git.EnvironmentValues.Add (new Microsoft.TeamFoundation.GitApi.Environment.Variable ("SSH_ASKPASS", AskPassPath));
			ctx.Git.EnvironmentValues.Add (new Microsoft.TeamFoundation.GitApi.Environment.Variable ("GIT_SSH", AskPassPath));
		}

		public void StartPipe ()
		{
			if (!File.Exists (AskPassPath))
				throw new FileNotFoundException ("Git askpass command line tool not found under: " + AskPassPath);
			server = new NamedPipeServerStream (Pipe);
			waitConnection = server.BeginWaitForConnection (HandleAsyncCallback, server);
		}

		public void Dispose()
		{
			if (server != null) {
				// server.EndWaitForConnection (waitConnection);
				server.Dispose ();
				server = null;
			}
		}

		void HandleAsyncCallback (IAsyncResult result)
		{
			var ns = server;
			if (ns == null)
				return;
			var reader = new GitAskPass.StreamStringReadWriter (ns);
			var request = reader.ReadLine ();
			switch (request) {
			case "Username":
				var url = reader.ReadLine ();
				if (GetCredentials (url)) {
					reader.WriteLine (userNameCredentials.Username);
				}
				break;
			case "Password":
				url = reader.ReadLine ();
				if (GetCredentials (url)) {
					reader.WriteLine (userNameCredentials.Password);
				}
				break;
			case "Continue connecting":
				url = reader.ReadLine ();
				string fingerprint = reader.ReadLine ();
				reader.WriteLine (OnGetContinueConnecting (url, fingerprint) ? "yes" : "no");
				break;
			case "SSHPassPhrase":
				string key = reader.ReadLine ();
				reader.WriteLine (OnGetSSHPassphrase (key));
				break;
			case "SSHPassword":
				string userName = reader.ReadLine ();
				reader.WriteLine (OnGetSSHPassword (userName));
				break;
			case "Error":
				throw new Exception (reader.ReadLine ());
			}
			server.Close ();

			server = new NamedPipeServerStream (Pipe);
			server.BeginWaitForConnection (HandleAsyncCallback, server);
		}

		bool OnGetContinueConnecting (string url, string fingerprint)
		{
			var result = MessageService.AskQuestion (
				GettextCatalog.GetString ("Are you sure you want to continue connecting?"),
				GettextCatalog.GetString (@"The authenticity of host '{0}' can't be established.
RSA key fingerprint is {1}.", url, fingerprint),
				AlertButton.Yes,
				AlertButton.No
				);
			if (result == AlertButton.No)
				ForceClose?.Invoke (this, EventArgs.Empty);
			return result == AlertButton.Yes;
		}

		public event EventHandler ForceClose;

		string OnGetSSHPassphrase (string key)
		{
			try {
				var cred = GitCredentials.TryGet (Url, key, GitCredentials.SshPassphrase, GitCredentialsType.Normal) as SshUserKeyCredentials;
				if (cred == null)
					throw new InvalidOperationException ("Can't get ssh passphrase.");
				return cred.Passphrase;
			} catch (Exception e) {
				LoggingService.LogInternalError (e);
				throw;
			}
		}

		string OnGetSSHPassword (string userName)
		{
			try {
				var cred = GitCredentials.TryGet (Url, userName, GitCredentials.SshPassphrase, GitCredentialsType.Normal) as SshUserKeyCredentials;
				if (cred == null)
					throw new InvalidOperationException ("Can't ssh password.");
				return cred.Passphrase;
			} catch (Exception e) {
				LoggingService.LogInternalError (e);
				throw;
			}
		}

		bool GetCredentials (string url)
		{
			try {
				if (userNameCredentials == null) {
					try {
						userNameCredentials = GitCredentials.TryGet (url, Environment.UserName, SupportedCredentialTypes.UsernamePassword, GitCredentialsType.Normal) as UsernamePasswordCredentials;
					} catch (Exception e) {
						LoggingService.LogInternalError (e);
					}
				}
				return userNameCredentials != null;
			} catch (Exception e) {
				LoggingService.LogInternalError (e);
				throw;
			}
		}
	}
}
