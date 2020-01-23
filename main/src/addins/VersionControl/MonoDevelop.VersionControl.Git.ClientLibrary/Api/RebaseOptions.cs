//*************************************************************************************************
// RebaseOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Options related to `<see cref="IRepository.RebaseBegin(IRevision, RebaseOptions)"/>`.
    /// </summary>
    public struct RebaseOptions
    {
        public static readonly RebaseOptions Default = new RebaseOptions
        {
            Flags = RebaseOptionFlags.Default,
            MergeStrategy = null,
            NewBase = null,
            ProgressCallback = null,
            Root = null,
        };

        /// <summary>
        /// Extended options related to a Git rebase operation.
        /// </summary>
        public RebaseOptionFlags Flags;

        /// <summary>
        /// Use the given `<see cref="MergeStrategy"/>`.
        /// <para/>
        /// 
        /// Because rebase replays each commit from the working branch on top of the {newBase}
        /// branch using the given strategy, using the ours strategy simply discards all patches from
        /// the {oldbase}, which makes little sense.
        /// 
        /// <para/>
        /// Must be combined with `<see cref="RebaseOptionFlags.Merge"/>`.
        /// </summary>
        public MergeStrategy MergeStrategy;

        /// <summary>
        /// <para>Starting point at which to create the new commits.</para>
        /// <para>If not specified, the starting point is "upstream".</para>
        /// </summary>
        public IRevision NewBase;

        /// <summary>
        /// Callback delegate to receive progress updates during an operation.
        /// </summary>
        public OperationProgressDelegate ProgressCallback;

        /// <summary>
        /// Rebase all commits reachable from {oldbase}, instead of limiting them with a {newBase}.
        /// <para/>
        /// This allows you to rebase the root commit(s) on a branch.
        /// <para/>
        /// When used with {newBase}, it will skip changes already contained in it
        /// (instead of {newBase}); whereas without {newBase} it will operate on every
        /// change.
        /// <para>When used together with both {newBase} and
        /// `<see cref="RebaseOptionFlags.PreserveMerges"/>`, all root commits will be rewritten to have
        /// `<see cref="NewBase"/>` as parent instead.</para>
        /// </summary>
        public IRevision Root;

        internal bool IsValid
        {
            get
            {
                // if RebaseOptionFlags.Merge is set, insure a merge strategy is included
                return ((Flags & RebaseOptionFlags.Merge) == 0)
                    || !ReferenceEquals(MergeStrategy, null);
            }
        }
    }
}
