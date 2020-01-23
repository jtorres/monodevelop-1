//*************************************************************************************************
// Branch.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model wrapper for a Git branch reference
    /// </summary>
    public interface IBranch : IBranchName, IComparable<IBranch>, IEquatable<IBranch>, IReference
    {
        /// <summary>
        /// Gets `<see langword="true"/>` if this branch is the current HEAD of the repository; otherwise `<see langword="false"/>`.
        /// </summary>
        bool IsHead { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` if this instance is a member of refs/heads; otherwise `<see langword="false"/>`.
        /// </summary>
        bool IsLocal { get; }

        /// <summary>
        /// Gets the name of the push target branch
        /// </summary>
        string PushTargetName { get; }

        /// <summary>
        /// Gets the remote name, if available; otherwise `<see langword="null"/>`.
        /// </summary>
        string RemoteName { get; }

        /// <summary>
        /// Gets the name of the upstream branch; `<see langword="null"/>` if there is no upstream.
        /// </summary>
        string UpstreamName { get; }
    }

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay, nq}")]
    [JsonObject(MemberSerialization.OptIn)]
    internal class Branch : Reference, IBranch, IComparable<Branch>, IEquatable<Branch>, IEquatable<BranchName>
    {
        public Branch(StringUtf8 canonicalName, ObjectId objectId, ObjectType objectType)
            : base(canonicalName, objectId, objectType)
        {
            if (_type != ReferenceType.Heads && _type != ReferenceType.Remotes)
                throw new ReferenceTypeMismatchException();

            if (_type == ReferenceType.Remotes)
            {
                _remoteNameUtf8 = _canonicalName.Substring(ReferenceName.PatternRefRemotes.Length);

                int separator = _remoteNameUtf8.FirstIndexOf('/');
                if (separator > 0)
                {
                    _remoteNameUtf8 = _remoteNameUtf8.Substring(0, separator);
                }
            }
        }

        private bool _isHead;
        private StringUtf8 _pushTargetNameUtf8;
        private readonly StringUtf8 _remoteNameUtf8;
        private StringUtf8 _upstreamNameUtf8;

        [JsonProperty]
        public bool IsHead
        {
            get { return _isHead; }
            internal set { _isHead = value; }
        }

        [JsonProperty]
        public bool IsLocal
        {
            get { return ReferenceType == ReferenceType.Heads; }
        }

        [JsonProperty]
        public string LocalName
        {
            get { return (string)LocalNameUtf8; }
        }

        public StringUtf8 LocalNameUtf8
        {
            get
            {
                var localName = _friendlyName;

                if (_type == ReferenceType.Remotes)
                {
                    int separator = localName.FirstIndexOf('/');
                    if (separator > 0)
                    {
                        localName = localName.Substring(separator + 1);
                    }
                }

                return localName;
            }
        }

        [JsonProperty]
        public string PushTargetName
        {
            get
            {
                string pushName = (string)_pushTargetNameUtf8;

                if (_cache == null)
                    return pushName;

                return _cache.Intern(pushName);
            }
        }

        public StringUtf8 PushTargetNameUtf8
        {
            get { return _pushTargetNameUtf8; }
            set { _pushTargetNameUtf8 = value; }
        }

        [JsonProperty]
        public string RemoteName
        {
            get { return (string)_remoteNameUtf8; }
        }

        public StringUtf8 RemoteNameUtf8
        {
            get { return _remoteNameUtf8; }
        }

        [JsonProperty]
        public string UpstreamName
        {
            get
            {
                string upstreamName = (string)_upstreamNameUtf8;

                if (_cache == null)
                    return upstreamName;

                return _cache.Intern(upstreamName);
            }
        }

        public StringUtf8 UpstreamNameUtf8
        {
            get { return _upstreamNameUtf8; }
            set { _upstreamNameUtf8 = value; }
        }

        protected new string DebuggerDisplay
        {
            get
            {
                if (_canonicalName == null)
                    return FormattableString.Invariant($"{nameof(Branch)}: null");

                return (_upstreamNameUtf8 == null && _pushTargetNameUtf8 == null)
                    ? FormattableString.Invariant($"{nameof(Branch)}: {_canonicalName}")
                    : (_remoteNameUtf8 == null)
                        ? FormattableString.Invariant($"{nameof(Branch)}: {_canonicalName} => {_upstreamNameUtf8 ?? _pushTargetNameUtf8}")
                        : FormattableString.Invariant($"{nameof(Branch)}: {_canonicalName} => {_remoteNameUtf8}/{_upstreamNameUtf8 ?? _pushTargetNameUtf8}");
            }
        }

        public int CompareTo(Branch other)
            => Comparer.Compare(this, other);

        public int CompareTo(IBranch other)
            => Comparer.Compare(this, other);

        public bool Equals(Branch other)
            => Comparer.Equals(this, other);

        public bool Equals(IBranch other)
            => Comparer.Equals(this, other);

        public bool Equals(BranchName other)
            => Comparer.Equals(this, other);

        public bool Equals(IBranchName other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;

            return Equals(obj as Branch)
                || Equals(obj as IBranch)
                || Equals(obj as BranchName)
                || Equals(obj as IBranchName)
                || base.Equals(obj);
        }

        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        public static bool IsLegalFullyQualifiedName(string branchName)
            => BranchName.IsLegalFullyQualifiedName(branchName);
    }
}
