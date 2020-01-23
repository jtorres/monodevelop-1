//*************************************************************************************************
// TreeDifferenceCopiedEntry.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;
using static System.FormattableString;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Tree-difference entry which has probably been renamed and thus accounts for an
    /// add to the current tree.
    /// <para/>
    /// Since Git does not explicitly track files, and therefore does not explicitly track file copying,
    /// it must use heuristics to determine if an "added" entry is actually a copy of another entry.
    /// <para/>
    /// See also: `<seealso cref="Confidence"/>`.
    /// </summary>
    public interface ITreeDifferenceCopiedEntry : IEquatable<ITreeDifferenceCopiedEntry>, ITreeDifferenceEntry
    {
        /// <summary>
        /// Gets the confidence, from 0 (least) to 100 (most), that this entry was copied from `<see cref="OriginalPath"/>`.
        /// <para/>
        /// Since Git does not explicitly track files, and therefore does not explicitly track file copying,
        /// it must use heuristics to determine if an "added" entry is actually a copy of another entry.
        /// </summary>
        int Confidence { get; }

        /// <summary>
        /// Gets the current path the entry was likely copied to, relative to the root of the worktree.
        /// </summary>
        string CurrentPath { get; }

        /// <summary>
        /// Gets the original path the entry was likely copied from, relative to the root of the worktree.
        /// </summary>
        string OriginalPath { get; }
    }

    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    [JsonObject(MemberSerialization.OptIn)]
    internal class TreeDifferenceCopiedEntry : TreeDifferenceEntry, IEquatable<TreeDifferenceCopiedEntry>, ITreeDifferenceCopiedEntry
    {
        public TreeDifferenceCopiedEntry(StringUtf8 originalPath,
                                         StringUtf8 renamedPath,
                                         int confidence,
                                          ITreeDifferenceDetail targetDetail,
                                          ITreeDifferenceDetail sourceDetail)
            : this(originalPath, renamedPath, confidence, targetDetail, new[] { sourceDetail })
        { }

        public TreeDifferenceCopiedEntry(StringUtf8 originalPath,
                                         StringUtf8 renamedPath,
                                         int confidence,
                                         ITreeDifferenceDetail targetDetail,
                                         ITreeDifferenceDetail[] sourceDetails)
                         : base(renamedPath, targetDetail, sourceDetails)
        {
            if (originalPath is null)
                throw new ArgumentNullException(nameof(originalPath));
            if (double.IsNaN(confidence))
                throw new ArgumentException(nameof(confidence));

            _confidence = confidence;
            _originalPath = originalPath;
        }

        private int _confidence;
        private StringUtf8 _originalPath;

        [JsonProperty]
        public int Confidence
        {
            get { return _confidence; }
        }

        public string CurrentPath
            => Path;

        public StringUtf8 CurrentPathUtf8
            => PathUtf8;

        public override TreeDifferenceType DifferenceType
        {
            get { return TreeDifferenceType.Renamed; }
        }

        [JsonProperty]
        public string OriginalPath
        {
            get { return (string)_originalPath; }
        }

        public StringUtf8 OriginalPathUtf8
        {
            get { return _originalPath; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private string DebuggerDisplay
        {
            get { return Invariant($"{nameof(TreeDifferenceCopiedEntry)}: \"{OriginalPathUtf8}\" -> \"{CurrentPathUtf8}\" [{_confidence}:00]"); }
        }

        public bool Equals(TreeDifferenceCopiedEntry other)
            => Comparer.Equals(this, other);

        public bool Equals(ITreeDifferenceCopiedEntry other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            return (obj is TreeDifferenceCopiedEntry a
                    && Equals(a))
                || (obj is TreeDifferenceCopiedEntry b
                    && Equals(b))
                || base.Equals(obj);
        }

        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        public override string ToString()
        {
            return Invariant($"\"{OriginalPathUtf8}\" -> \"{CurrentPathUtf8}\" [{_confidence}:00]");
        }
    }
}
