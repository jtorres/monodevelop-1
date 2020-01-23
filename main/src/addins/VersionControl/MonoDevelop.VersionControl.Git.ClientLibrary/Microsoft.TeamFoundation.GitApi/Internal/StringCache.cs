//*************************************************************************************************
// StringCache.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal interface IStringCache
    {
        string Intern(string value);
        StringUtf8 Intern(StringUtf8 value);
    }

    internal sealed class StringCache : IDisposable, IStringCache
    {
        public const int InitialCacheSize = 4 * 1024;

        public StringCache()
        {
            _strings = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
            _mbstrings = new ConcurrentDictionary<StringUtf8, StringUtf8>(StringUtf8Comparer.Ordinal);
        }

        public int Count { get { return _strings.Count + _mbstrings.Count; } }

        private ConcurrentDictionary<string, string> _strings;
        private ConcurrentDictionary<StringUtf8, StringUtf8> _mbstrings;

        public void Dispose()
        {
            ConcurrentDictionary<StringUtf8, StringUtf8> mbstrings;
            if ((mbstrings = Interlocked.Exchange(ref _mbstrings, null)) != null)
            {
                mbstrings.Clear();
            }

            ConcurrentDictionary<string, string> strings;
            if ((strings = Interlocked.Exchange(ref _strings, null)) != null)
            {
                strings.Clear();
            }
        }

        public string Intern(string value)
        {
            if (ReferenceEquals(value, null))
                return null;

            var values = Volatile.Read(ref _strings);

            if (ReferenceEquals(values, null))
                return value;

            return values.GetOrAdd(value, value);
        }

        public StringUtf8 Intern(StringUtf8 value)
        {
            if (ReferenceEquals(value, null))
                return null;

            var values = Volatile.Read(ref _mbstrings);

            return values.GetOrAdd(value, value);
        }
    }
}
