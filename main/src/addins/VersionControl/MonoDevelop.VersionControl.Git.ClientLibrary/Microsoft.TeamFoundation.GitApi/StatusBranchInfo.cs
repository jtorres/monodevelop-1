/**** Git Process Management Library ****
 *
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the ""Software""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/

using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Branch and tracking information from status.
    /// <para/>
    /// Produced by `<seealso cref="IRepository.ReadStatus(StatusOptions)"/>`.
    /// </summary>
    public interface IStatusBranchInfo
    {
        /// <summary>
        /// Number of commits that the local branch differs from the upstream. Returns null if no upstream
        /// branch is set -OR- if the referenced commit is missing (gone) from the local system.
        /// </summary>
        AheadBehind AheadBehind { get; }

        /// <summary>
        /// Current commit's `<see cref="ObjectId"/>`.
        /// <para/>
        /// `<see cref="ObjectId.Zero"/>` when initial.
        /// </summary>
        ObjectId CommitId { get; }

        /// <summary>
        /// Name of the current branch when `<see cref="HeadType"/>` is
        /// `<see cref="HeadType.Normal"/>`; otherwise `<see langword="null"/>`.
        /// </summary>
        string HeadBranchName { get; }

        /// <summary>
        /// Gets the state of the repository's HEAD.
        /// </summary>
        HeadType HeadType { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if the repository is in initial commit state; otherwise `<see langword="false"/>`.
        /// </summary>
        bool IsInitialCommit { get; }

        /// <summary>
        /// Upstream branch is set, but the commit is not present.
        /// </summary>
        bool IsUpstreamBranchGone { get; }

        /// <summary>
        /// Gets the name of the current branch's upstream if any; otherwise `<see langword="null"/>`.
        /// </summary>
        string UpstreamBranchName { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class StatusBranchInfo: IStatusBranchInfo
    {
        public StatusBranchInfo()
        { }

        private AheadBehind? _aheadBehind;
        private ObjectId _commitId;
        private StringUtf8 _headBranchName = null;
        private HeadType _headType;
        private StringUtf8 _upstreamBranchName = null;

        [JsonProperty]
        public AheadBehind AheadBehind
        {
            get
            {
                if (UpstreamBranchNameValid() && _aheadBehind.HasValue)
                    return _aheadBehind.Value;

                return new AheadBehind
                {
                    Ahead = -1,
                    Behind = -1,
                };
            }
        }

        [JsonProperty]
        public ObjectId CommitId
        {
            get { return _commitId; }
            internal set { _commitId = value; }
        }

        [JsonProperty]
        public string HeadBranchName
        {
            get
            {
                if (!(_headBranchName is null))
                    return (string)_headBranchName;

                switch (_headType)
                {
                    case HeadType.Detached:
                        return Head.DetachedLabel;

                    case HeadType.Unborn:
                        return Head.UnbornLabel;

                    case HeadType.Malformed:
                    default:
                        return Head.MalformedLabel;
                }
            }
        }

        [JsonProperty]
        public HeadType HeadType
        {
            get { return _headType; }
        }

        public bool IsInitialCommit
        {
            get { return _headType == HeadType.Unborn; }
        }

        public bool IsUpstreamBranchGone
        {
            get
            {
                return (UpstreamBranchNameValid() && !_aheadBehind.HasValue);
            }
        }

        [JsonProperty]
        public string UpstreamBranchName
        {
            get { return (string)_upstreamBranchName; }
        }

        internal StringUtf8 HeadBranchNameUtf8
        {
            get { return _headBranchName; }
        }

        internal StringUtf8 UpstreamBranchNameUtf8
        {
            get { return _upstreamBranchName; }
        }

        internal void SetAheadBehindCounts(int ahead, int behind)
        {
            if (ahead < 0)
                throw new ArgumentOutOfRangeException(nameof(ahead));
            if (behind < 0)
                throw new ArgumentOutOfRangeException(nameof(behind));

            // Caller should set upstream to a branch BEFORE trying to set ahead/behind.
            Debug.Assert(UpstreamBranchNameValid());

            _aheadBehind = new AheadBehind
            {
                Ahead = ahead,
                Behind = behind,
            };
        }

        internal void SetHeadBranchName(StringUtf8 headName)
        {
            if (headName is null)
                throw new ArgumentNullException(nameof(headName));
            if ((headName.Length == 0)
                || StringUtf8Comparer.Ordinal.Equals(headName, Head.MalformedLabelUtf8)
                || StringUtf8Comparer.Ordinal.Equals(headName, Head.DetachedLabelUtf8))
                throw new ArgumentException(nameof(headName));

            _headBranchName = headName;
            _headType = HeadType.Normal;
        }

        internal void SetHeadIsDetached()
        {
            _headBranchName = null;
            _headType = HeadType.Detached;
        }

        internal void SetHeadIsUnknown()
        {
            _headBranchName = null;
            _headType = HeadType.Unknown;
        }

        internal void SetUpstreamBranchName(StringUtf8 upstreamBranchName)
        {
            if (upstreamBranchName != null && upstreamBranchName.Length > 0)
            {
                _upstreamBranchName = upstreamBranchName;
            }
            else
            {
                _upstreamBranchName = null;
            }

            _aheadBehind = null;
        }

        private bool UpstreamBranchNameValid()
        {
            return (_upstreamBranchName != null && _upstreamBranchName.Length > 0);
        }
    }

    public struct AheadBehind: IEquatable<AheadBehind>
    {
        private int _ahead;
        private int _behind;

        /// <summary>
        /// Gets the number of commits the current reference is ahead of its upstream counterpart.
        /// <para/>
        /// Note that a reference can simultaneously be both ahead and behind its upstream counterpart.
        /// </summary>
        public int Ahead
        {
            get { return _ahead; }
            internal set { _ahead = value; }
        }

        /// <summary>
        /// Gets the number of commits the current reference is behind of its upstream counterpart.
        /// <para/>
        /// Note that a reference can simultaneously be both ahead and behind its upstream counterpart.
        /// </summary>
        public int Behind
        {
            get { return _behind; }
            internal set { _behind = value; }
        }

        public bool Equals(AheadBehind other)
        {
            return _ahead == other._ahead
                && _behind == other._behind;
        }

        public override bool Equals(object obj)
        {
            if (obj is AheadBehind a)
                return Equals(a);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ((_ahead & 0x0000FFFF) << 16)
                 + (_behind & 0x0000FFFF);
        }

        public override string ToString()
        {
            return FormattableString.Invariant($"{nameof(Ahead)}: {_ahead}, {nameof(Behind)}: {_behind}");
        }

        public static bool operator ==(AheadBehind left, AheadBehind right)
            => left.Equals(right);

        public static bool operator !=(AheadBehind left, AheadBehind right)
            => !left.Equals(right);
    }
}
