//*************************************************************************************************
// NamedObjectFilterOrdinal.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class NamedObjectFilterOrdinal : INamedObjectFilter
    {
        public NamedObjectFilterOrdinal(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            _name = name;
        }

        private readonly string _name;

        public IEqualityComparer<string> Comparer
        {
            get { return StringComparer.Ordinal; }
        }

        public string Name
        {
            get { return _name; }
        }

        public bool Equals(string name)
        {
            return Comparer.Equals(_name, name);
        }

        public override bool Equals(object obj)
        {
            return Equals((obj as NamedObjectFilterOrdinal)?.Name);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(Name);
        }
    }
}
