using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    partial class CompatMemoryExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsOrdinal(this ReadOnlySpan<char> span, ReadOnlySpan<char> value)
        {
            if (span.Length != value.Length)
                return false;
            if (value.Length == 0)  // span.Length == value.Length == 0
                return true;
            return span.SequenceEqual(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsOrdinalIgnoreCase(this ReadOnlySpan<char> span, ReadOnlySpan<char> value)
        {
            if (span.Length != value.Length)
                return false;
            if (value.Length == 0)  // span.Length == value.Length == 0
                return true;
            return string.Equals(span.ToString(), value.ToString(), StringComparison.OrdinalIgnoreCase);// Slow path
        }

#if !NET5_0_OR_GREATER
        /// <summary>
        /// Reports the zero-based index of the last occurrence of the specified <paramref name="value"/> in the current <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The value to seek within the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        public static int LastIndexOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            if (comparisonType == StringComparison.Ordinal)
            {
                return SpanHelpers.LastIndexOf(
                    ref MemoryMarshal.GetReference(span),
                    span.Length,
                    ref MemoryMarshal.GetReference(value),
                    value.Length);
            }

            return span.ToString().LastIndexOf(value.ToString(), comparisonType);// Slow path
        }
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EndsWithOrdinalIgnoreCase(this ReadOnlySpan<char> span, ReadOnlySpan<char> value)
            => value.Length <= span.Length
               && string.Equals(span.Slice(span.Length - value.Length).ToString(),
                   value.ToString(), StringComparison.OrdinalIgnoreCase);// Slow path

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool StartsWithOrdinalIgnoreCase(this ReadOnlySpan<char> span, ReadOnlySpan<char> value)
            => value.Length <= span.Length
               && string.Equals(span.Slice(0, value.Length).ToString(), value.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
