//*************************************************************************************************
// ReferenceCollection.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    public interface IReferenceCollection : ITypeCollection<IReference>
    {
        /// <summary>
        /// Gets the remote tracking branch matching <paramref name="remote"/>/<paramref name="name"/>.
        /// </summary>
        /// <param name="remote">The name of the remote.</param>
        /// <param name="name">The name of the branch.</param>
        IReference this[IRemoteName remote, IBranchName name] { get; }

        /// <summary>
        /// Gets the repository HEAD, if available; otherwise `<see langword="null"/>`.
        /// <para/>
        /// To insure a reference to HEAD is available use `<see cref="ReferenceOptionFlags.ReadHead"/>` with `<see cref="Cli.ForEachRefCommand.ReadCollection(ReferenceOptions)"/>`.
        /// </summary>
        IHead Head { get; }

        /// <summary>
        /// Gets a read-only list of all local branches contained in the collection.
        /// </summary>
        IReadOnlyList<IBranch> LocalBranches { get; }

        /// <summary>
        /// Gets a read-only list of all remote branches contained in the collection.
        /// </summary>
        IReadOnlyList<IBranch> RemoteBranches { get; }

        /// <summary>
        /// Gets a read-only list of all tags contained in the collection.
        /// </summary>
        IReadOnlyList<ITag> Tags { get; }

        /// <summary>
        /// Enumerates all branches in the collection.
        /// </summary>
        /// <param name="prefix">
        /// <para>The prefix, or namespace, of the branches to enumerate.</para>
        /// <para>Only branches beginning with the prefix will be returned.</para>
        /// </param>
        IEnumerable<IBranch> EnumerateBranches(string prefix);

        /// <summary>
        /// Enumerates all branches in the collection.
        /// </summary>
        IEnumerable<IBranch> EnumerateBranches();

        /// <summary>
        /// Enumerates all local branches in the collection containing `<paramref name="prefix"/>` in their name.
        /// </summary>
        /// <param name="prefix">The prefix, or namespace, to filter branches by.</param>
        IEnumerable<IBranch> EnumerateHeads(string prefix);

        /// <summary>
        /// Enumerates all local branches in the collection.
        /// </summary>
        IEnumerable<IBranch> EnumerateHeads();

        /// <summary>
        /// Enumerates all remote branches in the collection containing `<paramref name="prefix"/>` in their name.
        /// </summary>
        /// <param name="prefix">The prefix, or namespace, to filter branches by.</param>
        IEnumerable<IBranch> EnumerateRemotes(string prefix);

        /// <summary>
        /// Enumerates all remote branches in the collection.
        /// </summary>
        IEnumerable<IBranch> EnumerateRemotes();

        /// <summary>
        /// Enumerates all tags in the collection containing `<paramref name="prefix"/>` in their name.
        /// </summary>
        /// <param name="prefix">The prefix, or namespace, to filter branches by.</param>
        IEnumerable<ITag> EnumerateTags(string prefix);

        /// <summary>
        /// Enumerates all tags (refs/tags) contained in the collection.
        /// </summary>
        IEnumerable<ITag> EnumerateTags();
    }

    internal class ReferenceCollection : TypeCollection<IReference>, IReferenceCollection
    {
        public ReferenceComparer ReferenceComparer = Reference.Comparer;

        public ReferenceCollection(ReferenceOptionFlags flags)
            : base()
        {
            _list = new List<IReference>();
            _set = new HashSet<IReference>(new ReferenceCanonicalNameComparer());
            _syncpoint = new object();

            Debug.Assert((flags & ~ReferenceOptionFlags.Everything) == 0, $"Parameter `{nameof(flags)}` is invalid.");

            // Set these flags based on the requested flags rather than waiting
            // until refs are added.  It's possible that there will be no refs
            // in the repository that fall into some categories.
            _containsHeads = (flags & ReferenceOptionFlags.RefsHeads) != 0;
            _containsRemotes = (flags & ReferenceOptionFlags.RefsRemotes) != 0;
            _containsTags = (flags & ReferenceOptionFlags.RefsTags) != 0;
        }

        private bool _containsHeads;
        private bool _containsRemotes;
        private bool _containsTags;
        private IHead _head;
        private readonly List<IReference> _list;
        private readonly HashSet<IReference> _set;
        private readonly object _syncpoint;

        public override IReference this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));

                lock (_syncpoint)
                {
                    if (index >= _list.Count)
                        return null;

                    return _list[index];
                }
            }
        }

        public override IReference this[string name]
        {
            get
            {
                if (name is null)
                    return null;

                lock (_syncpoint)
                {
                    foreach (var reference in _list)
                    {
                        string friendlyName = reference.FriendlyName;

                        if (Reference.NameStringComparer.Equals(friendlyName, name))
                            return reference;

                        string canonicalName = reference.CanonicalName;

                        if (Reference.NameStringComparer.Equals(canonicalName, name))
                            return reference;
                    }
                }

                return null;
            }
        }

        public IReference this[IRemoteName remote, IBranchName name]
        {
            get
            {
                if (remote is null)
                    throw new ArgumentNullException(nameof(remote));
                if (name is null)
                    throw new ArgumentNullException(nameof(name));

                string target = FormattableString.Invariant($"{remote.Name}/{name.FriendlyName}");

                lock (_syncpoint)
                {
                    foreach (var reference in _list)
                    {
                        if (reference.ReferenceType == ReferenceType.Remotes
                            && ReferenceComparer.Equals(reference, target))
                        {
                            return reference;
                        }
                    }
                }

                return null;
            }
        }

        public override int Count
        {
            get { lock (_syncpoint) return _list.Count; }
        }

        public IHead Head
        {
            get { return Volatile.Read(ref _head); }
            internal set { Volatile.Write(ref _head, value); }
        }

        public IReadOnlyList<IBranch> LocalBranches
        {
            get
            {
                List<IBranch> result = new List<IBranch>();

                if (_containsHeads)
                {
                    foreach (var reference in _list)
                    {
                        if (reference.ReferenceType == ReferenceType.Heads)
                        {
                            if (reference is IBranch branch)
                            {
                                result.Add(branch);
                            }
                        }
                    }
                }

                return result;
            }
        }

        public IReadOnlyList<IBranch> RemoteBranches
        {
            get
            {
                lock (_syncpoint)
                {
                    List<IBranch> result = new List<IBranch>();

                    if (_containsRemotes)
                    {
                        foreach (var reference in _list)
                        {
                            if (reference.ReferenceType == ReferenceType.Remotes)
                            {
                                if (reference is IBranch branch)
                                {
                                    result.Add(branch);
                                }
                            }
                        }
                    }

                    return result;
                }
            }
        }

        public IReadOnlyList<ITag> Tags
        {
            get
            {
                lock (_syncpoint)
                {
                    List<ITag> result = new List<ITag>();

                    if (_containsTags)
                    {
                        foreach (var reference in _list)
                        {
                            if (reference.ReferenceType == ReferenceType.Tags)
                            {
                                if (reference is ITag tag)
                                {
                                    result.Add(tag);
                                }
                            }
                        }
                    }

                    return result;
                }
            }
        }

        internal override IReference this[StringUtf8 name]
            => this[(string)name];

        public override bool Contains(IReference item)
        {
            if (item is null)
                return false;

            lock (_syncpoint)
            {
                return _list.Contains(item);
            }
        }

        public override bool Contains(string name)
        {
            if (name is null)
                return false;

            return !(this[name] is null);
        }

        public IEnumerable<IBranch> EnumerateBranches(string @namespace)
        {
            foreach (var branch in EnumerateHeads(@namespace))
                yield return branch;

            foreach (var branch in EnumerateRemotes(@namespace))
                yield return branch;
        }
        public IEnumerable<IBranch> EnumerateBranches()
            => EnumerateBranches(null);

        public IEnumerable<IBranch> EnumerateHeads(string @namespace)
        {
            foreach (var item in this)
            {
                if (item.ReferenceType == ReferenceType.Heads
                    && (@namespace is null || item.FriendlyName.StartsWith(@namespace, Reference.NameStringComparison)))
                    yield return item as IBranch;
            }

            yield break;
        }
        public IEnumerable<IBranch> EnumerateHeads()
            => EnumerateHeads(null);

        public IEnumerable<IBranch> EnumerateRemotes(string @namespace)
        {
            foreach (var item in this)
            {
                if (item.ReferenceType == ReferenceType.Remotes
                    && (@namespace is null || item.FriendlyName.StartsWith(@namespace, Reference.NameStringComparison)))
                    yield return item as IBranch;
            }

            yield break;
        }
        public IEnumerable<IBranch> EnumerateRemotes()
            => EnumerateRemotes(null);

        public IEnumerable<ITag> EnumerateTags(string @namespace)
        {
            foreach (var item in this)
            {
                if (item.ReferenceType == ReferenceType.Tags
                    && (@namespace is null || item.FriendlyName.StartsWith(@namespace, Reference.NameStringComparison)))
                    yield return item as ITag;
            }

            yield break;
        }
        public IEnumerable<ITag> EnumerateTags()
            => EnumerateTags(null);

        public override IEnumerator<IReference> GetEnumerator()
        {
            IReference[] list;

            lock (_syncpoint)
            {
                list = _list.ToArray();
            }

            for (int i = 0; i < list.Length; i += 1)
            {
                yield return list[i];
            }
        }

        internal override bool Add(IReference reference)
        {
            if (reference is null)
                throw new ArgumentNullException(nameof(reference));

            lock (_syncpoint)
            {
                if (_set.Add(reference))
                {
                    _list.Add(reference);

                    return true;
                }
            }

            return false;
        }

        internal override void Clear()
        {
            lock (_syncpoint)
            {
                _list.Clear();
                _set.Clear();
            }
        }

        internal override bool Remove(IReference reference)
        {
            if (reference is null)
                throw new ArgumentNullException(nameof(reference));

            lock (_syncpoint)
            {
                return _set.Remove(reference)
                    && _list.Remove(reference);
            }
        }
    }
}
