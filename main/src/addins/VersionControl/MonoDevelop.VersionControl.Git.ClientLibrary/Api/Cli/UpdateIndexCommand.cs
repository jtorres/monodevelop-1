//*************************************************************************************************
// UpdateIndexCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class UpdateIndexCommand : GitCommand
    {
        public const string Command = "update-index";

        public UpdateIndexCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        static UpdateIndexCommand()
        {
            _typeTable = new Dictionary<StringUtf8, TreeDifferenceType>(StringUtf8Comparer.Ordinal)
            {
                { new StringUtf8("add"), TreeDifferenceType.Added },
                { new StringUtf8("removed"), TreeDifferenceType.Deleted },
            };
        }

        public IReadOnlyCollection<IUpdatedIndexEntry> Add(IEnumerable<string> paths, UpdateOptions options)
        {
            var updatedEntries = new List<IUpdatedIndexEntry>();

            using (var command = new ArgumentList(Command))
            {
                if (options.Flags != UpdateOptionFlags.None)
                {
                    if ((options.Flags & UpdateOptionFlags.Add) != 0)
                    {
                        command.Append("--add");
                    }

                    if ((options.Flags & UpdateOptionFlags.ForceRemove) != 0)
                    {
                        command.Append("--force-remove");
                    }

                    if ((options.Flags & UpdateOptionFlags.Remove) != 0)
                    {
                        command.Append("--remove");
                    }

                    if ((options.Flags & UpdateOptionFlags.Replace) != 0)
                    {
                        command.Append("--replace");
                    }

                    if ((options.Flags & UpdateOptionFlags.Unresolve) != 0)
                    {
                        command.Append("--unresolve");
                    }
                }

                command.Append("--verbose");
                command.Append("--stdin");

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                using (IProcess process = CreateProcess(command))
                {
                    try
                    {
                        string standardError = null;
                        // Setup an async reader of the standard error stream to collect any error messages.
                        var readerr = Task.Run(() => { standardError = process.StandardError.ReadToEnd(); });
                        // Setup an async reader of the standard output stream to collect any update information.
                        var readout = Task.Run(() => { ParseStdout(process.StdOut, updatedEntries); });

                        WriteStdin(paths, process.StdIn);

                        process.StdIn.Close();

                        process.WaitForExit();

                        if (!Task.WaitAll(new[] { readerr, readout }, TimeSpan.FromSeconds(2)))
                        {
                            Tracer.TraceWarning("Failed to release pipes", "Command: " + command, userData: _userData);
                        }

                        TestExitCode(process.ExitCode, command, standardError);
                    }
                    catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(UpdateIndexCommand)}.{nameof(Add)}", exception, command))
                    {
                        // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                        throw;
                    }
                }
            }

            return updatedEntries;
        }

        public IReadOnlyCollection<IUpdatedIndexEntry> SkipWorktreeAdd(IEnumerable<string> paths)
        {
            var updatedEntries = new List<IUpdatedIndexEntry>();

            using (var command = new ArgumentList(Command))
            {
                command.Append("--skip-worktree");
                command.Append("--verbose");
                command.Append("--stdin");

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                using (IProcess process = CreateProcess(command))
                {
                    try
                    {
                        // Setup an async reader of the standard error stream to collect any error messages.
                        var readerr = Task.Run(() => { ParseSkipWorktree(process.StdErr, updatedEntries); });
                        // Setup an async reader of the standard output stream to collect any update information.
                        var readout = Task.Run(() => { ParseStdout(process.StdOut, updatedEntries); });

                        WriteStdin(paths, process.StdIn);

                        process.StdIn.Close();

                        process.WaitForExit();

                        if (!Task.WaitAll(new[] { readerr, readout }, TimeSpan.FromSeconds(2)))
                        {
                            Tracer.TraceWarning("Failed to release pipes", "Command: " + command, userData: _userData);
                        }

                        TestExitCode(process.ExitCode, command);
                    }
                    catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(UpdateIndexCommand)}.{nameof(SkipWorktreeAdd)}", exception, command))
                    {
                        // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                        throw;
                    }
                }
            }

            return updatedEntries;
        }

        public IReadOnlyCollection<IUpdatedIndexEntry> SkipWorktreeRemove(IEnumerable<string> paths)
        {
            var updatedEntries = new List<IUpdatedIndexEntry>();

            using (var command = new ArgumentList(Command))
            {
                command.Append("--no-skip-worktree");
                command.Append("--verbose");
                command.Append("--stdin");

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                using (IProcess process = CreateProcess(command))
                {
                    try
                    {
                        // setup an async reader of the standard error stream to collect any error messages
                        var readerr = Task.Run(() => { ParseSkipWorktree(process.StdErr, updatedEntries); });
                        // setup an async reader of the standard output stream to collect any update information.
                        var readout = Task.Run(() => { ParseStdout(process.StdOut, updatedEntries); });

                        WriteStdin(paths, process.StdIn);

                        process.StdIn.Close();

                        process.WaitForExit();

                        if (!Task.WaitAll(new[] { readerr, readout }, TimeSpan.FromSeconds(2)))
                        {
                            Tracer.TraceWarning("Failed to release pipes", "Command: " + command, userData: _userData);
                        }

                        TestExitCode(process.ExitCode, command);
                    }
                    catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(UpdateIndexCommand)}.{nameof(SkipWorktreeRemove)}", exception, command))
                    {
                        // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                        throw;
                    }
                }
            }

            return updatedEntries;
        }

        private static readonly IReadOnlyDictionary<StringUtf8, TreeDifferenceType> _typeTable;

        private static TreeDifferenceType GetType(StringUtf8 value)
        {
            return (_typeTable.ContainsKey(value))
                ? _typeTable[value]
                : TreeDifferenceType.Invalid;
        }

        private void ParseStdout(Stream stdoutStream, ICollection<IUpdatedIndexEntry> updatedEntries)
        {
            using (var buffer = new ByteBuffer())
            {
                int read = 0;
                int get = 0;

                int r;
                while (read < buffer.Length && (r = stdoutStream.Read(buffer, read, buffer.Length - read)) > 0)
                {
                    read += r;

                    while (get < read)
                    {
                        // keep trying until we've got at least a line read
                        int eol = buffer.FirstIndexOf('\n', get, read - get);
                        if (eol < 0)
                            break;

                        // Parse the line, the format is: <action> '<path>'<LF>.
                        int i1 = buffer.FirstIndexOf('\'', get, eol - get);
                        if (i1 < 0)
                            throw new IndexParseException("separator1", new StringUtf8(buffer, 0, read), get);

                        int i2 = buffer.FirstIndexOf('\'', i1 + 1, eol - i1 - 1);
                        if (i2 < 0)
                            throw new IndexParseException("separator2", new StringUtf8(buffer, 0, read), get);

                        var action = new StringUtf8(buffer, get, i1 - get - 1);
                        var path = new StringUtf8(buffer, i1 + 1, i2 - i1 - 1);

                        TreeDifferenceType type = GetType(action);
                        if (type == TreeDifferenceType.Invalid)
                            throw new IndexParseException("type-invalid", new StringUtf8(buffer, 0, read), get);

                        var entry = new UpdatedIndexEntry(path, type);
                        updatedEntries.Add(entry);

                        get = eol + 1;
                    }

                    MakeSpace(buffer, ref get, ref read);
                }
            }
        }

        private void ParseSkipWorktree(Stream stderrStream, ICollection<IUpdatedIndexEntry> updatedEntries)
        {
            using (var buffer = new ByteBuffer())
            {
                int read = 0;
                int get = 0;

                int r;
                while (read < buffer.Length && (r = stderrStream.Read(buffer, read, buffer.Length - read)) > 0)
                {
                    read += r;

                    while (get < read)
                    {
                        // Keep trying until we've got at least a line read.
                        int eol = buffer.FirstIndexOf('\n', get, read - get);
                        if (eol < 0)
                            break;

                        if (buffer.StartsWith("fatal: ", get, eol - get))
                            throw new GitFatalException(Encoding.UTF8.GetString(buffer, get, eol - get));

                        // Parse the line, the format is: <action> '<path>'<LF>.
                        int i1 = buffer.FirstIndexOf(' ', get, eol - get);
                        if (i1 < 0)
                            throw new IndexParseException("space1", new StringUtf8(buffer, 0, read), get);

                        int i2 = buffer.FirstIndexOf(' ', i1 + 1, eol - i1 - 1);
                        if (i2 < 0)
                            throw new IndexParseException("space2", new StringUtf8(buffer, 0, read), get);

                        var action = new StringUtf8(buffer, get, i2 - get);
                        var path = new StringUtf8(buffer, i2 + 1, eol - i2 - 1);

                        if (action != (StringUtf8)"Ignoring path")
                            throw new IndexParseException("ignoring-path", new StringUtf8(buffer, 0, read), get);

                        var entry = new UpdatedIndexEntry(path, TreeDifferenceType.Skipped);
                        updatedEntries.Add(entry);

                        get = eol + 1;
                    }

                    MakeSpace(buffer, ref get, ref read);
                }
            }
        }

        private void WriteStdin(IEnumerable<string> paths, Stream stdinStream)
        {
            using (var buffer = new ByteBuffer())
            {
                foreach (string item in paths)
                {
                    int count = Encoding.UTF8.GetBytes(item, 0, item.Length, buffer, 0);
                    buffer[count] = 10;

                    stdinStream.Write(buffer, 0, count + 1);
                }
            }

            stdinStream.Flush();
        }
    }
}
