namespace System.Diagnostics.CodeAnalysis
{
#if NET_OLD

    /// <summary>Specifies that <see langword="null" /> is allowed as an input even if the corresponding type disallows it.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
    public sealed class AllowNullAttribute : Attribute
    {
    }

    /// <summary>Specifies that <see langword="null" /> is disallowed as an input even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
    public sealed class DisallowNullAttribute : Attribute
    {
    }

    /// <summary>Specifies that a method that will never return under any circumstance.</summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class DoesNotReturnAttribute : Attribute
    {
    }

    /// <summary>Specifies that an output is not <see langword="null" /> even if the corresponding type allows it. Specifies that an input argument was not <see langword="null" /> when the call returns.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = false)]
    public sealed class NotNullAttribute : Attribute
    {
    }

    /// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter will not be null even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class NotNullWhenAttribute : Attribute
    {
        /// <summary>Initializes the attribute with the specified return value condition.</summary>
        /// <param name="returnValue">
        /// The return value condition. If the method returns this value, the associated parameter will not be null.
        /// </param>
        public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

        /// <summary>Gets the return value condition.</summary>
        public bool ReturnValue { get; }
    }
#endif
}
