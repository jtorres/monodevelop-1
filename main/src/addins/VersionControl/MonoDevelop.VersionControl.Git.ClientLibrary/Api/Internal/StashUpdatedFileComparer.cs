//*************************************************************************************************
// StashUpdatedFileComparer.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    partial class TypeComparer
    {
        public static readonly ITypeComparer<StashUpdatedFile> StashUpdateFileComparer = StashUpdatedFile.Comparer;

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// <para/>
        /// Returns a signed integer that indicates the relative values of `<paramref name="left"/>` and `<paramref name="right"/>`.
        /// <para/>
        /// Less than zero when `<paramref name="left"/>` is less than `<paramref name="right"/>`.
        /// <para/>
        /// Zero when `<paramref name="left"/>` equals `<paramref name="right"/>`.
        /// <para/>
        /// Greater than zero when `<paramref name="left"/>` is greater than `<paramref name="right"/>`.
        /// </summary>
        public int Compare(StashUpdatedFile left, StashUpdatedFile right)
            => StashUpdateFileComparer.Compare(left, right);

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// <para/>
        /// Returns `<see langword="true"/>` if the specified objects are equal; otherwise, `<see langword="false"/>`.
        /// </summary>
        public bool Equals(StashUpdatedFile left, StashUpdatedFile right)
            => StashUpdateFileComparer.Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public int GetHashCode(StashUpdatedFile value)
            => StashUpdateFileComparer.GetHashCode(value);
    }

    namespace Internal
    {
        internal class StashUpdatedFileComparer : ITypeComparer<StashUpdatedFile>
        {
            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// <para/>
            /// Returns a signed integer that indicates the relative values of `<paramref name="left"/>` and `<paramref name="right"/>`.
            /// <para/>
            /// Less than zero when `<paramref name="left"/>` is less than `<paramref name="right"/>`.
            /// <para/>
            /// Zero when `<paramref name="left"/>` equals `<paramref name="right"/>`.
            /// <para/>
            /// Greater than zero when `<paramref name="left"/>` is greater than `<paramref name="right"/>`.
            /// </summary>
            public int Compare(object left, object right)
            {
                if (ReferenceEquals(left, right))
                    return 0;
                if (left is null)
                    return 1;
                if (right is null)
                    return -1;

                if (left is StashUpdatedFile leftUpdate && right is StashUpdatedFile rightUpdate)
                    return PathComparer.StringComparer.Compare(leftUpdate.Path, rightUpdate.Path);

                throw new NotSupportedException($"Compare({left.GetType().Name}, {right.GetType().Name}) is not supported.");
            }

            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// <para/>
            /// Returns a signed integer that indicates the relative values of `<paramref name="left"/>` and `<paramref name="right"/>`.
            /// <para/>
            /// Less than zero when `<paramref name="left"/>` is less than `<paramref name="right"/>`.
            /// <para/>
            /// Zero when `<paramref name="left"/>` equals `<paramref name="right"/>`.
            /// <para/>
            /// Greater than zero when `<paramref name="left"/>` is greater than `<paramref name="right"/>`.
            /// </summary>
            public int Compare(StashUpdatedFile left, StashUpdatedFile right)
            {
                return PathComparer.StringComparer.Compare(left.Path, right.Path);
            }

            /// <summary>
            /// Determines whether the specified objects are equal.
            /// <para/>
            /// Returns `<see langword="true"/>` if the specified objects are equal; otherwise, `<see langword="false"/>`.
            /// </summary>
            public new bool Equals(object left, object right)
            {
                if (ReferenceEquals(left, right))
                    return true;
                if (left is null || right is null)
                    return false;

                if (left is StashUpdatedFile leftUpdate && right is StashUpdatedFile rightUpdate)
                    return Equals(leftUpdate, rightUpdate);

                throw new NotSupportedException($"Equals({left.GetType().Name}, {right.GetType().Name}) is not supported.");
            }

            /// <summary>
            /// Determines whether the specified objects are equal.
            /// <para/>
            /// Returns `<see langword="true"/>` if the specified objects are equal; otherwise, `<see langword="false"/>`.
            /// </summary>
            public bool Equals(StashUpdatedFile left, StashUpdatedFile right)
            {
                return left.Type == right.Type
                    && PathComparer.StringComparer.Equals(left.Path, right.Path);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(object value)
            {
                if (value is null)
                    return 0;

                if (value is StashUpdatedFile entry)
                    return GetHashCode(entry);

                throw new NotSupportedException($"GetHashCode({value.GetType().Name}) is not supported.");
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(StashUpdatedFile value)
            {
                return PathComparer.StringComparer.GetHashCode(value.Path);
            }
        }
    }
}
