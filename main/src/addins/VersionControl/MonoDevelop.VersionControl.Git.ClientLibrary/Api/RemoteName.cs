//*************************************************************************************************
// RemoteName.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model abstraction for a Git remote or its name.
    /// </summary>
    public interface IRemoteName
    {
        /// <summary>
        /// Gets the human friendly name of this remote.
        /// </summary>
        string Name { get; }
    }

    public sealed class RemoteName : IRemoteName
    {
        public static readonly ITypeComparer<IRemoteName> Comparer = new RemoteNameComparer();
        public static readonly StringComparison StringComparison = StringComparison.Ordinal;
        public static readonly StringComparer StringComparer = StringComparer.Ordinal;

        internal static readonly StringUtf8Comparer StringUtf8Comparer = StringUtf8Comparer.Ordinal;

        public RemoteName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (!IsLegalName(name))
                throw RemoteNameException.FromName(name);

            _name = name;
        }

        internal RemoteName(StringUtf8 name)
            : this((string)name)
        { }

        private readonly string _name;

        public string Name
        {
            get { return _name; }
        }

        internal StringUtf8 NameUtf8
        {
            get { return (StringUtf8)_name; }
        }

        /// <summary>
        /// Determine if remoteName is syntactically valid remote name.
        /// </summary>
        public static bool IsLegalName(string name)
        {
            // Remote names appear inside a reference, so they must play
            // by the same rules.  But since they are never the terminal
            // part of the ref, the final "." rule doesn't apply.
            //
            // Put the suggested remote name inside an otherwise known-good
            // reference and validate that.

            string referenceName = FormattableString.Invariant($"refs/remotes/{name}/test");

            return ReferenceName.IsLegalName(referenceName);
        }

        public static implicit operator RemoteName(string value)
        {
            if (ReferenceEquals(value, null))
                return null;

            return new RemoteName(value);
        }
    }
}
