//*************************************************************************************************
// Progress.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal interface IOperation
    {
        /// <summary>
        /// Gets an exception trapped during the operation, if any.
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        /// Cancel the current running operation and stop the Git process.
        /// </summary>
        void Cancel();

        void Start(IProcess processes);

        /// <summary>
        /// Stop / tear down the Git process. This is meant to be called to
        /// make the process is cleaned up.
        /// </summary>
        void Stop();
    }

    [GitErrorMapping(typeof(GitFatalException), Prefix = Operation.PrefixFatal)]
    [GitErrorMapping(typeof(GitUsageException), Prefix = Operation.PrefixUsage)]
    internal abstract class Operation : Base, IOperation
    {
        internal const string ErrorUnstagedChanges = "have unstaged changes.";
        internal const string ErrorUncommitedChanges = "index contains uncommitted changes.";

        internal const string PrefixBuildingBitmaps = "Building bitmaps";
        internal const string PrefixCheckingConnectivity = "Checking connectivity";
        internal const string PrefixCoutingObjects = "Counting objects";
        internal const string PrefixError = "error: ";
        internal const string PrefixFatal = "fatal: ";
        internal const string PrefixIndexingObjects = "Indexing objects";
        internal const string PrefixRemovingDuplicateObjects = "Removing duplicate objects";
        internal const string PrefixUnpackingObjects = "Unpacking objects";
        internal const string PrefixUsage = "usage: ";
        internal const string PrefixVerifyingBitmapEntries = "Verifying bitmap entries";
        internal const string PrefixWarning = "warning: ";

        // matches strings like: Submodule path 'LibGit2Sharp/LibGit2Sharp': checked out '6a6bf2368ab886de9ae34d4b913ac04d6ff91bd6'
        protected const string PatternSubmoduleCheckout = @"^\s*Submodule\s+path\s+'([^']+)'\:\s+checked\s+out\s+'([^']+)'";
        // matches strings like: Submodule 'LibGit2Sharp/LibGit2Sharp' (https://mseng.visualstudio.com/DefaultCollection/LibGit2/_git/LibGit2Sharp) registered for path 'LibGit2Sharp/LibGit2Sharp'
        protected const string PatternSubmoduleRegistered = @"^\s*Submodule\s+'([^']+)'\s+\(([^\)]+)\)\s+registered\s+for\s+path\s+'([^']+)'";

        protected static readonly char[] NewLineCharacters = System.Environment.NewLine.ToCharArray();

        class NullOperationProgressCallback : OperationCallback
        {
            public static readonly NullOperationProgressCallback Instance = new NullOperationProgressCallback ();

            public override bool OperationProgress (OperationProgress operationProgress)
            {
                // do nothing
                return true;
            }

            public override void OperationOutput (OperationOutput output)
            {
            }

            NullOperationProgressCallback ()
            {
            }
        }

        internal Operation (ExecutionContext context, OperationCallback progressCallback)
            : base()
        {
            SetContext(context);

            _progressCallback = progressCallback ?? NullOperationProgressCallback.Instance;
            _syncpoint = new object();
        }


        /// <summary>
        /// Gets the <see cref="System.Exception"/> trapped, if any.
        /// </summary>
        public Exception Exception
        {
            get { lock (_syncpoint) return _exception; }
        }

        private Exception _exception;
        private readonly OperationCallback _progressCallback;
        private IProcess _process;
        private readonly object _syncpoint;

        /// <summary>
        /// Cancels the current operation.
        /// </summary>
        public void Cancel()
        {
            Fault(new System.OperationCanceledException());
        }

        public void Stop()
        {
            try { _process?.Exit(); } catch { /* squelch */ }

            lock (_syncpoint)
            {
                Monitor.PulseAll(_syncpoint);
            }
        }

        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Invokes any registered callbacks.
        /// </summary>
        /// <param name="progress">The value to pass the callbacks.</param>
        public void NotifyCallbacks(OperationProgress progress)
        {
            if (ReferenceEquals(progress, null))
                return;
            if (ReferenceEquals(_progressCallback, null))
                return;

            var callback = _progressCallback;

            try
            {
                // if the callback or error signaled to cancel the current operation, cancel the operation.
                if (!callback.OperationProgress(progress))
                {
                    Cancel();
                }
            }
            catch (Exception exception)
            {
                Fault(exception);
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates two new <see cref="Task"/> intsances to consume the output from
        /// <see cref="IProcess.StdOut"/> via <see cref="ParseStdOut"/> and
        /// <see cref="IProcess.StdErr"/> via <see cref="ParseStdErr"/>.
        /// </summary>
        /// <param name="process"></param>
        public void Start(IProcess process)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            _process = process;
            _process.Exited += ProcessExited;
            _process.ProcessOutput += OnProcessOutput;
            // do not bother parsing output if nobody will be listening, however this is blocking call
            // therefore we wait until the operation completes before giving the thread back to the caller
            if (ReferenceEquals(_progressCallback, null))
            {
                _process.WaitForExit();
                return;
            }

            process.WaitForExit ();

            // the tasks are basically try-catch blocks, so no need to try-catch around the wait-all

            // if an exception has been marshaled, now is the time to throw it
            if (!ReferenceEquals(_exception, null))
                throw _exception;

            _progressCallback.OperationOutput(OperationOutput.OutputClosed);
        }

        void OnProcessOutput (object sender, OperationOutput e)
        {
            if (ParseOutput (e)) {
            } else {
                _progressCallback.OperationOutput(e);
            }
        }

        protected static string CleanLine(string line)
        {
            if (line == null)
                return null;

            line = line.Trim(NewLineCharacters);

            if (line.StartsWith(PrefixError, StringComparison.Ordinal))
            {
                line = line.Remove(0, PrefixError.Length);
            }
            else if (line.StartsWith(PrefixFatal, StringComparison.Ordinal))
            {
                line = line.Remove(0, PrefixFatal.Length);
            }
            else if (line.StartsWith(PrefixUsage, StringComparison.Ordinal))
            {
                line = line.Remove(0, PrefixUsage.Length);
            }
            else if (line.StartsWith(PrefixWarning, StringComparison.Ordinal))
            {
                line = line.Remove(0, PrefixWarning.Length);
            }
            else if (line.StartsWith(PrefixBuildingBitmaps, StringComparison.Ordinal)
                  || line.StartsWith(PrefixCheckingConnectivity, StringComparison.Ordinal)
                  || line.StartsWith(PrefixCoutingObjects, StringComparison.Ordinal)
                  || line.StartsWith(PrefixIndexingObjects, StringComparison.Ordinal)
                  || line.StartsWith(PrefixRemovingDuplicateObjects, StringComparison.Ordinal)
                  || line.StartsWith(PrefixUnpackingObjects, StringComparison.Ordinal)
                  || line.StartsWith(PrefixVerifyingBitmapEntries, StringComparison.Ordinal))
            {
                // Do not display roll-up lines which include messaging noise
                line = null;
            }

            return line;
        }

        GitErrorMappingAttributeBase errorMappingAttribute;
        StringBuilder fatalMessage = new StringBuilder();
        /// <summary>
        /// Determines whether a progress message is reporting a fatal error. If it is, reads the remainder of the error message
        /// and creates an appropriate type of exception to describe the error, passes that to the Fault function.
        /// </summary>
        /// <param name="line">The line of progress output from git to examine</param>
        /// <param name="reader">Reader attached to the stdio stream from which the progress line was read</param>
        /// <returns>true if the message is reporting a fatal error, else false.</returns>
        protected bool IsMessageFatal(string line)
        {
            foreach (GitErrorMappingAttributeBase attr in GitErrorMappingAttributeBase.GetMappings(this.GetType()))
            {
                if (attr.IsMatchingError(line))
                {
                    line = CleanLine(line);
                    if (line != null)
                    {
                        fatalMessage.AppendLine(line);
                    }
                    this.errorMappingAttribute = attr;
                    return true;
                }
            }

            return false;
        }

        protected void ReadFault(OperationOutput output)
        {
            if (output == OperationOutput.OutputClosed)
            {
                // We don't report the error as progress. The caller can catch the exception and log as it sees fit.
                Fault(errorMappingAttribute.CreateException(new ExecuteResult (Cli.GitCommand.GitFatalExitCode, fatalMessage.ToString ())));
                return;
            }
            var line = CleanLine(output.Message);
            if (line != null)
            {
                fatalMessage.AppendLine(line);
            }
        }

        protected abstract bool ParseOutput(OperationOutput output);

        protected static string ReadToEnd(string line, StreamReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            // Capture all of the remaining output from Git
            using (var buffer = new StringBuffer())
            {
                line = CleanLine(line);

                if (line != null)
                {
                    buffer.Append(line);
                    buffer.AppendLine();
                }

                while ((line = reader.ReadLine()) != null)
                {
                    line = CleanLine(line);

                    if (line == null)
                        continue;

                    buffer.Append(line);
                    buffer.AppendLine();
                }

                return buffer.ToString();
            }
        }

        protected static bool TryParse(string line, OperationParser[] parsers, out OperationProgress progress)
        {
            for (int i = 0; i < parsers.Length; i += 1)
            {
                if (parsers[i].TryParse(line, out progress))
                    return true;
            }

            progress = null;
            return false;
        }

        protected void Update(OperationProgress progress)
        {
            if (ReferenceEquals(progress, null))
                throw new ArgumentNullException(nameof(progress));
            this._progressCallback.OperationProgress(progress);
        }

        private void Fault(System.Exception exception)
        {
            bool callStop = false;

            if (ReferenceEquals(exception, null))
                throw new ArgumentNullException(nameof(exception));

            // acquire the syncpoint, record the exception, set `_iteration` into error state
            lock (_syncpoint)
            {
                // Only set the exception 1 time. If another exception comes in,
                // assume the 1st one is the original exception that faulted
                // and ignore subsequent faults.
                if (_exception == null)
                {
                    _exception = exception;

                    // notify all waiters of the update
                    Monitor.PulseAll(_syncpoint);

                    callStop = true;
                }
            }

            // Stop the processes if this was the first fault.
            // If this is not the first fault, then the previous / original
            // fault will have stopped the Git processes.
            if (callStop)
            {
                Stop();
            }
        }

        private void ParseOutput(Stream readableStream, Action<Stream> parser)
        {
            if (ReferenceEquals(readableStream, null))
                throw new ArgumentNullException(nameof(readableStream));
            if (ReferenceEquals(parser, null))
                throw new ArgumentNullException(nameof(parser));
            if (ReferenceEquals(_process, null))
                throw new NullReferenceException(nameof(IProcess));
            if (!readableStream.CanRead)
                throw new ArgumentException(nameof(readableStream));

            try
            {
                parser(readableStream);
            }
            catch (ObjectDisposedException)
            { /* squelch */ }

            // notify all listening threads that state has change (the process has exited)
            lock (_syncpoint)
            {
                Monitor.PulseAll(_syncpoint);
            }
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            lock (_syncpoint)
            {
                Monitor.PulseAll(_syncpoint);
            }
        }
    }
}
