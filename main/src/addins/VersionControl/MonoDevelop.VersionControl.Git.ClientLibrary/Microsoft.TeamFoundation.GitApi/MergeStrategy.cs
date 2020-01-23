//*************************************************************************************************
// MergeStrategy.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Cli;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of Git merge strategy.
    /// <para/>
    /// Base class for `<seealso cref="MergeStrategyOctopus"/>`, `<seealso cref="MergeStrategyOurs"/>`, `<seealso cref="MergeStrategyRecursive"/>`, and `<seealso cref="MergeStrategyResolve"/>`.
    /// </summary>
    public abstract class MergeStrategy
    {
        public static readonly MergeStrategy Default = null;
        public static readonly MergeStrategyResolve Resolve = MergeStrategyResolve.Default;
        public static readonly MergeStrategyRecursive Recursive = MergeStrategyRecursive.Default;
        public static readonly MergeStrategyOurs Ours = MergeStrategyOurs.Default;
        public static readonly MergeStrategyOctopus Octopus = MergeStrategyOctopus.Default;

        protected const string StrategyPrefix = "--strategy=";

        internal protected MergeStrategy(MergeStrategyType type)
        {
            Type = type;
        }

        public readonly MergeStrategyType Type = MergeStrategyType.Default;

        internal abstract void ApplyOptions(ArgumentList command);
    }

    /// <summary>
    /// This can only resolve two heads (i.e. the current branch and another branch you pulled from) using a 3-way merge algorithm.
    /// <para/>
    /// It tries to carefully detect criss-cross merge ambiguities and is considered generally safe and fast.
    /// </summary>
    public sealed class MergeStrategyResolve : MergeStrategy
    {
        public new static readonly MergeStrategyResolve Default = new MergeStrategyResolve();

        private MergeStrategyResolve()
            : base(MergeStrategyType.Resolve)
        { }

        internal override void ApplyOptions(ArgumentList command)
        {
            command.AddOption(StrategyPrefix + "resolve");
        }
    }

    /// <summary>
    /// This can only resolve two heads using a 3-way merge algorithm.
    /// <para/>
    /// When there is more than one common ancestor that can be used for 3-way merge, it creates a merged tree of the common ancestors and uses that as the reference tree for the 3-way merge.
    /// <para/>
    /// Additionally this can detect and handle merges involving renames.
    /// <para/>
    /// This is the default merge strategy when pulling or merging one branch.
    /// </summary>
    public class MergeStrategyRecursive : MergeStrategy
    {
        public new static readonly MergeStrategyRecursive Default = new MergeStrategyRecursive();

        public MergeStrategyRecursive(DifferenceAlgorithm algorithm,
                                      RenameDetection renameDection,
                                      MergeRecursiveRenormalize renormalize,
                                      MergeRecursiveStrategy strategy,
                                      MergeRecursiveWhitespace whitespace)
            : base(MergeStrategyType.Recursive)
        {
            Algorithm = algorithm;
            RenameDetection = renameDection;
            Renormalize = renormalize;
            Strategy = strategy;
            Whitespace = whitespace;
        }
        private MergeStrategyRecursive()
            : base(MergeStrategyType.Recursive)
        {
            Algorithm = DifferenceAlgorithm.Default;
            RenameDetection = RenameDetection.Default;
            Renormalize = MergeRecursiveRenormalize.None;
            Strategy = MergeRecursiveStrategy.None;
            Whitespace = MergeRecursiveWhitespace.None;
        }

        /// <summary>
        /// Tells merge-recursive to use a different difference algorithm, which can help avoid mis-merges that occur due to unimportant matching lines (such as braces from distinct functions).
        /// </summary>
        public readonly DifferenceAlgorithm Algorithm;

        /// <summary>
        /// Turns on rename detection.
        /// </summary>
        public readonly RenameDetection RenameDetection;

        /// <summary>
        /// This runs a virtual check-out and check-in of all three stages of a file when resolving a three-way merge.
        /// <para/>
        /// This option is meant to be used when merging branches with different clean filters or end-of-line normalization rules.
        /// </summary>
        public readonly MergeRecursiveRenormalize Renormalize;

        /// <summary>
        /// The merge mechanism allows the back-end merge strategies to be chosen.
        /// </summary>
        public readonly MergeRecursiveStrategy Strategy;

        /// <summary>
        /// Treats lines with the indicated type of whitespace change as unchanged for the sake of a three-way merge.
        /// </summary>
        public readonly MergeRecursiveWhitespace Whitespace;

        internal override void ApplyOptions(ArgumentList command)
        {
            const string StrategyOptionPrefix = "--strategy-option=";
            const string DiffAlogrithmPrefix = "diff-algorithm=";

            command.AddOption(StrategyPrefix + "recursive");

            switch (Algorithm)
            {
                case DifferenceAlgorithm.Histogram:
                    command.AddOption(StrategyOptionPrefix + DiffAlogrithmPrefix + "histogram"); ;
                    break;

                case DifferenceAlgorithm.Minimal:
                    command.AddOption(StrategyOptionPrefix + DiffAlogrithmPrefix + "minimal"); ;
                    break;

                case DifferenceAlgorithm.Myers:
                    command.AddOption(StrategyOptionPrefix + DiffAlogrithmPrefix + "myers"); ;
                    break;

                case DifferenceAlgorithm.Patience:
                    command.AddOption(StrategyOptionPrefix + DiffAlogrithmPrefix + "patience"); ;
                    break;

                case DifferenceAlgorithm.Default:
                    break;

                default:
                    throw new ArgumentException(nameof(Algorithm));
            }

            switch (RenameDetection)
            {
                case RenameDetection.FollowRenames:
                    command.AddOption(StrategyOptionPrefix + "find-renames");
                    break;

                case RenameDetection.NoRenameDetection:
                    command.AddOption(StrategyOptionPrefix + "no-renames");
                    break;

                case RenameDetection.Default:
                    break;

                default:
                    throw new ArgumentException(nameof(RenameDetection));
            }

            switch (Renormalize)
            {
                case MergeRecursiveRenormalize.DoRenormalize:
                    command.AddOption(StrategyOptionPrefix + "renormalize");
                    break;

                case MergeRecursiveRenormalize.NoRenormalize:
                    command.AddOption(StrategyOptionPrefix + "no-renormalize");
                    break;

                case MergeRecursiveRenormalize.None:
                    break;

                default:
                    throw new ArgumentException(nameof(Renormalize));
            }

            switch (Strategy)
            {
                case MergeRecursiveStrategy.Ours:
                    command.AddOption(StrategyOptionPrefix + "ours");
                    break;

                case MergeRecursiveStrategy.Theirs:
                    command.AddOption(StrategyOptionPrefix + "theirs");
                    break;

                case MergeRecursiveStrategy.None:
                    break;

                default:
                    throw new ArgumentException(nameof(Strategy));
            }

            switch (Whitespace)
            {
                case MergeRecursiveWhitespace.IgnoreAllSpaces:
                    command.AddOption(StrategyOptionPrefix + "ignore-all-space");
                    break;

                case MergeRecursiveWhitespace.IgnoreSpaceAtEndOfLine:
                    command.AddOption(StrategyOptionPrefix + "ignore-space-at-eol");
                    break;

                case MergeRecursiveWhitespace.IgnoreWhitespaceChanges:
                    command.AddOption(StrategyOptionPrefix + "ignore-space-change");
                    break;

                case MergeRecursiveWhitespace.None:
                    break;

                default:
                    throw new ArgumentException(nameof(Whitespace));
            }
        }
    }

    /// <summary>
    /// Resolves any number of heads, but the resulting tree of the merge is always that of the current branch head, effectively ignoring all changes from all other branches.
    /// <para/>
    /// It is meant to be used to supersede old development history of side branches.
    /// <para/>
    /// Note that this is different from the "MergeRecursiveStrategy.Recursive" option to the recursive merge strategy.
    /// </summary>
    public sealed class MergeStrategyOurs : MergeStrategy
    {
        public new static readonly MergeStrategyOurs Default = new MergeStrategyOurs();

        private MergeStrategyOurs()
            : base(MergeStrategyType.Ours)
        { }

        internal override void ApplyOptions(ArgumentList command)
        {
            command.AddOption(StrategyPrefix + "ours");
        }
    }

    /// <summary>
    /// Resolves cases with more than two heads, but refuses to do a complex merge that needs manual resolution.
    /// <para/>
    /// It is primarily meant to be used for bundling topic branch heads together.
    /// <para/>
    /// This is the default merge strategy when pulling or merging more than one branch.
    /// </summary>
    public sealed class MergeStrategyOctopus : MergeStrategy
    {
        public new static readonly MergeStrategyOctopus Default = new MergeStrategyOctopus();

        private MergeStrategyOctopus()
            : base(MergeStrategyType.Octopus)
        { }

        internal override void ApplyOptions(ArgumentList command)
        {
            command.AddOption(StrategyPrefix + "octopus");
        }
    }
}
