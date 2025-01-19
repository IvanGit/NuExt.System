using System.ComponentModel;

namespace System.Runtime.CompilerServices;

#if !NET

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class CallerArgumentExpressionAttribute(string parameterName) : Attribute
{
    public string ParameterName { get; } = parameterName;
}

/// <summary>Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IsExternalInit
{
}

#endif

#if !NET7_0_OR_GREATER

/// <summary>Specifies that a type has required members or that a member is required.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property,
    AllowMultiple = false, Inherited = false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class RequiredMemberAttribute : Attribute
{
}

/// <summary>
/// Indicates that compiler support for a particular feature is required for the location where this attribute is applied.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
public sealed class CompilerFeatureRequiredAttribute(string featureName) : Attribute
{

    /// <summary>
    /// The name of the compiler feature.
    /// </summary>
    public string FeatureName { get; } = featureName;

    /// <summary>
    /// If true, the compiler can choose to allow access to the location where this attribute is applied if it does not understand <see cref="FeatureName"/>.
    /// </summary>
    public bool IsOptional { get; init; }

    /// <summary>
    /// The <see cref="FeatureName"/> used for the ref structs C# feature.
    /// </summary>
    public const string RefStructs = nameof(RefStructs);

    /// <summary>
    /// The <see cref="FeatureName"/> used for the required members C# feature.
    /// </summary>
    public const string RequiredMembers = nameof(RequiredMembers);
}

#endif

#if !NET9_0_OR_GREATER

/// <summary>
/// Specifies the priority of a member in overload resolution. When unspecified, the default priority is 0.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OverloadResolutionPriorityAttribute"/> class.
/// </remarks>
/// <param name="priority">The priority of the attributed member. Higher numbers are prioritized, lower numbers are deprioritized. 0 is the default if no attribute is present.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class OverloadResolutionPriorityAttribute(int priority) : Attribute
{

    /// <summary>
    /// The priority of the member.
    /// </summary>
    public int Priority { get; } = priority;
}

#endif