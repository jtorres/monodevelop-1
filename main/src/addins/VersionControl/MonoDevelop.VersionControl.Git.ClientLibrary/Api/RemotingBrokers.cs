//*************************************************************************************************
// RemotingBrokers.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Threading.Tasks;

namespace Microsoft.TeamFoundation.GitApi
{
    public interface IBroker
    {
        Task<T> GetProxyAsync<T, TImp>(string serviceMoniker) where TImp : T, new() where T : class;
        string TranslateToSharedPathIfNecessary(string path, bool isDirectory);
        string TranslateFromSharedPathIfNecessary(string shared, bool isDirectory);
        void DownloadSharedPathIfNecessary(string shared);
        void ExecuteTaskSynchronously(Func<Task> asyncMethod);
        T ExecuteTaskSynchronously<T>(Func<Task<T>> asyncMethod);

        event EventHandler InvalidateProxies;
    }

    // This broker is super trivial for unit tests etc.
    // Asynchronous brokers should throw if GetProxyAsync is requested on the main thread, then call IServiceBroker.GetProxyAsync.
    public class SynchronousBroker : IBroker
    {
        public static SynchronousBroker Instance => new SynchronousBroker();

        public Task<T> GetProxyAsync<T, TImp>(string serviceMoniker) where TImp : T, new() where T : class
        {
            return Task.FromResult<T>(new TImp());
        }

        public string TranslateToSharedPathIfNecessary(string path, bool isDirectory)
        {
            return path;
        }

        public string TranslateFromSharedPathIfNecessary(string shared, bool isDirectory)
        {
            return shared;
        }

        public void DownloadSharedPathIfNecessary(string shared)
        {

        }

        public void ExecuteTaskSynchronously(Func<Task> asyncMethod)
        {
            asyncMethod().Wait();
        }

        public T ExecuteTaskSynchronously<T>(Func<Task<T>> asyncMethod)
        {
            return asyncMethod().Result;
        }

#pragma warning disable 67
        public event EventHandler InvalidateProxies;
#pragma warning restore 67
    }
}
