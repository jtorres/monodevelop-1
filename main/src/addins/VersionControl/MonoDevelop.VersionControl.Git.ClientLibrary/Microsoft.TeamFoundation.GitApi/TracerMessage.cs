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
using System.Diagnostics;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// A "message" passed by an `<seealso cref="ITracer"/>` interface through an `<seealso cref="ITracerListener"/>`.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct TracerMessage
    {
        internal TracerMessage(
            ExecutionContext context,
            TimeSpan duration,
            TracerKind kind,
            TracerLevel level,
            string synopsis,
            string details,
            object userData,
            string source,
            string member)
        {
            Debug.Assert(duration >= TimeSpan.Zero, $"the `{nameof(duration)}` parameter is invalid.");
            Debug.Assert((kind & ~TracerKind.Any) == 0, $"The `{nameof(kind)}` parameter is undefined.");
            Debug.Assert(Enum.IsDefined(typeof(TracerLevel), level), $"The `{nameof(level)}` parameter is undefined");
            Debug.Assert(synopsis != null, $"The `{nameof(synopsis)}` parameter is null.");
            Debug.Assert(source != null, $"The `{nameof(source)}` parameter is null.");
            Debug.Assert(member != null, $"The `{nameof(member)}` parameter is null.");

            _context = context;
            _details = details ?? string.Empty;
            _duration = duration;
            _kind = kind;
            _level = level;
            _member = member;
            _synopsis = synopsis;
            _userData = userData;
            _source = source;
            _timestamp = DateTime.UtcNow;
        }

        internal TracerMessage(
            ExecutionContext context,
            TracerKind kind,
            TracerLevel level,
            string synopsis,
            string details,
            object userData,
            string source,
            string member)
            : this(context, TimeSpan.Zero, kind, level, synopsis, details, userData, source, member)
        { }

        private ExecutionContext _context;
        private string _details;
        private TimeSpan _duration;
        private TracerKind _kind;
        private TracerLevel _level;
        private string _member;
        private object _userData;
        private string _source;
        private string _synopsis;
        private DateTime _timestamp;

        /// <summary>
        /// The duration or time it took for the trace to complete; `<see cref="TimeSpan.Zero"/>` for non-timing traces.
        /// </summary>
        public TimeSpan Duration
        {
            get { return _duration; }
        }

        /// <summary>
        /// The extended details, if any, of the message; `<see cref="string.Empty"/>` by default.
        /// </summary>
        public string Details
        {
            get { return _details; }
        }

        /// <summary>
        /// The kind of trace collected.
        /// </summary>
        public TracerKind Kind
        {
            get { return _kind; }
        }

        /// <summary>
        /// The level of detail the trace has collected.
        /// </summary>
        public TracerLevel Level
        {
            get { return _level; }
        }

        /// <summary>
        /// The name of the member the trace was collected from.
        /// </summary>
        public string Member
        {
            get { return _member; }
        }

        /// <summary>
        /// Reference to caller meta if supplied; otherwise `<see langword="null"/>`.
        /// </summary>
        public object UserData
        {
            get { return _userData; }
        }

        /// <summary>
        /// The file associated with the member the trace was collected from.
        /// </summary>
        public string Source
        {
            get { return _source; }
        }

        /// <summary>
        /// The title, or synopsis, of the message associated with the trace.
        /// </summary>
        public string Synopsis
        {
            get { return _synopsis; }
        }

        /// <summary>
        /// The time-stamp of the trace collection (completion if timing trace).
        /// </summary>
        public DateTime TimeStamp
        {
            get { return _timestamp; }
        }

        internal ExecutionContext Context
        {
            get { return _context; }
        }
    }
}
