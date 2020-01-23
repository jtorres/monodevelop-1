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
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.TeamFoundation.GitApi.Internal;
using static System.FormattableString;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git configuration entry.
    /// <para/>
    /// Git reads configuration data from several places, with a defined precedence for determining which values are applied.
    /// <para/>
    /// `<see cref="Level"/>` determines precedence, while `<see cref="Source"/>` explains origin.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public struct ConfigurationEntry : IComparable<ConfigurationEntry>, IEquatable<ConfigurationEntry>
    {
        /// <summary>
        /// Character used to escape `<seealso cref="IllegalCharacters"/>`.
        /// </summary>
        public const char EscapeCharacter = '\\';
        /// <summary>
        /// Characters which are illegal in environment variables, unless escaped (preceded) by `<seealso cref="EscapeCharacter"/>`.
        /// </summary>
        public static readonly IReadOnlyList<char> IllegalCharacters = new char[] { '"' };

        internal static readonly StringComparer KeyComparer = StringComparer.OrdinalIgnoreCase;
        internal static readonly StringComparer ValueComparer = StringComparer.OrdinalIgnoreCase;

        internal static readonly ConfigurationEntryComparer Comparer = new ConfigurationEntryComparer();

        internal ConfigurationEntry(string key, string value)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (!ValidateKey(key))
                throw new ArgumentException(nameof(key));

            _key = key;
            _value = value;
            _level = ConfigurationLevel.None;
            _source = null;
        }

        internal ConfigurationEntry(string key, string value, ConfigurationLevel level)
        {
            Debug.Assert(!(key is null), $"The `{nameof(key)}` parameter is null.");
            Debug.Assert(ValidateKey(key), $"The `{nameof(key)}` parameter is invalid.");
            Debug.Assert(!(value is null), $"The `{nameof(value)}` parameter is null.");
            Debug.Assert((level & ~ConfigurationLevel.Any) == 0, $"The `{nameof(level)}` parameter is undefined.");

            _key = key;
            _value = value;
            _level = level;
            _source = (level == ConfigurationLevel.Command)
                ? "command line"
                : null;
        }

        internal ConfigurationEntry(string key, string value, ConfigurationLevel level, string source)
        {
            Debug.Assert(!ReferenceEquals(key, null), $"The `{nameof(key)}` parameter is null.");
            Debug.Assert(ValidateKey(key), $"The `{nameof(key)}` parameter is invalid.");
            Debug.Assert(!ReferenceEquals(value, null), $"The `{nameof(value)}` parameter is null.");
            Debug.Assert((level & ~ConfigurationLevel.Any) == 0, $"The `{nameof(level)}` parameter is undefined.");
            Debug.Assert(!ReferenceEquals(source, null), $"The `{nameof(source)}` parameter is null");

            _key = key;
            _value = value;
            _level = level;
            _source = source;
        }

        private string _key;
        private ConfigurationLevel _level;
        private string _source;
        private string _value;

        /// <summary>
        /// The unique name of the specific configuration
        /// <para/>
        /// The key is actually the section and the name separated by a dot.
        /// </summary>
        public string Key
        {
            get { return _key; }
        }

        /// <summary>
        /// The level of the configuration, given by the canonical configuration file which introduced it.
        /// <para/>
        /// Levels in order of precidence are `<see cref="ConfigurationLevel.Local"/>`, `<see cref="ConfigurationLevel.Global"/>`, `<see cref="ConfigurationLevel.Xdg"/>`, `<see cref="ConfigurationLevel.System"/>`, and `<see cref="ConfigurationLevel.Portable"/>`.
        /// </summary>
        public ConfigurationLevel Level
        {
            get { return _level; }
        }

        /// <summary>
        /// The path to the file which contained the configuration entries.
        /// </summary>
        public string Source
        {
            get { return _source; }
        }

        /// <summary>
        /// The value of the configuration entry.
        /// </summary>
        public string Value
        {
            get { return _value; }
        }

        private string DebuggerDisplay
        {
            get { return Invariant($"{nameof(ConfigurationEntry)}: [{_level}] \"{_key}\": \"{_value}\" (\"{_source}\")"); }
        }

        public int CompareTo(ConfigurationEntry other)
            => Comparer.Compare(this, other);

        public bool Equals(ConfigurationEntry other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            return obj is ConfigurationEntry entry
                && Equals(entry);
        }

        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        public override string ToString()
        {
            return $"{Key} = {Value} [{Level}]";
        }

        internal static bool ValidateKey(string key)
        {
            if (key is null || key.Length == 0)
                return false;

            for (int i = 0; i < IllegalCharacters.Count; i += 1)
            {
                if (key.IndexOf(IllegalCharacters[i]) >= 0)
                    return false;
            }

            return true;
        }

        public static bool operator ==(ConfigurationEntry left, ConfigurationEntry right)
            => Comparer.Equals(left, right);

        public static bool operator !=(ConfigurationEntry left, ConfigurationEntry right)
            => !Comparer.Equals(left, right);
    }
}
