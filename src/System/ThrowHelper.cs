using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/*
 * https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis
 * https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/caller-argument-expression
 */

namespace System
{
    [DebuggerStepThrough]
    public static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowArgumentException(string? message)
        {
            throw new ArgumentException(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowArgumentExceptionIf(bool condition, string message, [CallerArgumentExpression(nameof(condition))] string? conditionExpression = null)
        {
            if (condition) throw new ArgumentException(message: message, paramName: conditionExpression);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowInvalidOperationExceptionIf(bool condition, string? message)
        {
            if (condition) throw new InvalidOperationException(message);
        }

#if NETFRAMEWORK || NETSTANDARD2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WhenNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            _ = argument ?? throw new ArgumentNullException(paramName);
        }
#endif

#if NETFRAMEWORK || NET6_0 || NETSTANDARD2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WhenNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            _ = argument ?? throw new ArgumentNullException(paramName);
            if (0u >= (uint)argument.Length)
            {
                throw new ArgumentException("The value cannot be an empty string.", paramName);
            }
        }
#endif
    }
}
