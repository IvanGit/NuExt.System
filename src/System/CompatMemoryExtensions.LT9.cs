// Notice: includes code derived from the .NET Runtime, licensed under the MIT License.
#if !NET9_0_OR_GREATER

using System.Runtime.CompilerServices;

namespace System
{
    partial class CompatMemoryExtensions
    {
        /// <summary>
        /// Determines whether the specified value appears at the start of the span.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to compare.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>? =>
            span.Length != 0 && (span[0]?.Equals(value) ?? (object?)value is null);

        /// <summary>
        /// Determines whether the specified value appears at the end of the span.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to compare.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>? =>
#if NETFRAMEWORK || NETSTANDARD2_0
            span.Length != 0 && (span[span.Length - 1]?.Equals(value) ?? (object?)value is null);
#else
            span.Length != 0 && (span[^1]?.Equals(value) ?? (object?)value is null);
#endif
    }
}

#endif