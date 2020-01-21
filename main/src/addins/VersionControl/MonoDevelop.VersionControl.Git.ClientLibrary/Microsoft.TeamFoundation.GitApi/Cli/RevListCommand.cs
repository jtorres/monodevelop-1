//*************************************************************************************************
// RevListCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;
using static System.FormattableString;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    [GitErrorMapping(typeof(WorkingDirectoryInvalidPathException), Prefix = "fatal: ambiguous argument", Suffix = "no such path in the working tree.")]
    internal class RevListCommand : GitCommand
    {
        public const string Command = "rev-list";

        public RevListCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public long CountCommits(InclusiveRange range, HistoryOptions options)
        {
            if (ReferenceEquals(range, null))
                throw new ArgumentNullException(nameof(range));
            if (ReferenceEquals(range.Since, null))
                throw new ArgumentNullException(nameof(range.Since));
            if (ReferenceEquals(range.Until, null))
                throw new ArgumentNullException(nameof(range.Until));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--count");
                ApplyOptions(command, options, range);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        string standardError;
                        string standardOuput;

                        int exitCode = Execute(command, out standardError, out standardOuput);

                        TestExitCode(exitCode, command, standardError);

                        long count = long.Parse(standardOuput);

                        return count;
                    }
                }
                catch (BadRevisionException)
                {
                    return 0L;
                }
                catch (ParseException exception) when (ParseHelper.AddContext("command", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy.
                    throw;
                }
            }
        }

        public IEnumerable<ICommit> EnumerateCommits(IRevision revision, HistoryOptions options)
        {
            if (revision == null)
                throw new ArgumentNullException(nameof(revision));
            if (options.MaxCount < -1) // we assume `0` (default(int)) or `-1` here to mean "unlimited"
                throw new ArgumentException(nameof(options.MaxCount));

            Debug.Assert(Enum.IsDefined(typeof(HistoryOrder), options.Order), $"The `{nameof(options.Order)}` parameter is undefined.");
            Debug.Assert(Enum.IsDefined(typeof(HistorySimplification), options.Simplification), $"The `{nameof(options.Simplification)}` parameter is undefined.");

            // build up the command buffer
            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, options, revision.RevisionText, null);

                // This is no until revision, so just pass null
                return ExecuteRevList(command, until: null);
            }
        }

        public IEnumerable<ICommit> EnumerateCommits(IRevision revision)
            => EnumerateCommits(revision, HistoryOptions.Default);

        public IEnumerable<ICommit> EnumerateCommits(IRevision until, IRevision since, HistoryOptions options)
        {
            if (until == null)
                throw new ArgumentNullException(nameof(until));
            if (options.MaxCount < -1) // We assume `0` (default(int)) or `-1` here to mean "unlimited".
                throw new ArgumentException(nameof(options.MaxCount));

            Debug.Assert(Enum.IsDefined(typeof(HistoryOrder), options.Order), $"The `{nameof(options.Order)}` parameter is undefined.");
            Debug.Assert(Enum.IsDefined(typeof(HistorySimplification), options.Simplification), $"The `{nameof(options.Simplification)}` parameter is undefined.");

            // Build up the command buffer.
            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, options, until.RevisionText, since?.RevisionText);

                // This is no until revision, so just pass null
                return ExecuteRevList(command, until: null);
            }
        }

        public IEnumerable<ICommit> EnumerateCommits(IRevision until, IEnumerable<IRevision> since, HistoryOptions options)
        {
            if (until == null)
                throw new ArgumentNullException(nameof(until));
            if (options.MaxCount < -1) // We assume `0` (default(int)) or `-1` here to mean "unlimited".
                throw new ArgumentException(nameof(options.MaxCount));

            Debug.Assert(Enum.IsDefined(typeof(HistoryOrder), options.Order), $"The `{nameof(options.Order)}` parameter is undefined.");
            Debug.Assert(Enum.IsDefined(typeof(HistorySimplification), options.Simplification), $"The `{nameof(options.Simplification)}` parameter is undefined.");

            // Build up the command buffer.
            using (var command = new ArgumentList(Command))
            {
                // Add --stdin since until is a collection that'll be written via stdin.
                command.AddOption("--stdin");
                ApplyOptions(command, options, until.RevisionText, since: null);

                return ExecuteRevList(command, since);
            }
        }

        public bool HasCommits()
        {
            // Build up the command buffer
            using (var command = new ArgumentList(Command))
            {
                // Use --all to check all refs for commits, and limit to returning just one
                command.AddOption(" --all --max-count=1 HEAD");

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    int exitCode = Execute(command, out string standardError, out string standardOutput);

                    switch (exitCode)
                    {
                        case 0:
                            return true;

                        case GitUsageExitCode:
                            throw new GitUsageException(standardError);

                        case GitFatalExitCode:
                        default:
                            return false;
                    }
                }
            }
        }

        internal static void ApplyOptions(ArgumentList command, HistoryOptions options, string until, string since)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (until == null)
                throw new ArgumentNullException(nameof(until));

            switch (options.Order)
            {
                case HistoryOrder.Default:
                // Nothing to append, let rev-list choose the ordering.
                break;

                case HistoryOrder.AuthorDateOrderAscending:
                {
                    command.AddOption("--author-date-order --reverse");
                }
                break;

                case HistoryOrder.AuthorDateOrderDecending:
                {
                    command.AddOption("--author-date-order");
                }
                break;

                case HistoryOrder.DateOrderAscending:
                {
                    command.AddOption("--date-order --reverse");
                }
                break;

                case HistoryOrder.DateOrderDecending:
                {
                    command.AddOption("--date-order");
                }
                break;

                case HistoryOrder.TopographicalOrderAscending:
                {
                    command.AddOption("--topo-order --reverse");
                }
                break;

                case HistoryOrder.TopographicalOrderDecending:
                {
                    command.AddOption("--topo-order");
                }
                break;

                default:
                {
                    Debug.Fail("Unexpected case reached.");
                }
                break;
            }

            switch (options.Simplification)
            {
                case HistorySimplification.Default:
                // Nothing to append, let rev-list choose the simplification.
                break;

                case HistorySimplification.AncestryPath:
                {
                    command.AddOption("--ancestry-path");
                }
                break;

                case HistorySimplification.FullHistory:
                {
                    command.AddOption("--full-history");
                }
                break;

                case HistorySimplification.Dense:
                {
                    command.AddOption("--dense");
                }
                break;

                case HistorySimplification.SimplifyMerges:
                {
                    command.AddOption("--simplify-merges");
                }
                break;

                case HistorySimplification.Sparse:
                {
                    command.AddOption("--sparse");
                }
                break;

                default:
                {
                    Debug.Fail("Unexpected case reached.");
                }
                break;
            }

            if (options.MaxCount > 0)
            {
                command.AddOption("--max-count", options.MaxCount.ToString ());
            }

            if ((options.Flags & HistoryOptionFlags.CherryPick) != 0)
            {
                command.AddOption("--cherry-pick");
            }
            if ((options.Flags & HistoryOptionFlags.FirstParent) != 0)
            {
                command.AddOption("--first-parent");
            }
            if ((options.Flags & HistoryOptionFlags.IncludeReflog) != 0)
            {
                if ((options.Flags & HistoryOptionFlags.WalkReflogs) != 0)
                    throw new ArgumentException(Invariant($"Cannot combine {nameof(HistoryOptionFlags.IncludeReflog)} and {nameof(HistoryOptionFlags.WalkReflogs)}."), nameof(options));

                command.AddOption("--reflog");
            }
            if ((options.Flags & HistoryOptionFlags.LeftOnly) != 0)
            {
                if ((options.Flags & HistoryOptionFlags.RightOnly) != 0)
                    throw new ArgumentException(Invariant($"Cannot combine `{nameof(HistoryOptionFlags.LeftOnly)}` and `{nameof(HistoryOptionFlags.RightOnly)}`."), nameof(options));

                command.AddOption("--left-only");
            }
            if ((options.Flags & HistoryOptionFlags.OmitMerges) != 0)
            {
                if ((options.Flags & HistoryOptionFlags.OnlyMerges) != 0)
                    throw new ArgumentException(Invariant($"Cannot combine `{nameof(HistoryOptionFlags.OmitMerges)}` and `{nameof(HistoryOptionFlags.OnlyMerges)}`."), nameof(options));

                command.AddOption("--no-merges");
            }
            if ((options.Flags & HistoryOptionFlags.OnlyMerges) != 0)
            {
                // Already confirmed !(OmitMerges + OnlyMerges)
                command.AddOption("--merges");
            }
            if ((options.Flags & HistoryOptionFlags.RightOnly) != 0)
            {
                // Already confirmed !(LeftOnly + RightOnly)
                command.AddOption("--right-only");
            }
            if ((options.Flags & HistoryOptionFlags.UseBitmapIndex) != 0)
            {
                command.AddOption("--use-bitmap-index");
            }
            if ((options.Flags & HistoryOptionFlags.WalkReflogs) != 0)
            {
                if (options.Order != HistoryOrder.Default)
                    throw new ArgumentException(Invariant($"Cannot combine `{nameof(HistoryOptionFlags.WalkReflogs)}` and a non-default value of `{nameof(HistoryOrder)}`."), nameof(options));

                // Already confirmed !(Reflog + WalkReflogs)
                command.AddOption("--walk-reflogs");
            }

            command.AddOption(" --header ");

            if (since != null)
            {
                command.AddOption(since + ".." + until);
            }
            else
            {
                command.Add(until);
            }

            // We are done with command arguments and now are on to paths.
            // This is required even if there are no paths so that we can
            // disambiguate revision names (branch master) from paths
            // (local path / directory master).
            command.EndOptions();

            if (options.HintPath != null)
            {
                command.Add(options.HintPath);
            }
        }

        internal static void ApplyOptions(ArgumentList command, HistoryOptions options, CommitRange range)
            => ApplyOptions(command, options, range?.Until?.RevisionText, range?.Since?.RevisionText);

        private IEnumerable<ICommit> ExecuteRevList(string command, IEnumerable<IRevision> until)
        {
            const char Eol = '\n';
            const char Nul = '\0';

            using (var buffer = new ByteBuffer())
            using (Tracer.TraceCommand(Command, command, userData: _userData))
            using (IProcess process = CreateProcess(command))
            {
                var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                // If a collection of until is provided, send them to stdin one per line
                // with a blank line at the end
                if (until != null)
                {
                    foreach (IRevision iuntil in until.Select(u => u))
                    {
                        if (iuntil != null)
                        {
                            process.StandardInput.Write('^');
                            process.StandardInput.Write(iuntil.RevisionText);
                            process.StandardInput.Write('\n');
                        }
                    }

                    // Follow with a blank line to signal the end of the revision list
                    process.StandardInput.Write('\n');
                    process.StandardInput.Flush();
                }

                int read = 0; // The count of bytes read into the buffer.
                int idx = 0; // The first unparsed byte in the buffer.

                int r;
                while (read < buffer.Length
                    && (r = process.StdOut.Read(buffer, read, buffer.Length - read)) > 0)
                {
                    // Compute total read length.
                    read += r;

                    // So long as the current read point is inside the available buffer...
                    while (idx < read)
                    {
                        ObjectId objectId = ObjectId.Zero;
                        var header = new ObjectHeader();
                        Commit commit = null;

                        try
                        {
                            // Find the next LF character.
                            int eol = buffer.FirstIndexOf(Eol, idx, read - idx);
                            if (eol < 0)
                            {
                                // If no LF was found, we need more buffer paged in move the read idx
                                // by the read amount, which will trigger a buffer page.
                                break;
                            }

                            // Parse the 40 characters preceding the LF character.
                            objectId = ObjectId.FromUtf8(buffer, eol - ObjectId.Length);
                            idx = eol + 1;

                            // All Git objects are received as headers + data Alloc the header here.
                            header = new ObjectHeader(objectId, ObjectType.Commit);
                            // Type the object as a commit and link it back to the owning repository here.
                            commit = new Commit(header);
                            commit.SetContextAndCache(Context, _repository as IStringCache);

                            int nul = -1;

                            // Git marks the end of the commit data block with a null character, so
                            // keep reading until the first null character is encountered.
                            while (nul < 0)
                            {
                                nul = buffer.FirstIndexOf(Nul, idx, read - idx);

                                // If no null character is found, page in more buffer.
                                if (nul < 0)
                                {
                                    MakeSpace(buffer, ref idx, ref read);

                                    r = process.StdOut.Read(buffer, read, buffer.Length - read);

                                    if (r <= 0)
                                        throw new RevListParseException("commitData", new StringUtf8(buffer, 0, read), 0);

                                    read += r;
                                }
                                else
                                {
                                    // Calculate the section of the buffer to write the to output buffer.
                                    int writeStart = idx;
                                    int writeLength = nul - writeStart;

                                    // Copy data into the output buffer.
                                    commit.ParseData(buffer, ref writeStart, writeLength, 4, null);
                                }
                            }

                            // Move beyond the null character.
                            idx = nul + 1;
                        }
                        catch (ParseException exception) when (ParseHelper.AddContext("command", exception, command, objectId, header, commit))
                        {
                            // Used for logging parse failures via exception filter. This code will not be reached but the throw is required to keep the compiler happy.
                            throw;
                        }

                        yield return commit;
                    }

                    // When we've moved beyond what we can reliably read we need to shift the bytes
                    // left to make room in the buffer for new data.
                    MakeSpace(buffer, ref idx, ref read);
                }

                try
                {
                    TestExitCode(process, command, stderrTask);
                }
                catch (BadRevisionException)
                {
                    yield break;
                }
            }
        }
    }
}
