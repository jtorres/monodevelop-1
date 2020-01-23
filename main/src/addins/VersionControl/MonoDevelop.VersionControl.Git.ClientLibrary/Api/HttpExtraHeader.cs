//*************************************************************************************************
// HttpExtraHeader.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    public struct HttpExtraHeader : IComparable<HttpExtraHeader>, IEquatable<HttpExtraHeader>
    {
        public static readonly StringComparer NameStringComparer = StringComparer.OrdinalIgnoreCase;
        public static readonly StringComparison NameStringComparison = StringComparison.OrdinalIgnoreCase;
        public static readonly StringComparer ValueStringComparer = StringComparer.OrdinalIgnoreCase;
        public static readonly StringComparison ValueStringComparison = StringComparison.OrdinalIgnoreCase;

        internal static readonly HttpExtraHeaderComparer Comparer = new HttpExtraHeaderComparer();

        public HttpExtraHeader(string name, string value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (!Extensions.IsEscapedCorrectly(name, '\\', '\''))
                throw new ArgumentException($"'\\'' characters are invalid.", nameof(name));
            if (!Extensions.IsEscapedCorrectly(name, '\\', '"'))
                throw new ArgumentException($"'\"' characters are invalid.", nameof(name));
            if (!Extensions.IsEscapedCorrectly(value, '\\', '\''))
                throw new ArgumentException($"'\\'' characters are invalid.", nameof(value));
            if (!Extensions.IsEscapedCorrectly(value, '\\', '"'))
                throw new ArgumentException($"'\"' characters are invalid.", nameof(value));

            Name = name;
            Value = value;
        }

        public readonly string Name;
        public readonly string Value;

        public int CompareTo(HttpExtraHeader other)
        {
            return Comparer.Compare(this, other);
        }

        public bool Equals(HttpExtraHeader other)
        {
            return Comparer.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return obj is HttpExtraHeader
                && Equals((HttpExtraHeader)obj);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }

        public override string ToString()
        {
            return FormattableString.Invariant($"{Name} = {Value}");
        }

        public static bool operator ==(HttpExtraHeader left, HttpExtraHeader right)
            => Equals(left, right);

        public static bool operator !=(HttpExtraHeader left, HttpExtraHeader right)
            => !Equals(left, right);
    }
}
