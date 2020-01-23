//*************************************************************************************************
// Tracer.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.TeamFoundation.GitApi.Internal;
using static System.FormattableString;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Tracing utility designed to enable run-time tracing of events, errors, and other messages
    /// to be reported to the user and/or written into a log.
    /// </summary>
    public interface ITracer
    {
        /// <summary>
        /// Adds a registered trace listener.
        /// <para/>
        /// Returns `<see langword="true"/>` if addition is successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        bool AddListener(ITracerListener listener);

        /// <summary>
        /// Remove a registered trace listener
        /// <para/>
        /// Returns `<see langword="true"/>` if removal is successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        bool RemoveListener(ITracerListener listener);

        /// <summary>
        /// Broadcasts a message to all registered trace listeners registered for `<paramref name="kind"/>` and a `<see cref="TracerLevel"/>` less than or equal to `<paramref name="level"/>`.
        /// </summary>
        /// <param name="kind">The type or kind of message.</param>
        /// <param name="synopsis">The message, or synopsis, of a detailed message.</param>
        /// <param name="details">
        /// Extended message details.
        /// <para/>
        /// Intended for very long messages which would interfere with logging, or for messages with personally identifiable information and/or secrets in them.
        /// <para/>
        /// Enables listeners to omit potentially harmful data being logged or broadcast as part of telemetry.
        /// </param>
        /// <param name="level">The importance or level of the message.</param>
        /// <param name="callbackData">
        /// Caller assigned data which is return with every message sent to a listener.
        /// <para/>
        /// Intended for callers which want to add additional layers of filtering to messages received via tracing.
        /// </param>
        void Trace(TracerKind kind, FormattableString synopsis, FormattableString details = null, TracerLevel level = TracerLevel.Diagnostic, object userData = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");

        /// <summary>
        /// Broadcasts a message to all registered trace listeners registered for `<paramref name="kind"/>` and a `<see cref="TracerLevel"/>` less than or equal to `<paramref name="level"/>`.
        /// </summary>
        /// <param name="kind">The type or kind of message.</param>
        /// <param name="synopsis">The message, or synopsis, of a detailed message.</param>
        /// <param name="details">
        /// Extended message details.
        /// <para/>
        /// Intended for very long messages which would interfere with logging, or for messages with personally identifiable information and/or secrets in them.
        /// <para/>
        /// Enables listeners to omit potentially harmful data being logged or broadcast as part of telemetry.
        /// </param>
        /// <param name="level">The importance or level of the message.</param>
        /// <param name="callbackData">
        /// Caller assigned data which is return with every message sent to a listener.
        /// <para/>
        /// Intended for callers which want to add additional layers of filtering to messages received via tracing.
        /// </param>
        void Trace(TracerKind kind, string synopsis, string details = null, TracerLevel level = TracerLevel.Diagnostic, object userData = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");

        /// <summary>
        /// Broadcasts a message to all registered trace listeners registered for `<see cref="TracerKind.CommandPerformance"/>` and a `<see cref="TracerLevel"/>` less than or equal to `<paramref name="level"/>`.
        /// </summary>
        /// <param name="synopsis">The message, or synopsis, of a detailed message.</param>
        /// <param name="details">
        /// Extended message details.
        /// <para/>
        /// Intended for very long messages which would interfere with logging, or for messages with personally identifiable information and/or secrets in them.
        /// <para/>
        /// Enables listeners to omit potentially harmful data being logged or broadcast as part of telemetry.
        /// </param>
        /// <param name="level">The importance or level of the message.</param>
        /// <param name="callbackData">
        /// Caller assigned data which is return with every message sent to a listener.
        /// <para/>
        /// Intended for callers which want to add additional layers of filtering to messages received via tracing.
        /// </param>
        IDisposable TraceCommand(FormattableString synopsis, FormattableString details, TracerLevel level = TracerLevel.Diagnostic, object userData = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");

        /// <summary>
        /// Broadcasts a message to all registered trace listeners registered for `<see cref="TracerKind.CommandPerformance"/>` and a `<see cref="TracerLevel"/>` less than or equal to `<paramref name="level"/>`.
        /// </summary>
        /// <param name="synopsis">The message, or synopsis, of a detailed message.</param>
        /// <param name="details">
        /// Extended message details.
        /// <para/>
        /// Intended for very long messages which would interfere with logging, or for messages with personally identifiable information and/or secrets in them.
        /// <para/>
        /// Enables listeners to omit potentially harmful data being logged or broadcast as part of telemetry.
        /// </param>
        /// <param name="level">The importance or level of the message.</param>
        /// <param name="callbackData">
        /// Caller assigned data which is return with every message sent to a listener.
        /// <para/>
        /// Intended for callers which want to add additional layers of filtering to messages received via tracing.
        /// </param>
        IDisposable TraceCommand(FormattableString synopsis, string details, TracerLevel level = TracerLevel.Diagnostic, object userData = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");

        /// <summary>
        /// Broadcasts a message to all registered trace listeners registered for `<see cref="TracerKind.CommandPerformance"/>` and a `<see cref="TracerLevel"/>` less than or equal to `<paramref name="level"/>`.
        /// </summary>
        /// <param name="synopsis">The message, or synopsis, of a detailed message.</param>
        /// <param name="details">
        /// Extended message details.
        /// <para/>
        /// Intended for very long messages which would interfere with logging, or for messages with personally identifiable information and/or secrets in them.
        /// <para/>
        /// Enables listeners to omit potentially harmful data being logged or broadcast as part of telemetry.
        /// </param>
        /// <param name="level">The importance or level of the message.</param>
        /// <param name="callbackData">
        /// Caller assigned data which is return with every message sent to a listener.
        /// <para/>
        /// Intended for callers which want to add additional layers of filtering to messages received via tracing.
        /// </param>
        IDisposable TraceCommand(string synopsis, string details = null, TracerLevel level = TracerLevel.Diagnostic, object userData = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");

        /// <summary>
        /// Broadcasts a message to all registered trace listeners registered for `<see cref="TracerKind.Error"/>` and a `<see cref="TracerLevel"/>` less than or equal to `<paramref name="level"/>`.
        /// </summary>
        /// <param name="synopsis">The message, or synopsis, of a detailed message.</param>
        /// <param name="details">Extended message details.
        /// <para/>
        /// Intended for very long messages which would interfere with logging, or for messages with personally identifiable information and/or secrets in them.
        /// <para/>
        /// Enables listeners to omit potentially harmful data being logged or broadcast as part of telemetry.
        /// </param>
        /// <param name="level">The importance or level of the message.</param>
        /// <param name="callbackData">
        /// Caller assigned data which is return with every message sent to a listener.
        /// <para/>
        /// Intended for callers which want to add additional layers of filtering to messages received via tracing.
        /// </param>
        void TraceError(FormattableString synopsis, FormattableString details = null, TracerLevel level = TracerLevel.Normal, object userData = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");

        /// <summary>
        /// Broadcasts a message to all registered trace listeners registered for `<see cref="TracerKind.Error"/>` and a `<see cref="TracerLevel"/>` less than or equal to `<paramref name="level"/>`.
        /// </summary>
        /// <param name="synopsis">The message, or synopsis, of a detailed message.</param>
        /// <param name="details">
        /// Extended message details.
        /// <para/>
        /// Intended for very long messages which would interfere with logging, or for messages with personally identifiable information and/or secrets in them.
        /// <para/>
        /// Enables listeners to omit potentially harmful data being logged or broadcast as part of telemetry.
        /// </param>
        /// <param name="level">The importance or level of the message.</param>
        /// <param name="callbackData">
        /// Caller assigned data which is return with every message sent to a listener.
        /// <para/>
        /// Intended for callers which want to add additional layers of filtering to messages received via tracing.
        /// </param>
        void TraceError(string synopsis, string details = null, TracerLevel level = TracerLevel.Normal, object userData = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");

        /// <summary>
        /// Broadcasts a message to all registered trace listeners registered for `<see cref="TracerKind.Error"/>` and a `<see cref="TracerLevel"/>` less than or equal to `<paramref name="level"/>`.
        /// </summary>
        /// <param name="exception">An exception to format into a message and broadcast.</param>
        /// <param name="level">The importance or level of the message.</param>
        /// <param name="callbackData">
        /// Caller assigned data which is return with every message sent to a listener.
        /// <para/>
        /// Intended for callers which want to add additional layers of filtering to messages received via tracing.
        /// </param>
        bool TraceException(Exception exception, TracerLevel level = TracerLevel.Normal, object userData = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");

        /// <summary>
        /// Broadcasts a message to all registered trace listeners registered for `<see cref="TracerKind.Message"/>` and a `<see cref="TracerLevel"/>` less than or equal to `<paramref name="level"/>`.
        /// </summary>
        /// <param name="synopsis">The message, or synopsis, of a detailed message.</param>
        /// <param name="details">
        /// Extended message details.
        /// <para/>
        /// Intended for very long messages which would interfere with logging, or for messages with personally identifiable information and/or secrets in them.
        /// <para/>
        /// Enables listeners to omit potentially harmful data being logged or broadcast as part of telemetry.
        /// </param>
        /// <param name="level">The importance or level of the message.</param>
        /// <param name="callbackData">
        /// Caller assigned data which is return with every message sent to a listener.
        /// <para/>
        /// Intended for callers which want to add additional layers of filtering to messages received via tracing.
        /// </param>
        void TraceMessage(FormattableString synopsis, FormattableString details = null, TracerLevel level = TracerLevel.Detailed, object userData = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");

        /// <summary>
        /// Broadcasts a message to all registered trace listeners registered for `<see cref="TracerKind.Message"/>` and a `<see cref="TracerLevel"/>` less than or equal to `<paramref name="level"/>`.
        /// </summary>
        /// <param name="synopsis">The message, or synopsis, of a detailed message.</param>
        /// <param name="details">Extended message details.
        /// <para/>
        /// Intended for very long messages which would interfere with logging, or for messages with personally identifiable information and/or secrets in them.
        /// <para/>
        /// Enables listeners to omit potentially harmful data being logged or broadcast as part of telemetry.
        /// </param>
        /// <param name="level">The importance or level of the message.</param>
        /// <param name="callbackData">
        /// Caller assigned data which is return with every message sent to a listener.
        /// <para/>
        /// Intended for callers which want to add additional layers of filtering to messages received via tracing.
        /// </param>
        void TraceMessage(string synopsis, string details = null, TracerLevel level = TracerLevel.Detailed, object userData = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");

        /// <summary>
        /// Broadcasts a message to all registered trace listeners registered for `<see cref="TracerKind.ProcessPerformance"/>` and a `<see cref="TracerLevel"/>` less than or equal to `<paramref name="level"/>`.
        /// </summary>
        /// <param name="synopsis">The message, or synopsis, of a detailed message.</param>
        /// <param name="details">Extended message details.
        /// <para/>
        /// Intended for very long messages which would interfere with logging, or for messages with personally identifiable information and/or secrets in them.
        /// <para/>
        /// Enables listeners to omit potentially harmful data being logged or broadcast as part of telemetry.
        /// </param>
        /// <param name="level">The importance or level of the message.</param>
        /// <param name="callbackData">
        /// Caller assigned data which is return with every message sent to a listener.
        /// <para/>
        /// Intended for callers which want to add additional layers of filtering to messages received via tracing.
        /// </param>
        IDisposable TraceProcess(string synopsis, string details = null, TracerLevel level = TracerLevel.Diagnostic, object userData = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");

        /// <summary>
        /// Broadcasts a message to all registered trace listeners registered for `<see cref="TracerKind.Warning"/>` and a `<see cref="TracerLevel"/>` less than or equal to `<paramref name="level"/>`.
        /// </summary>
        /// <param name="synopsis">The message, or synopsis, of a detailed message.</param>
        /// <param name="details">
        /// Extended message details.
        /// <para/>
        /// Intended for very long messages which would interfere with logging, or for messages with personally identifiable information and/or secrets in them.
        /// <para/>
        /// Enables listeners to omit potentially harmful data being logged or broadcast as part of telemetry.
        /// </param>
        /// <param name="level">The importance or level of the message.</param>
        /// <param name="callbackData">
        /// Caller assigned data which is return with every message sent to a listener.
        /// <para/>
        /// Intended for callers which want to add additional layers of filtering to messages received via tracing.
        /// </param>
        void TraceWarning(FormattableString synopsis, FormattableString details = null, TracerLevel level = TracerLevel.Detailed, object userData = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");

        /// <summary>
        /// Broadcasts a message to all registered trace listeners registered for `<see cref="TracerKind.Warning"/>` and a `<see cref="TracerLevel"/>` less than or equal to `<paramref name="level"/>`.
        /// </summary>
        /// <param name="synopsis">The message, or synopsis, of a detailed message.</param>
        /// <param name="details">
        /// Extended message details.
        /// <para/>
        /// Intended for very long messages which would interfere with logging, or for messages with personally identifiable information and/or secrets in them.
        /// <para/>
        /// Enables listeners to omit potentially harmful data being logged or broadcast as part of telemetry.
        /// </param>
        /// <param name="level">The importance or level of the message.</param>
        /// <param name="callbackData">
        /// Caller assigned data which is return with every message sent to a listener.
        /// <para/>
        /// Intended for callers which want to add additional layers of filtering to messages received via tracing.
        /// </param>
        void TraceWarning(string synopsis, string details = null, TracerLevel level = TracerLevel.Detailed, object userData = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "");
    }

    /// <summary>
    /// Listener for `<see cref="TracerMessage"/>` from an instance of `<see cref="ITracer"/>`, which
    /// can specify filtering on `<see cref="TracerKind"/>` and `<see cref="TracerLevel"/>`.
    /// </summary>
    public interface ITracerListener
    {
        /// <summary>
        /// The kind of `<see cref="TracerMessage"/>` reported to this listener.
        /// </summary>
        TracerKind Kind { get; }

        /// <summary>
        /// The maximum level of `<see cref="TracerMessage"/>` reported to this listener.
        /// </summary>
        TracerLevel Level { get; }

        /// <summary>
        /// Called by the trace infrastructure for every message which passes the `<see cref="Kind"/>` and `<see cref="Level"/>` filters.
        /// </summary>
        /// <param name="message">The message to be handled.</param>
        void Write(TracerMessage message);
    }

    internal class Tracer : ITracer
    {
        public Tracer(ExecutionContext context)
        {
            _context = context;
            _kinds = TracerKind.None;
            _listeners = new ConcurrentSet<ITracerListener>();
            _syncpoint = new object();
        }

        private ExecutionContext _context;
        private TracerKind _kinds;
        private TracerLevel _level;
        private readonly ConcurrentSet<ITracerListener> _listeners;
        private readonly object _syncpoint;

        internal TracerKind Kinds
        {
            get { lock (_syncpoint) return _kinds; }
        }

        internal TracerLevel Level
        {
            get { lock (_syncpoint) return _level; }
        }

        internal int ListenerCount
        {
            get { lock (_syncpoint) return _listeners.Count; }
        }

        public bool AddListener(ITracerListener listener)
        {
            Debug.Assert(listener != null, $"The `{nameof(listener)}` parameter is null.");

            if (listener is null)
                return false;

            _listeners.Add(listener);

            GetTracerLevelKinds(_listeners, out TracerKind kinds, out TracerLevel level);

            lock (_syncpoint)
            {
                _kinds = kinds;
                _level = level;
            }

            return true;
        }

        public bool RemoveListener(ITracerListener listener)
        {
            Debug.Assert(listener != null, $"The `{nameof(listener)}` parameter is null.");

            if (listener is null)
                return false;

            bool success = _listeners.Remove(listener);

            if (success)
            {
                GetTracerLevelKinds(_listeners, out TracerKind kinds, out TracerLevel level);

                lock (_syncpoint)
                {
                    _kinds = kinds;
                    _level = level;
                }
            }

            return success;
        }

        public void Trace(
            TracerKind kind,
            FormattableString synopsis,
            FormattableString details = null,
            TracerLevel level = TracerLevel.Diagnostic,
            object userData = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
            => Write(kind, level, synopsis, details, TimeSpan.Zero, userData, filePath, lineNumber, memberName);

        public void Trace(
            TracerKind kind,
            string synopsis,
            string details = null,
            TracerLevel level = TracerLevel.Diagnostic,
            object userData = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
            => Write(kind, level, synopsis, details, TimeSpan.Zero, userData, filePath, lineNumber, memberName);

        public IDisposable TraceCommand(
            FormattableString synopsis,
            FormattableString details,
            TracerLevel level = TracerLevel.Diagnostic,
            object userData = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
            => StartTimer(TracerKind.CommandPerformance, level, synopsis, details, userData, filePath, lineNumber, memberName);

        public IDisposable TraceCommand(
            FormattableString synopsis,
            string details,
            TracerLevel level = TracerLevel.Diagnostic,
            object userData = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
            => StartTimer(TracerKind.CommandPerformance, level, synopsis, details, userData, filePath, lineNumber, memberName);

        public IDisposable TraceCommand(
            string synopsis,
            string details = null,
            TracerLevel level = TracerLevel.Diagnostic,
            object userData = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
            => StartTimer(TracerKind.CommandPerformance, level, synopsis, details, userData, filePath, lineNumber, memberName);

        public void TraceError(
            FormattableString synopsis,
            FormattableString details = null,
            TracerLevel level = TracerLevel.Normal,
            object userData = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
            => Write(TracerKind.Error, level, synopsis, details, TimeSpan.Zero, userData, filePath, lineNumber, memberName);

        public void TraceError(
            string synopsis,
            string details = null,
            TracerLevel level = TracerLevel.Normal,
            object userData = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
            => Write(TracerKind.Error, level, synopsis, details, TimeSpan.Zero, userData, filePath, lineNumber, memberName);

        public bool TraceException(
            Exception exception,
            TracerLevel level = TracerLevel.Normal,
            object userData = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            Debug.Assert(exception != null, $"The `{nameof(exception)}` parameter is null.");

            if (!(exception is null))
            {
                // Special case parse errors and trace the additional context they contain
                if (exception is ParseException parseException)
                {
                    string details = parseException.GetParseSummary(100);
                    Write(TracerKind.Error, level, "fatal: Parse error occurred.", details, TimeSpan.Zero, userData, filePath, lineNumber, memberName);
                }
                else if (exception is System.ComponentModel.Win32Exception win32Exception)
                {
                    string details = win32Exception.ToString();
                    Write(TracerKind.Error, level, "fatal: Windows run-time error occurred.", details, TimeSpan.Zero, userData, filePath, lineNumber, memberName);
                }
                else if (exception is ApplicationException applicationException
                    && applicationException.InnerException is System.ComponentModel.Win32Exception winException)
                {
                    string details = Invariant($"{applicationException.Message}: {winException.ToString()}");
                    Write(TracerKind.Error, level, "fatal: Application error occurred.", details, TimeSpan.Zero, userData, filePath, lineNumber, memberName);
                }
                else
                {
                    Write(TracerKind.Error, level, "fatal: Error occurred.", exception.ToString(), TimeSpan.Zero, userData, filePath, lineNumber, memberName);
                }
            }
            return false;
        }

        public void TraceMessage(
            FormattableString synopsis,
            FormattableString details = null,
            TracerLevel level = TracerLevel.Detailed,
            object userData = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
            => Write(TracerKind.Message, level, synopsis, details, TimeSpan.Zero, userData, filePath, lineNumber, memberName);

        public void TraceMessage(
            string synopsis,
            string details = null,
            TracerLevel level = TracerLevel.Detailed,
            object userData = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
            => Write(TracerKind.Message, level, synopsis, details, TimeSpan.Zero, userData, filePath, lineNumber, memberName);


        public IDisposable TraceProcess(
            string synopsis,
            string details = null,
            TracerLevel level = TracerLevel.Diagnostic,
            object userData = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
            => StartTimer(TracerKind.ProcessPerformance, level, synopsis, details, userData, filePath, lineNumber, memberName);

        public void TraceWarning(
            FormattableString synopsis,
            FormattableString details = null,
            TracerLevel level = TracerLevel.Detailed,
            object userData = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
            => Write(TracerKind.Warning, level, synopsis, details, TimeSpan.Zero, userData, filePath, lineNumber, memberName);

        public void TraceWarning(
            string synopsis,
            string details = null,
            TracerLevel level = TracerLevel.Detailed,
            object userData = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
            => Write(TracerKind.Warning, level, synopsis, details, TimeSpan.Zero, userData, filePath, lineNumber, memberName);

        internal IDisposable StartTimer(
            TracerKind kind,
            TracerLevel level,
            FormattableString synopsis,
            FormattableString details,
            object userData,
            string filePath,
            int lineNumber,
            string memberName)
        {
            return new TracerTimer(kind, level, synopsis, details, userData, filePath, lineNumber, memberName, this);
        }

        internal IDisposable StartTimer(
            TracerKind kind,
            TracerLevel level,
            FormattableString synopsis,
            string details,
            object userData,
            string filePath,
            int lineNumber,
            string memberName)
        {
            return new TracerTimer(kind, level, synopsis, $"{details}", userData, filePath, lineNumber, memberName, this);
        }

        internal IDisposable StartTimer(
            TracerKind kind,
            TracerLevel level,
            string synopsis,
            string details,
            object userData,
            string filePath,
            int lineNumber,
            string memberName)
        {
            return new TracerTimer(kind, level, $"{synopsis}", $"{details}", userData, filePath, lineNumber, memberName, this);
        }

        internal void Write(TracerMessage message)
        {
            foreach (var listener in _listeners)
            {
                if ((listener.Kind & message.Kind) != 0 && listener.Level >= message.Level)
                {
                    listener.Write(message);
                }
            }
        }

        internal void Write(
            TracerKind kind,
            TracerLevel level,
            FormattableString synopsis,
            FormattableString details,
            TimeSpan duration,
            object userData,
            string filePath,
            int lineNumber,
            string memberName)
        {
            Debug.Assert((kind & ~TracerKind.Any) == 0, $"The `{nameof(kind)}` parameter is not defined.");
            Debug.Assert(Enum.IsDefined(typeof(TracerLevel), level), $"The `{nameof(level)}` parameter is not defined.");
            Debug.Assert(synopsis != null, $"The `{nameof(synopsis)}` parameter is null.");
            Debug.Assert(synopsis.Format != null, $"The `{nameof(synopsis)}.{nameof(FormattableString.Format)}` parameter is null");
            Debug.Assert(duration >= TimeSpan.Zero, $"The `{nameof(duration)}` parameter is invalid.");
            Debug.Assert(filePath != null, $"The `{nameof(filePath)}` parameter is null.");
            Debug.Assert(lineNumber > 0, $"The `{nameof(lineNumber)}` parameter is invalid.");
            Debug.Assert(memberName != null, $"The `{nameof(memberName)}` parameter is null.");

            // Skip doing work for untraced messages.
            if (kind == TracerKind.None)
                return;
            if (synopsis == null || string.IsNullOrWhiteSpace(synopsis.Format))
                return;

            lock (_syncpoint)
            {
                // Insure there's at least one valid listener before continuing
                if (level > _level || (_kinds & kind) == 0)
                    return;
            }

            string formattedMessage = Invariant(synopsis);
            string formattedDetails = (details is null)
                ? string.Empty
                : Invariant(details);
            string source = FormatSource(filePath, lineNumber);

            var tracerMessage = new TracerMessage(_context, duration, kind, level, formattedMessage, formattedDetails, userData, source, memberName);

            Write(tracerMessage);
        }

        internal void Write(
            TracerKind kind,
            TracerLevel level,
            FormattableString synopsis,
            string details,
            TimeSpan duration,
            object userData,
            string filePath,
            int lineNumber,
            string memberName)
        {
            Debug.Assert((kind & ~TracerKind.Any) == 0, $"The `{nameof(kind)}` parameter is not defined.");
            Debug.Assert(Enum.IsDefined(typeof(TracerLevel), level), $"The `{nameof(level)}` parameter is not defined.");
            Debug.Assert(synopsis != null, $"The `{nameof(synopsis)}` parameter is null.");
            Debug.Assert(synopsis.Format != null, $"The `{nameof(synopsis)}.{nameof(FormattableString.Format)}` parameter is null");
            Debug.Assert(duration >= TimeSpan.Zero, $"The `{nameof(duration)}` parameter is invalid.");
            Debug.Assert(filePath != null, $"The `{nameof(filePath)}` parameter is null.");
            Debug.Assert(lineNumber > 0, $"The `{nameof(lineNumber)}` parameter is invalid.");
            Debug.Assert(memberName != null, $"The `{nameof(memberName)}` parameter is null.");

            // Skip doing work for untraced messages.
            if (kind == TracerKind.None)
                return;
            if (synopsis == null || string.IsNullOrWhiteSpace(synopsis.Format))
                return;

            lock (_syncpoint)
            {
                // Insure there's at least one valid listener before continuing
                if (level > _level || (_kinds & kind) == 0)
                    return;
            }

            details = details ?? string.Empty;
            string formattedMessage = Invariant(synopsis);
            string source = FormatSource(filePath, lineNumber);

            var tracerMessage = new TracerMessage(_context, duration, kind, level, formattedMessage, details, userData, source, memberName);

            Write(tracerMessage);
        }

        internal void Write(
            TracerKind kind,
            TracerLevel level,
            string synopsis,
            string details,
            TimeSpan duration,
            object userData,
            string filePath,
            int lineNumber,
            string memberName)
        {
            Debug.Assert((kind & ~TracerKind.Any) == 0, $"The `{nameof(kind)}` parameter is not defined.");
            Debug.Assert(Enum.IsDefined(typeof(TracerLevel), level), $"The `{nameof(level)}` parameter is not defined.");
            Debug.Assert(synopsis != null, $"The `{synopsis}` parameter is null.");
            Debug.Assert(kind != TracerKind.None, $"The `{nameof(kind)}` parameter is `{nameof(TracerKind.None)}`.");
            Debug.Assert(duration >= TimeSpan.Zero, $"The `{nameof(duration)}` parameter is invalid.");
            Debug.Assert(filePath != null, $"The `{nameof(filePath)}` parameter is null.");
            Debug.Assert(lineNumber > 0, $"The `{nameof(lineNumber)}` parameter is invalid.");
            Debug.Assert(memberName != null, $"The `{nameof(memberName)}` parameter is null.");

            // Skip doing work for untraced messages.
            if (kind == TracerKind.None)
                return;
            if (string.IsNullOrWhiteSpace(synopsis))
                return;

            lock (_syncpoint)
            {
                // Insure there's at least one valid listener before continuing
                if (level > _level || (_kinds & kind) == 0)
                    return;
            }

            details = details ?? string.Empty;

            string source = FormatSource(filePath, lineNumber);

            var tracerMessage = new TracerMessage(_context, duration, kind, level, synopsis, details, userData, source, memberName);

            Write(tracerMessage);
        }

        [Conditional("DEBUG")]
        [Conditional("TRACE")]
        private void EnableDebugTrace()
        {
            if (Debugger.IsAttached && Debugger.IsLogging())
            {
                var dtl = new DebuggerTracerListener();
                AddListener(dtl);
            }
        }

        private static string FormatSource(string filePath, int lineNumber)
        {
            return FormattableString.Invariant($"{filePath}:{lineNumber:N0}");
        }

        private static void GetTracerLevelKinds(IEnumerable<ITracerListener> listeners, out TracerKind kinds, out TracerLevel level)
        {
            kinds = 0;
            level = 0;

            foreach (var listener in listeners)
            {
                kinds |= listener.Kind;
                level = (TracerLevel)Math.Max((uint)level, (uint)listener.Level);
            }
        }
    }
}
