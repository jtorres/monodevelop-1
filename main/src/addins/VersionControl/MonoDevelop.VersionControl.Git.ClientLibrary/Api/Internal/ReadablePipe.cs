//*************************************************************************************************
// ReadablePipe.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.IO;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    /// <summary>
    /// A readable implementation of <see cref="AbstractPipe"/> which allows buffer reading from 
    /// an underlying native pipe.
    /// </summary>
    internal sealed class ReadablePipe : AbstractPipe
    {
        public ReadablePipe(SafeFileHandle standardHandle)
            : base(standardHandle, FileAccess.Read)
        { }

        /// <summary>
        /// DESIGNED FOR TESTING OF <see cref="AbstractPipe"/> ONLY - DO NOT USE IN PROD CODE.
        /// </summary>
        internal ReadablePipe(Stream baseStream)
            : base(baseStream, FileAccess.Read)
        { }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the 
        /// stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains 
        /// the specified byte array with the values between <paramref name="offset"/> and 
        /// (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read 
        /// from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which 
        /// to begin storing the data read from the current stream.</param>
        /// <param name="count">he maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number 
        /// of bytes requested if that many bytes are not currently available, or zero (0) if the end 
        /// of the stream has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var channel = Volatile.Read(ref _channel);
            var stream = Volatile.Read(ref _stream);

            if (channel == null || stream == null)
                throw new ObjectDisposedException(typeof(ReadablePipe).Name);

            return channel.Read(buffer, offset, count);
        }

        protected sealed override void WorkerAction()
        {
            var channel = Volatile.Read(ref _channel);
            var stream = Volatile.Read(ref _stream);

            try
            {
                channel.FromStream(stream);
            }
            catch (Exception exception)
             when (exception is ObjectDisposedException)
            {
                /* squelch */
            }
        }
    }
}
