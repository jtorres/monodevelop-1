//*************************************************************************************************
// Head.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    public interface IHead : IReference, IEquatable<IReference>, IEquatable<IReferenceName>
    {
        HeadType HeadType { get; }

        IBranch AsBranch();

        ICommit AsCommit();
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class Head : Base, IHead, ILoggable
    {
        /// <summary>
        /// The value used by Git when referencing the head of current workspace.
        /// </summary>
        public const string CanonicalLabel = "HEAD";

        /// <summary>
        /// <para>Name of branch when HEAD is detached.</para>
        /// <para>The value of the label is derived from 'git status --porcelain=v2'.</para>
        /// </summary>
        public const string DetachedLabel = "(detached)";

        /// <summary>
        /// <para>Substitute branch name when in a bogus state, such as when HEAD points to a broken ref.</para>
        /// <para>The value of the label is derived from 'git status --porcelain=v2'.</para>
        /// </summary>
        public const string MalformedLabel = "(unknown)";

        /// <summary>
        /// <para>Substitute commit name when prior to the initial commit.</para>
        /// <para>The value of the label is derived from 'git status --porcelain=v2'.</para>
        /// </summary>
        public const string UnbornLabel = "(initial)";

        internal static readonly StringUtf8 CanonicalLabelUtf8 = (StringUtf8)CanonicalLabel;

        internal static readonly StringUtf8 DetachedLabelUtf8 = (StringUtf8)DetachedLabel;

        internal static readonly StringUtf8 MalformedLabelUtf8 = (StringUtf8)MalformedLabel;

        internal static readonly StringUtf8 UnbornLabelUtf8 = (StringUtf8)UnbornLabel;

        public Head()
            : base()
        {
            _canonicalName = null;
            _collection = null;
            _commit = null;
            _objectId = ObjectId.Zero;
            _objectType = ObjectType.Unknown;
            _reference = null;
            _headType = HeadType.Unknown;
            _syncpoint = new object();
        }

        public Head(IReferenceCollection collection, IReference reference)
            : this()
        {
            if (ReferenceEquals(collection, null))
                throw new ArgumentNullException(nameof(collection));

            if (reference is Branch)
            {
                (reference as Branch).IsHead = true;
            }

            _collection = collection;
            _reference = reference;
        }

        internal void SetContextAndCache(ExecutionContext context, IStringCache cache)
        {
            SetContext(context);
            _cache = cache;

            (_commit as Commit)?.SetContextAndCache(context, cache);
        }

        private IStringCache _cache;
        private string _canonicalName;
        private IReferenceCollection _collection;
        private Commit _commit;
        private string _friendlyName;
        private HeadType _headType;
        private ObjectId _objectId;
        private ObjectType _objectType;
        private IReference _reference;
        private readonly object _syncpoint;

        [JsonProperty]
        public string CanonicalName
        {
            get
            {
                lock (_syncpoint)
                {
                    if (_headType == HeadType.Detached)
                        return _objectId.RevisionText;

                    if (_canonicalName != null)
                        return _canonicalName;

                    if (_reference != null)
                        return _reference.CanonicalName;

                    return null;
                }
            }
            internal set { lock (_syncpoint) _canonicalName = value; }
        }

        public IReferenceCollection Collection
        {
            get { return Volatile.Read(ref _collection); }
            internal set { Volatile.Write(ref _collection, value); }
        }

        [JsonProperty]
        public ICommit Commit
        {
            get { lock (_syncpoint) return _commit; }
            internal set
            {
                if (value == null)
                {
                    Volatile.Write(ref _commit, null);
                }
                else
                {
                    lock (_syncpoint)
                    {
                        if (value is Commit)
                        {
                            _commit = value as Commit;
                        }
                        else
                        {
                            _commit = new Commit(value.Header);
                            _commit.SetContextAndCache(Context, _cache);
                            _commit.SetData(value.Author,
                                            value.Committer,
                                            (StringUtf8)value.FirstLine,
                                            (StringUtf8)value.Message,
                                            value.ParentIdentities,
                                            value.TreeId);
                        }
                    }
                }
            }
        }

        [JsonProperty]
        public string FriendlyName
        {
            get
            {
                lock (_syncpoint)
                {
                    switch (_headType)
                    {
                        case HeadType.Detached:
                            return DetachedLabel;

                        case HeadType.Malformed:
                            return MalformedLabel;

                        default:
                            return (_reference == null)
                                ? _friendlyName ?? CanonicalLabel
                                : _reference.FriendlyName;
                    }
                }
            }
            internal set { lock (_syncpoint) _friendlyName = value; }
        }

        [JsonProperty]
        public HeadType HeadType
        {
            get { lock (_syncpoint) return _headType; }
            internal set { lock (_syncpoint) _headType = value; }
        }

        [JsonProperty]
        public ObjectId ObjectId
        {
            get
            {
                lock (_syncpoint)
                {
                    if (_headType == HeadType.Unborn
                        || _headType == HeadType.Malformed)
                        return ObjectId.Zero;

                    return _commit?.ObjectId
                        ?? _reference?.ObjectId
                        ?? _objectId;
                }
            }
            internal set { lock (_syncpoint) _objectId = value; }
        }

        [JsonProperty]
        public ObjectType ObjectType
        {
            get
            {
                lock (_syncpoint)
                {
                    if (_headType == HeadType.Malformed
                        || _headType == HeadType.Unborn)
                        return ObjectType.Unknown;

                    return _commit?.ObjectType
                        ?? _reference?.ObjectType
                        ?? _objectType;
                }
            }
            internal set { lock (_syncpoint) _objectType = value; }
        }

        [JsonProperty]
        public ReferenceType ReferenceType
        {
            get { return ReferenceType.Head; }
        }

        public string RevisionText
        {
            get
            {
                lock (_syncpoint)
                {
                    if (_headType == HeadType.Malformed)
                        return CanonicalLabel;

                    if (_headType != HeadType.Detached && _reference != null)
                        return _reference.RevisionText;

                    if (_headType != HeadType.Unborn && _objectId != ObjectId.Zero)
                        return _objectId.ToString();

                    return CanonicalLabel;
                }
            }
        }

        internal StringUtf8 CanonicalNameUtf8
        {
            get
            {
                lock (_syncpoint)
                {

                    if (_canonicalName != null)
                        return (StringUtf8)_canonicalName;

                    if (_reference is Reference)
                        return (_reference as Reference).CanonicalNameUtf8;

                    return null;
                }
            }
        }

        internal StringUtf8 FriendlyNameUtf8
        {
            get
            {
                lock (_syncpoint)
                {
                    switch (_headType)
                    {
                        case HeadType.Detached:
                            return (StringUtf8)DetachedLabel;

                        case HeadType.Malformed:
                            return (StringUtf8)MalformedLabel;

                        default:
                            return (_reference is Reference)
                                    ? (_reference as Reference)?.FriendlyNameUtf8
                                    : (StringUtf8)_reference.CanonicalName
                                ?? (StringUtf8)CanonicalLabel;
                    }
                }
            }
        }

        public IReference AsReference()
        {
            lock (_syncpoint)
            {
                if (_headType == HeadType.Detached || _headType == HeadType.Malformed)
                    return null;

                return _reference as IReference;
            }
        }

        public IBranch AsBranch()
        {
            lock (_syncpoint)
            {
                if (_headType == HeadType.Detached || _headType == HeadType.Malformed)
                    return null;

                return _reference as IBranch;
            }
        }

        public ICommit AsCommit()
        {
            lock (_syncpoint)
            {
                return _commit as ICommit;
            }
        }

        public int CompareTo(IReference other)
            => Reference.Comparer.Compare(this, other);

        public bool Equals(IReference other)
            => Reference.Comparer.Equals(this, other);

        public bool Equals(IReferenceName other)
            => Reference.Comparer.Equals(this, other);

        public bool Equals(IRevision other)
            => Reference.Comparer.Equals(this, other);

        public bool Equals(string other)
            => Reference.Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            return Equals(obj as IReference)
                || Equals(obj as IRevision)
                || Equals(obj as string)
                || base.Equals(obj);
        }

        public override int GetHashCode()
            => Reference.Comparer.GetHashCode(this);

        public override string ToString()
        {
            return RevisionText;
        }

        internal void Initialize(IRepository repository, bool initReference, bool initCommit)
        {
            lock (_syncpoint)
            {
                GetHeadType(repository);

                if (initReference)
                {
                    GetCollection(repository, false);
                    GetReference(repository, false);
                }

                if (initCommit)
                {
                    GetCommit(repository, false);
                }
            }
        }

        private void GetCollection(IRepository repository, bool allowRecusion)
        {
            Debug.Assert(Monitor.IsEntered(_syncpoint), "Expected lock not held");

            if (_collection == null)
            {
                GetHeadType(repository);

                if (_headType == HeadType.Detached || _headType == HeadType.Malformed)
                    return;

                var options = new ReferenceOptions()
                {
                    Flags = ReferenceOptionFlags.RefsHeads,
                };
                _collection = repository.ReadReferences(options);

                if (allowRecusion)
                {
                    GetReference(repository, false);
                    GetCommit(repository, false);
                }
            }
        }

        private void GetCommit(IRepository repository, bool allowRecusion)
        {
            Debug.Assert(Monitor.IsEntered(_syncpoint), "Expected lock not held");

            if (_commit == null)
            {
                GetHeadType(repository);

                if (_headType == HeadType.Unborn || _headType == HeadType.Malformed)
                    return;

                if (!_objectId.Equals(ObjectId.Zero))
                {
                    _commit = repository.ReadObject<Commit>(_objectId);
                }
                else if (_headType != HeadType.Detached)
                {
                    if (allowRecusion)
                    {
                        GetCollection(repository, false);
                        GetReference(repository, false);
                    }

                    if (_reference != null)
                    {
                        // Get the commit if possible
                        if (_reference.Commit != null)
                        {
                            if (_reference.Commit is Commit)
                            {
                                _commit = _reference.Commit as Commit;
                            }
                            else
                            {
                                _commit = new Commit(_reference.Commit.Header);
                                _commit.SetContextAndCache(Context, _cache);
                            }
                        }
                        else if (!_reference.ObjectId.Equals(ObjectId.Zero))
                        {
                            _commit = repository.ReadObject<Commit>(_reference.ObjectId);
                        }
                    }

                    // If we got the commit, record its oid
                    if (_commit != null)
                    {
                        _objectId = _commit.ObjectId;
                    }
                }

                // Update the reference's commit reference accordingly
                if (_reference is Reference)
                {
                    (_reference as Reference).Commit = _commit;
                }
            }
        }

        private void GetHeadType(IRepository repository)
        {
            Debug.Assert(Monitor.IsEntered(_syncpoint), "Expected lock not held");

            if (_headType == HeadType.Unknown)
            {
                // Because Git can be progressing between reading the branch name and utilizing
                // the result, there's a chance we'll fault because of it. Re-trying is the best
                // bet here.
                for (int attempt = 0; attempt < 5; attempt += 1)
                {
                    HeadType headType;
                    string revparseName = repository.ReadCurrentBranchName(false, out headType);

                    try
                    {
                        if (headType == HeadType.Unborn)
                        {
                            _canonicalName = repository.ReadCurrentHeadValue();
                            _friendlyName = _canonicalName;
                            _headType = HeadType.Unborn;
                            break;
                        }
                        else if (headType == HeadType.Detached)
                        {
                            _canonicalName = repository.ReadCurrentHeadValue();
                            _friendlyName = _canonicalName;
                            _objectId = ObjectId.FromString(_canonicalName);
                            _objectType = ObjectType.Commit;
                            _headType = HeadType.Detached;
                            break;
                        }
                        else if (Branch.IsLegalFullyQualifiedName(revparseName))
                        {
                            _canonicalName = revparseName;
                            // If there's no collection to check or the canonical name exists in
                            // the collection, assume normal head type otherwise, assume unborn
                            // head type
                            _headType = (_collection == null)
                                ? HeadType.Normal
                                : (_collection.Contains(_canonicalName))
                                    ? HeadType.Normal
                                    : HeadType.Unborn;

                            StringUtf8 canonicalName = (StringUtf8)_canonicalName;
                            StringUtf8 friendlyName;
                            ReferenceType type;

                            Reference.DecomposeCanonicalName(canonicalName, out friendlyName, out type);

                            _friendlyName = (string)friendlyName;

                            break;
                        }
                        else
                        {
                            _headType = HeadType.Malformed;
                            break;
                        }
                    }
                    // Catch and re-try the query, but no more than five times.
                    catch (Exception exception) when (attempt < 5)
                    {
                        Tracer.TraceException(exception, TracerLevel.Diagnostic, userData: repository?.UserData);
                    }
                }
            }
        }

        private void GetReference(IRepository repository, bool allowRecusion)
        {
            Debug.Assert(Monitor.IsEntered(_syncpoint), "Expected lock not held");

            if (_reference == null)
            {
                GetHeadType(repository);

                if (_headType == HeadType.Detached || _headType == HeadType.Malformed)
                    return;

                GetCollection(repository, false);

                if (_headType == HeadType.Unborn && !(_canonicalName == null))
                {
                    var branch = new Branch((StringUtf8)_canonicalName, ObjectId.Zero, ObjectType.Unknown)
                    {
                        IsHead = true,
                    };
                    branch.SetContextAndCache(Context, _cache);
                    _reference = branch;
                }
                else
                {
                    if (_collection[_canonicalName] is IReference)
                    {
                        _reference = _collection[_canonicalName] as IReference;
                    }
                    else
                    {
                        var iref = _collection[_canonicalName];
                        if (iref != null)
                        {
                            string canonicalName = iref.CanonicalName;

                            if (canonicalName.StartsWith(ReferenceName.PatternRefHeads, StringComparison.Ordinal)
                                || canonicalName.StartsWith(ReferenceName.PatternRefRemotes, StringComparison.Ordinal))
                            {
                                var branch = new Branch((StringUtf8)canonicalName, iref.ObjectId, iref.ObjectType)
                                {
                                    IsHead = true,
                                };
                                branch.SetContextAndCache(Context, _cache);
                                _reference = branch;
                            }
                            else if (canonicalName.StartsWith(ReferenceName.PatternRefTags, StringComparison.Ordinal))
                            {
                                var tag = new Tag((StringUtf8)canonicalName, iref.ObjectId, iref.ObjectType);
                                tag.SetContextAndCache(Context, _cache);
                                _reference = tag;
                            }
                        }
                    }
                }

                if (allowRecusion)
                {
                    GetCommit(repository, false);
                }
                else if (_reference != null
                        && _reference.Commit == null
                        && _reference is Reference)
                {
                    (_reference as Reference).Commit = _commit;
                }
            }
        }

        void ILoggable.Log(ExecutionContext context, StringBuilder log, int indent)
        {
            string prefix = context.ParseHelper.GetParseErrorIndent(indent);
            log.Append(prefix).AppendLine(FormattableString.Invariant($"{nameof(Head)} {{ rev: {RevisionText} }}"));
        }
    }

    public sealed class MalformedHeadException : ExceptionBase
    {
        public const string ExceptionMessage = "The repository HEAD is missing or corrupt.";

        internal MalformedHeadException()
            : base(ExceptionMessage)
        { }

        internal MalformedHeadException(Exception innerException)
            : base(ExceptionMessage, innerException)
        { }

        internal MalformedHeadException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}
