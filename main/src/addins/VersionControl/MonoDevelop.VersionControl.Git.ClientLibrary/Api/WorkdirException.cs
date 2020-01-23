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
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// An error occured due to the state of the working directory.
    /// </summary>
    [Serializable]
    public class WorkingDirectoryException : GitException
    {
        internal WorkingDirectoryException(string message)
            : base(message)
        { }

        internal WorkingDirectoryException(string message, string errorText)
            : base(message, errorText)
        { }

        internal WorkingDirectoryException(string errorText, int exitCode)
            : base(errorText, exitCode)
        { }

        internal WorkingDirectoryException(string message, string errorText, int exitCode)
            : base(message, errorText, exitCode)
        { }

        internal WorkingDirectoryException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal WorkingDirectoryException(string message, string errorText, Exception innerException)
            : base(message, errorText, innerException)
        { }

        internal WorkingDirectoryException(string message, int exitCode, Exception innerException)
            : base(message, exitCode, innerException)
        { }

        internal WorkingDirectoryException(string message, int exitCode, string errorText, Exception innerException)
            : base(message, exitCode, errorText, innerException)
        { }

        internal WorkingDirectoryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// The path to the working directory is invalid.
    /// <para/>
    /// Either the directory does not exists, the directory does not contain a Git repository, or 
    /// the path itself is invalid (either invalid format or illegal characters).
    /// </summary>
    [Serializable]
    public class WorkingDirectoryInvalidPathException : WorkingDirectoryException
    {
        internal WorkingDirectoryInvalidPathException(string message)
            : base(message)
        { }

        internal WorkingDirectoryInvalidPathException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal WorkingDirectoryInvalidPathException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// Raised when there are local changes (either staged or unstaged) in the working directory that 
    /// conflict with an operation; for example attempting to pull commits that update the same files 
    /// are changed locally.
    /// </summary>
    [Serializable]
    public class WorkingDirectoryLocalChangesException : WorkingDirectoryException
    {
        internal const string ErrorPrefix = "error: Your local changes to the following files would be overwritten";

        internal WorkingDirectoryLocalChangesException(string message)
            : base(message)
        {
            _files = ParseMessage(message);
        }

        internal WorkingDirectoryLocalChangesException(string message, Exception exception)
            : base(message, exception)
        {
            _files = ParseMessage(message);
        }

        internal WorkingDirectoryLocalChangesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            string data = info.GetString(nameof(Files));

            // Deserialize the data into a list; avoid null values by using conditional allocator
            _files = DeserializeFileList(data) ?? new List<string>();
        }

        private readonly List<string> _files;

        public IReadOnlyList<string> Files
        {
            get { return _files; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            string data = SerializeFileList(_files);

            if (data?.Length > 0)
            {
                info.AddValue(nameof(Files), data);
            }

            base.GetObjectData(info, context);
        }

        internal static List<string> DeserializeFileList(string data)
        {
            if (data is null)
                return null;

            string[] files = data.Split((char)Cli.GitCommand.Eol);

            return new List<string>(files);
        }

        internal static List<string> ParseMessage(string message)
        {
            // Parse the file list from the Git error message. The format is simple - the first line is the error text,
            // and subsequent lines that start with a tab are the file names. There may be no such content, in which
            // case the file list will be empty.
            var files = new List<string>();

            foreach (string line in message.Split((char)Cli.GitCommand.Eol).Skip(1))
            {
                if (!line.StartsWith("\t", StringComparison.Ordinal))
                {
                    // Assume the file list terminates on the first non-tabbed line
                    break;
                }

                files.Add(line.Trim());
            }

            return files;
        }

        internal static string SerializeFileList(List<string> files)
        {
            if (files is null)
                return null;

            using (StringBuffer buffer = new StringBuffer())
            {
                foreach (var file in files)
                {
                    buffer.Append(file)
                          .Append((char)Cli.GitCommand.Eol);
                }

                return buffer.ToString();
            }
        }
    }

    /// <summary>
    /// Uncommitted changes in the working directory have caused an error to occur.
    /// </summary>
    [Serializable]
    public class WorkingDirectoryUncommittedException : WorkingDirectoryLocalChangesException
    {
        internal const string ExceptionMessage = "The index contains uncommitted changes.";

        internal WorkingDirectoryUncommittedException()
            : base(ExceptionMessage)
        { }

        internal WorkingDirectoryUncommittedException(string message)
            : base(message)
        { }

        internal WorkingDirectoryUncommittedException(Exception innerException)
            : base(ExceptionMessage, innerException)
        { }

        internal WorkingDirectoryUncommittedException(string message, Exception exception)
            : base(message, exception)
        { }

        internal WorkingDirectoryUncommittedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    /// <summary>
    /// Unmerged changes, into the HEAD or upstream branch, have caused an error to occur.
    /// </summary>
    [Serializable]
    public class WorkingDirectoryUnmergedException : WorkingDirectoryException
    {
        internal WorkingDirectoryUnmergedException(string message)
            : base(message)
        {
            _upstreamBranchName = Head.CanonicalLabel;
        }

        internal WorkingDirectoryUnmergedException(string message, string upstreamBranchName)
        : this(message)
        {
            _upstreamBranchName = upstreamBranchName ?? Head.CanonicalLabel;
        }

        internal WorkingDirectoryUnmergedException(string message, Exception innerException)
            : base(message, innerException)
        {
            _upstreamBranchName = Head.CanonicalLabel;
        }

        internal WorkingDirectoryUnmergedException(string message, string upstreamBranchName, Exception innerException)
            : base(message, innerException)
        {
            _upstreamBranchName = upstreamBranchName ?? Head.CanonicalLabel;
        }

        internal WorkingDirectoryUnmergedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _upstreamBranchName = info.GetString(nameof(UpstreamBranchName)) ?? Head.CanonicalLabel;
        }

        private readonly string _upstreamBranchName;

        /// <summary>
        /// Gets the name of the upstream branch, if any, that this branch is not merged into.
        /// </summary>
        public string UpstreamBranchName { get { return _upstreamBranchName; } }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(UpstreamBranchName), _upstreamBranchName);
        }
    }

    /// <summary>
    /// Unstaged changes in the working directory have cause an error to occur.
    /// </summary>
    [Serializable]
    public class WorkingDirectoryUnstagedException : WorkingDirectoryLocalChangesException
    {
        internal const string ExceptionMessage = "The workspace contains unstaged changes. Please commit, stash, or revert them.";
        internal const string UntrackedPrefix = "error: The following untracked working tree files would be overwritten";

        internal WorkingDirectoryUnstagedException()
            : base(ExceptionMessage)
        { }

        internal WorkingDirectoryUnstagedException(string message)
            : base(message)
        { }

        internal WorkingDirectoryUnstagedException(Exception innerException)
            : base(ExceptionMessage, innerException)
        { }

        internal WorkingDirectoryUnstagedException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal WorkingDirectoryUnstagedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
