using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    /// <summary>
    /// Extension methods for Span{T}, Memory{T}, and friends.
    /// </summary>
    /// <remarks>
    ///
    /// Notice: includes code derived from the .NET Runtime, licensed under the MIT License.
    /// See LICENSE file in the project root for full license information.
    /// Original source code can be found at https://github.com/dotnet/runtime.
    /// </remarks>
    [SuppressMessage("Style", "IDE0057", Justification = "<Pending>")]
    public static partial class CompatMemoryExtensions
    {
#if !NET5_0_OR_GREATER
        /// <inheritdoc cref="Contains{T}(ReadOnlySpan{T}, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [OverloadResolutionPriority(-1)]
        public static bool Contains<T>(this Span<T> span, T value) where T : IEquatable<T>? =>
            ((ReadOnlySpan<T>)span).Contains(value);

        /// <summary>
        /// Searches for the specified value and returns true if found. If not found, returns false. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>?
        {
            if (span.Length == 0)
                return false;

            if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
            {
                int size = Unsafe.SizeOf<T>();
                if (size == sizeof(byte))
                {
                    return SpanHelpers.ContainsValueType(
                        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, byte>(value),
                        span.Length);
                }
                else if (size == sizeof(short))
                {
                    return SpanHelpers.ContainsValueType(
                        ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, short>(value),
                        span.Length);
                }
                else if (size == sizeof(int))
                {
                    return SpanHelpers.ContainsValueType(
                        ref Unsafe.As<T, int>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, int>(value),
                        span.Length);
                }
                else if (size == sizeof(long))
                {
                    return SpanHelpers.ContainsValueType(
                        ref Unsafe.As<T, long>(ref MemoryMarshal.GetReference(span)),
                        Unsafe.Reinterpret<T, long>(value),
                        span.Length);
                }
            }

            return SpanHelpers.Contains(ref MemoryMarshal.GetReference(span), value, span.Length);
        }
#endif

#if !NET6_0_OR_GREATER
        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements using an <see cref="IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="span">The first sequence to compare.</param>
        /// <param name="other">The second sequence to compare.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>true if the two sequences are equal; otherwise, false.</returns>
        [OverloadResolutionPriority(-1)]
        public static bool SequenceEqual<T>(this Span<T> span, ReadOnlySpan<T> other, IEqualityComparer<T>? comparer = null) =>
            ((ReadOnlySpan<T>)span).SequenceEqual(other, comparer);

        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements using an <see cref="IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="span">The first sequence to compare.</param>
        /// <param name="other">The second sequence to compare.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
        /// <returns>true if the two sequences are equal; otherwise, false.</returns>
        public static bool SequenceEqual<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other, IEqualityComparer<T>? comparer = null)
        {
            // If the spans differ in length, they're not equal.
            if (span.Length != other.Length)
            {
                return false;
            }

            if (typeof(T).IsValueType)
            {
                if (comparer is null || comparer == EqualityComparer<T>.Default)
                {
                    // If no comparer was supplied and the type is bitwise equatable, take the fast path doing a bitwise comparison.
                    if (RuntimeHelpers.IsKnownBitwiseEquatable<T>())
                    {
                        int size = Unsafe.SizeOf<T>();
                        return SpanHelpers.SequenceEqual(
                            ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
                            ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)),
                            ((uint)span.Length) * (nuint)size);  // If this multiplication overflows, the Span we got overflows the entire address range. There's no happy outcome for this API in such a case so we choose not to take the overhead of checking.
                    }

                    // Otherwise, compare each element using EqualityComparer<T>.Default.Equals in a way that will enable it to devirtualize.
                    for (int i = 0; i < span.Length; i++)
                    {
                        if (!EqualityComparer<T>.Default.Equals(span[i], other[i]))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            // Use the comparer to compare each element.
            comparer ??= EqualityComparer<T>.Default;
            for (int i = 0; i < span.Length; i++)
            {
                if (!comparer.Equals(span[i], other[i]))
                {
                    return false;
                }
            }

            return true;
        }

#endif
    }
}
