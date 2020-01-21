//*************************************************************************************************
// NamedObjectFilterOrdinalUtf8.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    internal interface INamedObjectFilterUtf8 : INamedObjectFilter
    {
        IEqualityComparer<StringUtf8> ComparerUtf8 { get; }

        StringUtf8 NameUtf8 { get; }

        bool Equals(StringUtf8 name);
    }

    internal class NamedObjectFilterOrdinalUtf8: INamedObjectFilterUtf8
    {
        public NamedObjectFilterOrdinalUtf8(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            _name = name;
            _nameUtf8 = (StringUtf8)name;
        }

        public NamedObjectFilterOrdinalUtf8(StringUtf8 name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            _name = (string)name;
            _nameUtf8 = name;
        }

        private readonly string _name;
        private readonly StringUtf8 _nameUtf8;

        public IEqualityComparer<string> Comparer
        {
            get { return StringComparer.Ordinal; }
        }

        public IEqualityComparer<StringUtf8> ComparerUtf8
        {
            get { return StringUtf8Comparer.Ordinal; }
        }

        public string Name
        {
            get { return _name; }
        }

        public StringUtf8 NameUtf8
        {
            get { return _nameUtf8; }
        }

        public bool Equals(string name)
        {
            return Comparer.Equals(_name, name);
        }

        public bool Equals(StringUtf8 name)
        {
            return ComparerUtf8.Equals(_nameUtf8, name);
        }
    }
}
