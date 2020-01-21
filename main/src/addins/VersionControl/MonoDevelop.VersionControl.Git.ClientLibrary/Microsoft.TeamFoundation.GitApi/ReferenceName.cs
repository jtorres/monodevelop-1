//*************************************************************************************************
// ReferenceName.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model abstraction representing a Git reference or its name.
    /// </summary>
    public interface IReferenceName : IRevision
    {
        /// <summary>
        /// Gets the canonical name of the reference.
        /// <para/>
        /// Usually in the format of "refs/heads/master" or similar.
        /// </summary>
        string CanonicalName { get; }

        /// <summary>
        /// Gets the friendly name of the reference.
        /// <para/>
        /// Example: "master" part of "refs/heads/master".
        /// <para/>
        /// Example: "origin/master" part of "refs/remotes/origin/master".
        /// </summary>
        string FriendlyName { get; }
    }

    public abstract class ReferenceName : Revision, IComparable<ReferenceName>, IComparable<IReferenceName>, IEquatable<ReferenceName>, IEquatable<IReferenceName>, IReferenceName
    {
        public const string ReferencePrefix = "refs/";
        public const string PatternRefHeads = ReferencePrefix + "heads/";
        public const string PatternRefNotes = ReferencePrefix + "notes/";
        public const string PatternRefRemotes = ReferencePrefix + "remotes/";
        public const string PatternRefStash = ReferencePrefix + "stash";
        public const string PatternRefTags = ReferencePrefix + "tags/";

        internal static readonly StringUtf8 PatternRefHeadsUtf8 = new StringUtf8(PatternRefHeads);
        internal static readonly StringUtf8 PatternRefTagsUtf8 = new StringUtf8(PatternRefTags);

        protected ReferenceName(string name)
            : this((StringUtf8)name)
        { }

        internal ReferenceName(StringUtf8 name)
            : base(name)
        {
            if (ReferenceEquals(name, null))
                throw new ArgumentNullException(nameof(name));

            ParseName(name, out StringUtf8 canonicalName, out _friendlyName);

            SetRevisionText(canonicalName);
        }

        protected readonly StringUtf8 _friendlyName;

        public string CanonicalName
        {
            get { return RevisionText; }
        }

        public string FriendlyName
        {
            get { return (string)_friendlyName; }
        }

        internal StringUtf8 CanonicalNameUtf8
        {
            get => RevisionTextUtf8;
        }

        internal StringUtf8 FriendlyNameUtf8
        {
            get { return _friendlyName; }
        }

        public int CompareTo(ReferenceName other)
            => Comparer.Compare(this, other);

        public int CompareTo(IReferenceName other)
            => Comparer.Compare(this, other);

        public bool Equals(ReferenceName other)
            => Comparer.Equals(this, other);

        public bool Equals(IReferenceName other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            var a = obj as ReferenceName;
            if (!ReferenceEquals(a, null))
                return Equals(a);

            var b = obj as IReferenceName;
            if (!ReferenceEquals(b, null))
                return Equals(b);

            return base.Equals(obj);
        }

        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        public override string ToString()
        {
            return RevisionText;
        }

        protected string DebuggerDisplay
        {
            get { return CanonicalName; }
        }

        /// <summary>
        /// <para>Determine if the reference name is syntactically valid.</para>
        ///
        /// <para>RefNames may not be empty or end with "." or "/".</para>
        ///
        /// <para>RefNames may not contain control characters (0x01..0x1f, 0x7f),
        /// spaces, or the special characters ("*", ":", "?", "[", "\", "^", "~").</para>
        ///
        /// <para>RefNames may not contain the special sequences ("@{", "..").</para>
        ///
        /// <para>Path components within a RefName may not be empty, begin with ".",
        /// or end with ".lock".  (This is case sensitive.)</para>
        ///
        /// </summary>
        /// <param name="name">Reference name (with or without "refs/TYPE/" prefix).</param>
        public static bool IsLegalName(string name)
        {
            // We DO NOT care whether the name has a "refs/<TYPE>/" prefix or not.

            if (name == null)
                return false;

            int len = name.Length;
            if (len == 0 || name[len - 1] == '.' || name[len - 1] == '/')
                return false;

            for (int k = 0; k < len; k++)
            {
                char ch = name[k];
                if (ch <= ' ' || ch == (char)0x7f)
                    return false;
                if (ch == '*' || ch == ':' || ch == '?' || ch == '[' || ch == '\\' || ch == '^' || ch == '~')
                    return false;

                if ((k + 1) < len)
                {
                    if ((ch == '.') && (name[k + 1] == '.')) // look for ".."
                        return false;
                    if ((ch == '@') && (name[k + 1] == '{')) // look for "@{"
                        return false;
                }
            }

            string[] components = name.Split('/');
            int nr = components.Length;

            if (nr == 0)
                return false;

            for (int k = 0; k < nr; k++)
            {
                string entry = components[k];

                if (entry.Length == 0) // an interior "//" or a leading "/".
                    return false;

                if (entry[0] == '.')
                    return false;

                if (entry.EndsWith(".lock", StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        protected abstract void ParseName(StringUtf8 name, out StringUtf8 canonicalName, out StringUtf8 friendlyName);
    }
}
