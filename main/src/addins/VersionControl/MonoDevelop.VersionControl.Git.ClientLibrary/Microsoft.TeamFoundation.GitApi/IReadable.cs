//*************************************************************************************************
// IReadable.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Represents a readable stream or buffer of data.
    /// </summary>
    public interface IReadable
    {
        /// <summary>
        /// Gets the size, in bytes, currently available to be read from the underlying storage.
        /// </summary>
        int Available { get; }

        /// <summary>
        /// Get `<see langword="true"/>` if the underlying storage has been closed by either the reader or the writer.
        /// <para/>
        /// Reads after the underlying storage may succeed for fail, depending on the underlying storage's implementation.
        /// </summary>
        bool Closed { get; }

        /// <summary>
        /// Gets the size, in bytes, currently contained in the underlying storage.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Close the underlying storage, signaling to the writer side that the read has abandoned and that the writer can terminate writing to the underlying storage.
        /// </summary>
        void Close();

        /// <summary>
        /// Reads bytes from the underlying storage.
        /// <para/>
        /// In the case the underlying storage is a pipe, this call will block the reader until bytes become available to read or the pipe closes.
        /// <para/>
        /// The number of bytes read into `<paramref name="buffer"/>`; the number of bytes read will less than or equal to `<paramref name="count"/>`.
        /// </summary>
        /// <param name="buffer">Allocation of bytes to read data into.</param>
        /// <param name="index">The offset into `<paramref name="buffer"/>` to begin read bytes into.</param>
        /// <param name="count">The number of bytes to read into `<paramref name="buffer"/>`.</param>
        int Read(byte[] buffer, int index, int count);
    }
}
