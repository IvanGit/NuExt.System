namespace System.Diagnostics.CodeAnalysis
{
#if NET_OLD

    /// <summary>Specifies that null is allowed as an input even if the corresponding type disallows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    public sealed class AllowNullAttribute : Attribute { }

    /// <summary>Specifies that null is disallowed as an input even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    public sealed class DisallowNullAttribute : Attribute { }

    /// <summary>Applied to a method that will never return under any circumstance.</summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class DoesNotReturnAttribute : Attribute { }

    /// <summary>Specifies that the method will not return if the associated Boolean parameter is passed the specified value.</summary>
    /// <remarks>Initializes the attribute with the specified parameter value.</remarks>
    /// <param name="parameterValue">
    /// The condition parameter value. Code after the method will be considered unreachable by diagnostics if the argument to
    /// the associated parameter matches this value.
    /// </param>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class DoesNotReturnIfAttribute(bool parameterValue) : Attribute
    {
        /// <summary>Gets the condition parameter value.</summary>
        public bool ParameterValue { get; } = parameterValue;
    }


    /// <summary>Specifies that an output will not be null even if the corresponding type allows it. Specifies that an input argument was not null when the call returns.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    public sealed class NotNullAttribute : Attribute { }

    /// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter will not be null even if the corresponding type allows it.</summary>
    /// <remarks>Initializes the attribute with the specified return value condition.</remarks>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the associated parameter will not be null.
    /// </param>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class NotNullWhenAttribute(bool returnValue) : Attribute
    {

        /// <summary>Gets the return value condition.</summary>
        public bool ReturnValue { get; } = returnValue;
    }
#endif
}
