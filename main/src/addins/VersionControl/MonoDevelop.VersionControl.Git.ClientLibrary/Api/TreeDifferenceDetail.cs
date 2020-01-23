//*************************************************************************************************
// TreeDifferenceDetail.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using Microsoft.TeamFoundation.GitApi.Internal;
using System;
using System.Diagnostics;
using System.Text;
using static System.FormattableString;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Details of the state of a file in a tree-difference.
    /// </summary>
    public interface ITreeDifferenceDetail : IEquatable<ITreeDifferenceDetail>
    {
        /// <summary>
        /// Gets the mode of the file the tree-entry represents.
        /// </summary>
        TreeEntryDetailMode Mode { get; }

        /// <summary>
        /// Gets the object identity of the file the tree-entry represents.
        /// </summary>
        ObjectId ObjectId { get; }

        /// <summary>
        /// Gets the type of change the tree-entry represents.
        /// </summary>
        TreeDifferenceType Type { get; }
    }

    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    internal class TreeDifferenceDetail : IEquatable<TreeDifferenceDetail>, ILoggable, ITreeDifferenceDetail
    {
        public static readonly TreeDifferenceDetailComparer Comparer = new TreeDifferenceDetailComparer();

        public TreeDifferenceDetail(ObjectId objectId,
                                    TreeEntryDetailMode mode,
                                    TreeDifferenceType type)
        {
            _mode = mode;
            _objectId = objectId;
            _type = type;
        }

        private TreeEntryDetailMode _mode;
        private ObjectId _objectId;
        private TreeDifferenceType _type;

        public TreeEntryDetailMode Mode
        {
            get { return _mode; }
        }

        public ObjectId ObjectId
        {
            get { return _objectId; }
        }

        public TreeDifferenceType Type
        {
            get { return _type; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private string DebuggerDisplay
        {
            get { return Invariant($"{nameof(TreeDifferenceDetail)}: {nameof(Mode)}: {_mode}, {nameof(ObjectId)}: {_objectId}, {nameof(Type)}: {_type}"); }
        }

        public bool Equals(TreeDifferenceDetail other)
            => Comparer.Equals(this, other);

        public bool Equals(ITreeDifferenceDetail other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            return (obj is TreeDifferenceDetail a
                    && Equals(a))
                || (obj is ITreeDifferenceDetail b
                    && Equals(b))
                || base.Equals(obj);
        }

        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        public override string ToString()
        {
            return Invariant($"{_mode} {_objectId} {_type}");
        }

        void ILoggable.Log(ExecutionContext context, StringBuilder log, int indent)
        {
            var padding = context.ParseHelper.GetParseErrorIndent(indent);
            log.Append(padding).AppendLine($"TreeDifferenceDetail {{ Mode: {_mode}, Type: {_type}, ObjectId: {_objectId} }}");
        }
    }

    /// <summary>
    /// An ITreeDifferenceDetail implementation that is optimized to hold --name-status data.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    internal class TreeDifferenceNameStatusDetail : IEquatable<TreeDifferenceNameStatusDetail>, ILoggable, ITreeDifferenceDetail
    {
        public static readonly TreeDifferenceNameStatusDetail Unmodified = new TreeDifferenceNameStatusDetail(TreeDifferenceType.Unmodified);

        public TreeDifferenceNameStatusDetail(TreeDifferenceType type)
        {
            _type = type;
        }

        private TreeDifferenceType _type;

        public TreeEntryDetailMode Mode
        {
            get { return TreeEntryDetailMode.Nonexistent; }
        }

        public ObjectId ObjectId
        {
            get { return ObjectId.Zero; }
        }

        public TreeDifferenceType Type
        {
            get { return _type; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private string DebuggerDisplay
        {
            get { return Invariant($"{nameof(TreeDifferenceNameStatusDetail)}: {nameof(Type)}: {_type}"); }
        }

        public bool Equals(TreeDifferenceNameStatusDetail other)
            => TreeDifferenceDetail.Comparer.Equals(this, other);

        public bool Equals(ITreeDifferenceDetail other)
            => TreeDifferenceDetail.Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            return (obj is TreeDifferenceNameStatusDetail a
                    && Equals(a))
                || (obj is ITreeDifferenceDetail b
                    && Equals(b))
                || base.Equals(obj);
        }

        public override int GetHashCode()
            => TreeDifferenceDetail.Comparer.GetHashCode(this);

        public override string ToString()
        {
            return Invariant($"{_type}");
        }

        void ILoggable.Log(ExecutionContext context, StringBuilder log, int indent)
        {
            var padding = context.ParseHelper.GetParseErrorIndent(indent);
            log.Append(padding).AppendLine($"TreeDifferenceNameStatusDetail {{ Type: {_type} }}");
        }
    }
}
