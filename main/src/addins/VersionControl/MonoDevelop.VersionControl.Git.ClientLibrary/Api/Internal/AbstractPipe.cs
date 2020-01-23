//*************************************************************************************************
// AbstractPipe.cs
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
    /// Abstraction for a native pipe with buffer caching and full compatibility with NetFx 
    /// <see cref="Stream"/> operations and consumers.
    /// </summary>
    internal abstract class AbstractPipe : Stream
    {
        protected AbstractPipe(SafeFileHandle standardHandle, FileAccess direction)
        {
            if (standardHandle == null)
                throw new ArgumentNullException(nameof(standardHandle));
            if (standardHandle.IsClosed)
                throw new ArgumentException($"{nameof(standardHandle)}.IsClosed");
            if (standardHandle.IsInvalid)
                throw new ArgumentException($"{nameof(standardHandle)}.IsInvalid");
            if (direction == FileAccess.ReadWrite)
                throw new ArgumentException($"{nameof(direction)} == FileAccess.ReadWrite");

            _channel = new CircularBuffer();
            _direction = direction;
            _stream = DetachedProcess.CreateStreamFromStandardHandle(standardHandle, _direction);
            _thread = CreateWorkerThread(_direction);
        }

        protected AbstractPipe(Stream baseStream, FileAccess direction)
        {
            if (baseStream == null)
                throw new ArgumentNullException(nameof(baseStream));
            if (direction == FileAccess.ReadWrite)
                throw new ArgumentException($"{nameof(direction)} == FileAccess.ReadWrite");
            if (direction == FileAccess.Read && !baseStream.CanRead)
                throw new ArgumentException($"{nameof(direction)} == FileAccess.Read && !{nameof(baseStream)}.CanRead");
            if (direction == FileAccess.Write && !baseStream.CanWrite)
                throw new ArgumentException($"{nameof(direction)} == FileAccess.Write && !{nameof(baseStream)}.CanWrite");

            _channel = new CircularBuffer();
            _direction = direction;
            _stream = baseStream;
            _thread = CreateWorkerThread(_direction);
        }

        /// <summary>
        /// Gets <see langword="true"/> if the pipe supports read operations; otherwise <see langword="false"/>.
        /// </summary>
        public sealed override bool CanRead
            => _direction == FileAccess.Read;

        /// <summary>
        /// Pipes do not support seeking, always <see langword="false"/>
        /// </summary>
        public sealed override bool CanSeek
            => false;

        /// <summary>
        /// Gets <see langword="true"/> if the pipe supports write operations; otherwise <see langword="false"/>.
        /// </summary>
        public sealed override bool CanWrite
            => _direction == FileAccess.Write;

        /// <summary>
        /// Gets the direction of the pipe. Either <see cref="FileAccess.Read"/> or 
        /// <see cref="FileAccess.Write"/>
        /// </summary>
        public FileAccess Direction
            => _direction;
        protected readonly FileAccess _direction;

        internal long TotalBytesRead
        {
            get
            {
                CircularBuffer channel = Volatile.Read(ref _channel);
                if (channel == null)
                    throw new ObjectDisposedException(typeof(AbstractLock).Name);

                return _channel.TotalBytesRead;
            }
        }

        internal long TotalBytesWritten
        {
            get
            {
                CircularBuffer channel = Volatile.Read(ref _channel);
                if (channel == null)
                    throw new ObjectDisposedException(typeof(AbstractLock).Name);

                return _channel.TotalBytesWritten;
            }
        }

        /// <summary>
        /// Pipes do now support Length querying, always throws <see cref="NotSupportedException"/>
        /// </summary>
        public sealed override long Length
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Pipes do now support position querying, always throws <see cref="NotSupportedException"/>
        /// </summary>
        public sealed override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        protected CircularBuffer _channel;
        protected Stream _stream;

        private Thread _thread;

        /// <summary>
        /// Releases all native resources.
        /// </summary>
        /// <param name="disposing">Some stupid value NetFx expects...</param>
        protected sealed override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                Stream stream = Interlocked.Exchange(ref _stream, null);
                CircularBuffer channel = Interlocked.Exchange(ref _channel, null);
                Thread thread = Interlocked.Exchange(ref _thread, null);

                if (thread != null)
                {
                    channel.Close();
                    if (!thread.Join(TimeSpan.FromSeconds(3)))
                    {
                        thread.Abort();
                    }

                    try { channel.Dispose(); } catch { /* squelch */ }
                    try { stream.Dispose(); } catch { /* squelch */ }
                }
            }
        }

        /// <summary>
        /// Flushes all data in the underlying native pipe
        /// </summary>
        public override void Flush()
        {
            Stream stream = Volatile.Read(ref _stream);
            if (stream != null)
            {
                stream.Flush();
            }
        }

        /// <summary>
        /// Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Pipes do now support seek operations, always throws <see cref="NotSupportedException"/>
        /// </summary>
        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Pipes do now support set operations, always throws <see cref="NotSupportedException"/>
        /// </summary>
        public sealed override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <para>Must be implemented by derived class.</para>
        /// <para>Expected to be a long running operation that continually copies bytes between the internal 
        /// <see cref="CircularBuffer"/> and <see cref="Stream"/> instances as long the stream exists.</para>
        /// </summary>
        protected abstract void WorkerAction();

        private Thread CreateWorkerThread(FileAccess direction)
        {
            string name = direction == FileAccess.Read
                ? $"{nameof(ReadablePipe)} Worker"
                : $"{nameof(WritablePipe)} Worker";

            _thread = new Thread(ThreadStart)
            {
                CurrentCulture = System.Globalization.CultureInfo.InvariantCulture,
                CurrentUICulture = System.Globalization.CultureInfo.InstalledUICulture,
                IsBackground = true,
                Name = name,
                Priority = ThreadPriority.Normal,
            };

            _thread.Start();

            return _thread;
        }

        private void ThreadStart()
        {
            try
            {
                WorkerAction();
            }
            finally
            {
                _channel?.Close();
                _stream?.Close();
            }
        }
    }
}
