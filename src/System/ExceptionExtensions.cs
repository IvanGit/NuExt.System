using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System
{
    public static class ExceptionExtensions
    {
        extension(ArgumentException)
        {
#if !NET7_0_OR_GREATER
#pragma warning disable CS8777 //Parameter must have a non-null value when exiting.
            /// <summary>Throws an exception if <paramref name="argument"/> is null or empty.</summary>
            /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
            /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
            /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
            /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
            public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
            {
                if (string.IsNullOrEmpty(argument))
                {
                    ThrowNullOrEmptyException(argument, paramName);
                }
            }
#pragma warning restore CS8777 //Parameter must have a non-null value when exiting.
#endif

#if !NET8_0_OR_GREATER
#pragma warning disable CS8777 //Parameter must have a non-null value when exiting.
            /// <summary>Throws an exception if <paramref name="argument"/> is null, empty, or consists only of white-space characters.</summary>
            /// <param name="argument">The string argument to validate.</param>
            /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
            /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
            /// <exception cref="ArgumentException"><paramref name="argument"/> is empty or consists only of white-space characters.</exception>
            public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
            {
                if (string.IsNullOrWhiteSpace(argument))
                {
                    ThrowNullOrWhiteSpaceException(argument, paramName);
                }
            }
#pragma warning restore CS8777 //Parameter must have a non-null value when exiting.
#endif

            [DoesNotReturn]
            private static void ThrowNullOrEmptyException(string? argument, string? paramName)
            {
                ArgumentNullException.ThrowIfNull(argument, paramName);
                throw new ArgumentException(SR.Argument_EmptyString, paramName);
            }

            [DoesNotReturn]
            private static void ThrowNullOrWhiteSpaceException(string? argument, string? paramName)
            {
                ArgumentNullException.ThrowIfNull(argument, paramName);
                throw new ArgumentException(SR.Argument_EmptyOrWhiteSpaceString, paramName);
            }
        }

        extension(ArgumentNullException)
        {
#if !NET6_0_OR_GREATER
            /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
            /// <param name="argument">The reference type argument to validate as non-null.</param>
            /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
            public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
            {
                if (argument is null)
                {
                    Throw(paramName);
                }
            }
#endif

#if !NET7_0_OR_GREATER
            /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
            /// <param name="argument">The pointer argument to validate as non-null.</param>
            /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
            public static unsafe void ThrowIfNull([NotNull] void* argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
            {
                if (argument is null)
                {
                    Throw(paramName);
                }
            }
#endif
            /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
            /// <param name="argument">The pointer argument to validate as non-null.</param>
            /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
            public static void ThrowIfNull(IntPtr argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
            {
                if (argument == IntPtr.Zero)
                {
                    Throw(paramName);
                }
            }

            [DoesNotReturn]
            internal static void Throw(string? paramName) =>
                throw new ArgumentNullException(paramName);
        }


#if !NET7_0_OR_GREATER
        extension(ObjectDisposedException)
        {
            /// <summary>Throws an <see cref="ObjectDisposedException"/> if the specified <paramref name="condition"/> is <see langword="true"/>.</summary>
            /// <param name="condition">The condition to evaluate.</param>
            /// <param name="instance">The object whose type's full name should be included in any resulting <see cref="ObjectDisposedException"/>.</param>
            /// <exception cref="ObjectDisposedException">The <paramref name="condition"/> is <see langword="true"/>.</exception>
            [StackTraceHidden]
            public static void ThrowIf([DoesNotReturnIf(true)] bool condition, object instance)
            {
                if (condition)
                {
                    ThrowHelper.ThrowObjectDisposedException(instance);
                }
            }

            /// <summary>Throws an <see cref="ObjectDisposedException"/> if the specified <paramref name="condition"/> is <see langword="true"/>.</summary>
            /// <param name="condition">The condition to evaluate.</param>
            /// <param name="type">The type whose full name should be included in any resulting <see cref="ObjectDisposedException"/>.</param>
            /// <exception cref="ObjectDisposedException">The <paramref name="condition"/> is <see langword="true"/>.</exception>
            [StackTraceHidden]
            public static void ThrowIf([DoesNotReturnIf(true)] bool condition, Type type)
            {
                if (condition)
                {
                    ThrowHelper.ThrowObjectDisposedException(type);
                }
            }
        }
#endif
    }
}
