//*************************************************************************************************
// StatusCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class StatusCommand : GitCommand
    {
        public const string Command = "status";

        private delegate void LineParserDelegate(StatusSnapshot snapshot, string buffer, int index, int count);

        private struct LineParser
        {
            public LineParser(string k, LineParserDelegate v)
            {
                Key = k;
                Value = v;
            }
            public string Key;
            public LineParserDelegate Value;
        }

        public StatusCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        {
            _patterns = new[]
            {
                // These are ordered by expected frequency.
                // We including the space following the prefix in our patterns.
                new LineParser("1 ", ParseLineOnePathChange),
                new LineParser("? ", ParseLineUntracked),
                new LineParser("! ", ParseLineIgnored),
                new LineParser("u ", ParseLineUnmerged),
                new LineParser("2 ", ParseLineTwoPathChange),

                new LineParser("# branch.oid ", ParseLineBranchOid),
                new LineParser("# branch.head ", ParseLineBranchHead),
                new LineParser("# branch.upstream ", ParseLineBranchUpstream),
                new LineParser("# branch.ab ", ParseLineBranchAheadBehind)
            };
        }

        private readonly IReadOnlyList<LineParser> _patterns;

        public IStatusSnapshot ReadSnapshot(StatusOptions options)
        {
            using (var command = new ArgumentList(Command))
            {
                // To avoid having locks takes during status, the global --no-optional-locks arg
                // must preceed the command
                command.Prepend("--no-optional-locks");

                // We want "--porcelain=v2" output which contains
                // SHA and MODE information for each tracked entry.
                //
                // We also force branch info; this will help us ensure
                // that we have a recent version of git.

                command.AddOption("--porcelain=v2");
                command.AddOption("--branch");

                switch (options.AheadBehind)
                {
                    case StatusAheadBehind.Default:
                        break;

                    case StatusAheadBehind.No:
                        command.AddOption("--no-ahead-behind");
                        break;

                    case StatusAheadBehind.Yes:
                        command.AddOption("--ahead-behind");
                        break;
                }

                switch (options.UntrackedFiles)
                {
                    case StatusUntrackedFiles.All:
                        command.AddOption("--untracked-files=all");
                        break;

                    case StatusUntrackedFiles.None:
                        command.AddOption("--untracked-files=none");
                        break;

                    case StatusUntrackedFiles.Normal:
                        command.AddOption("--untracked-files=normal");
                        break;
                }

                switch (options.Ignored)
                {
                    case StatusIgnored.Default:
                        break;

                    case StatusIgnored.Matching:
                        command.AddOption("--ignored=matching");
                        break;

                    case StatusIgnored.No:
                        command.AddOption("--ignored=no");
                        break;

                    case StatusIgnored.Traditional:
                        command.AddOption("--ignored=traditional");
                        break;
                }

                switch (options.IgnoreSubmodules)
                {
                    case StatusIgnoreSubmodule.All:
                        command.AddOption("--ignore-submodules=all");
                        break;

                    case StatusIgnoreSubmodule.Dirty:
                        command.AddOption("--ignore-submodules=dirty");
                        break;

                    case StatusIgnoreSubmodule.None:
                        command.AddOption("--ignore-submodules=none");
                        break;

                    case StatusIgnoreSubmodule.Untracked:
                        command.AddOption("--ignore-submodules=untracked");
                        break;
                }

                if (options.Path != null)
                {
                    command.EndOptions();
                    command.Add(options.Path);
                }

                try
                {
                    return ExecuteStatus(command.ToString ());
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(StatusCommand)}.{nameof(ReadSnapshot)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        private IStatusSnapshot ExecuteStatus(string command)
        {
            const char Eol = '\n';

            var snapshot = new StatusSnapshot();

            using (Tracer.TraceCommand(Command, command, userData: _userData))
            using (IProcess process = CreateProcess(command, true))
            {
                process.ProcessOutput += (sender, o) =>
                {
                    if (o.Source == OutputSource.Out)
                    {
                        ParseLine(snapshot, o.Message, 0, o.Message.Length);
                    }
                };

                RunAndTestProcess(process, command);
            }

            return snapshot;
        }

        /// <summary>
        /// Parse a single line of output from "git status".
        /// </summary>
        internal void ParseLine(StatusSnapshot shapshot, string buffer, int index, int count)
        {
            int start = index;
            int end = index + count;
            try
            {
                foreach (var t in _patterns)
                {
                    if (buffer.StartsWith(t.Key, start, (end - start)))
                    {
                        try
                        {
                            // Subordinate parsers get the remainder of the line.
                            start += t.Key.Length;
                            t.Value(shapshot, buffer, start, (end - start));
                            return;
                        }
                        catch (Exception ex) when (!(ex is ExceptionBase))
                        {
                            // map argument-out-of-range and the like into a parser exception.
                            // we give the new exception the whole line rather than the remainder.
                            throw new StatusParseException("Error parsing status output",
                                                           new StringUtf8(buffer, index, count), 0,
                                                           ex);
                        }
                    }
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(StatusCommand)}.{nameof(ParseLine)}", exception))
            {
                // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                throw;
            }

            // We did not match a known prefix.
            throw new StatusParseException("invalid-detail-line-prefix", new StringUtf8(buffer, index, count), 0);
        }

        #region ParseLine Branch Headers
        private void ParseLineBranchOid(StatusSnapshot snapshot, string buffer, int index, int count)
        {
            if (buffer.StartsWith(Head.UnbornLabel, index, count))
            {
                (snapshot.BranchInfo as StatusBranchInfo).CommitId = ObjectId.Zero;
            }
            else
            {
                (snapshot.BranchInfo as StatusBranchInfo).CommitId = ParseSha(buffer, index, count);
            }
        }

        private void ParseLineBranchHead(StatusSnapshot snapshot, string buffer, int index, int count)
        {
            if (buffer.StartsWith(Head.DetachedLabel, index, count))
            {
                (snapshot.BranchInfo as StatusBranchInfo).SetHeadIsDetached();
            }
            else if (buffer.StartsWith(Head.MalformedLabel, index, count))
            {
                (snapshot.BranchInfo as StatusBranchInfo).SetHeadIsUnknown();
            }
            else
            {
                (snapshot.BranchInfo as StatusBranchInfo).SetHeadBranchName(new StringUtf8(buffer, index, count));
            }
        }

        private void ParseLineBranchUpstream(StatusSnapshot snapshot, string buffer, int index, int count)
        {
            (snapshot.BranchInfo as StatusBranchInfo).SetUpstreamBranchName(new StringUtf8(buffer, index, count));
        }

        private void ParseLineBranchAheadBehind(StatusSnapshot snapshot, string buffer, int index, int count)
        {
            int start = index;
            int end = index + count;

            // Expect "+<ahead> -<behind>"
            if (buffer[start] != (byte)'+')
                throw new StatusParseException("invalid-ahead", new StringUtf8(buffer, index, count), 0);

            start++; // skip over the leading '+'.
            int gap = GetNextTokenLength(buffer, index, count, start, ' ', "Invalid branch ahead/behind");
            // Following the '+' could be a number or a '?' if --no-ahead-behind is specified
            int ahead = 0;
            if (!ParseQuestionMark(buffer, start, (gap - start)))
            {
                ahead = ParsePositiveIntValue(buffer, start, (gap - start));
            }

            start = gap + 1;

            if (buffer[start] != (byte)'-')
                throw new StatusParseException("invalid-behind", new StringUtf8(buffer, index, count), 0);

            start++; // skip over the leading '-'.
            // Following the '-' could be a number or a '?' if --no-ahead-behind is specified
            int behind = 0;
            if (!ParseQuestionMark(buffer, start, (end - start)))
            {
                behind = ParsePositiveIntValue(buffer, start, (end - start));
            }

            (snapshot.BranchInfo as StatusBranchInfo).SetAheadBehindCounts(ahead, behind);
        }

        #endregion // ParseLine Branch Headers

        #region ParseLine Data Lines

        private void ParseLineOnePathChange(StatusSnapshot snapshot, string buffer, int index, int count)
        {
            int start = index;
            int end = index + count;
            int gap;

            TreeDifferenceType stagedChange = ParseTreeDifferenceType(buffer[start]);
            TreeDifferenceType unstagedChange = ParseTreeDifferenceType(buffer[start + 1]);

            start += 2;
            if (buffer[start] != (byte)' ')
                throw new StatusParseException("invalid-submodule-key", new StringUtf8(buffer, index, count), 0);

            start += 1;

            // Expect "<submodule_keys> ..."
            gap = GetNextTokenLength(buffer, index, count, start, ' ', 4, "Invalid submodule key");
            StatusSubmoduleState submoduleState = ParseSubmoduleStatus(buffer, start, (gap - start));

            start = gap + 1;

            // Expect: "<mh> <mi> <mw> <idh> <idi> <path>"
            gap = GetNextTokenLength(buffer, index, count, start, ' ', 6, "Invalid mode");
            TreeEntryDetailMode modeHead = ParseOctalMode(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', 6, "Invalid mode");
            TreeEntryDetailMode modeIndex = ParseOctalMode(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', 6, "Invalid mode");
            TreeEntryDetailMode modeWorktree = ParseOctalMode(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', ObjectId.Length, "Invalid Object Id");
            ObjectId shaHead = ParseSha(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', ObjectId.Length, "Invalid Object Id");
            ObjectId shaIndex = ParseSha(buffer, start, (gap - start));

            start = gap + 1;
            var pathCurrent = PathHelper.ParsePathValue(buffer, start, (end - start));

            // validate that the value return is good
            if (!PathHelper.IsValidRepositoryPath(pathCurrent))
                throw new PathParseException("one-path-current", new StringUtf8(buffer, 0, index + count), index);

            StatusEntry statusEntry = new StatusEntry(stagedChange, unstagedChange, submoduleState, modeHead, modeIndex, modeWorktree, shaHead, shaIndex, new StringUtf8(pathCurrent));
            (snapshot.TrackedItems as StatusTrackedEntries).Add(statusEntry);
        }

        private void ParseLineTwoPathChange(StatusSnapshot snapshot, string buffer, int index, int count)
        {
            int start = index;
            int end = index + count;
            int gap;

            TreeDifferenceType stagedChange = ParseTreeDifferenceType(buffer[start]);
            TreeDifferenceType unstagedChange = ParseTreeDifferenceType(buffer[start + 1]);

            start += 2;
            if (buffer[start] != (byte)' ')
                throw new StatusParseException("invalid-submodule-key", new StringUtf8(buffer, index, count), 0);

            start += 1;

            // Expect "<submodule_keys> ..."
            gap = GetNextTokenLength(buffer, index, count, start, ' ', 4, "Invalid submodule key");
            StatusSubmoduleState submoduleStatus = ParseSubmoduleStatus(buffer, start, (gap - start));

            start = gap + 1;

            // Expect: "<mh> <mi> <mw> <idh> <idi> ..."
            gap = GetNextTokenLength(buffer, index, count, start, ' ', 6, "Invalid mode");
            TreeEntryDetailMode modeHead = ParseOctalMode(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', 6, "Invalid mode");
            TreeEntryDetailMode modeIndex = ParseOctalMode(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', 6, "Invalid mode");
            TreeEntryDetailMode modeWorktree = ParseOctalMode(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', ObjectId.Length, "Invalid Object Id");
            ObjectId shaHead = ParseSha(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', ObjectId.Length, "Invalid Object Id");
            ObjectId shaIndex = ParseSha(buffer, start, (gap - start));

            start = gap + 1;

            // Expect: "[RC]<score> <path>\t<path2>"
            if (buffer[start] == (byte)'R')
            {
                start++;

                gap = GetNextTokenLength(buffer, index, count, start, ' ', "Invalid Object Id");
                int score = ParsePositiveIntValue(buffer, start, (gap - start));

                start = gap + 1;
                gap = GetNextTokenLength(buffer, index, count, start, '\t', "Invalid Path");
                var pathCurrent = PathHelper.ParsePathValue(buffer, start, (gap - start));

                // validate that the value return is good
                if (!PathHelper.IsValidRepositoryPath(pathCurrent))
                    throw new PathParseException("two-path-current", new StringUtf8(buffer, 0, index + count), index);

                start = gap + 1;
                var pathOriginal = PathHelper.ParsePathValue(buffer, start, (end - start));

                // validate that the value return is good
                if (!PathHelper.IsValidRepositoryPath(pathOriginal))
                    throw new PathParseException("two-path-original", new StringUtf8(buffer, 0, index + count), index);

                var statusEntry = new StatusRenamedEntry(stagedChange, unstagedChange, submoduleStatus, modeHead, modeIndex, modeWorktree, shaHead, shaIndex, score, new StringUtf8(pathOriginal), new StringUtf8(pathCurrent));
                (snapshot.TrackedItems as StatusTrackedEntries).Add(statusEntry);
            }
            else if (buffer[start] == (byte)'C')
            {
                start++;

                gap = GetNextTokenLength(buffer, index, count, start, ' ', "Invalid Object Id");
                int score = ParsePositiveIntValue(buffer, start, (gap - start));

                start = gap + 1;
                gap = GetNextTokenLength(buffer, index, count, start, '\t', "Invalid Path");
                var pathCurrent = PathHelper.ParsePathValue(buffer, start, (gap - start));

                // validate that the value return is good
                if (!PathHelper.IsValidRepositoryPath(pathCurrent))
                    throw new PathParseException("two-path-current", new StringUtf8(buffer, 0, index + count), index);

                start = gap + 1;
                var pathOriginal = PathHelper.ParsePathValue(buffer, start, (end - start));

                // validate that the value return is good
                if (!PathHelper.IsValidRepositoryPath(pathOriginal))
                    throw new PathParseException("two-path-original", new StringUtf8(buffer, 0, index + count), index);

                var statusEntry = new StatusCopiedEntry(stagedChange, unstagedChange, submoduleStatus, modeHead, modeIndex, modeWorktree, shaHead, shaIndex, new StringUtf8(pathCurrent), new StringUtf8(pathOriginal), score);
                (snapshot.TrackedItems as StatusTrackedEntries).Add(statusEntry);
            }
            else
            {
                // Actual unexpected data or a real parser bug.
                throw new StatusParseException("unexpected-data-line", new StringUtf8(buffer, index, count), 0);
            }
        }

        private void ParseLineUnmerged(StatusSnapshot snapshot, string buffer, int index, int count)
        {
            int start = index;
            int end = index + count;
            int gap;

            // Expect "<X><Y> ..."
            char columnX = (char)buffer[start];
            char columnY = (char)buffer[start + 1];
            StatusUnmergedState unmergedState = ParseUnmergedXY(columnX, columnY);

            start += 2;
            if (buffer[start] != (byte)' ')
                throw new StatusParseException("invalid-submodule-key", new StringUtf8(buffer, index, count), 0);

            start += 1;

            // Expect "<submodule_keys> ..."
            gap = GetNextTokenLength(buffer, index, count, start, ' ', 4, "Invalid submodule key");
            StatusSubmoduleState submoduleState = ParseSubmoduleStatus(buffer, start, (gap - start));

            start = gap + 1;

            // Expect: "<m1> <m2> <m3> <mw> <id1> <id2> <id3> <path>"
            gap = GetNextTokenLength(buffer, index, count, start, ' ', 6, "Invalid mode");
            TreeEntryDetailMode stage1mode = ParseOctalMode(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', 6, "Invalid mode");
            TreeEntryDetailMode stage2mode = ParseOctalMode(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', 6, "Invalid mode");
            TreeEntryDetailMode stage3mode = ParseOctalMode(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', 6, "Invalid mode");
            TreeEntryDetailMode modeWorktree = ParseOctalMode(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', ObjectId.Length, "Invalid Object Id");
            ObjectId shaStage1 = ParseSha(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', ObjectId.Length, "Invalid Object Id");
            ObjectId stage2sha = ParseSha(buffer, start, (gap - start));

            start = gap + 1;
            gap = GetNextTokenLength(buffer, index, count, start, ' ', ObjectId.Length, "Invalid Object Id");
            ObjectId stage3sha = ParseSha(buffer, start, (gap - start));

            start = gap + 1;
            var pathCurrent = PathHelper.ParsePathValue(buffer, start, (end - start));

            // validate that the value return is good
            if (!PathHelper.IsValidRepositoryPath(pathCurrent))
                throw new PathParseException("unmerged", new StringUtf8(buffer, 0, index + count), index);

            StatusUnmergedEntry unmergedEntry = new StatusUnmergedEntry(unmergedState, submoduleState, stage1mode, stage2mode, stage3mode, modeWorktree, shaStage1, stage2sha, stage3sha, new StringUtf8(pathCurrent));
            snapshot.AddUnmergedEntry(unmergedEntry);
        }

        private void ParseLineUntracked(StatusSnapshot snapshot, string buffer, int index, int count)
        {
            var path = PathHelper.ParsePathValue(buffer, index, count);

            // validate that the value return is good
            if (!PathHelper.IsValidRepositoryPath(path))
                throw new PathParseException("untracked", new StringUtf8(buffer, 0, index + count), index);

            snapshot.AddUntrackedItem((string)path);
        }

        private void ParseLineIgnored(StatusSnapshot snapshot, string buffer, int index, int count)
        {
            var path = PathHelper.ParsePathValue(buffer, index, count);

            // validate that the value return is good
            if (!PathHelper.IsValidRepositoryPath(path))
                throw new PathParseException("ignore", new StringUtf8(buffer, 0, index + count), index);

            snapshot.AddIgnoredItem((string)path);
        }

        #endregion

        #region ParseLine Helpers

        /// <summary>
        /// Get the length of the next token on the line.
        /// </summary>
        private static int GetNextTokenLength(
            string buffer,
            int index,
            int count,
            int start,
            char delimiter,
            string errmsg)
        {
            int gap = buffer.IndexOf(delimiter, start, (index + count - start));
            if (gap < 0)
                throw new StatusParseException(errmsg, new StringUtf8(buffer, index, count), 0);

            return gap;
        }

        /// <summary>
        /// Get the length of the next token on the line and assert it is the proper length.
        /// </summary>
        private static int GetNextTokenLength(
            string buffer,
            int index,
            int count,
            int start,
            char delimiter,
            int requiredLength,
            string errmsg)
        {
            int gap = buffer.IndexOf(delimiter, start, (index + count - start));
            if (gap < 0 || gap != start + requiredLength)
                throw new StatusParseException(errmsg, new StringUtf8(buffer, index, count), 0);

            return gap;
        }

        /// <summary>
        /// Convert X or Y column value into a different type.
        /// Silently handle '.' characters as aliases for a space.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        internal static TreeDifferenceType ParseTreeDifferenceType(char col)
        {
            if (col == '.')
            {
                col = ' ';
            }

            return DiffCommand.GetDifferenceType(col);
        }

        internal static StatusUnmergedState ParseUnmergedXY(char x, char y)
        {
            string xy = $"{x}{y}";
            switch (xy)
            {
                case "DD": return StatusUnmergedState.DeleteDelete;
                case "AU": return StatusUnmergedState.AddedUnmerged;
                case "UD": return StatusUnmergedState.UnmergedDeleted;
                case "UA": return StatusUnmergedState.UnmergedAdded;
                case "DU": return StatusUnmergedState.DeletedUnmerged;
                case "AA": return StatusUnmergedState.AddedAdded;
                case "UU": return StatusUnmergedState.UnmergedUnumerged;
                default: return StatusUnmergedState.None;
            }
        }

        private static bool TryLetter(
            char letter,
            char choiceA, StatusSubmoduleState bitA,
            char choiceB, StatusSubmoduleState bitB,
            out StatusSubmoduleState value)
        {
            if (letter == choiceA)
            {
                value = bitA;
                return true;
            }
            if (letter == choiceB)
            {
                value = bitB;
                return true;
            }

            value = 0; // make compiler happy.
            return false;
        }

        internal static StatusSubmoduleState ParseSubmoduleStatus(string buffer, int index, int count)
        {
            // Expect "N..." | "S[C.][M.][U.]"
            Debug.Assert(count == 4);

            if (buffer.StartsWith("N...", index, count))
                return StatusSubmoduleState.None;

            if (buffer[index] != (byte)'S')
                throw new StatusParseException("Invalid submodule key", new StringUtf8(buffer, index, count), 0);

            StatusSubmoduleState valueC, valueM, valueU;
            if (!TryLetter((char)buffer[index + 1], 'C', StatusSubmoduleState.NewCommit, '.', StatusSubmoduleState.None, out valueC)
                || !TryLetter((char)buffer[index + 2], 'M', StatusSubmoduleState.Modified, '.', StatusSubmoduleState.None, out valueM)
                || !TryLetter((char)buffer[index + 3], 'U', StatusSubmoduleState.UntrackedChanges, '.', StatusSubmoduleState.None, out valueU))
                throw new StatusParseException("Invalid submodule key", new StringUtf8(buffer, index, count), 0);

            return valueC | valueM | valueU;
        }

        /// <summary>
        /// Parse 6 digit OCTAL mode.
        /// </summary>
        internal static TreeEntryDetailMode ParseOctalMode(string buffer, int index, int count)
        {
            // Since we don't actually want to write an OCTAL parser
            // and don't need to actually return the octal value, we
            // can fake it using a DECIMAL parser.
            int fakeOctal = ParsePositiveIntValue(buffer, index, count);

            switch (fakeOctal)
            {
                case 100644: return TreeEntryDetailMode.NormalFile;
                case 000000: return TreeEntryDetailMode.Nonexistent;
                case 100755: return TreeEntryDetailMode.ExecutableFile;
                case 160000: return TreeEntryDetailMode.Submodule;
                case 120000: return TreeEntryDetailMode.Symlink;
                case 40000: return TreeEntryDetailMode.Directory;
                case 100664: return TreeEntryDetailMode.GroupWritableFile;

                default:
                    throw new StatusParseException("Expected octal value", new StringUtf8(buffer, index, count), 0);
            }
        }

        /// <summary>
        /// Convenience wrapper to parse an ID from the data.
        /// </summary>
        internal static ObjectId ParseSha(string buffer, int index, int count)
        {
            return ObjectId.FromString(buffer, index);
        }

        /// <summary>
        /// The format spec defines a DECIMAL integer field in the data.
        /// Parse it as such and return the integer value.  Throw if the
        /// format is wrong or we are out of sync.
        /// </summary>
        internal static int ParsePositiveIntValue(string buffer, int index, int count)
        {
            int value;
            if (Internal.Extensions.TryParse(buffer, index, count, out value) && value >= 0)
                return value;

            throw new StatusParseException("Expected integer value", new StringUtf8(buffer, index, count), 0);
        }

        /// <summary>
        /// Parse a question mark from the buffer.
        /// </summary>
        private static bool ParseQuestionMark(string buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (index < 0 || index >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0 || index + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            // This can only match if it's a single char
            if (count > 1)
            {
                return false;
            }

            return buffer[index] == '?';
        }

        #endregion
    }
}
