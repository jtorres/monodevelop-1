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
using System.Globalization;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    public interface ICheckoutConflict : IEquatable<ICheckoutConflict>
    {
        /// <summary>
        /// Gets the path of the conflict.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the type of the conflict
        /// </summary>
        CheckoutConfictType Type { get; }
    }

    public enum CheckoutConfictType
    {
        Unknown = 0,

        /// <summary>
        /// Cannot update sparse checkout: the following entries are not up-to-date:\n%s
        /// </summary>
        SparseFileOutOfDate,
        /// <summary>
        /// The following Working tree files would be overwritten by sparse checkout update:\n%s
        /// </summary>
        SparseFilesOverwrite,
        /// <summary>
        /// The following Working tree files would be removed by sparse checkout update:\n%s
        /// </summary>
        SparseFileRemove,
        /// <summary>
        /// Updating the following directories would lose untracked files in it:\n%s
        /// </summary>
        UntrackedFilesDirectoryUpdate,
        /// <summary>
        /// Your local changes to the following files would be overwritten by %s:\n%%s
        /// </summary>
        TrackedFileOverwrite,
        /// <summary>
        /// The following untracked working tree files would be removed by %s:\n%%s
        /// </summary>
        UntrackedFileRemove,
        /// <summary>
        /// The following untracked working tree files would be overwritten by %s:\n%%s
        /// </summary>
        UntrackedFileOverwrite,
    }

    internal class CheckoutConflict : ICheckoutConflict, IEquatable<CheckoutConflict>
    {
        internal static readonly CheckoutConflictComparer Comparer = new CheckoutConflictComparer();

        public CheckoutConflict(string path, CheckoutConfictType type)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));

            _path = (StringUtf8)path;
            _type = type;
        }

        private StringUtf8 _path;
        private CheckoutConfictType _type;

        public string Path
        {
            get { return (string)_path; }
        }

        public CheckoutConfictType Type
        {
            get { return _type; }
        }

        internal StringUtf8 PathUtf8
        {
            get { return _path; }
        }

        public bool Equals(CheckoutConflict other)
            => Comparer.Equals(this, other);

        public bool Equals(ICheckoutConflict other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            return (obj is CheckoutConflict a
                    && Equals(a))
                || (obj is ICheckoutConflict b
                    && Equals(b))
                || base.Equals(obj);
        }

        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        public override string ToString()
        {
            return Path;
        }

        internal static CheckoutConflict FromSerialized(string format)
        {
            if (format is null)
                throw new ArgumentNullException(nameof(format));

            var parts = format.Split(':');
            if (parts is null || parts.Length != 2)
                throw new ArgumentException("Invalid data", nameof(format), new System.IO.InvalidDataException(format));

            CheckoutConfictType type;
            string path;

            if (int.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out int result))
            {
                type = (CheckoutConfictType)result;
                path = parts[1];

                return new CheckoutConflict(path, type);
            }

            throw new ArgumentException("Invalid data", nameof(format), new System.IO.InvalidDataException(format));
        }

        internal string ToSerialized()
        {
            return FormattableString.Invariant($"{(int)Type}:{Path}");
        }
    }
}
