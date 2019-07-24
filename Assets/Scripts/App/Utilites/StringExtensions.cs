using System;

namespace Loom.ZombieBattleground
{
    public static class StringExtensions
    {
        public static string SubstringIndexed(this string str, int startIndex, int endIndex)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), $"{nameof(startIndex)} must be larger than 0");

            if (endIndex > str.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), $"{nameof(endIndex)} can't exceed string length");

            if (endIndex < startIndex)
                throw new ArgumentOutOfRangeException(nameof(endIndex), $"{nameof(endIndex)} must be larger than or equal to {nameof(startIndex)}");

            return str.Substring(startIndex, endIndex - startIndex);
        }
    }
}
