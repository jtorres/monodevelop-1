/**** Git Process Management Library ****
 *
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the ""Software""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    partial class TypeComparer : ITypeComparer<ConfigurationEntry>
    {
        public static readonly ITypeComparer<ConfigurationEntry> ConfigurationEntryComparer = new ConfigurationEntryComparer();

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// <para>Returns a signed integer that indicates the relative values of `<paramref name="left"/>` and `<paramref name="right"/>`.</para>
        /// <para>Less than zero when `<paramref name="left"/>` is less than `<paramref name="right"/>`.</para>
        /// <para>Zero when `<paramref name="left"/>` equals `<paramref name="right"/>`.</para>
        /// <para>Greater than zero when `<paramref name="left"/>` is greater than `<paramref name="right"/>`.</para>
        /// </summary>
        public int Compare(ConfigurationEntry left, ConfigurationEntry right)
            => ConfigurationEntryComparer.Compare(left, right);

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// <para>Returns `<see langword="true"/>` if the specified objects are equal; otherwise, `<see langword="false"/>`.</para>
        /// </summary>
        public bool Equals(ConfigurationEntry left, ConfigurationEntry right)
            => ConfigurationEntryComparer.Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public int GetHashCode(ConfigurationEntry value)
            => ConfigurationEntryComparer.GetHashCode(value);
    }

    namespace Internal
    {
        internal class ConfigurationEntryComparer : ITypeComparer<ConfigurationEntry>
        {
            public StringComparer KeyComparer = ConfigurationEntry.KeyComparer;
            public StringComparer ValueComparer = ConfigurationEntry.ValueComparer;

            public ConfigurationEntryComparer()
            { }

            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// <para>Returns a signed integer that indicates the relative values of `<paramref name="left"/>` and `<paramref name="right"/>`.</para>
            /// <para>Less than zero when `<paramref name="left"/>` is less than `<paramref name="right"/>`.</para>
            /// <para>Zero when `<paramref name="left"/>` equals `<paramref name="right"/>`.</para>
            /// <para>Greater than zero when `<paramref name="left"/>` is greater than `<paramref name="right"/>`.</para>
            /// </summary>
            public int Compare(ConfigurationEntry left, ConfigurationEntry right)
            {
                int result;

                if ((result = KeyComparer.Compare(left.Key, right.Key)) != 0)
                    return result;

                if ((result = ValueComparer.Compare(left.Value, right.Value)) != 0)
                    return result;

                return -(left.Level - right.Level);
            }

            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// <para>Returns a signed integer that indicates the relative values of `<paramref name="left"/>` and `<paramref name="right"/>`.</para>
            /// <para>Less than zero when `<paramref name="left"/>` is less than `<paramref name="right"/>`.</para>
            /// <para>Zero when `<paramref name="left"/>` equals `<paramref name="right"/>`.</para>
            /// <para>Greater than zero when `<paramref name="left"/>` is greater than `<paramref name="right"/>`.</para>
            /// </summary>
            public int Compare(object left, object right)
            {
                if (ReferenceEquals(left, right))
                    return 0;
                if (left is null)
                    return 1;
                if (right is null)
                    return -1;

                if (left is ConfigurationEntry leftEntry && right is ConfigurationEntry rightEntry)
                    return Compare(leftEntry, rightEntry);

                throw new NotSupportedException($"Compare({left.GetType().Name}, {right.GetType().Name}) is not supported.");
            }

            /// <summary>
            /// Determines whether the specified objects are equal.
            /// <para>Returns `<see langword="true"/>` if the specified objects are equal; otherwise, `<see langword="false"/>`.</para>
            /// </summary>
            public bool Equals(ConfigurationEntry left, ConfigurationEntry right)
            {
                // TODO: revisit the logic here and document what we actually want to do with `Level` comparisons
                return left.Level == right.Level
                    && KeyComparer.Equals(left.Key, right.Key)
                    && ValueComparer.Equals(left.Value, right.Value);
            }

            /// <summary>
            /// Determines whether the specified objects are equal.
            /// <para>Returns `<see langword="true"/>` if the specified objects are equal; otherwise, `<see langword="false"/>`.</para>
            /// </summary>
            public new bool Equals(object left, object right)
            {
                if (ReferenceEquals(left, right))
                    return true;
                if (left is null || right is null)
                    return false;

                if (left is ConfigurationEntry leftEntry && right is ConfigurationEntry rightEntry)
                    return Equals(leftEntry, rightEntry);

                throw new NotSupportedException($"Equals({left.GetType().Name}, {right.GetType().Name}) is not supported.");
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(ConfigurationEntry value)
            {
                return KeyComparer.GetHashCode(value.Key);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(object value)
            {
                if (value is null)
                    return 0;

                if (value is ConfigurationEntry entry)
                    return GetHashCode(entry);

                throw new NotSupportedException($"GetHashCode({value.GetType().Name}) is not supported.");
            }
        }
    }
}
