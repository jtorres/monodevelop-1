/**** Git Process Management Library ****
 *
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the ""Software""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    public interface IExecutionContext
    {
        /// <summary>
        /// Gets or sets the file system access interface.
        /// </summary>
        IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the active set of Git related options.
        /// </summary>
        GitOptions Git { get; }

        /// <summary>
        /// Gets the current instance of<see cref="Tracer"/>.
        /// </summary>
        ITracer Tracer { get; }

        /// <summary>
        /// Gets or sets the delegate called to terminate instance of <see cref="IProcess"/>.
        /// </summary>
        TerminateProcessDelegate TerminateProcessCallback { get; set; }

        /// <summary>
        /// Gets the version of the library.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Gets an instance of <see cref="IWhere"/> from the context.
        /// </summary>
        IWhere Where { get; }

        /// <summary>
        /// Gets or sets the broker used to control remoting for the context.
        /// </summary>
        IBroker Broker { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="Environment"/> from the current
        /// <see cref="ExecutionContext"/> with <paramref name="workingDirectory"/> as the environment's
        /// working directory.
        /// </summary>
        /// <param name="workingDirectory">The working directory for the new instance of <see cref="Environment"/>.</param>
        Environment EnvironmentCreate(string workingDirectory);

        /// <summary>
        /// Creates a new instance of <see cref="Environment"/> from the current
        /// <see cref="ExecutionContext"/> with the current working directory as the environment's working directory.
        /// </summary>
        Environment EnvironmentCreate();
    }

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay, nq}")]
    public sealed class ExecutionContext : IExecutionContext
    {
        internal ExecutionContext(int managedThreadId)
        {
            _contextId = Interlocked.Increment(ref _uniqueIdCounter);
            _createProcessCallback = CreateProcessImpl;
            _createEnvironmentCallback = CreateEnvironmentImpl;
            _fileSystem = new FileSystem(this);
            _gitOptions = new GitOptions(this);
            _parseHelper = new ParseHelper(this);
            _pathHelper = new PathHelper(this);
            _terminateProcessCallback = Command.DefaultTerminateProcessCallback;
            _threadId = managedThreadId;
            _tracer = new Tracer(this);
            _where = new Where(this);
            _broker = SynchronousBroker.Instance;
        }

        internal ExecutionContext()
            : this(Thread.CurrentThread.ManagedThreadId)
        { }

        internal ExecutionContext(IExecutionContext copy)
            : this()
        {
            _fileSystem = copy.FileSystem;
            _gitOptions = copy.Git;
            _terminateProcessCallback = copy.TerminateProcessCallback;
            _tracer = copy.Tracer;
            _where = copy.Where;
        }

        static ExecutionContext()
        {
            _current = new ExecutionContext();
            _version = typeof(ExecutionContext).GetTypeInfo().Assembly?.GetName()?.Version ?? new Version();
        }

        private readonly int _contextId;
        private CreateProcessDelegate _createProcessCallback;
        private CreateEnvironmentDelegate _createEnvironmentCallback;
        private static IExecutionContext _current;
        private IFileSystem _fileSystem;
        private GitOptions _gitOptions;
        private ParseHelper _parseHelper;
        private PathHelper _pathHelper;
        private readonly int _threadId;
        private TerminateProcessDelegate _terminateProcessCallback;
        private ITracer _tracer;
        private static int _uniqueIdCounter = 0;
        private static readonly Version _version;
        private IWhere _where;
        private IBroker _broker;

        public int ContextId
        {
            get { return _contextId; }
        }

        public static IExecutionContext Current
        {
            get { return Volatile.Read(ref _current); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Current));

                Volatile.Write(ref _current, value);
            }
        }

        public IFileSystem FileSystem
        {
            get { return Volatile.Read(ref _fileSystem); }
            internal set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(FileSystem));

                Volatile.Write(ref _fileSystem, value);
            }
        }

        public GitOptions Git
        {
            get { return Volatile.Read(ref _gitOptions); }
            internal set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Git));

                Volatile.Write(ref _gitOptions, value);
            }
        }

        public TerminateProcessDelegate TerminateProcessCallback
        {
            get { return Volatile.Read(ref _terminateProcessCallback); }
            set { Volatile.Write(ref _terminateProcessCallback, value); }
        }

        public ITracer Tracer
        {
            get { return Volatile.Read(ref _tracer); }
            internal set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Tracer));

                Volatile.Write(ref _tracer, value);
            }
        }

        public Version Version
        {
            get { return _version; }
        }

        public IWhere Where
        {
            get { return Volatile.Read(ref _where); }
            internal set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Where));

                Volatile.Write(ref _where, value);
            }
        }

        public IBroker Broker
        {
            get { return Volatile.Read(ref _broker); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Broker));

                Volatile.Write(ref _broker, value);
            }
        }

        internal CreateApplicationNameDelegate CreateApplicationNameCallback
        {
            get { return Git.GetApplicationName; }
        }

        internal CreateEnvironmentDelegate CreateEnvironmentCallback
        {
            get { return Volatile.Read(ref _createEnvironmentCallback); }
            set { Volatile.Write(ref _createEnvironmentCallback, value); }
        }

        internal CreateProcessDelegate CreateProcessCallback
        {
            get { return Volatile.Read(ref _createProcessCallback); }
            set { Volatile.Write(ref _createProcessCallback, value); }
        }

        internal CreateProcessCommandLineDelegate CreateProcessCommandLineCallback
        {
            get { return Git.GetCommandLine; }
        }

        internal int ThreadId
        {
            get { return _threadId; }
        }

        internal string DebuggerDisplay
        {
            get { return FormattableString.Invariant($"{nameof(ExecutionContext)}: {_contextId}"); }
        }

        internal ParseHelper ParseHelper
        {
            get { return Volatile.Read(ref _parseHelper); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(ParseHelper));

                Volatile.Write(ref _parseHelper, value);
            }
        }

        internal PathHelper PathHelper
        {
            get { return Volatile.Read(ref _pathHelper); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(PathHelper));

                Volatile.Write(ref _pathHelper, value);
            }
        }

        public static IExecutionContext CreateNew()
        {
            return new ExecutionContext();
        }

        public Environment EnvironmentCreate(string workingDirectory)
        {
            return CreateEnvironmentCallback(workingDirectory);
        }

        public Environment EnvironmentCreate()
        {
            string currentWorkingDirectory = global::System.Environment.CurrentDirectory;

            var environment = EnvironmentCreate(currentWorkingDirectory);

            return environment;
        }

        internal DetachedProcess CreateProcess(object userData)
        {
            return new DetachedProcess(this, userData);
        }

        private Environment CreateEnvironmentImpl(string workingDirectory)
        {
            IDictionary systemEnvironment = global::System.Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
            var variables = new List<Environment.Variable>(systemEnvironment.Count);

            foreach (var item in systemEnvironment.Keys)
            {
                var key = (string)item;
                var value = (string)systemEnvironment[key];

                variables.Add(new Environment.Variable(key, value));
            }

            var environment = new Environment(variables, workingDirectory);

            Tracer.TraceMessage($"New environment [{environment.Variables.Count}] created.", $"{environment.WorkingDirectory}", TracerLevel.Diagnostic);

            return environment;
        }

        private static DetachedProcess CreateProcessImpl(IExecutionContext context)
        {
            return new DetachedProcess(context);
        }
    }

    internal delegate Environment CreateEnvironmentDelegate(string workingDirectory);

    internal delegate ExecutionContext CreateInstanceDelegate();

    internal delegate DetachedProcess CreateProcessDelegate(IExecutionContext context);
}
