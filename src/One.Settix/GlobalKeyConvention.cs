using System;
using System.Collections.Generic;
using System.Text;

namespace One.Settix
{
    public static class GlobalKeyConvention
    {
        public const string Prefix = "settix:global:";

        public static string ToGlobalKey(this string key)
            => $"{Prefix}{key}";

        public static bool IsGlobalKey(this string key)
            => key.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);

        public static string StripGlobalPrefix(this string key)
            => key.Substring(Prefix.Length);
    }
}
