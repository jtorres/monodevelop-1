//*************************************************************************************************
// WritablePipe.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    /// <summary>
    /// A writable implementation of <see cref="AbstractPipe"/> which allows buffer writing to an
    /// underlying native pipe.
    /// </summary>
    internal sealed class WritablePipe : AbstractPipe
    {
        public WritablePipe(SafeFileHandle standardHandle)
            : base(standardHandle, FileAccess.Write)
        { }

        public WritablePipe(Stream baseStream)
            : base(baseStream, FileAccess.Write)
        { }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position
        /// within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/>
        /// bytes from <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            var channel = Volatile.Read(ref _channel);
            var stream = Volatile.Read(ref _stream);

            if (channel == null || stream == null)
                throw new ObjectDisposedException(typeof(WritablePipe).Name);

            int written = channel.WriteWait(buffer, offset, count);
            Debug.Assert(written == count, "Unexpected count written.");
        }

        protected sealed override void WorkerAction()
        {
            var channel = Volatile.Read(ref _channel);
            var stream = Volatile.Read(ref _stream);

            if (channel == null || stream == null)
                return;

            try
            {
                channel.ToStream(stream, true);
            }
            catch (Exception exception)
             when (exception is ObjectDisposedException)
            {
                /* squelch */
            }
        }
    }
}
