//*************************************************************************************************
// ObjectHeader.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Represents an object stored in the Git object database.
    /// </summary>
    public struct ObjectHeader : IEquatable<ObjectHeader>, ILoggable
    {
        /// <summary>
        /// Represents an unknown object size.
        /// </summary>
        public const long InvalidSize = -1;

        /// <summary>
        /// Represents an read error, missing, or unknown header
        /// </summary>
        public static readonly ObjectHeader Unknown = new ObjectHeader(ObjectType.Unknown);

        internal static readonly ObjectHeaderComparer Comparer = new ObjectHeaderComparer();

        internal ObjectHeader(ObjectId objectId, ObjectType objectType, long objectSize)
            : this(objectId, objectType)
        {
            Size = objectSize;
        }

        internal ObjectHeader(ObjectId objectId, ObjectType objectType)
        {
            Debug.Assert(objectId != ObjectId.Zero, $"The `{nameof(objectId)}` is invalid.");
            Debug.Assert(Enum.IsDefined(typeof(ObjectType), objectType), $"The '{nameof(objectType)}` is undefined.");

            ObjectId = objectId;
            Type = objectType;
            Size = InvalidSize;
        }

        private ObjectHeader(ObjectType objectType)
        {
            ObjectId = ObjectId.Zero;
            Type = objectType;
            Size = InvalidSize;
        }

        /// <summary>
        /// The object identity associated with the header and referenced object.
        /// </summary>
        public ObjectId ObjectId;

        /// <summary>
        /// Gets the size of the object the header represents.
        /// <para/>
        /// Returns `<see cref="InvalidSize"/>` if the size is unknown.
        /// </summary>
        public long Size;

        /// <summary>
        /// Gets the type of the object the header represents.
        /// </summary>
        public ObjectType Type;

        public bool Equals(ObjectHeader other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            if (obj is ObjectHeader)
                return Equals((ObjectHeader)obj);

            return base.Equals(obj);
        }

        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        public override string ToString()
        {
            return FormattableString.Invariant($"{Type}: {ObjectId}");
        }

        void ILoggable.Log(ExecutionContext context, System.Text.StringBuilder log, int indent)
        {
            string prefix = context.ParseHelper.GetParseErrorIndent(indent);
            log.Append(prefix).AppendLine($"ObjectHeader {{ Type: {Type}, Size: {Size}, ObjectId: {ObjectId.ToString()} }}");
        }

        public static bool operator ==(ObjectHeader left, ObjectHeader right)
            => Comparer.Equals(left, right);

        public static bool operator !=(ObjectHeader left, ObjectHeader right)
            => !Comparer.Equals(left, right);
    }
}
