//*************************************************************************************************
// Reference.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git reference.
    /// </summary>
    public interface IReference : IComparable<IReference>, IEquatable<IReference>, IReferenceName
    {
        /// <summary>
        /// Gets the commit referenced, if available; otherwise `<see langword="null"/>`.
        /// <para/>
        /// Requires `<see cref="Cli.ForEachRefCommand"/>` is called with `<see cref="ReferenceOptionFlags.TipsAll"/>`, `<see cref="ReferenceOptionFlags.TipsHeads"/>`, `<see cref="ReferenceOptionFlags.TipsRemotes"/>`, and/or `<see cref="ReferenceOptionFlags.TipsTags"/>`.
        /// </summary>
        ICommit Commit { get; }

        /// <summary>
        /// Gets the identity of the object the reference is referencing.
        /// </summary>
        ObjectId ObjectId { get; }

        /// <summary>
        /// Gets the type of object `<see cref="ObjectId"/>` identifies.
        /// </summary>
        ObjectType ObjectType { get; }

        /// <summary>
        /// Gets the type of this references.
        /// </summary>
        ReferenceType ReferenceType { get; }
    }

    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    [JsonObject(MemberSerialization.OptIn)]
    internal abstract class Reference : Base, IComparable<Reference>, IEquatable<Reference>, IEquatable<ReferenceName>, IEquatable<Revision>, IReference, ILoggable
    {
        public static readonly StringComparison NameStringComparison = ReferenceName.StringComparison;
        public static readonly StringComparer NameStringComparer = ReferenceName.StringComparer;

        internal static readonly ReferenceComparer Comparer = new ReferenceComparer();
        internal static readonly StringUtf8Comparer NameStringUtf8Comparer = StringUtf8Comparer.Ordinal;

        private const string RefHeadName = Head.CanonicalLabel;
        private const string RefStashName = "stash";
        private const char Separator = '/';

        protected Reference(StringUtf8 canonicalName, ObjectId objectId, ObjectType objectType)
            : base()
        {
            if (canonicalName == null)
                throw new ArgumentNullException(nameof(canonicalName));

            StringUtf8 name;
            ReferenceType type;

            DecomposeCanonicalName(canonicalName, out name, out type);

            if (name == null)
                throw ReferenceNameException.Create((string)canonicalName, type);

            _canonicalName = canonicalName;
            _friendlyName = name;
            _objectId = objectId;
            _objectType = objectType;
            _syncpoint = new object();
            _type = type;
        }

        internal void SetContextAndCache(IExecutionContext context, IStringCache cache)
        {
            SetContext(context);
            _cache = cache;
        }

        protected IStringCache _cache;
        protected readonly StringUtf8 _canonicalName;
        private ICommit _commit;
        protected readonly StringUtf8 _friendlyName;
        protected readonly ObjectId _objectId;
        protected readonly ObjectType _objectType;
        protected readonly ReferenceType _type;
        protected object _syncpoint;

        [JsonProperty]
        public virtual string CanonicalName
        {
            get
            {
                string canonicalName = (string)_canonicalName;

                if (_cache == null)
                    return canonicalName;

                return _cache.Intern(canonicalName);
            }
        }

        public StringUtf8 CanonicalNameUtf8
        {
            get { return _canonicalName; }
        }

        [JsonProperty]
        public virtual ICommit Commit
        {
            get { return Volatile.Read(ref _commit); }
            internal set { Volatile.Write(ref _commit, value); }
        }

        [JsonProperty]
        public virtual string FriendlyName
        {
            get
            {
                string friendlyName = (string)_friendlyName;

                if (_cache == null)
                    return friendlyName;

                return _cache.Intern(friendlyName);
            }
        }

        public StringUtf8 FriendlyNameUtf8
        {
            get { return _friendlyName; }
        }

        [JsonProperty]
        public virtual ObjectId ObjectId
        {
            get { return _objectId; }
        }

        [JsonProperty]
        public virtual ObjectType ObjectType
        {
            get { return _objectType; }
        }

        public string RevisionText
        {
            get { return CanonicalName; }
        }

        [JsonProperty]
        public virtual ReferenceType ReferenceType
        {
            get { return _type; }
        }

        protected virtual string DebuggerDisplay
        {
            get { return FormattableString.Invariant($"{GetType().Name}: {CanonicalName}"); }
        }

        public static StringUtf8 ComposeCanonicalName(StringUtf8 name, ReferenceType type)
        {
            // this is an allocation horror story, but it is the best we have until a mbstringBulder is available

            using (var buffer = new StringBuffer())
            {
                switch (type)
                {
                    case ReferenceType.Head:
                        return (StringUtf8)RefHeadName;

                    case ReferenceType.Heads:
                        buffer.Append(ReferenceName.PatternRefHeads);
                        break;

                    case ReferenceType.Notes:
                        buffer.Append(ReferenceName.PatternRefNotes);
                        break;

                    case ReferenceType.Remotes:
                        buffer.Append(ReferenceName.PatternRefRemotes);
                        break;

                    case ReferenceType.Stash:
                        return (StringUtf8)RefStashName;

                    case ReferenceType.Tags:
                        buffer.Append(ReferenceName.PatternRefTags);
                        break;

                    case ReferenceType.Unknown:
                        return name;
                }

                buffer.Append((string)name);

                return (StringUtf8)buffer.ToString();
            }
        }

        public int CompareTo(Reference other)
            => Comparer.Compare(this, other);

        public int CompareTo(IReference other)
            => Comparer.Compare(this, other);

        public static void DecomposeCanonicalName(StringUtf8 canonicalName, out StringUtf8 name, out ReferenceType type)
        {
            if (canonicalName == null)
                throw new ArgumentNullException(nameof(canonicalName));

            if (canonicalName.StartsWith(ReferenceName.PatternRefHeads))
            {
                name = canonicalName.Substring(ReferenceName.PatternRefHeads.Length);
                type = ReferenceType.Heads;
            }
            else if (canonicalName.StartsWith(ReferenceName.PatternRefRemotes))
            {
                name = canonicalName.Substring(ReferenceName.PatternRefRemotes.Length);
                type = ReferenceType.Remotes;
            }
            else if (canonicalName.StartsWith(ReferenceName.PatternRefTags))
            {
                name = canonicalName.Substring(ReferenceName.PatternRefTags.Length);
                type = ReferenceType.Tags;
            }
            else if (canonicalName.StartsWith(ReferenceName.PatternRefNotes))
            {
                name = canonicalName.Substring(ReferenceName.PatternRefNotes.Length);
                type = ReferenceType.Notes;
            }
            else if (canonicalName.StartsWith(ReferenceName.PatternRefStash))
            {
                name = canonicalName.Substring(ReferenceName.PatternRefStash.Length);
                type = ReferenceType.Stash;
            }
            else
            {
                Debug.Fail("unexpected condition: " + (string)canonicalName);

                name = canonicalName;
                type = ReferenceType.Unknown;
            }
        }

        public bool Equals(Reference other)
            => Comparer.Equals(this, other);

        public bool Equals(IReference other)
            => Comparer.Equals(this, other);

        public bool Equals(ReferenceName other)
            => Comparer.Equals(this, other);

        public bool Equals(IReferenceName other)
            => Comparer.Equals(this, other);

        public bool Equals(Revision other)
            => Comparer.Equals(this, other);

        public bool Equals(IRevision other)
            => Comparer.Equals(this, other);

        public bool Equals(string other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            var a = obj as Reference;
            if (!ReferenceEquals(a, null))
                return Equals(a);

            var b = obj as IReference;
            if (!ReferenceEquals(b, null))
                return Equals(b);

            var c = obj as ReferenceName;
            if (!ReferenceEquals(c, null))
                return Equals(c);

            var d = obj as IReferenceName;
            if (!ReferenceEquals(d, null))
                return Equals(d);

            var e = obj as Revision;
            if (!ReferenceEquals(e, null))
                return Equals(e);

            var f = obj as IRevision;
            if (!ReferenceEquals(f, null))
                return Equals(f);

            var g = obj as string;
            if (!ReferenceEquals(g, null))
                return Equals(g);

            return base.Equals(obj);
        }

        public override int GetHashCode()
            => StringUtf8Comparer.Ordinal.GetHashCode(_canonicalName);

        public static bool IsLegalName(string name)
            => ReferenceName.IsLegalName(name);

        public static string TypeToName(ReferenceType type)
        {
            switch (type)
            {
                case ReferenceType.Head:
                    return "HEAD";

                case ReferenceType.Heads:
                case ReferenceType.Remotes:
                    return "branch";

                case ReferenceType.Notes:
                    return "note";

                case ReferenceType.Stash:
                    return "stash";

                case ReferenceType.Tags:
                    return "tag";
            }

            return "unknown";
        }

        void ILoggable.Log(ExecutionContext context, System.Text.StringBuilder log, int indent)
        {
            string prefix = context.ParseHelper.GetParseErrorIndent(indent);
            log.Append(prefix).AppendLine($"{nameof(Reference)} {{ rev: {RevisionText} }}");
        }
    }
}
