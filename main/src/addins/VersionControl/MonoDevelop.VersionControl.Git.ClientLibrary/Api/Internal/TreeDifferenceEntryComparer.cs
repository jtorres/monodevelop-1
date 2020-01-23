//*************************************************************************************************
// TreeDifferenceEntryComparer.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System.Collections.Generic;

namespace Microsoft.TeamFoundation.GitApi
{
    partial class TypeComparer : IEqualityComparer<ITreeDifferenceEntry>, IEqualityComparer<ITreeDifferenceRenamedEntry>
    {
        public readonly IEqualityComparer<ITreeDifferenceEntry> TreeDifferenceEntryComparer = TreeDifferenceEntry.Comparer;

        /// <summary>
        /// <para>Determines whether the specified objects are equal.</para>
        /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
        /// </summary>
        public bool Equals(ITreeDifferenceEntry left, ITreeDifferenceEntry right)
            => TreeDifferenceEntryComparer.Equals(left, right);

        /// <summary>
        /// <para>Determines whether the specified objects are equal.</para>
        /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
        /// </summary>
        public bool Equals(ITreeDifferenceRenamedEntry left, ITreeDifferenceRenamedEntry right)
            => TreeDifferenceEntryComparer.Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public int GetHashCode(ITreeDifferenceEntry value)
            => TreeDifferenceEntryComparer.GetHashCode(value);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public int GetHashCode(ITreeDifferenceRenamedEntry value)
            => TreeDifferenceEntryComparer.GetHashCode(value);

        /// <summary>
        /// <para>Determines whether the specified objects are equal.</para>
        /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
        /// </summary>
        internal bool Equals(TreeDifferenceEntry left, TreeDifferenceEntry right)
            => TreeDifferenceEntryComparer.Equals(left, right);

        /// <summary>
        /// <para>Determines whether the specified objects are equal.</para>
        /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
        /// </summary>
        internal bool Equals(TreeDifferenceRenamedEntry left, TreeDifferenceRenamedEntry right)
            => TreeDifferenceEntryComparer.Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        internal int GetHashCode(TreeDifferenceEntry value)
            => TreeDifferenceEntryComparer.GetHashCode(value);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        internal int GetHashCode(TreeDifferenceRenamedEntry value)
            => TreeDifferenceEntryComparer.GetHashCode(value);
    }

    namespace Internal
    {
        internal class TreeDifferenceEntryComparer : IEqualityComparer<ITreeDifferenceEntry>, IEqualityComparer<TreeDifferenceEntry>, IEqualityComparer<ITreeDifferenceRenamedEntry>, IEqualityComparer<TreeDifferenceRenamedEntry>
        {
            private static readonly TypeComparer Comparer = new TypeComparer();

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public bool Equals(TreeDifferenceEntry left, TreeDifferenceEntry right)
            {
                if (ReferenceEquals(left, right))
                    return true;
                if (left is null || right is null)
                    return false;

                if (Comparer.PathEquals(left.PathUtf8, right.PathUtf8)
                    && Comparer.Equals(left.Target, right.Target)
                    && left.Sources.Count == right.Sources.Count)
                {
                    // Comparer the details per item
                    for (int i = 0; i < left.Sources.Count; i += 1)
                    {
                        if (!Comparer.Equals(left.Sources[i], right.Sources[i]))
                            return false;
                    }

                    return true;
                }

                return false;
            }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public bool Equals(TreeDifferenceRenamedEntry left, TreeDifferenceRenamedEntry right)
            {
                if (ReferenceEquals(left, right))
                    return true;
                if (left is null || right is null)
                    return false;

                if (Comparer.PathEquals(left.CurrentPathUtf8, right.CurrentPathUtf8)
                    && Comparer.PathEquals(left.OriginalPathUtf8, right.OriginalPathUtf8)
                    && Comparer.Equals(left.Target, right.Target)
                    && left.Sources.Count == right.Sources.Count)
                {
                    // Comparer the details per item
                    for (int i = 0; i < left.Sources.Count; i += 1)
                    {
                        if (!Comparer.Equals(left.Sources[i], right.Sources[i]))
                            return false;
                    }

                    return true;
                }

                return false;
            }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public bool Equals(ITreeDifferenceEntry left, ITreeDifferenceEntry right)
            {
                if (ReferenceEquals(left, right))
                    return true;
                if (left is null || right is null)
                    return false;

                if (left is TreeDifferenceEntry l && right is TreeDifferenceEntry r)
                    return Equals(l, r);

                if (Comparer.PathEquals(left.Path, right.Path)
                    && Comparer.Equals(left.Target, right.Target)
                    && left.Sources?.Count == right.Sources?.Count)
                {
                    // If the left side is null, then so is the right - no need to compare per item
                    if (left.Sources is null)
                        return true;

                    // Comparer the details per item
                    for (int i = 0; i < left.Sources.Count; i += 1)
                    {
                        if (!Comparer.Equals(left.Sources[i], right.Sources[i]))
                            return false;
                    }

                    return true;
                }

                return false;
            }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public bool Equals(ITreeDifferenceRenamedEntry left, ITreeDifferenceRenamedEntry right)
            {
                if (ReferenceEquals(left, right))
                    return true;
                if (left is null || right is null)
                    return false;

                if (left is TreeDifferenceRenamedEntry l && right is TreeDifferenceRenamedEntry r)
                    return Equals(l, r);

                if (Comparer.PathEquals(left.CurrentPath, right.CurrentPath)
                    && Comparer.PathEquals(left.OriginalPath, right.OriginalPath)
                    && Comparer.Equals(left.Target, right.Target)
                    && left.Sources?.Count == right.Sources?.Count)
                {
                    // If the left side is null, then so is the right - no need to compare per item
                    if (left.Sources is null)
                        return true;

                    // Comparer the details per item
                    for (int i = 0; i < left.Sources.Count; i += 1)
                    {
                        if (!Comparer.Equals(left.Sources[i], right.Sources[i]))
                            return false;
                    }

                    return true;
                }

                return false;
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(TreeDifferenceEntry value)
            {
                if (value is null)
                    return 0;

                return Comparer.PathGetHashCode(value.PathUtf8);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(TreeDifferenceRenamedEntry value)
            {
                if (value is null)
                    return 0;

                return Comparer.PathGetHashCode(value.PathUtf8);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(ITreeDifferenceEntry value)
            {
                if (value is null)
                    return 0;

                if (value is TreeDifferenceEntry entry)
                    return GetHashCode(entry);

                return Comparer.PathGetHashCode(value.Path);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(ITreeDifferenceRenamedEntry value)
            {
                if (value is null)
                    return 0;

                if (value is TreeDifferenceRenamedEntry entry)
                    return GetHashCode(entry);

                return Comparer.PathGetHashCode(value.Path);
            }
        }
    }
}
