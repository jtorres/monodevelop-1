//*************************************************************************************************
// LsFilesCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class LsFilesCommand : GitCommand
    {
        public const string Command = "ls-files";

        public LsFilesCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public IReadOnlyDictionary<string, ITreeDifferenceDetail> ReadFileInfo(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using (var command = new ArgumentList(Command))
            {
                string escapedPath = PathHelper.EscapePath(path);

                command.AddOption("--full-name --stage");
                command.EndOptions();
                command.Add(escapedPath);

                try
                {
                    Dictionary<string, ITreeDifferenceDetail> fileInfo = ExecuteLsFiles(command, TreeDifferenceType.Ignored);

                    return fileInfo;
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(LsFilesCommand)}.{nameof(ReadFileInfo)}", exception))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public IReadOnlyDictionary<string, ITreeDifferenceDetail> ReadIgnoredFiles()
        {
            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--ignored --exclude-standard --full-name --stage --directory");

                try
                {
                    Dictionary<string, ITreeDifferenceDetail> ignoredFiles = ExecuteLsFiles(command, TreeDifferenceType.Ignored);

                    return ignoredFiles;
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(LsFilesCommand)}.{nameof(ReadIgnoredFiles)}", exception))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public IReadOnlyDictionary<string, ITreeDifferenceDetail> ReadUntrackedFiles()
        {
            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--others --exclude-standard --full-name --stage --directory");

                try
                {
                    Dictionary<string, ITreeDifferenceDetail> untrackedFiles = ExecuteLsFiles(command, TreeDifferenceType.Untracked);

                    return untrackedFiles;
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(LsFilesCommand)}.{nameof(ReadUntrackedFiles)}", exception))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public IReadOnlyList<string> GetCanonicalPaths(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using (var command = new ArgumentList(Command))
            {
                string escapedPath = PathHelper.EscapePath(path);

                command.AddOption("--full-name");
                command.Add(escapedPath);

                try
                {
                    // Since this is a case-insensitive path lookup, GIT_ICASE_PATHSPECS will
                    // need to be set in the environment for this process
                    return ExecuteLsFiles(command, setCaseInsensitivePaths: true);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(LsFilesCommand)}.{nameof(GetCanonicalPaths)}", exception))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        private Dictionary<string, ITreeDifferenceDetail> ExecuteLsFiles(string command, TreeDifferenceType type)
        {
            Dictionary<string, ITreeDifferenceDetail> ignoredFiles = new Dictionary<string, ITreeDifferenceDetail>(StringComparer.Ordinal);

            try
            {
                using (Tracer.TraceCommand(Command, command, userData: _userData))
                using (IProcess process = CreateProcess(command))
                {
                    var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                    using (var buffer = new ByteBuffer())
                    {
                        int get = 0;
                        int read = 0;

                        int r;
                        while (read < buffer.Length && (r = process.StdOut.Read(buffer, 0, buffer.Length - read)) > 0)
                        {
                            read += r;

                            while (get < read)
                            {
                                // find the next LF character
                                int eol = buffer.FirstIndexOf(Eol, get, read - get);
                                if (eol < 0)
                                {
                                    // if eol was not found, we need more buffer paged in
                                    // move the read idx by the read amount, which will
                                    // trigger a buffer page
                                    break;
                                }

                                int i1 = get,
                                    i2 = get;

                                i2 = buffer.FirstIndexOf(' ', i1, read - i1);
                                if (i2 < 0)
                                    throw new LsFilesParseException(new StringUtf8(buffer, 0, read), i1);

                                string modeStr = Encoding.UTF8.GetString(buffer, i1, i2 - i1);
                                uint modeVal;
                                if (!uint.TryParse(modeStr, NumberStyles.Number, CultureInfo.InvariantCulture, out modeVal))
                                    throw new LsFilesParseException(new StringUtf8(buffer, 0, read), i1);

                                TreeEntryDetailMode mode = (TreeEntryDetailMode)modeVal;

                                i1 = i2;

                                i2 = buffer.FirstIndexOf(' ', i1 + 1, read - i1 - 1);
                                if (i2 < 0)
                                    throw new LsFilesParseException(new StringUtf8(buffer, 0, read), i1);

                                StringUtf8 oidStr = new StringUtf8(buffer, i1 + 1, i2 - i1 - 1);
                                ObjectId objectId = oidStr.ToObjectId();

                                i1 = i2;

                                i2 = buffer.FirstIndexOf('\t', i1, read - i1);
                                if (i2 < 0)
                                    throw new LsFilesParseException(new StringUtf8(buffer, 0, read), i1);

                                StringUtf8 stageStr = new StringUtf8(buffer, i1 + 1, i2 - i1 - 1);
                                // read stage number

                                string path = Encoding.UTF8.GetString(buffer, i2 + 1, eol - i2 - 1);

                                TreeDifferenceDetail detail = new TreeDifferenceDetail(objectId, mode, type);
                                ignoredFiles.Add(path, detail);

                                get = eol + 1;
                            }

                            // when we've moved beyond what we can reliably read we need to shift the bytes
                            // left to make room in the buffer for new data
                            MakeSpace(buffer, ref get, ref read);
                        }
                    }

                    TestExitCode(process, command, stderrTask);
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(LsFilesCommand)}.{nameof(ExecuteLsFiles)}", exception, command))
            {
                // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                throw;
            }

            return ignoredFiles;
        }

        private List<string> ExecuteLsFiles(string command, bool setCaseInsensitivePaths)
        {
            List<string> files = new List<string>();

            try
            {
                if (setCaseInsensitivePaths)
                {
                    _environment = _environment.CreateWith(new Environment.Variable(Environment.Key.GitInsensitivePathspecs, "1"));
                }

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                using (IProcess process = CreateProcess(command))
                {
                    var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                    using (var buffer = new ByteBuffer())
                    {
                        int get = 0;
                        int read = 0;

                        int r;
                        while (read < buffer.Length && (r = process.StdOut.Read(buffer, 0, buffer.Length - read)) > 0)
                        {
                            read += r;

                            while (get < read)
                            {
                                // find the next LF character
                                int eol = buffer.FirstIndexOf(Eol, get, read - get);
                                if (eol < 0)
                                {
                                    // if eol was not found, we need more buffer paged in
                                    // move the read idx by the read amount, which will
                                    // trigger a buffer page
                                    break;
                                }

                                string path = Encoding.UTF8.GetString(buffer, get, eol - get);
                                files.Add(path);

                                get = eol + 1;
                            }

                            // when we've moved beyond what we can reliably read we need to shift the bytes
                            // left to make room in the buffer for new data
                            MakeSpace(buffer, ref get, ref read);
                        }
                    }

                    TestExitCode(process, command, stderrTask);
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(LsFilesCommand)}.{nameof(ExecuteLsFiles)}", exception, command))
            {
                // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                throw;
            }

            return files;
        }
    }
}
