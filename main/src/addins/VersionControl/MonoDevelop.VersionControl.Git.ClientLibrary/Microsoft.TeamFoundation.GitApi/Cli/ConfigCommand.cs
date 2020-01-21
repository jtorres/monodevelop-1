//*************************************************************************************************
// Config.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class ConfigCommand : GitCommand
    {
        public const string Command = "config";

        public ConfigCommand(ExecutionContext context, Environment environment, object userData)
            : base(context, environment, userData)
        {
            _where = Where;
        }

        public ConfigCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        {
            _where = Where;
        }
        public Configuration ReadConfig() => Configuration.Create(ReadConfigList());

        public IReadOnlyList<ConfigurationEntry> ReadConfigList()
        {
            var cachedConfigPaths = CacheConfigPaths();

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--show-origin");
                command.AddOption("--list");
                command.AddOption("-z");

                List<ConfigurationEntry> config = new List<ConfigurationEntry>();
                ConfigurationLevel currentLevel = ConfigurationLevel.Portable;

                try
                {
                    using (var buffer = new ByteBuffer())
                    using (IProcess process = CreateProcess(command))
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        var stderrReadTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                        int get = 0;
                        int read = 0;

                        int r;
                        while ((read < buffer.Length) && (r = process.StdOut.Read(buffer, read, buffer.Length - read)) != 0)
                        {
                            read += r;
                            while (get < read)
                            {
                                // config -z EBNF: source = utf8; key = utf8; value = utf8; record =
                                // source'\0'key'\n'value'\0' config = {record};
                                int recordEnd = buffer.SecondIndexOf('\0', get, read - get);
                                if (recordEnd < 0)
                                {
                                    // If the null wasn't found in what has been read, grab more.
                                    // [DUMB ASSUMPTION] No one will put a config key/value pair
                                    // greater than 256 KiB.
                                    break; // inner read
                                }

                                // Find where to break for scope and key.
                                int scopeEnd = buffer.FirstIndexOf('\0', get, recordEnd - get);
                                if (scopeEnd < 0)
                                    throw new ConfigParseException("scope", new StringUtf8(buffer, 0, read), get);

                                int keyEnd = buffer.FirstIndexOf('\n', scopeEnd, recordEnd - scopeEnd + 1);
                                if (keyEnd < 0)
                                    throw new ConfigParseException("key", new StringUtf8(buffer, 0, read), scopeEnd);

                                // Grab the tokens.
                                var source = new StringUtf8(buffer, get, scopeEnd - get);
                                var key = new StringUtf8(buffer, scopeEnd + 1, keyEnd - scopeEnd - 1);
                                var value = new StringUtf8(buffer, keyEnd + 1, recordEnd - keyEnd - 1);

                                // Convert the path to String
                                string source0 = (string)source;
                                string source1 = NormalizeSource(source0);

                                if (!source0.Equals(source1, StringComparison.Ordinal))
                                {
                                    source = (StringUtf8)source1;
                                }

                                // Git reports configuration data from lowest to highest priority.
                                // This means that we can assume that any output is in the config
                                // level of the last known level. Test our list of cached paths to
                                // see if a new level has been reached.
                                if (cachedConfigPaths.TryGetValue(source, out ConfigurationLevel level)
                                    && currentLevel != level)
                                {
                                    currentLevel = level;
                                }

                                var configEntry = new ConfigurationEntry(key.ToString(), value.ToString(), currentLevel, source.ToString());

                                config.Add(configEntry);

                                get = recordEnd + 1;
                            }

                            MakeSpace(buffer, ref get, ref read);
                        }

                        // Ensure the output was fully consumed and there wasn't a parse problem on
                        // the last line.
                        if (get < read)
                            throw new ConfigParseException("lastLine", new StringUtf8(buffer, 0, read), get);

                        TestExitCode(process, command, stderrReadTask);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(ConfigCommand)}.{nameof(ReadConfigList)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }

                return config;
            }
        }

        private IWhere _where;

        /// <summary>
        /// Set a config value.
        /// </summary>
        public void Set(string name, string value, ConfigurationLevel level)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            // Build up the command buffer
            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, level);

                command.Add(name);
                command.Add(value);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        string standardError;
                        string standardOutput;

                        int exitCode = Execute(command, out standardError, out standardOutput);

                        TestExitCode(exitCode, Command, standardError);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(ConfigCommand)}.{nameof(Set)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        /// <summary>
        /// Unset a config value.
        /// </summary>
        public void Unset(string name, ConfigurationLevel level)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            // Build up the command buffer
            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, level);

                command.AddOption("--unset");
                command.Add(name);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        string standardError;
                        string standardOutput;

                        int exitCode = Execute(command, out standardError, out standardOutput);

                        TestExitCode(exitCode, Command, standardError);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(ConfigCommand)}.{nameof(Unset)}", exception, command))
                {
                    // should not be reached, but we'll re-throw just-in-case
                    throw new GitException(command, exception);
                }
                catch (Exception exception) when (!(exception is GitException))
                {
                    throw new GitException(command, exception);
                }
            }
        }

        /// <summary>
        /// Apply command options.
        /// </summary>
        private void ApplyOptions(ArgumentList command, ConfigurationLevel level)
        {
            Debug.Assert(command != null, $"The `{nameof(command)}` parameter is null");
            Debug.Assert(Enum.IsDefined(typeof(ConfigurationLevel), level), $"The `{nameof(level)}` is not defined.");

            switch (level)
            {
                case ConfigurationLevel.Global:
                    command.AddOption("--global");
                    break;

                case ConfigurationLevel.Local:
                    command.AddOption("--local");
                    break;

                case ConfigurationLevel.System:
                    command.AddOption("--system");
                    break;

                case ConfigurationLevel.Portable:
                    string portableConfigPath;
                    if (_where.GitPortableConfig(out portableConfigPath))
                    {
                        command.AddOption("--file");
                        command.Add(portableConfigPath);

                        Tracer.TraceMessage($"found {level}", $"\"{portableConfigPath}\"", userData: _userData);
                        break;
                    }
                    throw new ConfigurationException($"Portable config file not found.");

                case ConfigurationLevel.Xdg:
                    string xdgConfigPath;
                    if (_where.GitXdgConfig(out xdgConfigPath))
                    {
                        command.AddOption("--file");
                        command.Add(xdgConfigPath);

                        Tracer.TraceMessage($"found {ConfigurationLevel.Xdg}", $"\"{xdgConfigPath}\"", userData: _userData);
                        break;
                    }
                    throw new ConfigurationException($"{ConfigurationLevel.Xdg} config file not found.");

                default:
                    throw new ConfigurationException($"Config level '{level}' is not supported when setting values.");
            }
        }

        internal string NormalizeSource(string source)
        {
            // Ignore non-file config options.
            // e.g., "command line:", "blob:", "standard input:"
            const string FilePrefix = "file:";

            if (!source.StartsWith(FilePrefix, StringComparison.OrdinalIgnoreCase))
                return source;

            // if we failed to find the key, check if it is a relative path and we can find a match
            string localPath = source.Substring(FilePrefix.Length);

            if (!Path.IsPathRooted(localPath))
            {
                localPath = Path.Combine(_repository.WorkingDirectory, localPath);
            }

            localPath = FileSystem.CanonicalizePath(localPath);

            return localPath;
        }

        internal IReadOnlyDictionary<StringUtf8, ConfigurationLevel> CacheConfigPaths()
        {
            string path;
            var cachedConfigPaths = new Dictionary<StringUtf8, ConfigurationLevel>(new PathComparer() as IEqualityComparer<StringUtf8>);

            if (_where.GitGlobalConfig(out path))
            {
                path = NormalizeSource(path);

                cachedConfigPaths.Add((StringUtf8)path, ConfigurationLevel.Global);

                Tracer.TraceMessage($"Cached config {ConfigurationLevel.Global}.", $"{ConfigurationLevel.Global} found at '{path}'.", TracerLevel.Diagnostic, userData: _userData);
            }

            if (_repository != null && _where.GitLocalConfig(_repository.WorkingDirectory, out path))
            {
                path = NormalizeSource(path);

                cachedConfigPaths.Add((StringUtf8)path, ConfigurationLevel.Local);

                Tracer.TraceMessage($"Cached config {ConfigurationLevel.Local}.", $"{ConfigurationLevel.Local} found at '{path}'.", TracerLevel.Diagnostic, userData: _userData);
            }

            if (_where.GitPortableConfig(out path))
            {
                path = NormalizeSource(path);

                cachedConfigPaths.Add((StringUtf8)path, ConfigurationLevel.Portable);

                Tracer.TraceMessage($"Cached config {ConfigurationLevel.Portable}.", $"{ConfigurationLevel.Portable} found at '{path}'.", TracerLevel.Diagnostic, userData: _userData);
            }

            if (_where.GitSystemConfig(Git.Installation, out path))
            {
                path = NormalizeSource(path);

                cachedConfigPaths.Add((StringUtf8)path, ConfigurationLevel.System);

                Tracer.TraceMessage($"Cached config {ConfigurationLevel.System}.", $"{ConfigurationLevel.System} found at '{path}'.", TracerLevel.Diagnostic, userData: _userData);
            }

            if (_where.GitXdgConfig(out path))
            {
                path = NormalizeSource(path);

                cachedConfigPaths.Add((StringUtf8)path, ConfigurationLevel.Xdg);

                Tracer.TraceMessage($"Cached config {ConfigurationLevel.Xdg}.", $"{ConfigurationLevel.Xdg} found at '{path}'.", TracerLevel.Diagnostic, userData: _userData);
            }

            return cachedConfigPaths;
        }
    }
}
