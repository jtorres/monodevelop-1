﻿//*************************************************************************************************
// CherryPickCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    [GitErrorMapping(typeof(AmbiguousCherryPickOfMergeException), Suffix = "is a merge but no -m option was given.")]
    [GitErrorMapping(typeof(WorkingDirectoryUncommittedException), Prefix = "error: your local changes would be overwritten")]
    [GitErrorMapping(typeof(WorkingDirectoryUnstagedException), Prefix = WorkingDirectoryLocalChangesException.ErrorPrefix)]
    [GitErrorMapping(typeof(WorkingDirectoryUnstagedException), Prefix = WorkingDirectoryUnstagedException.UntrackedPrefix)]
    [GitErrorMapping(typeof(MergeInProgressException), Suffix = MergeInProgressException.ErrorSuffix)]
    [GitErrorMapping(typeof(CherryPickConflictException), Prefix = ErrorConflicts)]
    [GitErrorMapping(typeof(CherryPickConflictException), Prefix = ErrorBinaryConflicts)]
    [GitErrorMapping(typeof(EmptyCommitException), Prefix = ErrorNothingToCommit)]
    [GitErrorMapping(typeof(EmptyCommitMessageException), Prefix = EmptyCommitMessageException.MessagePrefix)]
    internal class CherryPickCommand : GitCommand
    {
        public const string Command = "cherry-pick";
        public const string ErrorConflicts = "error: could not apply";
        public const string ErrorBinaryConflicts = "warning: Cannot merge binary files";
        public const string ErrorNothingToCommit = "The previous cherry-pick is now empty";

        public CherryPickCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        /// <summary>
        /// Cancel the operation and return to the pre-sequence state.
        /// </summary>
        public void Abort()
        {
            ExecuteCherryPickCommand("--abort", null);
        }

        /// <summary>
        /// Continue the operation in progress using the information in .git/sequencer. Can be used
        /// to continue after resolving conflicts in a failed cherry-pick or revert.
        /// </summary>
        public void Continue()
        {
            ExecuteCherryPickCommand("--continue", null);
        }

        /// <summary>
        /// Given a range of existing commits, apply the change each one introduces, recording a new
        /// commit for each. This requires your working tree to be clean (no modifications from the
        /// HEAD commit).
        /// </summary>
        /// <param name="range">Commits to cherry-pick.</param>
        /// <param name="options">Options which affect how the cherry-pick operation is completed
        /// see <see cref="CherryPickOptions"/> for more information.</param>
        public void PickRange(InclusiveRange range, CherryPickOptions options)
        {
            if (range == null)
                throw new ArgumentNullException(nameof(range));

            ExecuteCherryPickCommand(range.ToString(), options);
        }

        /// <summary>
        /// Applies the change, of a given commit, introduces, recording a new commit for each. This
        /// requires your working tree to be clean (no modifications from the HEAD commit).
        /// </summary>
        /// <param name="range">Commits to cherry-pick.</param>
        /// <param name="options">Options which affect how the cherry-pick operation is completed
        /// see <see cref="CherryPickOptions"/> for more information.</param>
        public void PickRevision(IRevision revision, CherryPickOptions options)
        {
            if (revision == null)
                throw new ArgumentNullException(nameof(revision));

            ExecuteCherryPickCommand(revision.RevisionText, options);
        }

        /// <summary>
        /// Forget about the current operation in progress. Can be used to clear the sequencer
        /// state after a failed cherry-pick or revert.
        /// </summary>
        public void Quit()
        {
            ExecuteCherryPickCommand("--quit", null);
        }

        private void ApplyOptions(StringBuffer command, CherryPickOptions options)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if ((options.Flags & CherryPickOptionFlags.AllowEmptyCommits) != 0)
            {
                command.Append(" --allow-empty");
            }

            if ((options.Flags & CherryPickOptionFlags.AllowEmptyMessage) != 0)
            {
                command.Append(" --allow-empty-message");
            }

            if ((options.Flags & CherryPickOptionFlags.AppendCherryPickedMessage) != 0)
            {
                command.Append(" -x");
            }

            if ((options.Flags & CherryPickOptionFlags.AllowFastFoward) != 0)
            {
                command.Append(" -ff");
            }

            if ((options.Flags & CherryPickOptionFlags.KeepRedundantCommits) != 0)
            {
                command.Append(" --keep-redundant-commits");
            }

            if (options.MergeStrategy != null)
            {
                const string StrategyPrefix = " --strategy=";
                const string StrategyOptionPrefix = " --strategy-option=";
                const string DiffAlogrithmPrefix = "diff-algoritm=";


                switch (options.MergeStrategy.Type)
                {
                    case MergeStrategyType.Octopus:
                        throw new NotImplementedException();

                    case MergeStrategyType.Ours:
                        command.Append(StrategyPrefix + "ours");
                        break;

                    case MergeStrategyType.Recursive:
                        {
                            MergeStrategyRecursive strategy = options.MergeStrategy as MergeStrategyRecursive;

                            command.Append(StrategyPrefix + "recursive");

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
                            }

                            switch (strategy.RenameDetection)
                            {
                                case RenameDetection.FollowRenames:
                                    command.Append(StrategyOptionPrefix + "find-renames");
                                    break;

                                case RenameDetection.NoRenameDetection:
                                    command.Append(StrategyOptionPrefix + "no-renames");
                                    break;
                            }

                            switch (strategy.Renormalize)
                            {
                                case MergeRecursiveRenormalize.DoRenormalize:
                                    command.Append(StrategyOptionPrefix + "renomalize");
                                    break;

                                case MergeRecursiveRenormalize.NoRenormalize:
                                    command.Append(StrategyOptionPrefix + "no-renormalize");
                                    break;
                            }

                            switch (strategy.Strategy)
                            {
                                case MergeRecursiveStrategy.Ours:
                                    command.Append(StrategyOptionPrefix + "ours");
                                    break;

                                case MergeRecursiveStrategy.Theirs:
                                    command.Append(StrategyOptionPrefix + "theirs");
                                    break;
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
                            }
                        }
                        break;

                    case MergeStrategyType.Resolve:
                        command.Append(StrategyPrefix + "resolve");
                        break;
                }
            }
        }

        private void ExecuteCherryPickCommand(string parameterText, CherryPickOptions? options)
        {
            if (string.IsNullOrEmpty(parameterText))
                throw new ArgumentException("message", nameof(parameterText));

            using (Tracer.TraceCommand(Command, parameterText, userData: _userData))
            using (StringBuffer command = new StringBuffer(Command))
            {
                if (options.HasValue)
                {
                    ApplyOptions(command, options.Value);
                }

                command.Append(' ')
                       .Append(parameterText);

                ExecuteCommand(Command, command);
            }
        }
    }
}
