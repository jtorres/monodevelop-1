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
using System.IO;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    public interface IFileSystem
    {
        /// <summary>
        /// Returns the canonical form of a path with all intermediate components normalized.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain absolute path information.</param>
        string CanonicalizePath(string path);

        /// <summary>
        /// Creates all directories and subdirectories in the specified path unless they already exist.
        /// <para>Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.</para>
        /// </summary>
        /// <param name="path">The directory to create.</param>
        bool CreateDirectory(string path);

        /// <summary>
        /// Deletes the specified directory and, if indicated, any subdirectories and files in the directory.
        /// <para>Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.</para>
        /// </summary>
        /// <param name="path">The name of the directory to remove.</param>
        /// <param name="recursive">`<see langword="true"/>` to remove even if not empty; otherwise `<see langword="false"/>`.</param>
        bool DeleteDirectory(string path, bool recursive);

        /// <summary>
        /// Deletes the specified directory, but only if it is empty.
        /// <para>Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.</para>
        /// </summary>
        /// <param name="path">The name of the directory to remove.</param>
        bool DeleteDirectory(string path);

        /// <summary>
        /// Deletes the specified file.
        /// <para>Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.</para>
        /// </summary>
        /// <param name="path">
        /// The name of the file to be deleted.
        /// <para>Wild-card characters are not supported.</para>
        /// </param>
        bool DeleteFile(string path);

        /// <summary>
        /// Returns true if the directory exists.
        /// </summary>
        /// <param name="path">The path to be checked for existence.</param>
        bool DirectoryExists(string path);

        /// <summary>
        /// Returns true if the file exists.
        /// </summary>
        /// <param name="path">The path to be checked for existence.</param>
        bool FileExists(string path);

        /// <summary>
        /// Returns the parent directory of the specified path.
        /// </summary>
        /// <param name="path">The path for which to retrieve the parent directory.</param>
        string GetParentPath(string path);

        /// <summary>
        /// Returns the path of the current user's temporary folder.
        /// </summary>
        string GetTempDirectoryPath();

        /// <summary>
        /// Opens or creates, and returns a `<see cref="FileStream"/>` to a file specified by `<paramref name="path"/>` using the specified mode, access and sharing options.
        /// </summary>
        /// <param name="path">The path to the file to open.</param>
        /// <param name="mode">
        /// Specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.
        /// </param>
        /// <param name="access">Specifies the operations that can be performed on the file.</param>
        /// <param name="share">Specifies the type of access other threads have to the file.</param>
        Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share);
    }

    internal class FileSystem : IFileSystem
    {
        public FileSystem(ExecutionContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            _context = context;
        }

        private readonly ExecutionContext _context;

        public string CanonicalizePath(string path)
        {
            try
            {
                return Path.GetFullPath(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                _context.Tracer.TraceException(e, TracerLevel.Diagnostic);
            }

            return null;
        }

        public bool CreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);

                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                _context.Tracer.TraceException(e, TracerLevel.Diagnostic);
            }

            return false;
        }

        public bool DeleteDirectory(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);

                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                _context.Tracer.TraceException(e, TracerLevel.Diagnostic);
            }

            return false;
        }

        public bool DeleteDirectory(string path)
            => DeleteDirectory(path, false);

        public bool DeleteFile(string path)
        {
            try
            {
                File.Delete(path);

                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                _context.Tracer.TraceException(e, TracerLevel.Diagnostic);
            }

            return false;
        }

        public bool DirectoryExists(string path)
        {
            try
            {
                return Directory.Exists(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                _context.Tracer.TraceException(e, TracerLevel.Diagnostic);
            }

            return false;
        }

        public bool FileExists(string path)
        {
            try
            {
                return File.Exists(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                _context.Tracer.TraceException(e, TracerLevel.Diagnostic);
            }

            return false;
        }

        public string GetParentPath(string path)
        {
            try
            {
                return Path.GetDirectoryName(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                _context.Tracer.TraceException(e, TracerLevel.Diagnostic);
            }

            return null;
        }

        public string GetTempDirectoryPath()
        {
            try
            {
                return Path.GetTempPath();
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                _context.Tracer.TraceException(e, TracerLevel.Diagnostic);
            }

            return null;
        }

        public Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
        {
            try
            {
                return File.Open(path, mode, access, share);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                _context.Tracer.TraceException(e, TracerLevel.Diagnostic);
            }

            return Stream.Null;
        }
    }

    /// <summary>
    /// Provides attributes for files and directories.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay, nq}")]
    public struct FileSystemAttributes
    {
        private static readonly PathComparer PathComparer = new PathComparer();

        internal FileSystemAttributes(FileAttributes? attributes, string path)
        {
            _attributes = attributes;
            _path = path;
        }

        private FileAttributes? _attributes;
        private string _path;

        /// <summary>
        /// Gets <see langword="true"/> if the entry, either a directory or file, exists; otherwise <see langword="false"/>.
        /// </summary>
        public bool Exists
        {
            get { return _attributes.HasValue; }
        }

        /// <summary>
        /// Gets <see langword="true"/> if the entry is a directory; otherwise <see langword="false"/>.
        /// </summary>
        public bool IsDirectory
        {
            get { return Exists && (_attributes & FileAttributes.Directory) == FileAttributes.Directory; }
        }

        /// <summary>
        /// Gets <see langword="true"/> if the entry is a file; otherwise <see langword="false"/>.
        /// </summary>
        public bool IsFile
        {
            get { return Exists && (_attributes & FileAttributes.Directory) == 0; }
        }

        /// <summary>
        /// Gets the path related to this `<see cref="FileSystemAttributes"/> `.
        /// </summary>
        public string Path
        {
            get { return _path; }
            internal set { _path = value; }
        }

        internal FileAttributes? Attributes
        {
            get { return _attributes; }
        }

        private string DebuggerDisplay
        {
            get { return $"{(_attributes.HasValue ? _attributes.Value.ToString() : "NotFound")} -> \"{Path}\""; }
        }

        public bool Equals(FileSystemAttributes other)
        {
            return _attributes == other._attributes
                && PathComparer.Equals(Path, other.Path);
        }

        public override bool Equals(object obj)
        {
            return (obj is FileSystemAttributes a && Equals(a))
                || base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return PathComparer.GetHashCode(Path)
                 | (_attributes.HasValue ? (int)_attributes : 0);
        }

        public static bool operator ==(FileSystemAttributes left, FileSystemAttributes right)
            => left.Equals(right);

        public static bool operator !=(FileSystemAttributes left, FileSystemAttributes right)
            => !left.Equals(right);
    }
}
