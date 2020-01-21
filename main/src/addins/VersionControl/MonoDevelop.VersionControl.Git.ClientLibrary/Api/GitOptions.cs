using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to the management and interaction with Git.
    /// <para/>
    /// Returned from `<seealso cref="IExecutionContext"/>`.
    /// </summary>
    public class GitOptions
    {
        protected static readonly Version GitMinVersion = new Version(2, 12, 0);

        internal static readonly char[] PathSplitTokens = new[] { ';' };

        internal GitOptions(ExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _context = context;
            _askpassResolver = null;
            _configurationArguments = new ConcurrentSet<ConfigurationEntry>(ConfigurationEntry.Comparer);
            _environmentValues = new ConcurrentSet<Environment.Variable>(Environment.ValueComparer);
            _httpExtraHeaders = new ConcurrentSet<HttpExtraHeader>(HttpExtraHeader.Comparer);
            _syncpoint = new object();

            _configurationArguments.Add(new ConfigurationEntry("gc.auto", "0", ConfigurationLevel.Command, "command line"));
        }

        private string _askpassResolver;
        private ConcurrentSet<ConfigurationEntry> _configurationArguments;
        private readonly ExecutionContext _context;
        private ConcurrentSet<Environment.Variable> _environmentValues;
        private ConcurrentSet<HttpExtraHeader> _httpExtraHeaders;
        private Installation _installation;
        private readonly object _syncpoint;

        /// <summary>
        /// Gets or sets the path to a file which Git should use to resolve any credential issues.
        /// <para/>
        /// If `<see langword="null"/>` the "EnvironmentKey.GitAskpass" value will not be passed to any Git process.
        /// </summary>
        public string AskPassResolverPath
        {
            get
            {
                lock (_syncpoint)
                {
                    if (_askpassResolver == null)
                    {
                        string path;
                        if (_context.Where.FindGitAskpass(Installation, out path))
                        {
                            _askpassResolver = path;
                        }
                    }

                    return _askpassResolver;
                }
            }
            set { lock (_syncpoint) _askpassResolver = value; }
        }

        /// <summary>
        /// Gets the collection of configuration overrides to appended to any Git operation.
        /// </summary>
        public ICollection<ConfigurationEntry> ConfigurationArguments
        {
            get { return Volatile.Read(ref _configurationArguments); }
        }

        /// <summary>
        /// Gets the collection of environment variable overrides to append to any Git operation.
        /// </summary>
        public ICollection<Environment.Variable> EnvironmentValues
        {
            get { return Volatile.Read(ref _environmentValues); }
        }

        /// <summary>
        /// Gets the collection of extra HTTP headers to append to any Git network operation.
        /// </summary>
        public ICollection<HttpExtraHeader> HttpExtraHeaders
        {
            get { return Volatile.Read(ref _httpExtraHeaders); }
        }

        /// <summary>
        /// Gets or set the installation of Git to use to create the new OS process.
        /// </summary>
        /// <exception cref="ArgumentException">When `<see cref="Installation"/>` is invalid.</exception>
        public Installation Installation
        {
            get
            {
                lock (_syncpoint)
                {
                    if (_installation == null)
                    {
                        List<Installation> installations;
                        if (_context.Where.FindGitInstallations(out installations))
                        {
                            _installation = installations[0];
                        }
                    }

                    return _installation;
                }
            }
            set
            {
                if (!(value == null || Installation.IsValid(_context, value)))
                    throw new ArgumentException(nameof(Installation));

                lock (_syncpoint)
                {
                    _installation = value;
                }
            }
        }

        /// <summary>
        /// Gets `<see langword="true"/>` if Git is installed and detected on the local system; otherwise `<see langword="false"/>`.
        /// </summary>
        public bool IsGitInstalled { get { return Installation != null; } }

        /// <summary>
        /// Gets the minimum version of Git required to operate correctly with the library.
        /// </summary>
        public Version MinimumVersion
        {
            get { return GitMinVersion; }
        }

        /// <summary>
        /// Returns a read-only list of global configuration values from Git.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<ConfigurationEntry> ConfigGlobalReadList(object userData = null)
        {
            var environment = GetNonWorktreeEnvironment();

            return new Cli.ConfigCommand(_context, environment, userData).ReadConfiguration();
        }

        /// <summary>
        /// Creates or updates a global configuration.
        /// </summary>
        /// <param name="name">The name of the configuration entry.</param>
        /// <param name="value">The value to set the configuration entry to.</param>
        /// <param name="callbackData">Optional data attached to any callbacks and/or trace events related to this operation.</param>
        public void ConfigGlobalSetValue(string name, string value, object userData = null)
        {
            var environment = GetNonWorktreeEnvironment();

            new Cli.ConfigCommand(_context, environment, userData).Set(name, value, ConfigurationLevel.Global);
        }

        /// <summary>
        /// Unsets, or removes, a global configuration value.
        /// </summary>
        /// <param name="name">The name of the configuration entry.</param>
        /// <param name="callbackData">Optional data attached to any callbacks and/or trace events related to this operation.</param>
        public void ConfigGlobalUnsetValue(string name, object userData = null)
        {
            var environment = GetNonWorktreeEnvironment();

            new Cli.ConfigCommand(_context, environment, userData).Unset(name, ConfigurationLevel.Global);
        }

        /// <summary>
        /// Gets <see langword="true"/> if this context has an installation of Git assigned to itself already; otherwise <see langword="false"/>.
        /// </summary>
        public bool HasInstallation { get { return _installation != null; } }

        /// <summary>
        /// Get an instance of <see cref="Environment"/> specialized for Git process creation.
        /// </summary>
        /// <param name="workingDirectory">
        /// The directory to use as the process' current working directory.
        /// </param>
        /// <param name="ignorePathCasing">
        /// <see langword="true"/> when Git should ignore path casing; otherwise <see langword="false"/>.
        /// </param>
        public Environment GetEnvironment(string workingDirectory, bool ignorePathCasing)
            => GetProcessEnvironment(workingDirectory, ignorePathCasing);

        /// <summary>
        /// Get the name of the application for use during process creation.
        /// </summary>
        /// <param name="environment">The environment to use when determining the name.</param>
        /// <returns>Application name for use during process creation.</returns>
        internal string GetApplicationName(Environment environment)
        {
            return null;
        }

        /// <summary>
        /// Gets the command line use to create the Git process.
        /// </summary>
        /// <param name="command">Command Git is expected to execute.</param>
        /// <param name="environment">Environment to be passed to Git during process creation.</param>
        /// <returns>Full command line for use during process creation.</returns>
        internal string GetCommandLine(string command, Environment environment)
        {
            const string CommandLineFormat = @"""{0}"" {1} --no-pager {2}";

            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (Installation == null)
                throw new InvalidOperationException();

            using (var configValues = new StringBuffer())
            {
                // If the environment is a git-process environment, then use its config settings
                // instead of the global set instanced
                // `GitProcessEnvironment.ConfigurationOverrides` values contain copied entries
                // from the global set
                if (environment is GitProcessEnvironment)
                {
                    foreach (ConfigurationEntry configurationOverride in (environment as GitProcessEnvironment).ConfigurationArguments)
                    {
                        configValues.Append(" -c \"")
                                    .Append(configurationOverride.Key)
                                    .Append("=")
                                    .AppendEscaped(configurationOverride.Value, ConfigurationEntry.IllegalCharacters, ConfigurationEntry.EscapeCharacter)
                                    .Append("\"");
                    }
                }
                else
                {
                    foreach (ConfigurationEntry configurationOverride in _configurationArguments)
                    {
                        configValues.Append(" -c \"")
                                    .Append(configurationOverride.Key)
                                    .Append("=")
                                    .AppendEscaped(configurationOverride.Value, ConfigurationEntry.IllegalCharacters, ConfigurationEntry.EscapeCharacter)
                                    .Append("\"");
                    }
                }

                string configString = configValues.ToString();
                string content = string.Format(CultureInfo.InvariantCulture, CommandLineFormat, Installation.Exe, configString, command);

                return content;
            }
        }

        /// <summary>
        /// Get an instance of <see cref="Environment"/> specialized for Git process creation.
        /// </summary>
        /// <param name="baseEnvironment">
        /// Environment to base the process creation environment on.
        /// </param>
        /// <param name="workingDirectory">
        /// The directory to use as the process' current working directory.
        /// </param>
        /// <param name="ignorePathCasing">
        /// <see langword="true"/> when Git should ignore path casing; otherwise <see langword="false"/>.
        /// </param>
        internal GitProcessEnvironment GetProcessEnvironment(Environment baseEnvironment, bool ignorePathCasing)
        {
            var customVariables = new List<Environment.Variable>
                {
                    new Environment.Variable(Environment.Key.GitPager, "cat"),
                    new Environment.Variable(Environment.Key.GitTerminalPrompt, "0"),
                    new Environment.Variable(Environment.Key.GitFlush, "1"),
                    new Environment.Variable(Environment.Key.GitMergeVerbosity, "4"),
                };

            // Craft the ceiling directory value
            string ceilingDirectory = baseEnvironment.WorkingDirectory;

            // Due to API "oddness", the path cannot end with a directory separator character, if
            // it does we need to strip it before asking the API for the parent of the working directory
            while (!string.IsNullOrEmpty(ceilingDirectory)
                && (ceilingDirectory[ceilingDirectory.Length - 1] == Path.DirectorySeparatorChar
                    || ceilingDirectory[ceilingDirectory.Length - 1] == Path.AltDirectorySeparatorChar))
            {
                ceilingDirectory = ceilingDirectory.Substring(0, ceilingDirectory.Length - 1);
            }

            // Skip setting the ceiling variable if the value is null/empty.
            if (!string.IsNullOrEmpty(ceilingDirectory))
            {
                // Get the parent of the working directory as the ceiling directory
                ceilingDirectory = _context.FileSystem.GetParentPath(ceilingDirectory);

                if (!string.IsNullOrEmpty(ceilingDirectory))
                {
                    // Transform the path into a local path (if it wasn't already).
                    ceilingDirectory = _context.PathHelper.ToLocalPath(ceilingDirectory);

                    if (!string.IsNullOrEmpty(ceilingDirectory))
                    {
                        // Set the ceiling to the parent of the workdir, if the workdir is device root
                        // then it'll be empty which is acceptable. If the value is null/empty, there's
                        // no reason to set the variable, so it will be omitted.
                        var ceilingVariable = new Environment.Variable(Environment.Key.GitCeilingDirectories, ceilingDirectory);

                        customVariables.Add(ceilingVariable);
                    }
                }
            }

            // Add all of the override variables
            foreach (Environment.Variable variable in EnvironmentValues)
            {
                for (int i = 0; i < customVariables.Count; i += 1)
                {
                    // do not add the same variable twice
                    if (variable != customVariables[i])
                    {
                        customVariables.Add(variable);
                    }
                }
            }

            Installation installation = Installation;

            if (installation == null)
                throw new InstallationNotFoundException(nameof(Installation), new NullReferenceException(nameof(Installation)));

            if (!Installation.IsValid(_context, installation))
                throw new InstallationInvalidException($"{nameof(Installation)}[\"{installation?.Path}\"] is invalid.");

            /* If the base environment hasn't already specified Git and SSH askpass resolvers, 
               we should check to see if we can find one and specify it as necissary. */

            bool hasGitAskpass = baseEnvironment.Contains(Environment.Key.GitAskpass);
            bool hasSshAskpass = baseEnvironment.Contains(Environment.Key.SshAskpass);

            if (!hasGitAskpass || !hasSshAskpass)
            {
                // Query the `AskPassResolverPath` property for the value.
                // This will either read a cached value, or probe the local disk to populate the cache
                // before returning the value. It'll returns `null` if no askpass resolver was discovered.
                string askpassResolver = AskPassResolverPath;

                if (askpassResolver != null)
                {
                    // Since we have the path to an askpass resolver, add them for Git and/or SSH if the process hasn't already added them.
                    if (!hasGitAskpass)
                    {
                        customVariables.Add(new Environment.Variable(Environment.Key.GitAskpass, askpassResolver));
                    }
                    if (!hasSshAskpass)
                    {
                        customVariables.Add(new Environment.Variable(Environment.Key.SshAskpass, askpassResolver));
                    }
                }
            }

            if (ignorePathCasing)
            {
                customVariables.Add(new Environment.Variable(Environment.Key.GitInsensitivePathspecs, "1"));
            }

            string mingw = installation.Is64Bit
                ? "MINGW64"
                : "MINGW32";

            //customVariables.Add(new Environment.Variable(Environment.Key.GitExecPath, installation.Bin));
            customVariables.Add(new Environment.Variable(Environment.Key.MsystemPath, mingw));
            //customVariables.Add(new Environment.Variable(Environment.Key.LocaleCategoryAll, "UTF-8"));

            if (!baseEnvironment.Contains(Environment.Key.CharSet))
            {
                // Add CHARSET environment variable to avoid libcurl attempting to do so (and failing).
                customVariables.Add(new Environment.Variable(Environment.Key.CharSet, "cp1252"));
            }

            if (!baseEnvironment.Contains(Environment.Key.Plink))
            {
                customVariables.Add(new Environment.Variable(Environment.Key.Plink, "ssh"));
            }

            string homePath;
            if (!_context.Where.FindHomePath(baseEnvironment, out homePath))
            {
                homePath = baseEnvironment.GetEnvironmentVariable(Environment.Key.WindowsUserProfile);
            }

            customVariables.Add(new Environment.Variable(Environment.Key.Home, homePath));

            using (var pathBuffer = new StringBuffer())
            {
                Environment.Variable pathVar;
                if (baseEnvironment.TryGet(Environment.Key.Path, out pathVar))
                {
                    if (pathVar.Name == null || pathVar.Value == null)
                        throw new InvalidOperationException(FormattableString.Invariant($"Critical environment variable: %{Environment.Key.Path}% not found in environment."));

                    string[] parts = pathVar.Value.Split(PathSplitTokens, StringSplitOptions.RemoveEmptyEntries);
                    var dedupe = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    for (int i = 0; i < parts.Length; i += 1)
                    {
                        if (dedupe.Add(parts[i]))
                        {
                            pathBuffer.Append(parts[i])
                                      .Append(';');
                        }
                    }
                }

                // Prepend the user's ~/bin folder, if it exists, to $path
                string homeBin = Path.Combine(homePath, "bin");
                if (_context.FileSystem.DirectoryExists(homeBin))
                {
                    pathBuffer.Prepend(Environment.PathSegmentDelimiter)
                              .Prepend(homeBin);
                }

                // Prepend the installation's $git/usr/bin to $path
                pathBuffer.Prepend(Environment.PathSegmentDelimiter)
                          .Prepend(installation.UsrBin);

                // Prepend the installation's $git/mingw__/libexec/git-core to $path
                pathBuffer.Prepend(Environment.PathSegmentDelimiter)
                          .Prepend(installation.Libexec);

                // Prepend the installation's $git/mingw__/bin to $path
                pathBuffer.Prepend(Environment.PathSegmentDelimiter)
                          .Prepend(installation.Bin);

                // Given that the operating system has a maximum length to $path, we'll trim
                // it down until it fits within the limit by truncating whole portions at the
                // value delimiter.
                if (pathBuffer.Length > Environment.MaximumPathVariableLength)
                {
                    var originalPath = pathBuffer.ToString();

                    while (pathBuffer.Length > Environment.MaximumPathVariableLength)
                    {
                        int idx = pathBuffer.LastIndexOf(Environment.PathSegmentDelimiter, 1, pathBuffer.Length - 1);
                        if (idx < 0)
                            break;

                        pathBuffer.Remove(idx, pathBuffer.Length - idx);
                    }

                    // Do not leave a trailing value delimiter on the value.
                    if (pathBuffer[pathBuffer.Length - 1] == Environment.PathSegmentDelimiter)
                    {
                        pathBuffer.Remove(pathBuffer.Length - 1, 1);
                    }

                    var finalPath = pathBuffer.ToString();
                    var truncatedPath = originalPath.Substring(finalPath.Length);

                    // Emit a tracer event to assist with debugging.
                    _context.Tracer.TraceMessage($"'%PATH%' value truncated from {originalPath.Length} to {finalPath.Length} characters; removing \"{truncatedPath}\".");
                }

                customVariables.Add(new Environment.Variable(Environment.Key.Path, pathBuffer.ToString()));
            }

            // Scan the environment variables for GIT_TRACE settings, and disable any
            foreach (Environment.Variable variable in baseEnvironment)
            {
                if (variable.Name.StartsWith("GIT_TRACE", StringComparison.OrdinalIgnoreCase))
                {
                    // Values of "0" and "false" disable trace, so they can be safely ignored
                    if (variable.Value != null
                        && !variable.Value.Equals("0", StringComparison.Ordinal)
                        && !variable.Value.Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        bool keep = false;

                        // Really the only legal value remain is an absolute path or "true", but
                        // we test anyways
                        if (!variable.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                // Absolute paths write to files on disk and won't impact parsing
                                // `IsPathRooted` throws if the value contains invalid chars, so
                                // protect against that
                                keep = Path.IsPathRooted(variable.Value);
                            }
                            catch { /* squelch */ }
                        }

                        if (!keep)
                        {
                            customVariables.Add(new Environment.Variable(variable.Name, "0"));
                        }
                    }
                }
            }

            Environment environment = baseEnvironment.CreateWith(customVariables);

            var gitProcessEnvironment = new GitProcessEnvironment(environment, _context.Git.ConfigurationArguments);

            return gitProcessEnvironment;
        }

        /// <summary>
        /// Get an instance of <see cref="Environment"/> specialized for Git process creation.
        /// </summary>
        /// <param name="baseEnvironment">
        /// Environment to base the process creation environment on.
        /// </param>
        /// <param name="workingDirectory">
        /// The directory to use as the process' current working directory.
        /// </param>
        /// <param name="ignorePathCasing">
        /// <see langword="true"/> when Git should ignore path casing; otherwise <see langword="false"/>.
        /// </param>
        internal GitProcessEnvironment GetProcessEnvironment(Environment baseEnvironment, string workingDirectory, bool ignorePathCasing)
            => GetProcessEnvironment(new Environment(baseEnvironment.Variables, workingDirectory), ignorePathCasing);

        /// <summary>
        /// Get an instance of <see cref="Environment"/> specialized for Git process creation.
        /// </summary>
        /// <param name="workingDirectory">
        /// The directory to use as the process' current working directory.
        /// </param>
        /// <param name="ignorePathCasing">
        /// <see langword="true"/> when Git should ignore path casing; otherwise <see langword="false"/>.
        /// </param>
        internal GitProcessEnvironment GetProcessEnvironment(string workingDirectory, bool ignorePathCasing)
            => GetProcessEnvironment(_context.EnvironmentCreate(workingDirectory), ignorePathCasing);

        /// <summary>
        /// Get an instance of <see cref="Environment"/> specialized for Git process creation.
        /// </summary>
        /// <param name="repository">The repository (not bare) the process will operate on.</param>
        internal GitProcessEnvironment GetProcessEnvironment(IRepository repository)
            => GetProcessEnvironment(_context.EnvironmentCreate(repository?.WorkingDirectory), false);

        /// <summary>
        /// Get an instance of <see cref="Environment"/> specialized for Git process creation.
        /// </summary>
        /// <param name="workingDirectory">
        /// The directory to use as the process' current working directory.
        /// </param>
        internal GitProcessEnvironment GetProcessEnvironment(string workingDirectory)
            => GetProcessEnvironment(_context.EnvironmentCreate(workingDirectory), false);

        /// <summary>
        /// Get an instance of <see cref="Environment"/> specialized for Git process creation.
        /// </summary>
        /// <param name="baseEnvironment">
        /// Environment to base the process creation environment on.
        /// </param>
        /// <param name="workingDirectory">
        /// The directory to use as the process' current working directory.
        /// </param>
        internal Environment GetProcessEnvironment(Environment baseEnvironment, string workingDirectory)
            => GetProcessEnvironment(new Environment(baseEnvironment.Variables, workingDirectory), false);

        private GitProcessEnvironment GetNonWorktreeEnvironment(string workingDirectory = null)
        {
            // We need to run the query for global configuration values in a non-repository;
            // using the system's temp folder seems reasonable, as nobody should place the temp
            // folder under version control.
            string tempPath = workingDirectory ?? _context.FileSystem.GetTempDirectoryPath();

            // Reading of global configuration values doesn't need the worktree value, in fact
            // setting can cause a whole host of unanticipated adverse effects. Therefore, we
            // craft an environment with cwd = system's temp dir and ignores path cases
            GitProcessEnvironment environment = GetProcessEnvironment(_context.EnvironmentCreate(tempPath),
                                                                      ignorePathCasing: true);
            return environment;
        }
    }
}
