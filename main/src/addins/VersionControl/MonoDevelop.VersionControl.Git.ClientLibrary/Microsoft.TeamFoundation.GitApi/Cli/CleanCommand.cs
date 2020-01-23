//*************************************************************************************************
// CleanCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    /// <summary>
    /// Creates, manages, and reports git-clean operations.
    /// </summary>
    internal class CleanCommand : GitCommand
    {
        public const string Command = "clean";

        public CleanCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        /// <summary>
        /// Don't actually remove anything from the working tree, just report what would be done.
        /// </summary>
        /// <param name="options">Options specifying how the clean operation should be preformed.</param>
        /// <returns>Collection of results of the clean operation.</returns>
        public ICollection<IUpdatedWorktreeEntry> DryRun(CleanOptions options)
        {
            if (!options.HasValidFlags())
                throw new ArgumentException(nameof(options));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--dry-run");

                ApplyOptions(command, options);

                return ExecuteClean(command);
            }
        }

        /// <summary>
        /// Remove untracked files from the working tree.
        /// </summary>
        /// <param name="options">Options specifying how the clean operation should be preformed.</param>
        /// <returns>Collection of results of the clean operation.</returns>
        public ICollection<IUpdatedWorktreeEntry> RemoveFiles(CleanOptions options)
        {
            if (!options.HasValidFlags())
                throw new ArgumentException(nameof(options));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--force");

                ApplyOptions(command, options);

                return ExecuteClean(command);
            }
        }

        private void ApplyOptions(ArgumentList command, CleanOptions options)
        {
            if (ReferenceEquals(command, null))
                throw new ArgumentNullException(nameof(command));
            if (!options.HasValidFlags())
                throw new ArgumentException(nameof(options));

            if (options.Flags != CleanOptionFlags.None
                && options.Flags != CleanOptionFlags.RemoveUntracked)
            {
                if ((options.Flags & CleanOptionFlags.RemoveDirectories) != 0)
                {
                    command.AddOption("-d");
                }

                if ((options.Flags & CleanOptionFlags.RemoveIgnored) != 0)
                {
                    if ((options.Flags & CleanOptionFlags.RemoveUntracked) != 0)
                    {
                        command.AddOption("-x");
                    }
                    else
                    {
                        command.AddOption("-X");
                    }
                }
                else
                {
                    // we should not be here
                    throw new InvalidOptionsException(nameof(options));
                }
            }

            if (options.ExcludedPaths != null)
            {
                foreach (var path in options.ExcludedPaths)
                {
                    command.AddOption("-e");
                    command.Add(path);
                }
            }

            if (options.IncludedPaths != null)
            {
                command.EndOptions();

                foreach (var path in options.IncludedPaths)
                {
                    command.Add(path);
                }
            }
        }

        private ICollection<IUpdatedWorktreeEntry> ExecuteClean(string command)
        {
            if (ReferenceEquals(command, null))
                throw new ArgumentNullException(nameof(command));

            List<IUpdatedWorktreeEntry> errList = null;
            List<IUpdatedWorktreeEntry> outList = null;

            using(Tracer.TraceCommand(Command, command, userData: _userData))
            using (IProcess process = CreateProcess(command))
            {
                try
                {
                    Task.WaitAll(Task.Run(() => { errList = ParseOutput(process.StdErr); }),
                                 Task.Run(() => { outList = ParseOutput(process.StdOut); }));

                    TestExitCode(process, $"{nameof(CleanCommand)}.{nameof(ExecuteClean)}");
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CleanCommand)}.{nameof(ExecuteClean)}", exception))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }

            List<IUpdatedWorktreeEntry> result = new List<IUpdatedWorktreeEntry>(outList);
            result.AddRange(errList);

            return result;
        }

        private List<IUpdatedWorktreeEntry> ParseOutput(Stream stdPipe)
        {
            // warning prefixed constants
            const string WarningPrefix = "warning: ";
            const string FailedPrefix = "Could not stat path "; // "warning: Could not stat path "
            const string RemoveFailedPrefix = "failed to remove "; // "warning: failed to remove "
            // would prefixed constants
            const string WouldPrefix = "Would ";
            const string WouldRemovePrefix = "remove "; // "Would remove "
            const string WouldSkipRepoPrefix = "skip repository "; // "Would skip repository "
            // un-prefixed constants
            const string RemovedPrefix = "Removing ";
            const string SkippedRepoPrefix = "Skipping repository ";

            List<IUpdatedWorktreeEntry> list = new List<IUpdatedWorktreeEntry>(64);

            using (var buffer = new ByteBuffer())
            {
                int index = 0;
                int read = 0;

                int r;
                while (read < buffer.Length && (r = stdPipe.Read(buffer, read, buffer.Length - read)) > 0)
                {
                    read += r;

                    while (index < read)
                    {
                        int eol;
                        if ((eol = buffer.FirstIndexOf(Eol, index, read - index)) < 0)
                            break;

                        int length = eol - index;
                        UpdatedWorktreeEntryType type = UpdatedWorktreeEntryType.Unknown;

                        // test from most to least likely
                        if (buffer.StartsWith(RemovedPrefix, index, length))
                        {
                            type = UpdatedWorktreeEntryType.Removed;
                            length = RemovedPrefix.Length;
                        }
                        // "warning: "
                        else if (buffer.StartsWith(WarningPrefix, index, length))
                        {
                            // "warning: failed to remove "
                            if (buffer.StartsWith(RemoveFailedPrefix, index + WarningPrefix.Length, length - WarningPrefix.Length))
                            {
                                type = UpdatedWorktreeEntryType.RemoveFailed;
                                length = WarningPrefix.Length + RemoveFailedPrefix.Length;
                            }
                            // "warning: Could not stat path "
                            else if (buffer.StartsWith(FailedPrefix, index + WarningPrefix.Length, length - WarningPrefix.Length))
                            {
                                type = UpdatedWorktreeEntryType.Failed;
                                length = WarningPrefix.Length + FailedPrefix.Length;
                            }
                        }
                        // "Skipping repository "
                        else if (buffer.StartsWith(SkippedRepoPrefix, index, length))
                        {
                            type = UpdatedWorktreeEntryType.SkippedRepository;
                            length = SkippedRepoPrefix.Length;
                        }
                        // "Would "
                        else if (buffer.StartsWith(WouldPrefix, index, length))
                        {
                            // "Would remove "
                            if (buffer.StartsWith(WouldRemovePrefix, index + WouldPrefix.Length, length - WouldPrefix.Length))
                            {
                                type = UpdatedWorktreeEntryType.WouldRemove;
                                length = WouldPrefix.Length + WouldRemovePrefix.Length;
                            }
                            // "Would skip repository "
                            else if (buffer.StartsWith(WouldSkipRepoPrefix, index + WouldPrefix.Length, length - WouldPrefix.Length))
                            {
                                type = UpdatedWorktreeEntryType.WouldSkipRepository;
                                length = WouldPrefix.Length + WouldSkipRepoPrefix.Length;
                            }
                        }

                        // if we still do not understand the output, we need to throw
                        if (type == UpdatedWorktreeEntryType.Unknown)
                            throw new CleanParseException(new StringUtf8(buffer, 0, read), index);

                        StringUtf8 details = null;
                        StringUtf8 path = new StringUtf8(buffer, index + length, eol - index - length);

                        // failed clean attempts report a 'reason' for failure
                        if (type == UpdatedWorktreeEntryType.Failed)
                        {
                            int idx = path.FirstIndexOf(':');
                            if (idx > 0)
                            {
                                details = path.Substring(idx + 2, path.Length - idx - 2);
                                path = path.Substring(0, idx - 1);
                            }
                        }

                        var file = new UpdatedWorktreeEntry(path, type, details);

                        list.Add(file);

                        index = eol + 1;
                    }

                    MakeSpace(buffer, ref index, ref read);
                }
            }

            return list;
        }
    }
}
