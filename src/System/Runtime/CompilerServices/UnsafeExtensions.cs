using System.Diagnostics;

namespace System.Runtime.CompilerServices
{
    public static class UnsafeExtensions
    {
        extension(Unsafe)
        {
#if !NET8_0_OR_GREATER
            /// <summary>
            /// Reinterprets the given value of type <typeparamref name="TFrom" /> as a value of type <typeparamref name="TTo" />.
            /// </summary>
            /// <exception cref="NotSupportedException">The sizes of <typeparamref name="TFrom" /> and <typeparamref name="TTo" /> are not the same
            /// or the type parameters are not <see langword="struct"/>s.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TTo BitCast<TFrom, TTo>(TFrom source)
                where TFrom : struct
                where TTo : struct
            {
                if (Unsafe.SizeOf<TFrom>() != Unsafe.SizeOf<TTo>())
                {
                    ThrowHelper.ThrowNotSupportedException();
                }

                TFrom v = source;
                return Unsafe.As<TFrom, TTo>(ref v);
            }
#endif

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static TTo Reinterpret<TFrom, TTo>(TFrom value)
            {
                Debug.Assert(Unsafe.SizeOf<TFrom>() == Unsafe.SizeOf<TTo>(),
                    "Reinterpret called with mismatched sizes.");
                return Unsafe.As<TFrom, TTo>(ref value);
            }
        }
    }
}
