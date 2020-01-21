//*************************************************************************************************
// Remote.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git remote.
    /// </summary>
    public interface IRemote : IRemoteName
    {
        /// <summary>
        /// Gets the URL, or address, used to download content from the remote repository.
        /// </summary>
        string FetchUrl { get; }

        /// <summary>
        /// Gets the URL, or address, used to upload content to the remote repository.
        /// </summary>
        string PushUrl { get; }
    }

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Remote : Base, IRemote
    {
        public static readonly ITypeComparer<IRemote> Comparer = new RemoteComparer();
        public static readonly ITypeComparer<IRemoteName> NameComparer = RemoteName.Comparer;
        public static readonly StringComparer NameStringComparer = RemoteName.StringComparer;
        public static readonly StringComparison NameStringComparison = RemoteName.StringComparison;
        public static readonly StringComparer UrlStringComparer = StringComparer.OrdinalIgnoreCase;
        public static readonly StringComparison UrlStringComparison = StringComparison.OrdinalIgnoreCase;

        internal static readonly StringUtf8Comparer NameStringUtf8Comparer = RemoteName.StringUtf8Comparer;
        internal static readonly StringUtf8Comparer UrlStringUtf8Comparer = StringUtf8Comparer.OrdinalIgnoreCase;

        internal Remote(StringUtf8 name, StringUtf8 fetchUrl, StringUtf8 pushUrl)
            : base()
        {
            if (ReferenceEquals(name, null))
                throw new ArgumentNullException(nameof(name));
            if (ReferenceEquals(fetchUrl, null))
                throw new ArgumentNullException(nameof(fetchUrl));
            if (!IsLegalName((string)name))
                throw RemoteNameException.FromName((string)name);

            _fetchUrl = fetchUrl;
            _name = name;
            _pushUrl = pushUrl ?? fetchUrl;
        }

        private readonly StringUtf8 _fetchUrl;
        private readonly StringUtf8 _pushUrl;
        private readonly StringUtf8 _name;

        [JsonProperty]
        public string FetchUrl
        {
            get { return (string)_fetchUrl; }
        }

        [JsonProperty]
        public string PushUrl
        {
            get { return (string)_pushUrl; }
        }

        [JsonProperty]
        public string Name
        {
            get { return (string)_name; }
            }

        internal StringUtf8 FetchUrlUtf8
        {
            get { return _fetchUrl; }
        }

        internal StringUtf8 PushUrlUtf8
        {
            get { return _pushUrl; }
        }

        internal StringUtf8 NameUtf8
        {
            get { return _name; }
        }

        private string DebuggerDisplay
        {
            get
            {
                return (_fetchUrl == _pushUrl)
                    ? $"{_name}: {_fetchUrl}"
                    : $"{_name}: {_fetchUrl} | {_pushUrl}";
            }
        }

        /// <summary>
        /// Determine if remoteName is syntactically valid remote name.
        /// </summary>
        public static bool IsLegalName(string name)
            => RemoteName.IsLegalName(name);
    }
}
