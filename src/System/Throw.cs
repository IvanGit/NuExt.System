using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/*
 * https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis
 * https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/caller-argument-expression
 */

namespace System
{
    /// <summary>
    /// Provides utility methods to throw various exceptions based on specified conditions.
    /// </summary>
    [DebuggerStepThrough]
    public static class Throw
    {
        /// <summary>
        /// Throws an <see cref="ArgumentException"/> with a specified message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgumentException(string? message)
        {
            throw new ArgumentException(message);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> with a specified message if the given condition is true.
        /// </summary>
        /// <param name="condition">The condition that determines whether to throw the exception.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="conditionExpression">The expression of the condition, automatically captured by the compiler.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgumentExceptionIf(bool condition, string message, [CallerArgumentExpression(nameof(condition))] string? conditionExpression = null)
        {
            if (condition) throw new ArgumentException(message: message, paramName: conditionExpression);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the given argument is null.
        /// </summary>
        /// <param name="argument">The argument to check for null.</param>
        /// <param name="paramName">The name of the parameter that caused the exception.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            _ = argument ?? throw new ArgumentNullException(paramName);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the given string argument is null or an <see cref="ArgumentException"/> if it is empty.
        /// </summary>
        /// <param name="argument">The string argument to check.</param>
        /// <param name="paramName">The name of the parameter that caused the exception.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            _ = argument ?? throw new ArgumentNullException(paramName);
            if (0u >= (uint)argument.Length)
            {
                throw new ArgumentException("The value cannot be an empty string.", paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> with a specified message if the given condition is true.
        /// </summary>
        /// <param name="condition">The condition that determines whether to throw the exception.</param>
        /// <param name="message">The message that describes the error.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InvalidOperationExceptionIf(bool condition, string? message)
        {
            if (condition) throw new InvalidOperationException(message);
        }
    }
}
