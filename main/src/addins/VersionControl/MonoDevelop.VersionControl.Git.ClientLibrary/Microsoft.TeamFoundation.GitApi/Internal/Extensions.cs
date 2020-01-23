//*************************************************************************************************
// Extension.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.IO;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal static class Extensions
    {
        public static int Compare(this byte[] left, int leftIndex, byte[] right, int rightIndex, int count)
        {
            if (ReferenceEquals(left, right))
                return 0;
            if (ReferenceEquals(left, null) || ReferenceEquals(null, right))
                return -1;
            if (left.Length - leftIndex < count || right.Length - rightIndex < count)
                return 1;

            unsafe
            {
                fixed (byte* lBytePtr = left)
                fixed (byte* rBytePtr = right)
                {
                    return Compare(lBytePtr, leftIndex, rBytePtr, rightIndex, count);
                }
            }
        }

        public static bool Equals(this byte[] left, int leftIndex, byte[] right, int rightIndex, int count)
        {
            if (ReferenceEquals(left, right) && leftIndex == rightIndex)
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(null, right))
                return false;
            if (left.Length - leftIndex < count || right.Length - rightIndex < count)
                return false;

            unsafe
            {
                fixed (byte* lBytePtr = left)
                fixed (byte* rBytePtr = right)
                {
                    return Equals(lBytePtr, leftIndex, rBytePtr, rightIndex, count);
                }
            }
        }

        public static int FirstIndexOf(this byte[] bytes, int index, int count, byte value)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(index + count) > bytes.Length)
                throw new IndexOutOfRangeException(nameof(count));

            unsafe
            {
                fixed (byte* b = bytes)
                {
                    int end = index + count;

                    for (int i = index; i < end; i += 1)
                    {
                        if (b[i] == value)
                            return i;
                    }
                }
            }

            return -1;
        }
        public static int FirstIndexOf(this byte[] bytes, int index, byte value)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return FirstIndexOf(bytes, index, bytes.Length, value);
        }
        public static int FirstIndexOf(this byte[] bytes, byte value)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return FirstIndexOf(bytes, 0, bytes.Length, value);
        }

        public static int GetMurmur3(this byte[] bytes, int index, int count)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index + count > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            uint hash = 0;

            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    Murmur3(ptr + index, count, ref hash);
                }
            }

            return unchecked((int)hash);
        }

        public static bool IsEscapedCorrectly(this string value, char escapeCharacter, char dangerousCharacter)
        {
            int len = value.Length;

            unsafe
            {
                fixed (char* p = value)
                {
                    for (int i = 0; i < len; i += 1)
                    {
                        if (p[i] == dangerousCharacter)
                        {
                            if (i == 0)
                                return false;
                            if (p[i - 1] != escapeCharacter)
                                return false;

                            int count = 0;
                            for (int j = 0; j < i; j += 1)
                            {
                                if (p[i - j] != escapeCharacter)
                                {
                                    count += 1;
                                }
                            }

                            if ((count & 1) == 0)
                                return false;
                        }
                    }
                }
            }

            return true;
        }
        public static bool IsEscapedCorrectly(this string value, char escapeCharacter, char dangerousCharacter1, char dangerousCharacter2)
        {
            if (!IsEscapedCorrectly(value, escapeCharacter, dangerousCharacter1))
                return false;
            if (!IsEscapedCorrectly(value, escapeCharacter, dangerousCharacter2))
                return false;

            return true;
        }

        public static int LastIndexOf(this byte[] bytes, int index, int count, byte value)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(index + count) > bytes.Length)
                throw new IndexOutOfRangeException(nameof(count));

            unsafe
            {
                fixed (byte* b = bytes)
                {
                    int end = index + count;

                    for (int i = end - 1; i >= index; i -= 1)
                    {
                        if (b[i] == value)
                            return i;
                    }
                }
            }

            return -1;
        }
        public static int LastIndexOf(this byte[] bytes, int count, byte value)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return LastIndexOf(bytes, bytes.Length - count, count, value);
        }
        public static int LastIndexOf(this byte[] bytes, byte value)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return LastIndexOf(bytes, 0, bytes.Length, value);
        }

        public static StringUtf8 ReadToEndUtf8(this Stream stream)
        {
            // Use a 64 KiB buffer because allocations >= 80 KiB are placed on the Large Object Heap [LOH]
            // and the LOH requires GC pause the application to perform collection, therefore avoiding any
            // allocations on LOH is idea. As for why "64", it is the largest power-of-two less than 80.
            const int DefaultCapacity = 64 * 1024;

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
                throw new ArgumentException(nameof(stream));

            using (var memory = new MemoryStream(DefaultCapacity))
            {
                stream.CopyTo(memory);

                return new StringUtf8(memory.ToArray());
            }
        }

        public static uint RotateLeft32(this uint value, int count)
        {
            unchecked
            {
                int mask = 8 * sizeof(uint) - 1;
                count &= mask;
                value = ((value << count) | (value >> ((-count) & mask)));
            }

            return value;
        }

        public static uint RotateRight32(this uint value, int count)
        {
            unchecked
            {
                int mask = 8 * sizeof(uint) - 1;
                count &= mask;
                value = ((value >> count) | (value << ((-count) & mask)));
            }

            return value;
        }

        public static bool TryParse(this byte[] bytes, int index, int count, out int result)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (index < 0 || index >= bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index + count > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            result = default(int);

            if (bytes.Length == 0 || count == 0)
                return false;

            bool negative = bytes[index] == '-';
            int start = index + ((negative) ? 1 : 0);
            int end = index + count;

            for (int i = start; i < end; i += 1)
            {
                if (bytes[i] > '9' || bytes[i] < '0')
                    return false;

                result *= 10;
                result += (int)(bytes[i] - '0');
            }

            if (negative)
            {
                result = -result;
            }

            return true;
        }

        public static bool TryParse(this byte[] bytes, int index, int count, out long result)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (index < 0 || index >= bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index + count > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            result = default(long);

            if (bytes.Length == 0 || count == 0)
                return false;

            bool negative = bytes[index] == '-';
            int start = index + ((negative) ? 1 : 0);
            int end = index + count;

            for (int i = start; i < end; i += 1)
            {
                if (bytes[i] > '9' || bytes[i] < '0')
                    return false;

                result *= 10;
                result += (int)(bytes[i] - '0');
            }

            if (negative)
            {
                result = -result;
            }

            return true;
        }

        public static bool TryParse(this byte[] bytes, int index, int count, out uint result)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (index < 0 || index >= bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index + count > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            result = default(uint);

            if (bytes.Length == 0 || count == 0)
                return false;

            int start = index;
            int end = index + count;

            for (int i = start; i < end; i += 1)
            {
                if (bytes[i] > '9' || bytes[i] < '0')
                    return false;

                result *= 10;
                result += (uint)(bytes[i] - '0');
            }

            return true;
        }

        public static bool TryParse(this byte[] bytes, int index, int count, out ulong result)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (index < 0 || index >= bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index + count > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            result = default(ulong);

            if (bytes.Length == 0 || count == 0)
                return false;

            int start = index;
            int end = index + count;

            for (int i = start; i < end; i += 1)
            {
                if (bytes[i] > '9' || bytes[i] < '0')
                    return false;

                result *= 10;
                result += (ulong)(bytes[i] - '0');
            }

            return true;
        }

        public static bool TryParse (this string bytes, int index, int count, out int result)
        {
            if (bytes == null)
                throw new ArgumentNullException (nameof (bytes));
            if (index < 0 || index >= bytes.Length)
                throw new ArgumentOutOfRangeException (nameof (index));
            if (count < 0 || index + count > bytes.Length)
                throw new ArgumentOutOfRangeException (nameof (index));

            result = default (int);

            if (bytes.Length == 0 || count == 0)
                return false;

            bool negative = bytes[index] == '-';
            int start = index + ((negative) ? 1 : 0);
            int end = index + count;

            for (int i = start; i < end; i += 1) {
                if (bytes[i] > '9' || bytes[i] < '0')
                    return false;

                result *= 10;
                result += (int)(bytes[i] - '0');
            }

            if (negative) {
                result = -result;
            }

            return true;
        }

        public static bool TryParse (this string bytes, int index, int count, out long result)
        {
            if (bytes == null)
                throw new ArgumentNullException (nameof (bytes));
            if (index < 0 || index >= bytes.Length)
                throw new ArgumentOutOfRangeException (nameof (index));
            if (count < 0 || index + count > bytes.Length)
                throw new ArgumentOutOfRangeException (nameof (index));

            result = default (long);

            if (bytes.Length == 0 || count == 0)
                return false;

            bool negative = bytes[index] == '-';
            int start = index + ((negative) ? 1 : 0);
            int end = index + count;

            for (int i = start; i < end; i += 1) {
                if (bytes[i] > '9' || bytes[i] < '0')
                    return false;

                result *= 10;
                result += (int)(bytes[i] - '0');
            }

            if (negative) {
                result = -result;
            }

            return true;
        }

        public static bool TryParse (this string bytes, int index, int count, out uint result)
        {
            if (bytes == null)
                throw new ArgumentNullException (nameof (bytes));
            if (index < 0 || index >= bytes.Length)
                throw new ArgumentOutOfRangeException (nameof (index));
            if (count < 0 || index + count > bytes.Length)
                throw new ArgumentOutOfRangeException (nameof (index));

            result = default (uint);

            if (bytes.Length == 0 || count == 0)
                return false;

            int start = index;
            int end = index + count;

            for (int i = start; i < end; i += 1) {
                if (bytes[i] > '9' || bytes[i] < '0')
                    return false;

                result *= 10;
                result += (uint)(bytes[i] - '0');
            }

            return true;
        }

        public static bool TryParse (this string bytes, int index, int count, out ulong result)
        {
            if (bytes == null)
                throw new ArgumentNullException (nameof (bytes));
            if (index < 0 || index >= bytes.Length)
                throw new ArgumentOutOfRangeException (nameof (index));
            if (count < 0 || index + count > bytes.Length)
                throw new ArgumentOutOfRangeException (nameof (index));

            result = default (ulong);

            if (bytes.Length == 0 || count == 0)
                return false;

            int start = index;
            int end = index + count;

            for (int i = start; i < end; i += 1) {
                if (bytes[i] > '9' || bytes[i] < '0')
                    return false;

                result *= 10;
                result += (ulong)(bytes[i] - '0');
            }

            return true;
        }


        internal static unsafe int Compare(byte* leftBytes, int leftIndex, byte* rightBytes, int rightIndex, int count)
        {
            byte* a = leftBytes + leftIndex;
            byte* b = rightBytes + rightIndex;

            return Win32.Msvscrt.Memcmp(a, b, count);
        }

        internal static unsafe bool Equals(byte* leftBytes, int leftIndex, byte* rightBytes, int rightIndex, int count)
        {
            byte* a = leftBytes + leftIndex;
            byte* b = rightBytes + rightIndex;

            return Win32.Msvscrt.Memcmp(a, b, count) == 0;
        }

        internal static unsafe void Murmur3(byte* bytes, int count, ref uint hash)
        {
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;
            const int r1 = 15;
            const int r2 = 13;
            const uint m = 5;
            const uint n = 0xe6546b64;
            unchecked
            {
                int len4 = count / sizeof(uint);

                byte* key = bytes;
                uint* blocks = (uint*)key;

                uint k;
                for (int i = 0; i < len4; i += 1)
                {
                    k = blocks[i];
                    k *= c1;
                    k = k.RotateRight32(r1);
                    k *= c2;

                    hash ^= k;
                    hash = hash.RotateRight32(r2) * m + n;
                }

                byte* tail = (key + len4 * sizeof(uint));
                uint k1 = 0;

                switch (count & (sizeof(uint) - 1))
                {
                    case 3:
                        k1 ^= ((uint)tail[2]) << 16;
                        goto case 2;

                    case 2:
                        k1 ^= ((uint)tail[1]) << 8;
                        goto case 1;

                    case 1:
                        k1 ^= ((uint)tail[0]) << 0;

                        k1 *= c1;
                        k1 = k1.RotateRight32(r1);
                        k1 *= c2;
                        hash ^= k1;
                        break;
                }

                hash ^= (uint)count;
                hash ^= (hash >> 16);
                hash *= 0x85ebca6b;
                hash ^= (hash >> 13);
                hash *= 0xc2b2ae35;
                hash ^= (hash >> 16);
            }
        }
    }
}
