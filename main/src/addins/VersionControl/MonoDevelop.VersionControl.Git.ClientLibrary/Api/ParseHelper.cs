//*************************************************************************************************
// ParseHelper.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Globalization;
using System.Text;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Parsing helper class.
    /// </summary>
    internal class ParseHelper
    {
        public const int IndentWidth = 2;

        private static readonly NumberStyles DecimalStyle = NumberStyles.AllowDecimalPoint
                                                          | NumberStyles.AllowLeadingSign
                                                          | NumberStyles.AllowLeadingWhite
                                                          | NumberStyles.AllowThousands
                                                          | NumberStyles.AllowTrailingSign;
        private static readonly NumberStyles IntegerStyle = NumberStyles.AllowLeadingSign
                                                          | NumberStyles.AllowLeadingWhite
                                                          | NumberStyles.AllowThousands
                                                          | NumberStyles.AllowTrailingSign;

        public ParseHelper(ExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _context = context;
        }

        private readonly ExecutionContext _context;

        /// <summary> Add context to a parser error. This always returns false for use with exception
        /// filtering. </remarks>
        public bool AddContext(string name, ParseException ex, params object[] loggables)
        {
            try
            {
                // Use the StringBuilder in the exception to avoid having to copy the entire
                // context string again during ex.AddContext.
                StringBuilder sb = ex.Context;

                sb.AppendLine(name);
                sb.AppendLine("{");
                {
                    string prefix = GetParseErrorIndent(1);

                    foreach (var loggable in loggables)
                    {
                        // Log a string as a string
                        if (loggable is string)
                        {
                            sb.Append(prefix)
                              .Append("\"")
                              .Append(loggable)
                              .AppendLine("\"");
                        }
                        // Otherwise we can log more complex things only if they're Iloggable
                        else
                        {
                            var iloggable = loggable as ILoggable;
                            if (iloggable == null)
                            {
                                sb.Append(prefix)
                                  .AppendLine("<null>");
                            }
                            else
                            {
                                iloggable.Log(_context, sb, 1);
                            }
                        }
                    }
                }
                sb.AppendLine("}");

                // Trace this as an error. This same exception may get traced multiple times as
                // it bubbles up through multiple exception filters, but it's easier to capture
                // it centrally this way instead of requiring all external callers that catch
                // exceptions to call back down into the library to log the full exception context.
                _context.Tracer.TraceException(ex);
            }
            catch (ExceptionBase)
            {
                // This will only be called during exception handlers, so do not let any
                // exceptions leak out
            }

            return false;
        }

        /// <summary>
        /// Get the prefix string for parse error logging based on the requested indent level.
        /// </summary>
        public string GetParseErrorIndent(int indent)
        {
            return (indent > 0) ? string.Empty.PadRight(indent * IndentWidth) : string.Empty;
        }

        /// <summary>
        /// <para>
        /// Converts the string representation of a number in a specified style and culture-invariant
        /// format to its 64-bit double-precision floating-point equivalent.
        /// </para>
        /// <para>A return value indicates whether the conversion succeeded or failed.</para>
        /// </summary>
        /// <param name="input">
        /// <para>A string containing a number to convert.</para>
        /// <para>The string is interpreted using the  <see cref="NumberStyles.Number"/> style.</para>
        /// </param>
        /// <param name="value">
        /// <para>
        /// 64-bit double-precision floating-point value equivalent of the number contained in
        /// <paramref name="input"/>, if the conversion succeeded, or <see cref="double.NaN"/> if
        /// the conversion failed.
        /// </para>
        /// <para>
        /// The conversion fails if  <paramref name="input"/> is <see langword="null"/> or
        /// <see cref="string.Empty"/>, is not format compliant with
        /// <see cref="NumberStyles.Number"/> style, or represents a number less than
        /// <see cref="double.MinValue"/> or greater than <see cref="long.MaxValue"/>.
        /// </para>
        /// </param>
        public bool TryParseNumber(string input, out double value)
        {
            return double.TryParse(input, DecimalStyle, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// <para>
        /// Converts the string representation of a number in a specified style and culture-invariant
        /// format to its 32-bit signed integer equivalent.
        /// </para>
        /// <para>A return value indicates whether the conversion succeeded or failed.</para>
        /// </summary>
        /// <param name="input">
        /// <para>A string containing a number to convert.</para>
        /// <para>The string is interpreted using the  <see cref="NumberStyles.Number"/> style.</para>
        /// </param>
        /// <param name="value">
        /// <para>
        /// 32-bit signed integer value equivalent of the number contained in
        /// <paramref name="input"/>, if the conversion succeeded, or zero if the conversion failed.
        /// </para>
        /// <para>
        /// The conversion fails if  <paramref name="input"/> is <see langword="null"/> or
        /// <see cref="string.Empty"/>, is not format compliant with
        /// <see cref="NumberStyles.Number"/> style, or represents a number less than
        /// <see cref="int.MinValue"/> or greater than <see cref="int.MaxValue"/>.
        /// </para>
        /// </param>
        public bool TryParseNumber(string input, out int value)
        {
            return int.TryParse(input, IntegerStyle, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// <para>
        /// Converts the string representation of a number in a specified style and culture-invariant
        /// format to its 64-bit signed integer equivalent.
        /// </para>
        /// <para>A return value indicates whether the conversion succeeded or failed.</para>
        /// </summary>
        /// <param name="input">
        /// <para>A string containing a number to convert.</para>
        /// <para>The string is interpreted using the <see cref="NumberStyles.Number"/> style.</para>
        /// </param>
        /// <param name="value">
        /// <para>
        /// 64-bit signed integer value equivalent of the number contained in
        /// <paramref name="input"/>, if the conversion succeeded, or zero if the conversion failed.
        /// </para>
        /// <para>
        /// The conversion fails if <paramref name="input"/> is <see langword="null"/> or
        /// <see cref="string.Empty"/>, is not format compliant with
        /// <see cref="NumberStyles.Number"/> style, or represents a number less than
        /// <see cref="long.MinValue"/> or greater than <see cref="long.MaxValue"/>.
        /// </para>
        /// </param>
        public bool TryParseNumber(string input, out long value)
        {
            return long.TryParse(input, IntegerStyle, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// <para>
        /// Converts the string representation of a number in a specified style and culture-invariant
        /// format to its 32-bit unsigned integer equivalent.
        /// </para>
        /// <para>A return value indicates whether the conversion succeeded or failed.</para>
        /// </summary>
        /// <param name="input">
        /// <para>A string containing a number to convert.</para>
        /// <para>The string is interpreted using the <see cref="NumberStyles.Number"/> style.</para>
        /// </param>
        /// <param name="value">
        /// <para>
        /// 32-bit unsigned integer value equivalent of the number contained in
        /// <paramref name="input"/>, if the conversion succeeded, or zero if the conversion failed.
        /// </para>
        /// <para>
        /// The conversion fails if <paramref name="input"/> is <see langword="null"/> or
        /// <see cref="string.Empty"/>, is not format compliant with
        /// <see cref="NumberStyles.Number"/> style, or represents a number less than
        /// <see cref="uint.MinValue"/> or greater than <see cref="uint.MaxValue"/>.
        /// </para>
        /// </param>
        public bool TryParseNumber(string input, out uint value)
        {
            return uint.TryParse(input, IntegerStyle, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// <para>
        /// Converts the string representation of a number in a specified style and culture-invariant
        /// format to its 64-bit unsigned integer equivalent.
        /// </para>
        /// <para>A return value indicates whether the conversion succeeded or failed.</para>
        /// </summary>
        /// <param name="input">
        /// <para>A string containing a number to convert.</para>
        /// <para>The string is interpreted using the <see cref="NumberStyles.Number"/> style.</para>
        /// </param>
        /// <param name="value">
        /// <para>
        /// 64-bit unsigned integer value equivalent of the number contained in
        /// <paramref name="input"/>, if the conversion succeeded, or zero if the conversion failed.
        /// </para>
        /// <para>
        /// The conversion fails if <paramref name="input"/> is <see langword="null"/> or
        /// <see cref="string.Empty"/>, is not format compliant with
        /// <see cref="NumberStyles.Number"/> style, or represents a number less than
        /// <see cref="ulong.MinValue"/> or greater than <see cref="ulong.MaxValue"/>.
        /// </para>
        /// </param>
        public bool TryParseNumber(string input, out ulong value)
        {
            return ulong.TryParse(input, IntegerStyle, CultureInfo.InvariantCulture, out value);
        }
    }
}
