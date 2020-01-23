//*************************************************************************************************
// BranchName.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// The object model representation of a name of a Git branch reference.
    /// </summary>
    public interface IBranchName : IEquatable<IBranchName>, IReferenceName
    {
        /// <summary>
        /// <summary>
        /// Gets the friendly name of the branch as if it were a local branch.
        /// <para/>
        /// For example, 'origin/master' =&gt; 'master', while 'master' =&gt; 'master'.
        /// </summary>
        string LocalName { get; }
    }

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay, nq}")]
    public sealed class BranchName : ReferenceName, IBranchName, IEquatable<BranchName>
    {
        public BranchName(string name)
            : this((StringUtf8)name)
        { }

        internal BranchName(StringUtf8 name)
            : base(name)
        { }

        public string LocalName
        {
            get
            {
                var localName = _friendlyName;

                if (CanonicalNameUtf8.StartsWith(PatternRefRemotes))
                {
                    int separator = localName.FirstIndexOf('/');
                    if (separator > 0)
                    {
                        localName = localName.Substring(separator + 1);
                    }
                }

                return (string)localName;
            }
        }

        /// <summary>
        /// Determine if branchName is a syntactically valid
        /// full branch name (beginning with "refs/heads/").
        /// </summary>
        public static bool IsLegalFullyQualifiedName(string branchName)
        {
            if (branchName == null)
                return false;

            // branch names must start with either refs/heads/ or refs/remotes/.
            if (!branchName.StartsWith(PatternRefHeads, StringComparison.Ordinal)
                && !branchName.StartsWith(PatternRefRemotes, StringComparison.Ordinal))
                return false;

            // go ahead and pass the fully-qualified name to avoid
            // allocating a substring.
            return ReferenceName.IsLegalName(branchName);
        }

        public bool Equals(BranchName other)
            => Comparer.Equals(this, other);

        public bool Equals(IBranchName other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            var a = obj as BranchName;
            if (!ReferenceEquals(a, null))
                return Equals(a);

            var b = obj as IBranchName;
            if (!ReferenceEquals(b, null))
                return Equals(b);

            return base.Equals(obj);
        }

        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        public override string ToString()
        {
            return base.ToString();
        }

        protected override void ParseName(StringUtf8 name, out StringUtf8 canonicalName, out StringUtf8 friendlyName)
        {
            var local = (string)name;

            if (IsLegalFullyQualifiedName(local))
            {
                canonicalName = name;
                friendlyName = (name.StartsWith(PatternRefHeads))
                    ? name.Substring(PatternRefHeads.Length)
                    : name.Substring(PatternRefRemotes.Length);
            }
            else if (IsLegalName(local))
            {
                friendlyName = name;
                canonicalName = PatternRefHeadsUtf8 + _friendlyName;
            }
            else
            {
                throw BranchNameException.FromName(local);
            }
        }

        public static explicit operator BranchName(string name)
        {
            if (name == null)
                return null;

            return new BranchName(name);
        }

        public static explicit operator string(BranchName branchName)
        {
            if (ReferenceEquals(branchName, null))
                return null;

            return branchName.RevisionText;
        }
    }
}
