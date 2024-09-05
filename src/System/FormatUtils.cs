﻿namespace System
{
    public static class FormatUtils
    {
        private const long _1KB = 1024;
        private const long _1MB = 1024 * _1KB;
        private const long _1GB = 1024 * _1MB;
        private const long _1TB = 1024 * _1GB;

        public static string FormatSize(IFormatProvider? provider, long size)
        {
            FormattableString message = size switch
            {
                < _1KB => $"{size} B",
                < _1MB => $"{(double)size / _1KB:N1} KB",
                < _1GB => $"{(double)size / _1MB:N1} MB",
                < _1TB => $"{(double)size / _1GB:N1} GB",
                _ => $"{(double)size / _1TB:N1} TB"
            };
            return message.ToString(provider);
        }
    }
}
