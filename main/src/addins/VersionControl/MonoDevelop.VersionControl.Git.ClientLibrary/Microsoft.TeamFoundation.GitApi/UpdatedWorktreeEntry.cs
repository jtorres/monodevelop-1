//*************************************************************************************************
// UpdatedWorktreeEntry.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of an updated Git worktree entry.
    /// </summary>
    public interface IUpdatedWorktreeEntry : IEquatable<IUpdatedWorktreeEntry>
    {
        /// <summary>
        /// Gets the additional details, if any, related to the affected entry.
        /// </summary>
        string Details { get; }

        /// <summary>
        /// Gets the relative path of the entry the operation was performed on.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// The kind of operation that was performed.
        /// </summary>
        UpdatedWorktreeEntryType Type { get; }
    }

    /// <summary>
    /// Represents an updated work tree entry
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class UpdatedWorktreeEntry: IEquatable<UpdatedWorktreeEntry>, IUpdatedWorktreeEntry
    {
        internal static readonly PathComparer PathComparer = new PathComparer();

        internal UpdatedWorktreeEntry(StringUtf8 path, UpdatedWorktreeEntryType type, StringUtf8 details)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            _details = details;
            _path = path;
            _type = type;
        }

        internal UpdatedWorktreeEntry(StringUtf8 path, UpdatedWorktreeEntryType type)
            : this(path, type, null)
        { }

        private StringUtf8 _details;
        private StringUtf8 _path;
        private UpdatedWorktreeEntryType _type;

        [JsonProperty]
        public string Details
        {
            get { return (string)_details; }
        }

        [JsonProperty]
        public string Path
        {
            get { return (string)_path; }
        }

        [JsonProperty]
        public UpdatedWorktreeEntryType Type
        {
            get { return _type; }
        }

        internal StringUtf8 DetailsUtf8
        {
            get { return _details; }
        }

        internal StringUtf8 PathUtf8
        {
            get { return _path; }
        }

        public bool Equals(UpdatedWorktreeEntry other)
        {
            return Type == other.Type
                && PathComparer.Equals(PathUtf8, other.PathUtf8);
        }

        public bool Equals(IUpdatedWorktreeEntry other)
        {
            return (other is UpdatedWorktreeEntry a
                    && Equals(a))
                || (Type == other.Type
                    && PathComparer.Equals(this.Path, other.Path))
                || base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return (obj is UpdatedWorktreeEntry a
                    && Equals(a))
                || (obj is IUpdatedWorktreeEntry b
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
