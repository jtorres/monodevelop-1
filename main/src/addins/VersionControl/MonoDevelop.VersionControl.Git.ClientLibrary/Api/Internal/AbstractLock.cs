//*************************************************************************************************
// AbstractLock.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Threading;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal abstract class AbstractLock
    {
        protected AbstractLock()
        {
            _syncpoint = new object();
        }

        protected readonly object _syncpoint;

        public IDisposable AcquireExclusive()
        {
            Monitor.Enter(_syncpoint);

            return new Releaser(ReleaseExclusive);
        }

        private void ReleaseExclusive()
        {
            Monitor.Exit(_syncpoint);
        }

        private struct Releaser : IDisposable
        {
            public Releaser(Action releaseCallback)
            {
                _releaseCallback = releaseCallback;
            }

            private Action _releaseCallback;

            public void Dispose()
            {
                Action releaseCallback;
                if ((releaseCallback = Interlocked.Exchange(ref _releaseCallback, null)) != null)
                {
                    releaseCallback();
                }
            }
        }
    }
}
