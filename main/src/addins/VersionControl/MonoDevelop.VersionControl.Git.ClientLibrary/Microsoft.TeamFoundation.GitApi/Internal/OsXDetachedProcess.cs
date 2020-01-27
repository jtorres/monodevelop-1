//
// OsXDetachedProcess.cs
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
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    partial class OsXDetachedProcess : DetachedProcess
    {
        Process process;

        public override int ExitCode {
            get {
                lock (_syncpoint) {
                    if (!_started)
                        throw new InvalidOperationException ("Cannot get the exit code of a process which has not yet started.");

                    TestProcessState ();

                    if (!_hasExited)
                        throw new InvalidOperationException ("Cannot get the exit code of a process which has not exited yet.");
                    try {
                        return process.ExitCode;
                    } catch {
                        return -1;
                    }
                }

            }
        }

        private void TestProcessState ()
        {
            lock (_syncpoint) {
                if (HasExited) {
                    _hasExited = true;
                    return;
                }
                process?.WaitForExit ();
                _hasExited = true;
            }
        }

        public override bool HasExited => process.HasExited;

        public override bool IsWow64 => true;

        public override int ProcessId => process.Id;

        public override bool RedirectStandardPipes { get; set; }

        public override StreamReader StandardError => process.StandardError;

        public override StreamWriter StandardInput => process.StandardInput;

        public override StreamReader StandardOutput => process.StandardOutput;

        public override Stream StdErr => process.StandardError.BaseStream;

        public override Stream StdIn => process.StandardInput.BaseStream;

		public override Stream StdOut => process.StandardOutput.BaseStream;

		IExecutionContext context;

        public OsXDetachedProcess (IExecutionContext context)
        {
            this.context = context;
            this.Encoding = Encoding.UTF8;
        }

        public override Task<bool> Exit (TimeSpan timeout)
		{
			lock (_syncpoint) {
				this.WaitForExit (timeout);
			}
			return Task.FromResult (true);
		}

		public override Task<bool> Exit ()
		{
			return Exit (TimeSpan.MaxValue);
		}


        public override Task Kill()
        {
            if (process.HasExited)
            {
                return Task.CompletedTask;
            }
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    // Hack:
                    // git spans an ssh process when cloning an repository authenticated by ssh for example
                    // process.Kill() won't kill the process tree. (.NET core 3.0 has an overload that may work here)
                    // But for now we just kill the processes by their start time and name.
                    if (p.ProcessName == "git" || p.ProcessName == "ssh")
                    {
                        if ((p.StartTime - process.StartTime).TotalSeconds < 1)
                        {
                            p.Kill();
                        }
                    }
                }
                catch { }
            }
            return Task.CompletedTask;
        }

		object processLock = new object ();
		public override void Start ()
		{
			var commandLine = _createProcessCommandLineCallback (Command, Environment);
			var idx = commandLine.IndexOf ('"', 1);

			var startInfo = new ProcessStartInfo (commandLine.Substring (1, idx - 1)) {
				WorkingDirectory = this.WorkingDirectory,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				RedirectStandardInput = true,
				UseShellExecute = false,
				Arguments = commandLine.Substring (idx + 2).TrimStart ()
			};
			foreach (var variable in context.Git.EnvironmentValues) {
				startInfo.EnvironmentVariables[variable.Name] = variable.Value;
			}
			process = Process.Start (startInfo);
			_started = true;
            if (RedirectStandardPipes) {
                process.OutputDataReceived += (sender, e) => {
                    try {
                        if (e?.Data == null)
                            return;
                        OnProcessOutput (new OperationOutput (OutputSource.Out, e.Data));
                    } catch (Exception ex) {
                        Console.WriteLine (ex);
                    }
                };
                process.BeginOutputReadLine ();
                process.ErrorDataReceived += (sender, e) => {
                    try {
                        if (e?.Data == null)
                            return;
                        OnProcessOutput (new OperationOutput (OutputSource.Error, e.Data));
                    } catch (Exception ex) {
                        Console.WriteLine (ex);
                    }
                };
                process.BeginErrorReadLine ();
            }
			process.Exited += delegate {
				OnExited ();
				_hasExited = true;
			};
		}

		protected readonly object _syncpoint = new object ();
		protected bool _started;
		private bool _hasExited;


		public override bool WaitForExit (TimeSpan timeout)
		{
			if (!Volatile.Read (ref _started))
				throw new InvalidOperationException ("Cannot wait for a process to exit which has not yet started.");
			// If the process has already exited, just return success
			if (Volatile.Read (ref _hasExited))
				return true;

            lock (_syncpoint) {
                return process.WaitForExit(timeout.Milliseconds);
            }
		}

		public override void WaitForExit ()
		{
			lock (_syncpoint) {
				process.WaitForExit ();
        }
    }

		protected override void Dispose (bool disposing)
		{
			lock (_syncpoint) {
				if (disposing) {
					process?.Dispose ();
					process = null;
				}
			}
			base.Dispose (disposing);
		}
	}
}
