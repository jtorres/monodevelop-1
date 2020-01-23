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

using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    partial class TypeComparer : ITypeComparer<Environment.Variable>
    {
        public readonly ITypeComparer<Environment.Variable> EnvironmentVariableComparer = new EnvironmentVariableComparer();

        /// <summary>
        /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
        /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
        /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
        /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
        /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
        /// </summary>
        public int Compare(Environment.Variable left, Environment.Variable right)
            => EnvironmentVariableComparer.Compare(left, right);

        /// <summary>
        /// <para>Determines whether the specified objects are equal.</para>
        /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
        /// </summary>
        public bool Equals(Environment.Variable left, Environment.Variable right)
            => EnvironmentVariableComparer.Equals(left, right);

        /// <summary>
        /// Returns a hash code for the specified value.
        /// </summary>
        public int GetHashCode(Environment.Variable value)
            => EnvironmentVariableComparer.GetHashCode(value);
    }

    namespace Internal
    {
        internal class EnvironmentVariableComparer : ITypeComparer<Environment.Variable>
        {
            public EnvironmentVariableComparer()
            { }

            /// <summary>
            /// <para>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</para>
            /// <para>Returns a signed integer that indicates the relative values of <paramref name="left"/> and <paramref name="right"/>.</para>
            /// <para>Less than zero when <paramref name="left"/> is less than <paramref name="right"/>.</para>
            /// <para>Zero when <paramref name="left"/> equals <paramref name="right"/>.</para>
            /// <para>Greater than zero when <paramref name="left"/> is greater than <paramref name="right"/>.</para>
            /// </summary>
            public int Compare(Environment.Variable left, Environment.Variable right)
            {
                return Environment.Variable.NameStringComparer.Compare(left.Name, right.Name);
            }

            /// <summary>
            /// <para>Determines whether the specified objects are equal.</para>
            /// <para>Returns <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.</para>
            /// </summary>
            public bool Equals(Environment.Variable left, Environment.Variable right)
            {
                return Environment.Variable.NameStringComparer.Equals(left.Name, right.Name)
                    && Environment.Variable.ValueStringComparer.Equals(left.Value, right.Value);
            }

            /// <summary>
            /// Returns a hash code for the specified value.
            /// </summary>
            public int GetHashCode(Environment.Variable value)
            {
                return Environment.Variable.NameStringComparer.GetHashCode(value.Name);
            }
        }
    }
}
