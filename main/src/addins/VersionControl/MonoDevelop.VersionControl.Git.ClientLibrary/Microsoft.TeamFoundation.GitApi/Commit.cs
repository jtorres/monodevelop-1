//*************************************************************************************************
// Commit.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git commit object.
    /// </summary>
    public interface ICommit : IEquatable<ICommit>, IObject
    {
        /// <summary>
        /// Gets an object containing information about the author of the commit.
        /// </summary>
        IIdentity Author { get; }

        /// <summary>
        /// Gets an object containing information about the committer of the commit.
        /// </summary>
        IIdentity Committer { get; }

        /// <summary>
        /// Gets the first line of the commit message.
        /// </summary>
        string FirstLine { get; }

        /// <summary>
        /// Gets the entire commit message.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets a read-only list of the parent commits.
        /// </summary>
        IReadOnlyList<ObjectId> ParentIdentities { get; }

        /// <summary>
        /// Gets the `<see cref="ObjectId"/>` of the commit's root tree.
        /// <para/>
        /// Use `<see cref="IRepository.ReadObject{T}(ObjectId)"/>` to read the actual `<seealso cref="ITree"/>` object.
        /// </summary>
        ObjectId TreeId { get; }
    }

    public interface IStash : ICommit
    {
        ObjectId BaseCommitId { get; }

        ObjectId IndexCommitId { get; }

        ObjectId WorktreeCommitId { get; }
    }

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [JsonObject(MemberSerialization.OptIn)]
    internal class Commit : ObjectBase, ICommit, IEquatable<Commit>, ILoggable, IStash
    {
        const string AuthorLinePrefix = "author ";
        const string CommitterLinePrefix = "committer ";
        const string ParentLinePrefix = "parent ";
        const string TreeLinePrefix = "tree "; 
        const int BaseParentOrdinal = 0;
        const int IndexParentOrdinal = 1;
        const int WorktreeParentOrdinal = 2;

        // TODO.GitApi: See if TagAnnotation.cs needs similar code for handling ExtendedHeaders.
        const string ExtendedHeaderMergeTag = "mergetag ";
        const string ExtendedHeaderGpgSigPrefix = "gpgsig ";

        bool _inExtendedHeader = false;

        public Commit(ObjectHeader header)
            : base(header, ObjectType.Commit)
        { }

        private IIdentity _author;
        private IIdentity _committer;
        private StringUtf8 _firstLine;
        private StringUtf8 _message;
        private List<ObjectId> _parents;
        private readonly object _syncpoint = new object();
        private ObjectId _treeId;

        protected internal override void SetContextAndCache(IExecutionContext context, IStringCache cache)
        {
            base.SetContextAndCache(context, cache);

            (_author as Identity)?.SetContextAndCache(context, cache);
            (_committer as Identity)?.SetContextAndCache(context, cache);
        }

        [JsonProperty]
        public IIdentity Author
        {
            get { lock (_syncpoint) return _author; }
        }

        [JsonProperty]
        public IIdentity Committer
        {
            get { lock (_syncpoint) return _committer; }
        }

        [JsonProperty]
        public string FirstLine
        {
            get { lock (_syncpoint) return (_firstLine ?? _message)?.ToString(); }
        }

        [JsonProperty]
        public string Message
        {
            get { lock (_syncpoint) return _message?.ToString(); }
        }

        [JsonProperty]
        public IReadOnlyList<ObjectId> ParentIdentities
        {
            get { lock (_syncpoint) return _parents; }
        }

        [JsonProperty]
        public ObjectId TreeId
        {
            get { lock (_syncpoint) return _treeId; }
        }

        #region IStash

        ObjectId IStash.BaseCommitId
        {
            get
            {
                return (_parents?.Count < BaseParentOrdinal + 1)
                    ? ObjectId.Zero
                    : _parents[BaseParentOrdinal];
            }
        }

        ObjectId IStash.IndexCommitId
        {
            get
            {
                return (_parents?.Count < IndexParentOrdinal + 1)
                    ? ObjectId.Zero
                    : _parents[IndexParentOrdinal];
            }
        }

        ObjectId IStash.WorktreeCommitId
        {
            get
            {
                return (_parents?.Count < WorktreeParentOrdinal + 1)
                    ? ObjectId.Zero
                    : _parents[WorktreeParentOrdinal];
            }
        }

        #endregion

        private string DebuggerDisplay
        {
            get { return $"{nameof(Commit)}: {((_firstLine == null) ? _header.ObjectId.RevisionText : FirstLine)}"; }
        }


        public static bool Equals(ICommit left, ICommit right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(null, right))
                return false;

            return ObjectId.Equals(left.ObjectId, right.ObjectId);
        }

        public bool Equals(Commit other)
            => Commit.Equals(this as ICommit, other as ICommit);

        public bool Equals(ICommit other)
            => Commit.Equals(this as ICommit, other);

        public override bool Equals(object obj)
            => Commit.Equals(this as ICommit, obj as ICommit);

        public override int GetHashCode()
            => ObjectId.GetHashCode();

        public void SetData(IIdentity author, IIdentity commiter, StringUtf8 firstLine, StringUtf8 message, IReadOnlyList<ObjectId> parentIdentities, ObjectId treeId)
        {
            lock (_syncpoint)
            {
                _author = author;
                _committer = commiter;
                _firstLine = firstLine;
                _message = message ?? firstLine;
                _treeId = treeId;
            }

            if (parentIdentities != null)
            {
                _parents = new List<ObjectId>(parentIdentities);
            }
        }

        public override string ToString()
            => base.ToString();

        internal override unsafe void ParseData(ByteBuffer buffer, ref int index, int count, int skipPrefix, INamedObjectFilter filter)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (skipPrefix < 0 || skipPrefix > 4)
                throw new ArgumentOutOfRangeException(nameof(skipPrefix));

            IIdentity author = null;
            IIdentity committer = null;
            List<ObjectId> parents = new List<ObjectId>();
            ObjectId? treeId = null;
            StringUtf8 message = null;
            StringUtf8 firstLine = null;

            int get = index;
            int end = index + count;

            // ignore stray new line characters
            while (get < end && buffer[get] == '\n')
            {
                get += 1;
            }

            while (get < end)
            {
                int eol = buffer.FirstIndexOf('\n', get, end - get);
                if (eol < 0)
                {
                    eol = buffer.FirstIndexOf('\0', get, end - get);
                    if (eol < 0)
                        break;
                }

                if (buffer.StartsWith(AuthorLinePrefix, get, AuthorLinePrefix.Length))
                {
                    int start = get + AuthorLinePrefix.Length;

                    author = Identity.FromUtf8(buffer, start, eol - start);
                    ((Identity)author).SetContextAndCache(Context, _cache);
                }
                else if (buffer.StartsWith(CommitterLinePrefix, get, CommitterLinePrefix.Length))
                {
                    int start = get + CommitterLinePrefix.Length;

                    committer = Identity.FromUtf8(buffer, start, eol - start);
                    ((Identity)committer).SetContextAndCache(Context, _cache);
                }
                else if (buffer.StartsWith(ParentLinePrefix, get, ParentLinePrefix.Length))
                {
                    ObjectId parentId;

                    parentId = ObjectId.FromUtf8(buffer, get + ParentLinePrefix.Length);

                    parents.Add(parentId);
                }
                else if (buffer.StartsWith(TreeLinePrefix, get, TreeLinePrefix.Length))
                {
                    int start = get + TreeLinePrefix.Length;

                    treeId = ObjectId.FromUtf8(buffer, start);
                }
                else if (buffer.StartsWith(ExtendedHeaderMergeTag, get, ExtendedHeaderMergeTag.Length)
                    || buffer.StartsWith(ExtendedHeaderGpgSigPrefix, get, ExtendedHeaderGpgSigPrefix.Length))
                {
                    // An extended header has a series of lines with a single
                    // leading space following it.  I think there can be multiple
                    // extended headers, so we eat until we get to the end of the
                    // header block or the start of another header line.
                    //
                    // For now just ignore these.
                    _inExtendedHeader = true;
                }
                else if (_inExtendedHeader && buffer[get] == ' ')
                {
                    // Keep eating extended header.
                }
                // Commit message prefix is a pair of new-line characters
                // and until we find those, ignore everything in between
                // as "meaningless" extra headers
                else if (buffer[get] == '\n')
                {
                    get += 1;

                    message = new StringUtf8(buffer, get, end - get);
                    message = message.TrimLeftTab(skipPrefix);
                    int i1 = message.FirstIndexOf('\n');
                    firstLine = (i1 > 0)
                        ? message.Substring(0, i1)
                        : message;

                    break;
                }

                get = eol + 1;
            }

            lock (_syncpoint)
            {
                if (treeId == null)
                    throw new ObjectParseException("tree-id", new StringUtf8(buffer, index, count), 0);

                _treeId = treeId.Value;
                _parents = parents;
                _author = author;
                _committer = committer;
                _message = message ?? StringUtf8.Empty;
                _firstLine = firstLine ?? _message;

                index += count + 1;
            }
        }

        void ILoggable.Log(ExecutionContext context, System.Text.StringBuilder log, int indent)
        {
            string prefix = context.ParseHelper.GetParseErrorIndent(indent);
            string prefix2 = context.ParseHelper.GetParseErrorIndent(indent + 1);
            string prefix3 = context.ParseHelper.GetParseErrorIndent(indent + 2);

            log.Append(prefix).AppendLine(nameof(Commit));
            log.Append(prefix).AppendLine("{");
            {
                log.Append(prefix2).Append("tree-id: ").AppendLine(TreeId.ToString());

                log.Append(prefix2).AppendLine($"parents[{ParentIdentities?.Count}]:");
                log.Append(prefix2).AppendLine("{");
                if (ParentIdentities?.Count > 0)
                {
                    foreach (var parentId in ParentIdentities)
                    {
                        log.Append(prefix3).Append("parent-id: ").AppendLine(parentId.ToString());
                    }
                }
                log.Append(prefix2).AppendLine("}");

                log.Append(prefix2).AppendLine("author:");
                log.Append(prefix2).AppendLine("{");
                {
                    (Author as ILoggable)?.Log(context, log, indent + 2);
                }
                log.Append(prefix2).AppendLine("}");

                log.Append(prefix2).AppendLine("commiter:");
                log.Append(prefix2).AppendLine("{");
                {
                    (Committer as ILoggable)?.Log(context, log, indent + 2);
                }
                log.Append(prefix2).AppendLine("}");

                log.Append(prefix2).Append("first-line: \"").Append(FirstLine).AppendLine("\"");
                log.Append(prefix2).Append("message: \"").Append(Message).AppendLine("\"");
            }
            log.Append(prefix).AppendLine("}");
        }

        bool IEquatable<ICommit>.Equals(ICommit other) => throw new NotImplementedException();
    }
}
