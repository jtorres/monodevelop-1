//*************************************************************************************************
// GitProcessEnvironment.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal class GitProcessEnvironment : Environment
    {
        public GitProcessEnvironment(Environment environment, ICollection<ConfigurationEntry> configurationArguments)
            : base(environment?.Variables ?? Array.Empty<Variable>(), environment?.WorkingDirectory ?? string.Empty)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            _configurationArguments = new ConcurrentSet<ConfigurationEntry>(configurationArguments, ConfigurationEntry.Comparer);
            _processNamespace = Guid.NewGuid().ToString("N");

            // Generate the standard pipe names
            _stdinPipeName = DetachedProcess.GetStandardPipeName(PipeKind.StandardInput, _processNamespace);
            _stdoutPipeName = DetachedProcess.GetStandardPipeName(PipeKind.StandardOutput, _processNamespace);
            _stderrPipeName = DetachedProcess.GetStandardPipeName(PipeKind.StandardError, _processNamespace);

            // Build an environment variable list
            var variables = new List<Variable>(_variables)
            {
                new Variable(Key.ProcessNamespace, _processNamespace),
                new Variable(Key.GitRedirectStdIn, _stdinPipeName),
                new Variable(Key.GitRedirectStdOut, _stdoutPipeName),
                new Variable(Key.GitRedirectStdErr, _stderrPipeName),
            };

            // Unroll the environment wrapper if necessary
            if (environment is GitProcessEnvironment)
            {
                foreach (var arg in (environment as GitProcessEnvironment).ConfigurationArguments)
                {
                    _configurationArguments.Add(arg);
                }
            }

            // Append the list to the passed in environment, since environments are immutable the
            // original owner is safe from this change. Additionally, appending and existing
            // key/value pair overwrites the existing key/value pair if one exists.
            _variables = DeduplicateVariables(variables);
        }

        internal GitProcessEnvironment(GitProcessEnvironment environment)
            : base(environment?.Variables ?? Array.Empty<Variable>(), environment?.WorkingDirectory ?? string.Empty)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            _configurationArguments = new ConcurrentSet<ConfigurationEntry>(environment._configurationArguments, ConfigurationEntry.Comparer);
            _processNamespace = Guid.NewGuid().ToString("N");

            // Generate the standard pipe names
            _stdinPipeName = DetachedProcess.GetStandardPipeName(PipeKind.StandardInput, _processNamespace);
            _stdoutPipeName = DetachedProcess.GetStandardPipeName(PipeKind.StandardOutput, _processNamespace);
            _stderrPipeName = DetachedProcess.GetStandardPipeName(PipeKind.StandardError, _processNamespace);

            // Build an environment variable list
            var variables = new List<Variable>(_variables)
            {
                new Variable(Key.ProcessNamespace, _processNamespace),
                new Variable(Key.GitRedirectStdIn, _stdinPipeName),
                new Variable(Key.GitRedirectStdOut, _stdoutPipeName),
                new Variable(Key.GitRedirectStdErr, _stderrPipeName),
            };

            // Append the list to the passed in environment, since environments are immutable the
            // original owner is safe from this change. Additionally, appending and existing
            // key/value pair overwrites the existing key/value pair if one exists.
            _variables = DeduplicateVariables(variables);
        }

        private readonly ICollection<ConfigurationEntry> _configurationArguments;
        private readonly string _processNamespace;
        private readonly string _stderrPipeName;
        private readonly string _stdinPipeName;
        private readonly string _stdoutPipeName;

        public string ProcessNamespace
        {
            get { return _processNamespace; }
        }

        public string StdErrPipeName
        {
            get { return _stderrPipeName; }
        }

        public string StdInPipeName
        {
            get { return _stdinPipeName; }
        }

        public string StdOutPipeName
        {
            get { return _stdoutPipeName; }
        }

        internal ICollection<ConfigurationEntry> ConfigurationArguments
        {
            get { return _configurationArguments; }
        }

        public string GetPipeName(PipeKind pipeKind)
        {
            switch (pipeKind)
            {
                case PipeKind.StandardInput:
                    return StdInPipeName;

                case PipeKind.StandardOutput:
                    return StdOutPipeName;

                case PipeKind.StandardError:
                    return StdErrPipeName;
            }

            throw new ArgumentException(nameof(pipeKind));
        }

    }
}
