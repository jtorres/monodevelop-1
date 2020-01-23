//*************************************************************************************************
// Environment.Variable.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    partial class Environment
    {
        /// <summary>
        /// Object-model representation of an operating system environment variable key/value pair.
        /// </summary>
        [DebuggerDisplay("{Name} => {Value}")]
        public struct Variable : IComparable<Variable>, IEquatable<Variable>
        {
            public static readonly Variable Empty = new Variable(string.Empty, string.Empty);
            public static readonly StringComparer NameStringComparer = StringComparer.OrdinalIgnoreCase;
            public static readonly StringComparer ValueStringComparer = StringComparer.OrdinalIgnoreCase;

            internal static readonly EnvironmentVariableComparer Comparer = new EnvironmentVariableComparer();

            /// <summary>
            /// Creates a new `<see cref="Variable"/>`.
            /// </summary>
            /// <param name="name">The name of the variable.</param>
            /// <param name="value">The value of the variable.</param>
            public Variable(string name, string value)
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));

                Name = name;
                Value = value;
            }

            /// <summary>
            /// The name or key of the variable.
            /// </summary>
            public readonly string Name;
            /// <summary>
            /// The value of the variable.
            /// </summary>
            public readonly string Value;

            public int CompareTo(Variable other)
                => Comparer.Compare(this, other);

            public override bool Equals(object obj)
            {
                return obj is Variable
                    && Comparer.Equals(this, (Variable)obj);
            }

            public bool Equals(Variable other)
                => Comparer.Equals(this, other);

            public override int GetHashCode()
                => NameStringComparer.GetHashCode(Name);

            public static bool operator ==(Variable left, Variable right)
                => Comparer.Equals(left, right);

            public static bool operator !=(Variable left, Variable right)
                => !Comparer.Equals(left, right);
        }
    }
}
