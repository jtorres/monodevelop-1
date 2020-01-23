//*************************************************************************************************
// UpdatedIndexEntry.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of an updated Git index entry.
    /// </summary>
    public interface IUpdatedIndexEntry : IEquatable<IUpdatedIndexEntry>
    {
        /// <summary>
        /// Gets the relative path of the file the operation was performed on.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// The kind of operation that was performed, only `<see cref="TreeDifferenceType.Added"/>` or `<see cref="TreeDifferenceType.Deleted"/>` are valid values.
        /// </summary>
        TreeDifferenceType Type { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class UpdatedIndexEntry : IEquatable<UpdatedIndexEntry>, IUpdatedIndexEntry
    {
        internal static readonly PathComparer PathComparer = new PathComparer();

        internal UpdatedIndexEntry(StringUtf8 path, TreeDifferenceType type)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (type != TreeDifferenceType.Added && type != TreeDifferenceType.Deleted)
                throw new ArgumentException(nameof(type));

            _path = path;
            _type = type;
        }

        private StringUtf8 _path;
        private TreeDifferenceType _type;

        [JsonProperty]
        public string Path
        {
            get { return (string)_path; }
        }

        [JsonProperty]
        public TreeDifferenceType Type
        {
            get { return _type; }
        }

        internal StringUtf8 PathUtf8
        {
            get { return _path; }
        }

        public bool Equals(UpdatedIndexEntry other)
        {
            return (Type == other.Type
                    && PathComparer.Equals(PathUtf8, other.PathUtf8))
                || base.Equals(other);
        }

        public bool Equals(IUpdatedIndexEntry other)
        {
            return (other is UpdatedIndexEntry a
                    && Equals(a))
                || (Type == other.Type
                    && PathComparer.Equals(Path, other.Path))
                || base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return (obj is UpdatedIndexEntry a
                    && Equals(a))
                || (obj is IUpdatedIndexEntry b
                    && Equals(b))
                || base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (PathUtf8 == null)
                ? (int)Type
                : PathComparer.GetHashCode(PathUtf8);
        }

        public override string ToString()
        {
            return $"{Type} '{Path}'";
        }
    }
}
