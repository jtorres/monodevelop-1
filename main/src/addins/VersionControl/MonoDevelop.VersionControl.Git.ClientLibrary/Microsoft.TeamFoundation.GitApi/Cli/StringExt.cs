//*************************************************************************************************
// StatusCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    static class StringExt
    {
        public static bool StartsWith(this string str, string pattern, int index, int count)
        {
            if (count < pattern.Length)
                return false;
            if (index < 0 || index > str.Length)
                throw new ArgumentOutOfRangeException(nameof(index), $"index was {index} should be between 0 and {str.Length}.");
            if (count < 0 || index + count > str.Length)
                throw new ArgumentOutOfRangeException(nameof(count), $"count was {count} should be between 0 and {str.Length - index}.");
            return string.Compare(str, index, pattern, 0, pattern.Length, StringComparison.Ordinal) == 0;
        }
    }
}
