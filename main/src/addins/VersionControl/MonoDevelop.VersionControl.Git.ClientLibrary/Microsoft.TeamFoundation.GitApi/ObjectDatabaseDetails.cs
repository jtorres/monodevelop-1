//*************************************************************************************************
// ObjectDatabaseDetails.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class ObjectDatabaseDetails
    {
        internal ObjectDatabaseDetails(long duplicateCount,
                                       long garbageCount,
                                       long garbageSize,
                                       long looseCount,
                                       long looseSize,
                                       long packedCount,
                                       long packedSize,
                                       int packsCount)
        {
            if (duplicateCount < 0)
                throw new ArgumentOutOfRangeException(nameof(duplicateCount));
            if (garbageCount < 0)
                throw new ArgumentOutOfRangeException(nameof(garbageCount));
            if (garbageSize < 0)
                throw new ArgumentOutOfRangeException(nameof(garbageSize));
            if (looseCount < 0)
                throw new ArgumentOutOfRangeException(nameof(looseCount));
            if (looseSize < 0)
                throw new ArgumentOutOfRangeException(nameof(looseSize));
            if (packedCount < 0)
                throw new ArgumentOutOfRangeException(nameof(packedCount));
            if (packedSize < 0)
                throw new ArgumentOutOfRangeException(nameof(packedSize));
            if (packsCount < 0)
                throw new ArgumentOutOfRangeException(nameof(packsCount));

            DuplicateCount = duplicateCount;
            GarbageCount = garbageCount;
            GarbageSize = garbageSize;
            LooseCount = looseCount;
            LooseSize = looseSize;
            PackedCount = packedCount;
            PackedSize = packedSize;
            PacksCount = packsCount;
        }

        /// <summary>
        /// The count of loose objects that are also present in the packs.
        /// </summary>
        public readonly long DuplicateCount;
        /// <summary>
        /// The count of files in object database that are neither valid loose objects nor valid packs.
        /// </summary>
        public readonly long GarbageCount;
        /// <summary>
        /// Disk space consumed by garbage files, in bytes.
        /// </summary>
        public readonly long GarbageSize;
        /// <summary>
        /// The count of loose objects.
        /// </summary>
        public readonly long LooseCount;
        /// <summary>
        /// Disk space consumed by loose objects.
        /// </summary>
        public readonly long LooseSize;
        /// <summary>
        /// The count of objects in the packs.
        /// </summary>
        public readonly long PackedCount;
        /// <summary>
        /// Disk space consumed by the packs in bytes.
        /// </summary>
        public readonly long PackedSize;
        /// <summary>
        /// The count of packs in the object database.
        /// </summary>
        public readonly int PacksCount;
    }
}
