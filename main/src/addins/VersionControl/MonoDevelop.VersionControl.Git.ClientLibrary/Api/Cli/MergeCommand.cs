﻿//*************************************************************************************************
// MergeCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class MergeCommand : GitCommand
    {
        public const string Command = "merge";
        private const string FatalPrefix = "fatal: ";
        private const string StoppedBeforeCommit = "Automatic merge went well; stopped before committing as requested";

        public MergeCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        {
            _result = MergeCommandResult.Undefined;
        }

        private MergeCommandResult _result;
        private int _sumMergeConflicts = 0;
        private bool _sawMergeConflictMessage = false;

        public void DoAbort()
        {
            using (var command = new StringBuffer(Command))
            {
                command.Append(" --abort");

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    string standardError;
                    string standardOutput;

                    int exitCode = Execute(command, out standardError, out standardOutput);

                    TestExitCode(exitCode, command, standardError);
                }
            }
        }

        public MergeCommandResult DoMerge(IRevision revision, MergeOptions options)
        {
            if (revision == null)
                throw new ArgumentNullException(nameof(revision));

            using (var command = new StringBuffer(Command))
            {
                ApplyMergeOptions(command, options);

                command.Append(' ')
                       .Append(revision.RevisionText);

                try
                {
                    return ExecuteMerge(command);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(MergeCommand)}.{nameof(DoMerge)}", exception))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        private void ApplyMergeOptions(StringBuffer command, MergeOptions options)
        {
            command.Append(" --no-stat"); // do not print changed files summary
            command.Append(" --no-edit"); // use automatically generated commit message
            command.Append(" --verbose"); // want "Merge made by the <strategy>" line

            switch (options.CommitFlags)
            {
                case MergeOptionCommitFlags.CommitAfterMerge:
                    command.Append(" --commit");
                    break;
                case MergeOptionCommitFlags.NoCommitAfterMerge:
                    command.Append(" --no-commit");
                    break;
                case MergeOptionCommitFlags.Default:
                    break;
                default:
                    throw new NotImplementedException();
            }

            switch (options.ReReReFlags)
            {
                case MergeOptionReReReFlags.ReReReAutoUpdate:
                    command.Append(" --rerere-autoupdate");
                    break;
                case MergeOptionReReReFlags.NoReReReAutoUpdate:
                    command.Append(" --no-rerere-autoupdate");
                    break;
                case MergeOptionReReReFlags.Default:
                    break;
                default:
                    throw new NotImplementedException();
            }

            switch (options.FastForwardFlags)
            {
                case MergeOptionFastForwardFlags.FastForwardOnly:
                    command.Append(" --ff-only");
                    break;
                case MergeOptionFastForwardFlags.FastForwardOrMerge:
                    command.Append(" --ff");
                    break;
                case MergeOptionFastForwardFlags.NoFastForward:
                    command.Append(" --no-ff");
                    break;
                case MergeOptionFastForwardFlags.Default:
                    break;
                default:
                    throw new NotImplementedException();
            }

            switch (options.SquashFlags)
            {
                case MergeOptionSquashFlags.Squash:
                    command.Append(" --squash");
                    break;
                case MergeOptionSquashFlags.NoSquash:
                    command.Append(" --no-squash");
                    break;
                case MergeOptionSquashFlags.Default:
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (options.Strategy != null)
            {
                const string StrategyPrefix = " --strategy=";
                const string StrategyOptionPrefix = " --strategy-option=";
                const string DiffAlogrithmPrefix = "diff-algoritm=";

                switch (options.Strategy.Type)
                {
                    case MergeStrategyType.Octopus:
                        throw new NotImplementedException();

                    case MergeStrategyType.Ours:
                        command.Append(StrategyPrefix + "ours");
                        break;

                    case MergeStrategyType.Recursive:
                        {
                            command.Append(StrategyPrefix + "recursive");

                            MergeStrategyRecursive strategy = options.Strategy as MergeStrategyRecursive;

                            switch (strategy.Algorithm)
                            {
                                case DifferenceAlgorithm.Histogram:
                                    command.Append(StrategyOptionPrefix + DiffAlogrithmPrefix + "histogram"); ;
                                    break;

                                case DifferenceAlgorithm.Minimal:
                                    command.Append(StrategyOptionPrefix + DiffAlogrithmPrefix + "minimal"); ;
                                    break;

                                case DifferenceAlgorithm.Myers:
                                    command.Append(StrategyOptionPrefix + DiffAlogrithmPrefix + "myers"); ;
                                    break;

                                case DifferenceAlgorithm.Patience:
                                    command.Append(StrategyOptionPrefix + DiffAlogrithmPrefix + "patience"); ;
                                    break;

                                case DifferenceAlgorithm.Default:
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }

                            switch (strategy.RenameDetection)
                            {
                                case RenameDetection.FollowRenames:
                                    command.Append(StrategyOptionPrefix + "find-renames");
                                    break;

                                case RenameDetection.NoRenameDetection:
                                    command.Append(StrategyOptionPrefix + "no-renames");
                                    break;

                                case RenameDetection.Default:
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }

                            switch (strategy.Renormalize)
                            {
                                case MergeRecursiveRenormalize.DoRenormalize:
                                    command.Append(StrategyOptionPrefix + "renomalize");
                                    break;

                                case MergeRecursiveRenormalize.NoRenormalize:
                                    command.Append(StrategyOptionPrefix + "no-renormalize");
                                    break;

                                case MergeRecursiveRenormalize.None:
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }

                            switch (strategy.Strategy)
                            {
                                case MergeRecursiveStrategy.Ours:
                                    command.Append(StrategyOptionPrefix + "ours");
                                    break;

                                case MergeRecursiveStrategy.Theirs:
                                    command.Append(StrategyOptionPrefix + "theirs");
                                    break;

                                case MergeRecursiveStrategy.None:
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }

                            switch (strategy.Whitespace)
                            {
                                case MergeRecursiveWhitespace.IgnoreAllSpaces:
                                    command.Append(StrategyOptionPrefix + "ignore-all-space");
                                    break;

                                case MergeRecursiveWhitespace.IgnoreSpaceAtEndOfLine:
                                    command.Append(StrategyOptionPrefix + "ignore-space-at-eol");
                                    break;

                                case MergeRecursiveWhitespace.IgnoreWhitespaceChanges:
                                    command.Append(StrategyOptionPrefix + "ignore-space-change");
                                    break;

                                case MergeRecursiveWhitespace.None:
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        break;

                    case MergeStrategyType.Resolve:
                        command.Append(StrategyPrefix + "resolve");
                        break;

                    case MergeStrategyType.Default:
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private MergeCommandResult ExecuteMerge(string command)
        {
            using (var buffer = new ByteBuffer())
            using (var fatalMessageBuffer = new StringBuffer())
            using (Tracer.TraceCommand(Command, command, userData: _userData))
            using (IProcess process = CreateProcess(command))
            {
                try
                {
                    // Gather all of stderr, but wait until the command has
                    // completed before throwing when we see an error in it.
                    var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                    int index = 0;
                    int read = 0;

                    int r;
                    while (read < buffer.Length && (r = process.StdOut.Read(buffer, read, buffer.Length - read)) > 0)
                    {
                        read += r;
                        while (index < read)
                        {
                            int eol;
                            if ((eol = buffer.FirstIndexOf(Eol, index, read - index)) < 0)
                                break;

                            ParseMergeOutputLine(buffer, index, (eol - index), fatalMessageBuffer);

                            index = eol + 1;
                        }

                        MakeSpace(buffer, ref index, ref read);
                    }

                    return ReportMergeResult(process, stderrTask, fatalMessageBuffer);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(MergeCommand)}.{nameof(ExecuteMerge)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        /// <summary>
        /// Parse a single line of output from a "git merge" command.
        /// </summary>
        private void ParseMergeOutputLine(ByteBuffer buffer, int index, int count, StringBuffer fatalMessageBuffer)
        {
            // git merge is very chatty and some messages are localized.
            // Extract what we can from each line of output and ignore the rest.
            //
            // Our only critical task is to try to identify what actually
            // happened (actual merge, fast-forward, etc.).

            // once we've seen a fatal indicator, drain all output
            if (fatalMessageBuffer.Length > 0)
            {
                fatalMessageBuffer.Append(Encoding.UTF8.GetString(buffer, index, count));
                return;
            }

            // Match both "Already up-to-date." and "Already up to date" (note the hyphen vs space).
            if (buffer.StartsWith("Already up", index, count))
            {
                _result = MergeCommandResult.AlreadyUpToDate;
                return;
            }

            if (buffer.StartsWith("Fast-forward", index, count))
            {
                _result = MergeCommandResult.FastForwardMerge;
                return;
            }

            if (buffer.StartsWith("Merge made by the", index, count))
            {
                // One of the actual merge algorithms ran. We don't care which one.
                // We don't always get this, so we cannot rely on it.
                _result = MergeCommandResult.NonFastForwardMerge;
                return;
            }

            if (buffer.StartsWith("CONFLICT ", index, count))
            {
                // These "CONFLICT" messages are printed by recursive merge
                // and they are localized and other strategies may have
                // different messages, so we don't depend on them.
                _sumMergeConflicts++;
                _result = MergeCommandResult.Conflict;
                return;
            }

            if (buffer.StartsWith("Automatic merge failed; fix conflicts", index, count))
            {
                // This is the last thing printed when there are conflicts
                // and git will exit with 1.  However, it might be localized
                // so we don't rely on it.
                _sawMergeConflictMessage = true;
                _result = MergeCommandResult.Conflict;
                return;
            }

            if (buffer.StartsWith(FatalPrefix, index, count))
            {
                fatalMessageBuffer.Append(Encoding.UTF8.GetString(buffer, index, count));
                _result = MergeCommandResult.Undefined;
                return;
            }

            // Disregard other chatty message lines.
            return;
        }

        /// <summary>
        /// Extract any merge status from the error buffer that we can.
        /// </summary>
        private void ParseMergeErrorBuffer(string standardError, StringBuffer fatalMessageBuffer)
        {
            using (var reader = new StringReader(standardError))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // if this line or any previous line started with "fatal:" append to the fatal message buffer
                    if (fatalMessageBuffer.Length > 0
                        || line.StartsWith(FatalPrefix, StringComparison.Ordinal))
                    {
                        fatalMessageBuffer.Append(line);
                        _result = MergeCommandResult.Undefined;
                    }

                    if (line.StartsWith(StoppedBeforeCommit, StringComparison.Ordinal))
                    {
                        Debug.Assert((_result == MergeCommandResult.NonFastForwardMerge) || (_result == MergeCommandResult.Undefined));

                        _result = MergeCommandResult.NonFastForwadMergeNoCommit;
                        // Let loop keep going (in case there is a fatal error later in the buffer).
                    }
                }
            }
        }

        private MergeCommandResult ReportMergeResult(IProcess process, Task<string> readStderrTask, StringBuffer fatalMessageBuffer)
        {
            process.WaitForExit();

            int exitCode = process.ExitCode;

            var standardError = readStderrTask.Result;

            ParseMergeErrorBuffer(standardError, fatalMessageBuffer);

            if ((fatalMessageBuffer != null) && (fatalMessageBuffer.Length > 0))
                throw new GitFatalException(fatalMessageBuffer.ToString());

            if (exitCode == 0)
                return _result;

            if (exitCode == 1)
            {
                // These are only Debug.Asserts() because we may not see them
                // because of localization issues.
                Debug.Assert((_sumMergeConflicts > 0) || (_sawMergeConflictMessage));
                Debug.Assert(_result == MergeCommandResult.Conflict);

                return MergeCommandResult.Conflict;
            }

            TestExitCode(exitCode, standardError);
            return MergeCommandResult.Undefined;
        }
    }
}
