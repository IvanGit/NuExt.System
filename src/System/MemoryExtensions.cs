using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// Extension methods for Span{T}, Memory{T}, and friends.
    /// </summary>
    [SuppressMessage("Style", "IDE0057", Justification = "<Pending>")]
    public static class MemoryExtensions
    {
#if !NET9_0_OR_GREATER
        /// <summary>
        /// Determines whether the specified value appears at the start of the span.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to compare.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith<T>(this scoped ReadOnlySpan<T> span, T value) where T : IEquatable<T>? =>
            span.Length != 0 && (span[0]?.Equals(value) ?? (object?)value is null);

        /// <summary>
        /// Determines whether the specified value appears at the end of the span.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to compare.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith<T>(this scoped ReadOnlySpan<T> span, T value) where T : IEquatable<T>? =>
#if NET_OLD
            span.Length != 0 && (span[span.Length - 1]?.Equals(value) ?? (object?)value is null);
#else
            span.Length != 0 && (span[^1]?.Equals(value) ?? (object?)value is null);
#endif
#endif


#if !NET
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this scoped ReadOnlySpan<char> span, char value)
        {
            return IndexOf(span, value) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(this scoped ReadOnlySpan<char> span, char value)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == value)
                    return i;
            }
            return -1;
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsOrdinal(this scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> value)
        {
            if (span.Length != value.Length)
                return false;
            if (value.Length == 0)  // span.Length == value.Length == 0
                return true;
            return span.SequenceEqual(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsOrdinalIgnoreCase(this scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> value)
        {
            if (span.Length != value.Length)
                return false;
            if (value.Length == 0)  // span.Length == value.Length == 0
                return true;
            return string.Equals(span.ToString(), value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWithOrdinalIgnoreCase(this scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> value)
            => value.Length <= span.Length
               && string.Equals(span.Slice(span.Length - value.Length).ToString(),
                   value.ToString(), StringComparison.OrdinalIgnoreCase);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithOrdinalIgnoreCase(this scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> value)
            => value.Length <= span.Length
               && string.Equals(span.Slice(0, value.Length).ToString(), value.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
