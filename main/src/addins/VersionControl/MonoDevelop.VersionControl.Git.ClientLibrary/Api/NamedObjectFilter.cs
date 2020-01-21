//*************************************************************************************************
// NamedObjectFilter.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git named-object filter.
    /// </summary>
    public interface INamedObjectFilter
    {
        /// <summary>
        /// Gets the comparer used during filter operations.
        /// </summary>
        IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// The name used when applying filters to `<see cref="Cli.CatFileCommand"/>` object reads.
        /// </summary>
        string Name { get; }

        bool Equals(string name);
    }

    internal class NamedObjectFilter : INamedObjectFilter
    {
        public NamedObjectFilter(string name, IEqualityComparer<string> comparer)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            _comparer = comparer;
            _name = name;
        }

        private NamedObjectFilter()
        {
            _comparer = null;
            _name = null;
        }

        private readonly IEqualityComparer<string> _comparer;
        private readonly string _name;

        public string Name
        {
            get { return _name; }
        }

        public IEqualityComparer<string> Comparer
        {
            get { return _comparer; }
        }

        public bool Equals(string name)
        {
            if (ReferenceEquals(name, null))
                return false;

            return _name == null
                || _comparer == null
                || _comparer.Equals(_name, name);
        }
    }
}
