//*************************************************************************************************
// StringHelper.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal static class StringHelper
    {
        public const string KiloName = "KiB";
        public const string MegaName = "MiB";
        public const string GigaName = "GiB";
        public const string TeraName = "TiB";
        public const long Kilobytes = 1024;
        public const long MegaBytes = Kilobytes * 1024;
        public const long GigaBytes = MegaBytes * 1024;
        public const long TeraBytes = GigaBytes * 1024;

        public static readonly StringComparer MagnitudeComparer = StringComparer.Ordinal;

        public static long GetBytesFromMagnitude(double value, string magnitude)
        {
            if (Double.IsNaN(value) || Double.IsInfinity(value) || value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            long result = 0;

            if (MagnitudeComparer.Equals(magnitude, MegaName))
            {
                result = (long)(MegaBytes * value);
            }
            else if (MagnitudeComparer.Equals(magnitude, KiloName))
            {
                result = (long)(Kilobytes * value);
            }
            else if (MagnitudeComparer.Equals(GigaName, magnitude))
            {
                result = (long)(GigaBytes * value);
            }
            else if (MagnitudeComparer.Equals(TeraName, magnitude))
            {
                result = (long)(TeraBytes * value);
            }
            else
            {
                result = (long)value;
            }

            return result;
        }

        public static string GetMagnitudeText(long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            string result = null;
            double dvalue = double.NaN;

            if (value < Kilobytes)
            {
                result = $"{dvalue:N0} B";
            }
            else if (value < MegaBytes)
            {
                dvalue = (double)value / Kilobytes;
                result = $"{dvalue:N2} {KiloName}";
            }
            else if (value < GigaBytes)
            {
                dvalue = (double)value / MegaBytes;
                result = $"{dvalue:N2} {MegaName}";
            }
            else if (value < TeraBytes)
            {
                dvalue = (double)value / GigaBytes;
                result = result = $"{dvalue:N2} {GigaName}";
            }
            else
            {
                dvalue = (double)value / TeraBytes;
                result = result = $"{dvalue:N2} {TeraName}";
            }

            return result;
        }

        public static bool SubEquals(string left, int leftStart, string right, int rightStart, int count)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (right == null)
                throw new ArgumentNullException(nameof(right));
            if (leftStart < 0)
                throw new ArgumentOutOfRangeException(nameof(leftStart));
            if (rightStart < 0)
                throw new ArgumentOutOfRangeException(nameof(rightStart));

            if (leftStart + count > left.Length || rightStart + count > right.Length)
                return false;

            unsafe
            {
                fixed (char* l = left)
                fixed (char* r = right)
                {
                    char* a = l + leftStart;
                    char* b = r + rightStart;

                    for (int i = 0; i < count; i += 1)
                    {
                        if (a[i] != b[i])
                            return false;
                    }

                    return true;
                }
            }
        }
    }
}
