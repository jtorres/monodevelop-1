//*************************************************************************************************
// StashUpdatedFile.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;
using static System.FormattableString;

namespace Microsoft.TeamFoundation.GitApi
{
    public enum StashUpdatedFileType
    {
        Unknown = 0,

        /// <summary>
        /// The file is an addition to the worktree, and has been added to the index.
        /// </summary>
        StagedAddition = 1,

        /// <summary>
        /// The file has been deleted from the worktree, and the deletion has been added to the index.
        /// </summary>
        StagedDeleted = 2,

        /// <summary>
        /// The file has been modfied in the worktree, and the modification has been added to the index.
        /// </summary>
        StagedModified = 3,

        /// <summary>
        /// The has been deleted in the worktree, but the deletion has not been added to the index.
        /// </summary>
        UnstagedDeleted = 4,

        /// <summary>
        /// The file has been modified in the worktree, but the change has not been added to the index.
        /// </summary>
        UnstagedModified = 5,

        /// <summary>
        /// The file is untracked (implicitly new and not added to the index).
        /// </summary>
        Untracked = 6,

        /// <summary>
        /// The file is left in a conflict state in the working directory.
        /// </summary>
        Conflict = 7,
    }

    /// <summary>
    /// Representation of the effect of a Git stash apply or pop operation on a single file.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay, nq}")]
    public struct StashUpdatedFile : IComparable<StashUpdatedFile>, IEquatable<StashUpdatedFile>
    {
        public static readonly ITypeComparer<StashUpdatedFile> Comparer = new StashUpdatedFileComparer();

        internal StashUpdatedFile(string name, StashUpdatedFileType type)
        {
            _name = name;
            _type = type;
        }

        private string _name;
        private StashUpdatedFileType _type;

        /// <summary>
        /// Gets the name of the updated file.
        /// </summary>
        public string Path
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the type of update applied to the file.
        /// </summary>
        public StashUpdatedFileType Type
        {
            get { return _type; }
        }

        internal string DebuggerDisplay
        {
            get { return Invariant($"{nameof(StashUpdatedFile)}: [{_type}] \"{_name}\""); }
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// <para/>
        /// Returns a signed integer that indicates the relative values of this <see cref="StashUpdatedFile"/>  and `<paramref name="other"/>`.
        /// <para/>
        /// Less than zero when this <see cref="StashUpdatedFile"/> is less than `<paramref name="other"/>`.
        /// <para/>
        /// Zero when this <see cref="StashUpdatedFile"/> equals `<paramref name="other"/>`.
        /// <para/>
        /// Greater than zero when this <see cref="StashUpdatedFile"/> is greater than `<paramref name="other"/>`.
        /// </summary>
        public int CompareTo(StashUpdatedFile other)
            => Comparer.Compare(this, other);

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// <para/>
        /// Returns `<see langword="true"/>` if the specified objects are equal; otherwise, `<see langword="false"/>`.
        /// </summary>
        public bool Equals(StashUpdatedFile other)
            => Comparer.Equals(this, other);

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// <para/>
        /// Returns `<see langword="true"/>` if the specified objects are equal; otherwise, `<see langword="false"/>`.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is StashUpdatedFile a
                && Equals(this, a);
        }

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        public override string ToString()
        {
            return _name;
        }

        public static bool operator ==(StashUpdatedFile left, StashUpdatedFile right)
            => Comparer.Equals(left, right);

        public static bool operator !=(StashUpdatedFile left, StashUpdatedFile right)
            => !Comparer.Equals(left, right);
    }
}
