//*************************************************************************************************
// ConcurrentSet.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    [DebuggerDisplay("ConcurrentSet({Count,nq})")]
    internal sealed class ConcurrentSet<T> : AbstractLock, ICollection<T>, IReadOnlyCollection<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class that is
        /// empty and has the default initial capacity.
        /// </summary>
        public ConcurrentSet()
        {
            _set = new HashSet<T>();
            _isReadOnly = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class that is
        /// empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity"></param>
        public ConcurrentSet(IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(comparer);
            _isReadOnly = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class that
        /// contains elements copied from the specified collection and has sufficient capacity to
        /// accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection"></param>
        public ConcurrentSet(IEnumerable<T> collection)
        {
            _set = new HashSet<T>(collection);
            _isReadOnly = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class that
        /// contains elements copied from the specified collection and has sufficient capacity to
        /// accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="comparer"></param>
        public ConcurrentSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(collection, comparer);
            _isReadOnly = false;
        }

        private ConcurrentSet(ConcurrentSet<T> original, bool isReadOnly)
        {
            lock (original._syncpoint)
            {
                _set = new HashSet<T>(original._set);
            }

            _isReadOnly = isReadOnly;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ConcurrentSet{T}"/>.
        /// </summary>
        public int Count
        {
            get { lock (_syncpoint) return _set.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ConcurrentSet{T}"/> is
        /// read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        private readonly HashSet<T> _set;
        private readonly bool _isReadOnly;

        /// <summary>
        /// Adds an object to the end of the <see cref="ConcurrentSet{T}"/>.
        /// </summary>
        /// <param name="item">The object to be added to the end of the
        /// <see cref="ConcurrentSet{T}"/>. The value can be null for reference types.</param>
        public void Add(T item)
        {
            lock (_syncpoint)
            {
                if (_isReadOnly)
                    throw new InvalidOperationException(nameof(IsReadOnly));

                _set.Add(item);
            }
        }

        /// <summary>
        /// Returns a read-only <see cref="IReadOnlyCollection{T}"/> wrapper for the current
        /// collection.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<T> AsReadOnly()
        {
            return new ConcurrentSet<T>(this, true);
        }

        /// <summary>
        /// Removes all elements from the <see cref="ConcurrentSet{T}"/>.
        /// </summary>
        public void Clear()
        {
            lock (_syncpoint)
            {
                if (_isReadOnly)
                    throw new InvalidOperationException(nameof(IsReadOnly));

                _set.Clear();
            }
        }

        /// <summary>
        /// Determines whether an element is in the <see cref="ConcurrentSet{T}"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ConcurrentSet{T}"/>.
        /// The value can be null for reference types.</param>
        /// <returns><see langword="true"/> if item is found in the
        /// <see cref="ConcurrentSet{T}"/>; otherwise, <see langword="false"/>.</returns>
        public bool Contains(T item)
        {
            lock (_syncpoint)
            {
                return _set.Contains(item);
            }
        }

        /// <summary>
        /// Copies the entire <see cref="ConcurrentSet{T}"/> to a compatible one-dimensional
        /// array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements
        /// copied from <see cref="ConcurrentSet{T}"/>. The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_syncpoint)
            {
                _set.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ConcurrentSet{T}"/>.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (_isReadOnly)
            {
                T item;
                IEnumerator<T> enumerator = null;

                try
                {
                    lock (_syncpoint)
                    {
                        enumerator = _set.GetEnumerator();
                    }

                    while (true)
                    {
                        lock (_syncpoint)
                        {
                            if (!enumerator.MoveNext())
                                break;

                            item = enumerator.Current;
                        }

                        yield return item;
                    }
                }
                finally
                {
                    if (enumerator != null)
                        enumerator.Dispose();
                }

                yield break;
            }
            else
            {
                T[] snapshot;

                lock (_syncpoint)
                {
                    snapshot = new T[_set.Count];
                    _set.CopyTo(snapshot);
                }

                for (int i=0; i < snapshot.Length; i += 1)
                {
                    yield return snapshot[i];
                }
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the
        /// <see cref="ConcurrentSet{T}"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ConcurrentSet{T}"/>.
        /// The value can be null for reference types.</param>
        /// <returns><see langword="true"/> if item is successfully removed; otherwise,
        /// <see langword="false"/>. This method also returns <see langword="false"/> if item was
        /// not found in the <see cref="ConcurrentSet{T}"/>.</returns>
        public bool Remove(T item)
        {
            lock (_syncpoint)
            {
                if (_isReadOnly)
                    throw new InvalidOperationException(nameof(IsReadOnly));

                return _set.Remove(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
