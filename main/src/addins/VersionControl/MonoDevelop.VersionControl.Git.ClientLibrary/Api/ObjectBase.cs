//*************************************************************************************************
// ObjectBase.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git object-dabase object.
    /// <para/>
    /// Base type for `<seealso cref="IBlob"/>`, `<seealso cref="ICommit"/>`, and `<seealso cref="ITree"/>`.
    /// </summary>
    public interface IObject : IEquatable<IObject>, IRevision
    {
        /// <summary>
        /// Gets the object's header.
        /// </summary>
        ObjectHeader Header { get; }

        /// <summary>
        /// Gets the object's unique identity.
        /// </summary>
        ObjectId ObjectId { get; }

        /// <summary>
        /// Gets the object's size in bytes, or -1 if unknown.
        /// </summary>
        ObjectType ObjectType { get; }

        /// <summary>
        /// Gets the size in bytes of this object if known; otherwise -1.
        /// </summary>
        long Size { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal abstract class ObjectBase : Base, IEquatable<ObjectBase>, IObject, ILoggable
    {
        internal static readonly ObjectComparer Comparer = new ObjectComparer();

        protected internal ObjectBase(ObjectHeader header, ObjectType expectedType)
            : base()
        {
            if (expectedType != header.Type)
                throw new InvalidOperationException($"`{nameof(header)}`({header.Type}) != `{nameof(expectedType)}`({expectedType}).");

            _header = header;
        }

        protected internal ObjectBase(IObject parent, ObjectHeader header, ObjectType expectedType)
            : this(header, expectedType)
        { }

        protected internal virtual void SetContextAndCache(IExecutionContext context, IStringCache cache)
        {
            SetContext(context);
            _cache = cache;
        }

        protected readonly ObjectHeader _header;
        internal IStringCache _cache;

        public ObjectHeader Header
        {
            get { return _header; }
        }

        [JsonProperty]
        public ObjectId ObjectId
        {
            get { return _header.ObjectId; }
        }

        public virtual string RevisionText
        {
            get { return ObjectId.RevisionText; }
        }

        [JsonProperty]
        public long Size
        {
            get { return _header.Size; }
        }

        [JsonProperty]
        public ObjectType ObjectType
        {
            get { return _header.Type; }
        }

        public bool Equals(ObjectBase other)
            => Comparer.Equals(this, other);

        public bool Equals(IObject other)
            => Comparer.Equals(this, other);

        public bool Equals(IRevision other)
            => Comparer.Equals(this, other);

        public bool Equals(string other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            return Equals(obj as ObjectBase)
                || Equals(obj as IObject)
                || Equals(obj as IRevision)
                || Equals(obj as string)
                || base.Equals(obj);
        }

        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        public override string ToString()
        {
            return $"{ObjectType}: {ObjectId}";
        }

        internal abstract void ParseData(ByteBuffer buffer, ref int index, int count, int skipPrefix, INamedObjectFilter filter);

        void ILoggable.Log(ExecutionContext context, System.Text.StringBuilder log, int indent)
        {
            string prefix = ParseHelper.GetParseErrorIndent(indent);
            log.Append(prefix).AppendLine($"{nameof(ObjectBase)} {{ rev: {RevisionText} }}");
        }
    }
}
