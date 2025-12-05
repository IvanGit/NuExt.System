// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text
{
    public ref partial struct ValueStringBuilder
    {
#if NET
        public void AppendFormattable<T>(T value, string? format = null, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            if (value.TryFormat(_chars.Slice(_pos), out int charsWritten, format, provider))
            {
                _pos += charsWritten;
            }
            else
            {
                Append(value.ToString(format, provider));
            }
        }
#else
        public void AppendFormattable<T>(T value, string? format = null, IFormatProvider? provider = null) where T : IFormattable
        {            
            Append(value.ToString(format, provider));
        }
#endif
    }
}
