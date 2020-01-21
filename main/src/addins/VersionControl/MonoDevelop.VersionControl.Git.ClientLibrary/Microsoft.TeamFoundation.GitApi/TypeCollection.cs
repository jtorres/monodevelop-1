//*************************************************************************************************
// TypeCollection.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    public interface ITypeCollection<T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets the element at `<paramref name="index"/>`.
        /// </summary>
        T this[int index] { get; }

        /// <summary>
        /// Gets the element matching `<paramref name="name"/>`.
        /// </summary>
        T this[string name] { get; }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Returns <see langword="true"/> if the collection contains a value equal to `<paramref name="item"/>`; otherwise <see langword="null"/>.
        /// </summary>
        bool Contains(T item);

        /// <summary>
        /// Returns <see langword="true"/> if the collection contains a value matching `<paramref name="name"/>`; otherwise <see langword="null"/>.
        /// </summary>
        bool Contains(string name);
    }

    internal abstract class TypeCollection<T> : Base, ITypeCollection<T>
    {
        protected TypeCollection()
            : base()
        { }

        public abstract T this[int index] { get; }

        public abstract T this[string name] { get; }

        public abstract int Count { get; }

        internal abstract T this[StringUtf8 name] { get; }

        public abstract bool Contains(T item);

        public abstract bool Contains(string name);

        public abstract IEnumerator<T> GetEnumerator();

        internal abstract bool Add(T item);

        internal abstract void Clear();

        internal abstract bool Remove(T item);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
