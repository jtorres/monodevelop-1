//*************************************************************************************************
// Msvcrt.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System.Runtime.InteropServices;

namespace Microsoft.TeamFoundation.GitApi.Internal.Win32
{
    internal static class Msvscrt
    {
        public const string Name = "msvcrt.dll";

        /// <summary>
        /// Compares the first <paramref name="num"/> bytes of the block of memory pointed by
        /// <paramref name="ptr1"/> to the first num bytes pointed by <paramref name="ptr2"/>,
        /// returning zero if they all match or a value different from zero representing which is
        /// greater if they do not.
        /// </summary>
        /// <param name="ptr1">Pointer to block of memory.</param>
        /// <param name="ptr2">Pointer to block of memory.</param>
        /// <param name="count">Number of bytes to compare.</param>
        /// <returns>Less than 0 when the first byte that does not match in both memory blocks has a
        /// lower value in <paramref name="ptr1"/> than in <paramref name="ptr2"/>; or greater than
        /// zero when the first byte that does not match in both memory blocks has a greater value in
        /// <paramref name="ptr1"/> than in <paramref name="ptr2"/>; otherwise zero.</returns>
        [DllImport(Name, BestFitMapping = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "memcmp", SetLastError = false)]
        public static unsafe extern int Memcmp(void* ptr1, void* ptr2, int num);
    }
}
