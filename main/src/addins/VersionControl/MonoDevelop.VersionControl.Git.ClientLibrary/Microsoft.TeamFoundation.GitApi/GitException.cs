﻿/**** Git Process Management Library ****
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
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using static System.FormattableString;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Exception related to a Git process failure.
    /// </summary>
    [Serializable]
    public class GitException : ExceptionBase
    {
        public const int GitErrorExitCode = Cli.GitCommand.GitErrorExitCode;

        internal GitException(string message)
            : base(message)
        { }

        internal GitException(string message, string errorText)
            : base(message)
        {
            _errorText = errorText;
        }

        internal GitException(string errorText, ExecuteResult exitCode)
            : base(errorText)
        {
            _executeResult = exitCode;
        }

        internal GitException(ExecuteResult exitCode)
            : base(exitCode.ErrorText)
        {
            _executeResult = exitCode;
        }

        internal GitException(string message, string errorText, ExecuteResult exitCode)
            : base(message)
        {
            _errorText = errorText;
            _executeResult = exitCode;
        }

        internal GitException(string message, Exception innerException)
            : base(message, innerException)
        { }

        internal GitException(string message, string errorText, Exception innerException)
            : base(message, innerException)
        {
            _errorText = errorText;
        }

        internal GitException(string message, ExecuteResult exitCode, Exception innerException)
            : base(message, innerException)
        {
            _executeResult = exitCode;
        }

        internal GitException(string message, ExecuteResult exitCode, string errorText, Exception innerException)
             : base(message, innerException)
        {
            _errorText = errorText;
            _executeResult = exitCode;
        }

        internal GitException(SerializationInfo info, StreamingContext context)
              : base(info, context)
        {
            _errorText = info.GetString(nameof(ErrorText));
            _executeResult = (ExecuteResult?)info.GetValue(nameof(ExecuteResult), typeof(int?));
        }

        private string _errorText;
        public ExecuteResult? _executeResult;

        public string ErrorText
        {
            get { return _errorText; }
        }

        public int? ExitCode
        {
            get { return _executeResult.HasValue ? _executeResult.Value.ExitCode : default; }
        }

        public override string Message
        {
            get
            {
                if (_executeResult == null)
                {
                    return _errorText != null ? Invariant($"{base.Message}\r\n{_errorText}") : base.Message;
                }
                return _errorText != null ? Invariant($"{base.Message}\r\n{_errorText}\r\n{_executeResult}") :
                    Invariant($"{base.Message}\r\n{_executeResult}");
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ErrorText), _errorText);
            info.AddValue(nameof(ExitCode), _executeResult);

            base.GetObjectData(info, context);
        }

    }

    /// <summary>
    /// Git process failed with an unhandled fatal error.
    /// </summary>
    [Serializable]
    public class GitFatalException : GitException
    {
        public static readonly ExecuteResult DefaultExitCode = new ExecuteResult(128, ExceptionMessage);
        internal const string ExceptionMessage = "Git failed with a fatal error.";

        internal GitFatalException()
            : base(ExceptionMessage, DefaultExitCode)
        { }

        internal GitFatalException(string errorText)
            : base(ExceptionMessage, errorText, DefaultExitCode)
        { }

        internal GitFatalException(ExecuteResult executeResult)
            : base(ExceptionMessage, executeResult)
        { }

        internal GitFatalException(Exception innerException)
            : base(ExceptionMessage, DefaultExitCode, innerException)
        { }

        internal GitFatalException(ExecuteResult exitCode, Exception innerException)
            : base(ExceptionMessage, exitCode, innerException)
        { }

        internal GitFatalException(string errorText, Exception innerException)
            : base(ExceptionMessage, DefaultExitCode, errorText, innerException)
        { }

        internal GitFatalException(ExecuteResult exitCode, string errorText, Exception innerException)
            : base(ExceptionMessage, exitCode, innerException)
        { }

        internal GitFatalException(SerializationInfo info, StreamingContext context)
                : base(info, context)
        { }
    }

    /// <summary>
    /// Git process failed with a usage error.
    /// </summary>
    [Serializable]
    public class GitUsageException : GitException
    {
        public static readonly ExecuteResult DefaultExitCode = new ExecuteResult(128, ExceptionMessage);
        internal const string ExceptionMessage = "Git failed with a usage error.";

        internal GitUsageException()
            : base(ExceptionMessage, DefaultExitCode)
        { }

        internal GitUsageException(ExecuteResult exitCode)
            : base(ExceptionMessage, exitCode)
        { }

        internal GitUsageException(Exception innerException)
             : base(ExceptionMessage, DefaultExitCode, innerException)
        { }

        internal GitUsageException(ExecuteResult exitCode, Exception innerException)
            : base(ExceptionMessage, exitCode, innerException)
        { }

        internal GitUsageException(string errorText, Exception innerException)
            : base(ExceptionMessage, DefaultExitCode, errorText, innerException)
        { }

        internal GitUsageException(ExecuteResult exitCode, string errorText, Exception innerException)
            : base(ExceptionMessage, exitCode, innerException)
        { }

        internal GitUsageException(SerializationInfo info, StreamingContext context)
             : base(info, context)
        { }
    }

    /// <summary>
    /// Git process failed due to git-hook configuration problems.
    /// </summary>
    [Serializable]
    public class GitHookConfigurationException : GitException
    {
        public const string ExceptionMessage = "Your git-hook is unsupported, likely because the first line is not \"#!/bin/sh\".";

        internal GitHookConfigurationException()
            : base(ExceptionMessage, new ExecuteResult(GitErrorExitCode, ExceptionMessage))
        { }

        internal GitHookConfigurationException(ExecuteResult executeResult)
            : base(executeResult)
        { }

        internal GitHookConfigurationException(string message)
            : base(Invariant($"{ExceptionMessage}\r\n{message}"), message, new ExecuteResult(GitErrorExitCode, ExceptionMessage))
        {
            string hookPath;
            if (ExtractHookPath(message, out hookPath))
            {
                _hookPath = hookPath;
            }
        }

        internal GitHookConfigurationException(string message, Exception innerException)
            : base(Invariant($"{ExceptionMessage}\r\n{message}"), new ExecuteResult(GitErrorExitCode, ExceptionMessage), message, innerException)
        {
            string hookPath;
            if (ExtractHookPath(message, out hookPath))
            {
                _hookPath = hookPath;
            }
        }

        internal GitHookConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _hookPath = info.GetString(nameof(HookPath));
        }

        private string _hookPath;

        public string HookPath
        {
            get { return _hookPath; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(HookPath), _hookPath);

            base.GetObjectData(info, context);
        }

        internal static bool ExtractHookPath(string message, out string path)
        {
            // The path match could start with a drive indicator [hence ([A-Za-z]:)? prefix}, but doesn't have to.
            const string ErrorMessagePattern = @"^\s*error:\s+cannot spawn\s+(([A-Za-z]:)?[^:]+):\s+No\s+such\s+file\s+or\s+directory";

            path = null;

            if (!string.IsNullOrWhiteSpace(message))
            {
                Regex regex = new Regex(ErrorMessagePattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

                Match match;
                if ((match = regex.Match(message)).Success)
                {
                    path = match.Groups[1].Value;
                }
            }

            return path != null;
        }
    }

    /// <summary>
    /// Git process failed due to git-hook requiring interactivity.
    /// </summary>
    [Serializable]
    public class GitHookInteractivityException : GitException
    {
        public const string ExceptionMessage = "Your git-hook is unsupported, likely because it needs \"/dev/tty\".";

        internal GitHookInteractivityException()
            : base(ExceptionMessage, new ExecuteResult(GitErrorExitCode, ExceptionMessage))
        { }

        internal GitHookInteractivityException(ExecuteResult executeResult)
            : base(executeResult)
        { }

        internal GitHookInteractivityException(string message)
            : base(Invariant($"{ExceptionMessage}\r\n{message}"), message, new ExecuteResult(GitErrorExitCode, ExceptionMessage))
        {
            string hookPath;
            int lineNumber;

            if (ExtractExceptionDetails(message, out hookPath, out lineNumber))
            {
                _hookPath = hookPath;
                _lineNumber = lineNumber;
            }
        }

        internal GitHookInteractivityException(string message, Exception innerException)
            : base(Invariant($"{ExceptionMessage}\r\n{message}"), new ExecuteResult(GitErrorExitCode, ExceptionMessage), message, innerException)
        {
            string hookPath;
            int lineNumber;

            if (ExtractExceptionDetails(message, out hookPath, out lineNumber))
            {
                _hookPath = hookPath;
                _lineNumber = lineNumber;
            }
        }

        internal GitHookInteractivityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _hookPath = info.GetString(nameof(HookPath));
            _lineNumber = info.GetInt32(nameof(LineNumber));
        }

        private string _hookPath;
        private int _lineNumber;

        public string HookPath
        {
            get { return _hookPath; }
        }

        public int LineNumber
        {
            get { return _lineNumber; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(HookPath), _hookPath);
            info.AddValue(nameof(LineNumber), _lineNumber);

            base.GetObjectData(info, context);
        }

        internal static bool ExtractExceptionDetails(string message, out string hookPath, out int lineNumber)
        {
            const string ErrorMessagePattern = @"^\s*([^:]+):\s+line\s+(\d+):\s+\/dev\/tty:\s+No\s+such\s+device\s+or\s+address";

            hookPath = null;
            lineNumber = 0;

            if (!string.IsNullOrWhiteSpace(message))
            {
                Regex regex = new Regex(ErrorMessagePattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

                Match match;
                if ((match = regex.Match(message)).Success)
                {
                    hookPath = match.Groups[1].Value;
                    lineNumber = Convert.ToInt32(match.Groups[2].Value);
                }
            }

            return hookPath != null;
        }
    }
}
