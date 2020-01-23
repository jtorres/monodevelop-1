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
using System.Runtime.Serialization;
using System.Text;
using Microsoft.TeamFoundation.GitApi.Cli;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    [Serializable]
    public abstract class ParseException : ExceptionBase
    {
        internal ParseException(string errorMessage, string parseHint, StringUtf8 buffer, int index)
            : base(errorMessage)
        {
            ParseHint = parseHint;
            Buffer = buffer;
            Index = index;
            Context = new StringBuilder();
        }

        internal ParseException(string errorMessage, string parseHint, StringUtf8 buffer, int index, Exception innerException)
            : base(errorMessage, innerException)
        {
            ParseHint = parseHint;
            Buffer = buffer;
            Index = index;
            Context = new StringBuilder();
        }

        internal ParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Buffer = new StringUtf8(info.GetString(nameof(Buffer)));
            Context = new StringBuilder(info.GetString(nameof(Context)));
            Index = info.GetInt32(nameof(Index));
            ParseHint = info.GetString(nameof(ParseHint));
        }

        internal void AddContext(string data)
        {
            Context.Append(data);
        }

        internal string GetParseSummary(int hintLength)
        {
            var sb = new StringBuilder();

            try
            {
                sb.Append("Message: ").AppendLine(Message);
                sb.Append("ParseHint: ").AppendLine(ParseHint);
                sb.Append("Index: ").AppendLine(Index.ToString());
                sb.AppendLine("Buffer:");
                sb.AppendLine(Buffer?.ToString() ?? "<null>");
                sb.AppendLine($"buffer[{Index}]=> {Buffer?.Substring(Index, Math.Min(hintLength, Buffer.Length - Index))}");
                sb.AppendLine("Context:");
                sb.AppendLine(Context.ToString());
                if (InnerException != null)
                {
                    sb.Append("InnerException: ").AppendLine(InnerException.Message);
                }
            }
            catch (ExceptionBase)
            {
                // Do not let errors during parse summary creation leak out
            }

            return sb.ToString();
        }

        public readonly string ParseHint;

        internal readonly StringUtf8 Buffer;
        internal readonly int Index;
        internal readonly StringBuilder Context;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Buffer), Buffer.ToString());
            info.AddValue(nameof(context), context.ToString());
            info.AddValue(nameof(Index), Index);
            info.AddValue(nameof(ParseHint), ParseHint);
        }
    }

    [Serializable]
    public class PathParseException : ParseException
    {
        const string DefaultMessage = "Unable to parse Git path value.";

        internal PathParseException(string parseHint, StringUtf8 buffer, int index)
            : base(DefaultMessage, parseHint, buffer, index)
        { }

        internal PathParseException(string parseHint, StringUtf8 buffer, int index, Exception innerException)
            : base(DefaultMessage, parseHint, buffer, index, innerException)
        { }

        internal PathParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class CleanParseException : ParseException
    {
        internal const string ErrorMessage = "failed to parse git-clean output";

        internal CleanParseException(StringUtf8 buffer, int index)
            : base(ErrorMessage, string.Empty, buffer, index)
        { }

        internal CleanParseException(StringUtf8 buffer, int index, Exception innerException)
            : base(ErrorMessage, string.Empty, buffer, index, innerException)
        { }

        internal CleanParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class CommitParseException : ParseException
    {
        private const string DefaultMessage = "fatal: unexpected sequence in " + CommitCommand.Command + " output.";

        internal CommitParseException(string parseHint, StringUtf8 buffer, int index)
            : base(DefaultMessage, parseHint, buffer, index)
        { }

        internal CommitParseException(string parseHint, StringUtf8 buffer, int index, Exception innerException)
            : base(DefaultMessage, parseHint, buffer, index, innerException)
        { }

        internal CommitParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class ConfigParseException : ParseException
    {
        public const string ErrorMessage = "error parsing the config output.";

        internal ConfigParseException(string parseHint, StringUtf8 buffer, int index)
            : base(ErrorMessage, parseHint, buffer, index)
        { }

        internal ConfigParseException(string parseHint, StringUtf8 buffer, int index, Exception innerException)
            : base(ErrorMessage, parseHint, buffer, index, innerException)
        { }

        internal ConfigParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class CountObjectsParseException : ParseException
    {
        private const string ErrorMessage = "fatal: unrecognized sequence when parsing git " + CountObjectsCommand.Command + " output";

        internal CountObjectsParseException(string parseHint, StringUtf8 buffer, int index)
            : base(ErrorMessage, parseHint, buffer, index)
        { }

        internal CountObjectsParseException(string parseHint, StringUtf8 buffer, int index, Exception innerException)
            : base(ErrorMessage, parseHint, buffer, index, innerException)
        { }

        internal CountObjectsParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class DifferenceParseException : ParseException
    {
        public const string ErrorMessage = "fatal: unrecognized sequence when parsing git diff output";

        internal DifferenceParseException(string parseHint, StringUtf8 buffer, int index)
            : base(ErrorMessage, parseHint, buffer, index)
        { }

        internal DifferenceParseException(string parseHint, StringUtf8 buffer, int index, Exception innerException)
            : base(ErrorMessage, parseHint, buffer, index, innerException)
        { }

        internal DifferenceParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class IndexParseException : ParseException
    {
        private const string DefaultMessage = "Unable to parse index output";

        internal IndexParseException(string hint, StringUtf8 buffer, int index)
            : base(DefaultMessage, hint, buffer, index)
        { }

        internal IndexParseException(string hint, StringUtf8 buffer, int index, Exception innerException)
            : base(DefaultMessage, hint, buffer, index, innerException)
        { }

        internal IndexParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class LsFilesParseException : ParseException
    {
        public const string ErrorMessage = "error parsing ls-files output.";

        internal LsFilesParseException(StringUtf8 buffer, int index)
            : base(ErrorMessage, string.Empty, buffer, index)
        { }

        internal LsFilesParseException(StringUtf8 buffer, int index, Exception innerException)
            : base(ErrorMessage, string.Empty, buffer, index, innerException)
        { }

        internal LsFilesParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class ObjectIdParseException : ParseException
    {
        internal ObjectIdParseException(string message, StringUtf8 buffer, int index)
            : base(message, "objectId", buffer, index)
        { }

        internal ObjectIdParseException(string message, StringUtf8 buffer, int index, Exception innerException)
            : base(message, "objectId", buffer, index, innerException)
        { }

        internal ObjectIdParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class ObjectParseException : ParseException
    {
        public const string DefaultMessage = "fatal: unexpected sequence from git " + CatFileCommand.Command + " encountered.";

        internal ObjectParseException(string parseHint, StringUtf8 buffer, int index)
            : base(DefaultMessage, parseHint, buffer, index)
        { }

        internal ObjectParseException(string parseHint, StringUtf8 buffer, int index, Exception innerException)
            : base(DefaultMessage, parseHint, buffer, index, innerException)
        { }

        internal ObjectParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class ReferenceParseException : ParseException
    {
        internal ReferenceParseException(string errorMessage, StringUtf8 buffer, int index)
            : base(errorMessage, string.Empty, buffer, index)
        { }

        internal ReferenceParseException(string errorMessage, StringUtf8 buffer, int index, Exception innerException)
            : base(errorMessage, string.Empty, buffer, index, innerException)
        { }

        internal ReferenceParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class RemoteParseException : ParseException
    {
        public const string DefaultErrorMessage = "failed to parse remote";

        internal RemoteParseException(string parseHint, StringUtf8 buffer, int index)
            : base(DefaultErrorMessage, parseHint, buffer, index)
        { }

        internal RemoteParseException(string parseHint, StringUtf8 buffer, int index, Exception innerException)
            : base(DefaultErrorMessage, parseHint, buffer, index, innerException)
        { }

        internal RemoteParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class RevListParseException : ParseException
    {
        private const string DefaultMessage = "fatal: unexpected sequence in " + RevListCommand.Command + " output.";

        internal RevListParseException(string parseHint, StringUtf8 buffer, int index)
            : base(DefaultMessage, parseHint, buffer, index)
        { }

        internal RevListParseException(string parseHint, StringUtf8 buffer, int index, Exception innerException)
            : base(DefaultMessage, parseHint, buffer, index, innerException)
        { }

        internal RevListParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class RevParseParseException : ParseException
    {
        public const string DefaultMessage = "fatal: unexpected sequence from git " + RevParseCommand.Command + " encountered.";

        internal RevParseParseException(string parseHint, StringUtf8 buffer, int index)
            : base(DefaultMessage, parseHint, buffer, index)
        { }

        internal RevParseParseException(string message, string parseHint, StringUtf8 buffer, int index)
            : base(FormattableString.Invariant($"{DefaultMessage}{System.Environment.NewLine}{message}"), parseHint, buffer, index)
        { }

        internal RevParseParseException(string parseHint, StringUtf8 buffer, int index, Exception innerException)
            : base(DefaultMessage, parseHint, buffer, index, innerException)
        { }

        internal RevParseParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class StatusParseException : ParseException
    {
        public const string ErrorMessage = "error parsing the git " + StatusCommand.Command + " output.";

        internal StatusParseException(string hint, StringUtf8 buffer, int index)
            : base(ErrorMessage, hint, buffer, index)
        { }

        internal StatusParseException(string hint, StringUtf8 buffer, int index, System.Exception innerException)
            : base(ErrorMessage, hint, buffer, index, innerException)
        { }

        internal StatusParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
