//*************************************************************************************************
// PathHelper.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal class PathHelper
    {
        public const char LocalPathSeparatorCharacter = '\\';
        public const char PosixPathSeparatorCharacter = '/';

        public const char EscapeCharacter = '\\';
        public const char QuoteCharacter = '"';

        internal static readonly StringComparer Comparer = new PathComparer();
        internal static readonly IReadOnlyList<string> IllegalFileNames = new string[]
        {
            "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        internal PathHelper(ExecutionContext context)
        {
            _context = context;
        }

        private readonly ExecutionContext _context;

        /// <summary>
        /// Escapes a string using path escaping rules.
        /// <para>Wraps a string in quotes if the string is not already wrapped.</para>
        /// <para>Escapes existing quotes with `<see cref="PathHelper.EscapeCharacter"/>`.</para>
        /// </summary>
        public string EscapePath(string path)
        {
            if (ReferenceEquals(path, null))
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                return path;

            bool needsEscape = false;
            bool needsQuotes = false;

            // If the path is already escaped (bookended by unescaped quotes)
            // just return it as is.
            if (path[0] == QuoteCharacter
                && path.Length > 2
                && path[path.Length - 1] == QuoteCharacter
                && path[path.Length - 2] != EscapeCharacter)
                return path;

            // Scan the string looking for whitespace and/or quote characters
            for (int i = 0; i < path.Length; i += 1)
            {
                // Check for quote character until we've found one.
                if (!needsEscape && path[i] == QuoteCharacter
                    && (i == 0 || path[i - 1] != EscapeCharacter))
                {
                    needsEscape = true;

                    // If we already know that we'll need to quote the buffer,
                    // then we can stop the scan now and go right to the work;
                    // otherwise we need to keep scanning.
                    if (needsQuotes)
                        break;
                    else
                        continue;
                }
                else if (!needsQuotes && char.IsWhiteSpace(path[i]))
                {
                    needsQuotes = true;

                    // If we already know that we'll need to escape the buffer,
                    // then we can stop the scan now and go right to the work;
                    // otherwise we need to keep scanning.
                    if (needsEscape)
                        break;
                    else
                        continue;
                }
            }

            // As a result of the scan, if we need to escape and/or enquote the
            // string do so by copying the string into an editable buffer.
            if (needsEscape || needsQuotes)
            {
                using (var buffer = new StringBuffer(path))
                {
                    // Escape as necessary first.
                    if (needsEscape)
                    {
                        EscapeBuffer(buffer);
                    }

                    // Enquote as necessary.
                    if (needsQuotes)
                    {
                        EnquoteBuffer(buffer);
                    }

                    path = buffer.ToString();
                }
            }

            return path;
        }

        /// <summary>
        /// Normalizes path separators using the POSIX safe '/' character, restores any drive letter prefix, and removes any trailing separator.
        /// <para>Escapes the path as necessary.</para>
        /// </summary>
        public string ToLocalPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));



            using (var buffer = new StringBuffer(path))
            {
                // Normalize and trim the buffer.
                NormalizeBuffer(buffer);

                // Check for a POSIX format drive letter conversion
                // and replace it with Windows style (/C/ -> C:/)
                if (buffer.Length >= 3
                    && buffer[0] == PosixPathSeparatorCharacter
                    && buffer[2] == PosixPathSeparatorCharacter
                    && char.IsLetter(buffer[1]))
                {
                    buffer[0] = buffer[1];
                    buffer[1] = ':';
                }

                TrimBuffer(buffer);

                return buffer.ToString();
            }
        }

        /// <summary>
        /// Normalizes path separators using the POSIX safe '/' character and removes any driver letter prefix.
        /// <para>Escapes the path as necessary.</para>
        /// </summary>
        public string ToPosixPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using (var buffer = new StringBuffer(path))
            {
                // Normalize and trim the buffer.
                NormalizeBuffer(buffer);
                TrimBuffer(buffer);

                // Check for Windows style drive letter prefix and replace it
                // with a POSIX compliant version (example: C: -> /C)
                if (buffer.Length >= 3 && buffer[1] == ':'
                    && buffer[2] == PosixPathSeparatorCharacter
                    && char.IsLetter(buffer[0]))
                {
                    buffer[1] = buffer[0];
                    buffer[0] = PosixPathSeparatorCharacter;
                }

                return buffer.ToString();
            }
        }

        public bool IsValidRepositoryPath(StringUtf8 path)
        {
            if (path == null || path.Length == 0)
                return false;

            // POSIX, and therefore Git, allow just about anything in paths
            for (int i = 0; i < path.Length; i += 1)
            {
                if (path[i] == 0)
                    return false;
            }

            return true;
        }

        public StringUtf8 ParsePathValue(ByteBuffer buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || checked(index + count) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            int start = index;
            int end = index + count;

            // A C-style quoted path is wrapped in DQUOTES.
            // So if the first byte is not a DQUOTE, we ASSUME an unquoted string.
            if (buffer[start] != (byte)'"')
                return new StringUtf8(buffer, start, (end - start));

            if (buffer[end - 1] != (byte)'"')
                throw new PathParseException("entry-name", new StringUtf8(buffer, index, count), 0);

            // We assume that the unquoted version of the buffer will be shorter
            // as we unescape special characters.
            byte[] unquoted = new byte[buffer.Length];
            int len = 0;

            start += 1; // skip leading DQUOTE
            end -= 1;   // skip trailing DQUOTE
            while (start < end)
            {
                byte b = buffer[start++];
                if (b == (byte)'"')
                {
                    // We should not see DQUOTEs here because we have
                    // already trimmed the end and inline DQUOTEs should
                    // be escaped with a leading backslash.
                    throw new PathParseException("unexpected d-quote", new StringUtf8(buffer, index, count), 0);
                }
                else if (b != (byte)'\\')
                {
                    // Normal characters get copied as is.
                    unquoted[len++] = b;
                    continue;
                }
                else
                {
                    // Start of C-style escape sequence.
                    // eat the backslash and look at what it was guarding.
                    byte c0 = buffer[start++];
                    switch (c0)
                    {
                        default:
                        case (byte)'\\':
                        case (byte)'"':
                            unquoted[len++] = c0;
                            break;

                        case (byte)'a':
                            unquoted[len++] = (byte)'\a';
                            break;

                        case (byte)'b':
                            unquoted[len++] = (byte)'\b';
                            break;

                        case (byte)'f':
                            unquoted[len++] = (byte)'\f';
                            break;

                        case (byte)'n':
                            unquoted[len++] = (byte)'\n';
                            break;

                        case (byte)'r':
                            unquoted[len++] = (byte)'\r';
                            break;

                        case (byte)'t':
                            unquoted[len++] = (byte)'\t';
                            break;

                        case (byte)'v':
                            unquoted[len++] = (byte)'\v';
                            break;

                        case (byte)'0':
                        case (byte)'1':
                        case (byte)'2':
                        case (byte)'3':
                            {
                                byte c1 = buffer[start++];
                                byte c2 = buffer[start++];
                                byte value;

                                if (!ParseOctalTriple(c0, c1, c2, out value))
                                    throw new PathParseException("invalid-octal-sequence", new StringUtf8(buffer, index, count), 0);

                                unquoted[len++] = value;
                            }
                            break;
                    }
                }
            }

            return new StringUtf8(unquoted, 0, len);
        }

        /// <summary>
        /// Wraps a string in quotes if it not already wrapped in quotes.
        /// </summary>
        private static void EnquoteBuffer(StringBuffer buffer)
        {
            // Prepend a quote character if the first character isn't already a quote
            if (buffer.Length > 0 && buffer[0] != QuoteCharacter)
            {
                buffer.Prepend(QuoteCharacter);
            }

            // Append a quote character if the last character isn't already quote.
            if (buffer.Length >= 2
                && (buffer[buffer.Length - 1] != QuoteCharacter
                    || buffer[buffer.Length - 2] == EscapeCharacter))
            {
                buffer.Append(QuoteCharacter);
            }

            // Make sure that the buffer doesn't end with an escaped quote
            // sequence (avoid acting if it is an double escaped quote) or
            // we're likely to corrupt any usage of the result.
            if (buffer[buffer.Length - 2] == EscapeCharacter
                && buffer[buffer.Length - 3] != EscapeCharacter)
            {
                buffer.Insert(buffer.Length - 2, EscapeCharacter);
            }
        }

        /// <summary>
        /// Escapes all unescaped quote characters, includes wrapping quotes.
        /// </summary>
        private static void EscapeBuffer(StringBuffer buffer)
        {
            int start = 0;
            int length = buffer.Length;

            if (buffer.Length > 2 && buffer[0] == QuoteCharacter
                && buffer[buffer.Length - 1] == QuoteCharacter
                && buffer[buffer.Length - 2] != EscapeCharacter)
            {
                start += 1;
                length -= 1;
            }

            for (int i = start; i < length; i += 1)
            {
                // If we find an unescaped double-quote character in the path,
                // we need to escape it
                if (buffer[i] == QuoteCharacter
                    && (i == start || buffer[i - 1] != EscapeCharacter))
                {
                    // Insert an escape character
                    buffer.Insert(i, EscapeCharacter);
                    // Move beyond the escaped character
                    i += 1;
                }
            }
        }

        /// <summary>
        /// Escapes and enquotes a string if it contains any whitespace characters.
        /// <para>Already enquoted strings will not be double enquoted.</para>
        /// <para>Already escaped strings will not be double escaped.</para>
        /// </summary>
        private static void EscapeAndEnquoteConditionally(StringBuffer buffer)
        {
            // Scan the buffer for any whitespace characters
            // and if any exist, then wrap the path in quotes.
            for (int i = 0; i < buffer.Length; i += 1)
            {
                if (char.IsWhiteSpace(buffer[i]))
                {
                    EscapeBuffer(buffer);
                    EnquoteBuffer(buffer);
                    break;
                }
            }
        }

        /// <summary>
        /// Replaces all Windows path separator characters with POSIX path separators.
        /// <para>A backslash character preceding a quote character is assumed to be an escape character, and will not be replaced.</para>
        /// </summary>
        private static void NormalizeBuffer(StringBuffer buffer)
        {
            for (int i = 0; i < buffer.Length; i += 1)
            {
                if (buffer[i] == LocalPathSeparatorCharacter)
                {
                    if (i + 1 >= buffer.Length || buffer[i + 1] != QuoteCharacter)
                    {
                        buffer[i] = PosixPathSeparatorCharacter;
                    }
                    else
                    {
                        // Since we know the next character is a '"', we can just skip it.
                        i += 1;
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to parse a byte value from an octal triple.
        /// <para>Returns <see langword="true"/> if successful; otherwise <see langword="false"/>.</para>
        /// </summary>
        private static bool ParseOctalTriple(byte b0, byte b1, byte b2, out byte value)
        {
            if ((b0 < (byte)'0') || (b0 > (byte)'3') ||
                (b1 < (byte)'0') || (b1 > (byte)'7') ||
                (b2 < (byte)'0') || (b2 > (byte)'7'))
            {
                value = 0;
                return false;
            }

            value = (byte)((b0 - (byte)'0') << 6
                         | (b1 - (byte)'0') << 3
                         | (b2 - (byte)'0') << 0);

            return true;
        }

        /// <summary>
        /// Removes leading and/or trailing slash and/or whitespace characters.
        /// </summary>
        private static void TrimBuffer(StringBuffer buffer)
        {
            // Remove all leading '/', '\', ' ' characters.
            while (buffer.Length > 0
                && (buffer[0] == LocalPathSeparatorCharacter
                    || buffer[0] == PosixPathSeparatorCharacter
                    || char.IsWhiteSpace(buffer[0])))
            {
                buffer.Remove(0, 1);
            }

            // Remove all leading '/', '\', ' ' characters.
            while (buffer.Length > 1
                && (buffer[buffer.Length - 1] == LocalPathSeparatorCharacter
                    || buffer[buffer.Length - 1] == PosixPathSeparatorCharacter
                    || char.IsWhiteSpace(buffer[buffer.Length - 1])))
            {
                buffer.Remove(buffer.Length - 1, 1);
            }
        }
    }
}
