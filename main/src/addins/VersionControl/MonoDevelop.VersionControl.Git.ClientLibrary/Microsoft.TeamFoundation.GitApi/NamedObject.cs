//*************************************************************************************************
// NamedObject.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git named-object.
    /// </summary>
    public interface INamedObject : IEquatable<INamedObject>
    {
        /// <summary>
        /// Get the object name.
        /// </summary>
        string ObjectName { get; }

        /// <summary>
        /// Get the object identity.
        /// </summary>
        ObjectId ObjectId { get; }

        /// <summary>
        /// Get the object type.
        /// </summary>
        ObjectType ObjectType { get; }
    }

    /// <summary>
    /// Strongly-typed derivation of `<seealso cref="INamedObject"/>`.
    /// </summary>
    /// <typeparam name="T">An `<seealso cref="IObject"/>` derived type.</typeparam>
    public interface INamedObject<T> : IEquatable<INamedObject<T>>, INamedObject
        where T : IObject
    {
        /// <summary>
        /// Gets the named <see cref="IObject"/>
        /// </summary>
        T Object { get; }
    }

    internal struct NamedObject<T> : IEquatable<NamedObject<T>>, INamedObject<T>
        where T : IObject
    {
        public static readonly NamedObjectComparer Comparer = new NamedObjectComparer();

        public NamedObject(StringUtf8 name, T @object)
        {
            if (ReferenceEquals(name, null))
                throw new ArgumentNullException(nameof(name));
            if (ReferenceEquals(@object, null))
                throw new ArgumentNullException(nameof(@object));

            _name = name;
            _object = @object;
        }

        private StringUtf8 _name;
        private T _object;

        public T Object
        {
            get { return _object; }
        }

        public string ObjectName
        {
            get { return (string)_name; }
        }

        public ObjectId ObjectId
        {
            get { return Object.ObjectId; }
        }

        public ObjectType ObjectType
        {
            get { return Object.ObjectType; }
        }

        internal StringUtf8 NameUtf8
        {
            get { return _name; }
        }

        public bool Equals(NamedObject<T> other)
            => Comparer.Equals(this, other);

        public bool Equals(INamedObject<T> other)
            => Comparer.Equals(this, other);

        public bool Equals(INamedObject other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            return (obj is NamedObject<T> && Equals((NamedObject<T>)obj))
                || Equals(obj as INamedObject)
                || base.Equals(obj);
        }

        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        public override string ToString()
        {
            return FormattableString.Invariant($"{ObjectId}:{ObjectName}");
        }

        public static bool operator ==(NamedObject<T> left, NamedObject<T> right)
            => Comparer.Equals(left, right);

        public static bool operator !=(NamedObject<T> left, NamedObject<T> right)
            => !Comparer.Equals(left, right);

        public static bool operator ==(NamedObject<T> left, INamedObject<T> right)
            => Comparer.Equals(left, right);

        public static bool operator !=(NamedObject<T> left, INamedObject<T> right)
            => !Comparer.Equals(left, right);

        public static bool operator ==(NamedObject<T> left, INamedObject right)
            => Comparer.Equals(left, right);

        public static bool operator !=(NamedObject<T> left, INamedObject right)
            => !Comparer.Equals(left, right);
    }
}
