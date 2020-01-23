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
using System.Threading;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal class TracerTimer : IDisposable
    {
        static TracerTimer()
        {
            _watches = new System.Collections.Concurrent.ConcurrentBag<Stopwatch>();
        }

        internal TracerTimer(
            TracerKind tracerKind,
            TracerLevel tracerLevel,
            FormattableString message,
            FormattableString details,
            object userData,
            string filePath,
            int lineNumber,
            string memberName,
            Tracer tracer)
        {
            Debug.Assert((tracerKind & ~TracerKind.Any) == 0, $"The `{nameof(tracerKind)}` parameter is undefined.");
            Debug.Assert(Enum.IsDefined(typeof(TracerLevel), tracerLevel), $"The `{nameof(tracerKind)}` parameter is undefined.");
            Debug.Assert(message != null, $"The `{nameof(message)}` parameter is null.");
            Debug.Assert(filePath != null, $"The `{nameof(filePath)}` parameter is null");
            Debug.Assert(lineNumber > 0, $"The `{nameof(lineNumber)}` parameter is invalid");
            Debug.Assert(memberName != null, $"The `{nameof(memberName)}` parameter is null");
            Debug.Assert(tracer != null, $"The `{nameof(tracer)}` parameter is null.");

            if (!_watches.TryTake(out _stopwatch))
            {
                _stopwatch = new Stopwatch();
            }

            _details = details;
            _message = message;
            _kind = tracerKind | TracerKind.Performance;
            _level = tracerLevel;
            _filePath = filePath;
            _lineNumber = lineNumber;
            _memberName = memberName;
            _userData = userData;
            _tracer = tracer;

            _stopwatch.Start();
        }

        private readonly FormattableString _details;
        private readonly string _filePath;
        private readonly TracerKind _kind;
        private readonly TracerLevel _level;
        private readonly int _lineNumber;
        private readonly string _memberName;
        private readonly FormattableString _message;
        private readonly object _userData;
        private Stopwatch _stopwatch;
        private readonly Tracer _tracer;
        private static readonly System.Collections.Concurrent.ConcurrentBag<Stopwatch> _watches;

        public void Dispose()
        {
            Stopwatch stopwatch;
            if ((stopwatch = Interlocked.Exchange(ref _stopwatch, null)) is null)
                return;

            stopwatch.Stop();

            try
            {
                _tracer.Write(_kind, _level, _message, _details, stopwatch.Elapsed, _userData, _filePath, _lineNumber, _memberName);
            }
            catch { /* squelch */ }

            stopwatch.Reset();

            try
            {
                _watches.Add(stopwatch);
            }
            catch { /* squelch */ }

            GC.SuppressFinalize(this);
        }
    }
}
