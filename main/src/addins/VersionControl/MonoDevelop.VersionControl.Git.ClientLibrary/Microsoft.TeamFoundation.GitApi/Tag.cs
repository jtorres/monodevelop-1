//*************************************************************************************************
// Tag.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Threading;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git tag object.
    /// </summary>
    public interface ITag : IComparable<ITag>, IEquatable<ITag>, IReference, ITagName
    {
        /// <summary>
        /// Gets the associated annotation, if any.
        /// <para/>
        /// If a tag is know to have an annotation, but one is not available, use `<see cref="IRepository.ReadObject{T}(ObjectId)"/>` to read it directly from the repository's object database.
        /// </summary>
        ITagAnnotation Annotation { get; }

        /// <summary>
        /// Gets `<see langword="true"/>` the `<see cref="Tag"/>` is annotated; otherwise `<see langword="false"/>`.
        /// </summary>
        bool IsAnnotated { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class Tag : Reference, ITag, IComparable<Tag>, IEquatable<Tag>
    {
        public Tag(StringUtf8 canonicalName, ObjectId objectId, ObjectType objectType)
            : base(canonicalName, objectId, objectType)
        {
            if (_type != ReferenceType.Tags)
                throw new ReferenceTypeMismatchException();
        }

        private ITagAnnotation _annotation;

        [JsonProperty]
        public ITagAnnotation Annotation
        {
            get { return Volatile.Read(ref _annotation); }
            internal set { Volatile.Write(ref _annotation, value); }
        }

        public bool IsAnnotated
        {
            get { return ObjectType == ObjectType.Tag; }
        }

        public int CompareTo(Tag other)
            => Comparer.Compare(this, other);

        public int CompareTo(ITag other)
            => Comparer.Compare(this, other);

        public bool Equals(Tag other)
            => Comparer.Equals(this, other);

        public bool Equals(ITag other)
            => Comparer.Equals(this, other);

        public bool Equals(TagName other)
            => Comparer.Equals(this, other);

        public bool Equals(ITagName other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            return Equals(obj as Tag)
                || Equals(obj as TagName)
                || Equals(obj as ITag)
                || Equals(obj as ITagName)
                || base.Equals(obj);

        }

        public override int GetHashCode()
            => base.GetHashCode();

        public static bool IsLegalFullyQualifiedName(string name)
        {
            if (name == null)
                return false;
            if (!name.StartsWith(ReferenceName.PatternRefTags))
                return false;

            return IsLegalName(name);
        }
    }
}
