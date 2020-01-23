//*************************************************************************************************
// RemoteCollection.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    public interface IRemoteCollection : ITypeCollection<IRemote>
    { }

    internal class RemoteCollection : TypeCollection<IRemote>, IRemoteCollection
    {
        public RemoteCollection()
            : base()
        {
            _remotes = new List<IRemote>();
            _syncpoint = new object();
        }

        private readonly List<IRemote> _remotes;
        private readonly object _syncpoint;

        public override int Count
        {
            get { lock (_syncpoint) return _remotes.Count; }
        }

        public override IRemote this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));

                lock (_syncpoint)
                {
                    if (index > _remotes.Count)
                        return null;

                    return _remotes[index];
                }
            }
        }

        public override IRemote this[string name]
        {
            get
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));

                lock (_syncpoint)
                {
                    foreach (var remote in _remotes)
                    {
                        if (Remote.NameStringComparer.Equals(remote.Name, name))
                            return remote;
                    }
                }

                return null;
            }
        }

        internal override IRemote this[StringUtf8 name]
            => this[(string)name];

        public override bool Contains(IRemote item)
        {
            lock (_syncpoint)
            {
                foreach (var remote in _remotes)
                {
                    if (remote == item)
                        return true;
                }
            }

            return false;
        }

        public override bool Contains(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return this[name] != null;
        }

        public override IEnumerator<IRemote> GetEnumerator()
        {
            try
            {
                Monitor.Enter(_syncpoint);

                IRemote result;
                for (int i = 0; i < _remotes.Count; i += 1)
                {
                    result = _remotes[i];
                    yield return result;
                }
            }
            finally
            {
                Monitor.Exit(_syncpoint);
            }
        }

        internal override bool Add(IRemote remote)
        {
            if (remote == null)
                throw new ArgumentNullException(nameof(remote));

            lock (_syncpoint)
            {
                if (!_remotes.Contains(remote))
                {
                    _remotes.Add(remote);
                    return true;
                }
            }

            return false;
        }

        internal override void Clear()
        {
            lock (_syncpoint)
            {
                _remotes.Clear();
            }
        }

        internal override bool Remove(IRemote remote)
        {
            if (remote == null)
                throw new ArgumentNullException(nameof(remote));

            lock (_syncpoint)
            {
                return _remotes.Remove(remote);
            }
        }
    }
}
