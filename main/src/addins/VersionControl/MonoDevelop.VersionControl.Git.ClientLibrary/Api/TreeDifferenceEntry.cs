//*************************************************************************************************
// TreeDifferenceEntry.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;
using static System.FormattableString;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Entry in a difference tree.
    /// </summary>
    public interface ITreeDifferenceEntry : IEnumerable<ITreeDifferenceDetail>, IEquatable<ITreeDifferenceEntry>
    {
        /// <summary>
        /// Gets the type of difference this `<see cref="ITreeDifferenceEntry"/>` represents.
        /// <para>The value is always `<see cref="TreeDifferenceType.Merged"/>` if the entry has multiple sources.</para>
        /// </summary>
        TreeDifferenceType DifferenceType { get; }

        /// <summary>
        /// Gets the path, relative to the root of the worktree, of the entry.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets details of the parent(s) of `<see cref="Target"/>`.
        /// </summary>
        IReadOnlyList<ITreeDifferenceDetail> Sources { get; }

        /// <summary>
        /// Gets details of the child of `<see cref="Sources"/>`.
        /// </summary>
        ITreeDifferenceDetail Target { get; }
    }

    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    [JsonObject(MemberSerialization.OptIn)]
    internal class TreeDifferenceEntry : Base, IEquatable<TreeDifferenceEntry>, ILoggable, ITreeDifferenceEntry
    {
        public static readonly TreeDifferenceEntryComparer Comparer = new TreeDifferenceEntryComparer();

        public TreeDifferenceEntry(StringUtf8 path, ITreeDifferenceDetail target, ITreeDifferenceDetail sources)
            : this(path, target, new[] { sources })
        { }

        public TreeDifferenceEntry(StringUtf8 path, ITreeDifferenceDetail target, ITreeDifferenceDetail[] sources)
            : base()
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));
            if (sources is null)
                throw new ArgumentNullException(nameof(sources));
            if (sources.Length < 1)
                throw new ArgumentException(nameof(sources));

            _path = path;
            _tgtDetails = target;
            _srcDetails = sources;
        }

        private StringUtf8 _path;
        private ITreeDifferenceDetail[] _srcDetails;
        private ITreeDifferenceDetail _tgtDetails;
        public virtual TreeDifferenceType DifferenceType
        {
            get
            {
                if (_srcDetails.Length == 1)
                    return _srcDetails[0].Type;

                return TreeDifferenceType.Merged;
            }
        }

        [JsonProperty]
        public string Path
        {
            get { return (string)_path; }
        }

        public StringUtf8 PathUtf8
        {
            get { return _path; }
        }

        [JsonProperty]
        public IReadOnlyList<ITreeDifferenceDetail> Sources
        {
            get { return _srcDetails; }
        }

        [JsonProperty]
        public ITreeDifferenceDetail Target
        {
            get { return _tgtDetails; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private string DebuggerDisplay
        {
            get { return Invariant($"{nameof(TreeDifferenceEntry)}: \"{_path}\""); }
        }

        public bool Equals(TreeDifferenceEntry other)
            => Comparer.Equals(this, other);

        public bool Equals(ITreeDifferenceEntry other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            return (obj is TreeDifferenceEntry a
                    && Equals(a))
                || (obj is ITreeDifferenceEntry b
                    && Equals(b))
                || base.Equals(obj);
        }

        public IEnumerator<ITreeDifferenceDetail> GetEnumerator()
        {
            var entries = new ITreeDifferenceDetail[_srcDetails.Length + 1];

            // Copy the source details first because presents them first.
            Array.Copy(_srcDetails, entries, _srcDetails.Length);
            // The final entry is always the target entry, because that's when Git presents it.
            entries[entries.Length - 1] = _tgtDetails;

            for (int i = 0; i < entries.Length; i += 1)
            {
                yield return entries[i];
            }
        }

        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        public override string ToString()
        {
            return Path;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        void ILoggable.Log(ExecutionContext context, StringBuilder log, int indent)
        {
            string prefix = context.ParseHelper.GetParseErrorIndent(indent);
            string prefix2 = context.ParseHelper.GetParseErrorIndent(indent + 1);

            log.Append(prefix).AppendLine(nameof(TreeDifferenceEntry));
            log.Append(prefix).AppendLine("{");
            {
                log.Append(prefix2).Append("path: \"").Append(Path).AppendLine("\"");

                log.Append(prefix2).AppendLine("target:");
                log.Append(prefix2).AppendLine("{");
                {
                    (Target as ILoggable)?.Log(context, log, indent + 2);
                }
                log.Append(prefix2).AppendLine("}");

                log.Append(prefix2).AppendLine($"sources[{Sources.Count}]:");
                log.Append(prefix2).AppendLine("{");
                {
                    foreach (TreeDifferenceDetail source in Sources)
                    {
                        (source as ILoggable)?.Log(context, log, indent + 2);
                    }
                }
                log.Append(prefix2).AppendLine("}");
            }
            log.Append(prefix).AppendLine("}");
        }
    }
}
