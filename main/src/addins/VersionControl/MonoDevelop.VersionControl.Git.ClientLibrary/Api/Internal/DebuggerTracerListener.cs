//*************************************************************************************************
// Tracer.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal sealed class DebuggerTracerListener : ITracerListener
    {
        private const string DebugOutputPrefix = "Gitapi>";

        public TracerKind Kind
        {
            get { return TracerKind.Any; }
        }

        public TracerLevel Level
        {
            get { return TracerLevel.Diagnostic; }
        }

        public void Write(TracerMessage tracerMessage)
        {
            const int MaxSourceColumnWidth = 29;
            const char PathSeparator = '\\';

            string source = tracerMessage.Source;
            int srclen = source.Length;
            int index = 0;

            while (index >= 0 && (srclen - index) > MaxSourceColumnWidth)
            {
                index = source.IndexOf(PathSeparator, index + 1);
            }

            if (index <= 0)
            {
                index = source.LastIndexOf(PathSeparator);
            }

            if (index > 0)
            {
                source = source.Substring(index);
                source = "..." + source;
            }

            Debug.Write(FormattableString.Invariant($"{DebugOutputPrefix} {tracerMessage.TimeStamp:HH:mm:ss.ffff} {source,-(MaxSourceColumnWidth + 3)} [{tracerMessage.Member}] "));

            // Multi-line messages should start on a separate line below the time stamp
            if (tracerMessage.Synopsis.IndexOf('\n') >= 0)
            {
                Debug.WriteLine(string.Empty);
            }

            Debug.WriteLine(tracerMessage.Synopsis);
        }
    }
}
