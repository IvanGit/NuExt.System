namespace System.Runtime.CompilerServices
{
    internal static class CompatRuntimeHelpers
    {
        extension(RuntimeHelpers)
        {
            /// <summary>
            /// Returns a value indicating whether the specified type can be compared by bitwise equality.
            /// This emulates the behavior of the BCL method RuntimeHelpers.IsBitwiseEquatable&lt;T&gt;() for older frameworks.
            /// </summary>
            /// <typeparam name="T">The type to inspect.</typeparam>
            /// <returns>
            /// <see langword="true"/> if <typeparamref name="T"/> is known to implement bitwise equality; otherwise, <see langword="false"/>.
            /// </returns>
            internal static bool IsKnownBitwiseEquatable<T>()
            {
                return TypeInfoCache<T>.IsKnownBitwiseEquatable;
            }

#if NETFRAMEWORK || NETSTANDARD2_0
            /// <summary>
            /// Returns a value indicating whether the specified type is a reference type
            /// or contains references. This emulates the behavior of the BCL method
            /// RuntimeHelpers.IsReferenceOrContainsReferences&lt;T&gt;() for older frameworks.
            /// </summary>
            /// <typeparam name="T">The type to inspect.</typeparam>
            /// <returns>
            /// <see langword="true"/> if <typeparamref name="T"/> is a reference type or contains
            /// any reference type fields; otherwise, <see langword="false"/>.
            /// </returns>
            public static bool IsReferenceOrContainsReferences<T>()
            {
                return TypeInfoCache<T>.IsReferenceOrContainsReferences;
            }
#endif
        }
    }
}