//*************************************************************************************************
// ObjectId.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model value representing a Git object identifier (SHA-1).
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay, nq}")]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public unsafe struct ObjectId : IEquatable<byte[]>, IEquatable<ObjectId>, ILoggable, IRevision
    {
        /// <summary>
        /// The number of characters is a SHA-1 string.
        /// </summary>
        public const int Length = 40;

        /// <summary>
        /// The number of bytes in a SHA-1 value.
        /// </summary>
        public static readonly int Size = sizeof(ObjectId);

        /// <summary>
        /// Represents a fully zeroed SHA-1 value (all bytes set to 0).
        /// </summary>
        public static readonly ObjectId Zero = new ObjectId
        {
            _u0 = 0,
            _u1 = 0,
            _u2 = 0,
            _u3 = 0,
            _u4 = 0,
        };

        internal const int MaskNibble0 = 0x0000000F;
        internal const int MaskNibble1 = 0x000000F0;
        internal const int NibbleBits0 = 0;
        internal const int NibbleBits1 = 4;

        internal static readonly ObjectIdComparer Comparer = new ObjectIdComparer();

        /// <summary>
        /// Gets the string representation of this `<see cref="ObjectId"/>`.
        /// </summary>
        public string RevisionText
        {
            get { return ToString(); }
        }

        private uint _u0;
        private uint _u1;
        private uint _u2;
        private uint _u3;
        private uint _u4;

        private string DebuggerDisplay
        {
            get { return ObjectId.ToString(this); }
        }

        public override bool Equals(object obj)
        {
            if (obj is ObjectId)
            {
                ObjectId other = (ObjectId)obj;
                return this == other;
            }

            return false;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Equals(ObjectId other)
            => Comparer.Equals(this, other);

        public bool Equals(IRevision other)
            => Comparer.Equals(this, other);

        public bool Equals(string other)
            => Comparer.Equals(this, other);

        public static bool Equals(ObjectId objectId, byte[] bytes, int index)
        {
            if (ReferenceEquals(bytes, null))
                return false;
            if (index >= bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            fixed (byte* bptr = bytes)
            {
                byte* lptr = (byte*)&objectId;
                byte* rptr = bptr + index;
                int len = Math.Min(Size, bytes.Length);

                return Internal.Win32.Msvscrt.Memcmp(lptr, rptr, len) == 0;
            }
        }

        public static bool Equals(ObjectId objectId, byte[] bytes)
            => ObjectId.Equals(objectId, bytes, 0);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Equals(byte[] bytes)
            => ObjectId.Equals(this, bytes, 0);

        /// <summary>
        /// Returns a new `<see cref="ObjectId"/>` derived from `<paramref name="bytes"/>`.
        /// </summary>
        /// <param name="bytes">
        /// Bytes used to create the new `<see cref="ObjectId"/>` instance.
        /// <para/>
        /// The data of bytes is expected to be the 20-bytes SHA-1 value.
        /// </param>
        /// <param name="index">Offset into `<paramref name="bytes"/>` to begin reading.</param>
        public static ObjectId FromBytes(byte[] bytes, int index)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index + Size > bytes.Length)
                throw new ArgumentOutOfRangeException($"{nameof(index)} + sizeof({nameof(ObjectId)}) > {nameof(bytes)}");

            ObjectId objectId = new ObjectId { };

            fixed (byte* b = bytes)
            {
                byte* dst = (byte*)&objectId;
                byte* src = (b + index);

                for (int i = 0; i < Size; i += 1)
                {
                    dst[i] = src[i];
                }
            }

            return objectId;
        }

        /// <summary>
        /// Returns a new `<see cref="ObjectId"/>` derived from `<paramref name="bytes"/>`.
        /// </summary>
        public static ObjectId FromBytes(byte[] bytes)
            => FromBytes(bytes, 0);

        /// <summary>
        /// Returns a new `<see cref="ObjectId"/>` derived from `<paramref name="value"/>`.
        /// </summary>
        /// <param name="value">
        /// Text representation used to create the new `<see cref="ObjectId"/>` instance.
        /// <para/>
        /// The `<see cref="string.Length"/> - <paramref name="index"/>` must be equal to `<see cref="Length"/>.
        /// </param>
        /// <param name="index">Offset into `<paramref name="value"/>` to begin parsing.</param>
        public static ObjectId FromString(string value, int index)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (index < 0 || checked(ObjectId.Length + index) > value.Length)
                throw new ArgumentOutOfRangeException(nameof(index), $"index was {index} should be between 0 and {value.Length - ObjectId.Length}.");

            ObjectId objectId = new ObjectId { };

            try
            {
                if (value != null && value.Length - index >= Length)
                {
                    unchecked
                    {
                        unsafe
                        {
                            fixed (char* ptr = value)
                            {
                                char* src = ptr + index;
                                byte* dst = (byte*)&objectId;

                                for (int  j = 0; j < ObjectId.Length / 2; j ++)
                                {
                                    char ca = *src++;
                                    char cb = *src++;
                                    int ia = HexToDec(ca);
                                    ia <<= NibbleBits0;
                                    ia &= MaskNibble0;

                                    int ib = HexToDec(cb);
                                    ib <<= NibbleBits1;
                                    ib &= MaskNibble1;

                                    *dst++ = (byte)(ia | ib);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ObjectIdParseException(ex.Message, new StringUtf8(value), index);
            }

            return objectId;
        }

        /// <summary>
        /// Returns a new `<see cref="ObjectId"/>` derived from `<paramref name="value"/>`.
        /// </summary>
        /// <param name="value">
        /// Text representation used to create the new `<see cref="ObjectId"/>` instance.
        /// <para/>
        /// The `<see cref="string.Length"/>` must be equal to `<see cref="Length"/>.
        /// </param>
        public static ObjectId FromString(string value)
            => ObjectId.FromString(value, 0);

        /// <summary>
        /// Returns a new `<see cref="ObjectId"/>` derived from `<paramref name="bytes"/>`.
        /// </summary>
        /// <param name="bytes">
        /// UTF-8 text representation used to create the new `<see cref="ObjectId"/>` instance.
        /// <para/>
        /// The `<see cref="Array.Length"/> - <paramref name="index"/>` must be equal to `<see cref="Size"/>`.
        /// </param>
        /// <param name="index">Offset into `<paramref name="bytes"/>` to begin parsing.</param>
        public static ObjectId FromUtf8(byte[] bytes, int index)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (index < 0 || checked(ObjectId.Length + index) > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            ObjectId objectId = new ObjectId { };

            try
            {
                if (bytes != null && bytes.Length - index >= Length)
                {
                    unchecked
                    {
                        unsafe
                        {
                            fixed (byte* ptr = bytes)
                            {
                                byte* src = ptr + index;
                                ObjectId* dst = &objectId;
                                int len = sizeof(ObjectId);

                                for (int i = 0, j = 0; i < len; i += 1, j += 2)
                                {
                                    byte ca = src[j + 1];
                                    byte cb = src[j + 0];

                                    int ia = HexToDec((char)ca);
                                    ia <<= NibbleBits0;
                                    ia &= MaskNibble0;

                                    int ib = HexToDec((char)cb);
                                    ib <<= NibbleBits1;
                                    ib &= MaskNibble1;

                                    ((byte*)dst)[i] = (byte)(ia | ib);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ObjectIdParseException(ex.Message, new StringUtf8(bytes), index);
            }

            return objectId;
        }

        /// <summary>
        /// Returns a new `<see cref="ObjectId"/>` derived from `<paramref name="bytes"/>`.
        /// </summary>
        /// <param name="bytes">
        /// UTF-8 text representation used to create the new `<see cref="ObjectId"/>` instance.
        /// <para/>
        /// The `<see cref="Array.Length"/>` must be equal to `<see cref="Size"/>`.
        /// </param>
        public static ObjectId FromUtf8(byte[] bytes)
            => FromUtf8(bytes, 0);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return (int)_u2;
        }

        public static string ToString(ObjectId value)
        {
            unchecked
            {
                unsafe
                {
                    ObjectId* src = &value;
                    char* dst = stackalloc char[Length];
                    int len = sizeof(ObjectId);

                    for (int i = 0, j = 0; i < len; i += 1, j += 2)
                    {
                        int ia = ((byte*)src)[i];
                        ia &= MaskNibble0;
                        ia >>= NibbleBits0;

                        int ib = ((byte*)src)[i];
                        ib &= MaskNibble1;
                        ib >>= NibbleBits1;

                        char ca = DecToHex(ia);
                        dst[j + 1] = ca;

                        char cb = DecToHex(ib);
                        dst[j + 0] = cb;
                    }

                    return new string(dst, 0, Length);
                }
            }
        }

        public override string ToString()
            => ObjectId.ToString(this);

        private static char[] DecToHexLookup = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        internal static char DecToHex(int dec)
        {
            if (dec < 0 || dec > 0xF)
            {
                // This will be caught one level up where the full buffer being parsed
                // is available for a more meaningful exception
                throw new InvalidOperationException($"Unexpected hexadecimal value '{dec}'.");
            }

            return DecToHexLookup[dec];
        }

        private static sbyte[] HexToDecLookup =
        {
            -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
             0, 1, 2, 3, 4, 5, 6, 7, 8, 9,-1,-1,-1,-1,-1,-1,
            -1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
            -1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
        };

        internal static int HexToDec(char hex)
        {
            if (hex < 0 || hex > 0x7F)
            {
                // This will be caught one level up where the full buffer being parsed
                // is available for a more meaningful exception
                throw new InvalidOperationException($"Unexpected hexadecimal character '{hex}'.");
            }

            int dec = HexToDecLookup[(byte)hex];
            if (dec < 0)
            {
                // This will be caught one level up where the full buffer being parsed
                // is available for a more meaningful exception
                throw new InvalidOperationException($"Unexpected hexadecimal character '{hex}'.");
            }
            return dec;
        }

        void ILoggable.Log(ExecutionContext context, System.Text.StringBuilder log, int indent)
        {
            string prefix = context.ParseHelper.GetParseErrorIndent(indent);
            log.Append(prefix).Append("ObjectID: ").AppendLine(ToString());
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ObjectId left, ObjectId right)
        {
            return left._u0 == right._u0
                && left._u1 == right._u1
                && left._u2 == right._u2
                && left._u3 == right._u3
                && left._u4 == right._u4;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ObjectId left, ObjectId right)
            => !(left == right);
    }
}
