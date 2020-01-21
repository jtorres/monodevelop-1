//*************************************************************************************************
// DifferenceEngine.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using Microsoft.TeamFoundation.GitApi.Cli;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of Git diff-tree.
    /// <para/>
    /// Long-running operation designed to optimize scenarios where multiple difference operations are needed for the same repository.
    /// <para/>
    /// Returned by `<see cref="IRepository.OpenDifferenceEngine(DifferenceOptions)"/>`.
    /// </summary>
    public interface IDifferenceEngine : IDisposable
    {
        /// <summary>
        /// Gets the options used to intialize this instance.
        /// </summary>
        DifferenceOptions Options { get; }

        /// <summary>
        /// Compares two revisions.
        /// <para/>
        /// Returns an `<see cref="ITreeDifference"/>` describing the differences between the revisions.
        /// </summary>
        /// <param name="source">The left-hand side, or "ours", of the comparison.</param>
        /// <param name="target">The right-hand side, or "theirs", of the comparison.</param>
        ITreeDifference CompareCommits(ICommit source, ICommit target);
    }

    internal class DifferenceEngine : CriticalFinalizerObject, IDifferenceEngine
    {
        const char Break = DiffCommand.Break;
        const char Eol = DiffCommand.Eol;
        const char Nul = DiffCommand.Nul;
        const char Separator = DiffCommand.Separator;

        public DifferenceEngine(DiffTreeCommand diffTree, DifferenceOptions options, IProcess process)
        {
            if (diffTree == null)
                throw new ArgumentNullException(nameof(diffTree));
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            _difftree = diffTree;
            _options = options;
            _process = process;
            _syncpoint = new object();
        }

        ~DifferenceEngine()
        {
            Dispose();
        }

        private DiffTreeCommand _difftree;
        private bool _isBusy;
        private DifferenceOptions _options;
        private IProcess _process;
        private readonly object _syncpoint;

        public bool IsBusy
            => Volatile.Read(ref _isBusy);

        public DifferenceOptions Options
            => _options;

        public ITreeDifference CompareCommits(ICommit source, ICommit target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            lock (_syncpoint)
            {
                _isBusy = true;

                _process.StandardInput.Write(source.ObjectId);
                _process.StandardInput.Write(Separator);
                _process.StandardInput.Write(target.ObjectId);
                _process.StandardInput.Write(Eol);

                // write a break char to the feed to force the Cli.Diff reader to exit
                _process.StandardInput.Write(Break);
                _process.StandardInput.Write(Eol);

                _difftree.ValidateHeadObjectId(_process, source.ObjectId);

                // we need to actually collect the data into a structure because the method is yielding
                // if we do not, the process will stay blocked and cross command errors will ensue if this instance is
                // used again
                return _difftree.ParseDiffOutput(_process.StdOut, _options);
            }
        }

        public void Dispose()
        {
            IProcess process;
            if ((process = Interlocked.Exchange(ref _process, null)) != null)
            {
                try
                {
                    // Close the process' standard input handle, this prevents the it from
                    // get stuck attempting to read data that is never coming. Since it may
                    // have previously been closed and NetFx, for reasons unknown, throws
                    // if that's the case we need to guard against that.
                    process.StdIn.Close();
                }
                catch { /* squelch */ }

                process.WaitForExit();

                process.Dispose();
            }

            _difftree = null;
        }
    }
}
