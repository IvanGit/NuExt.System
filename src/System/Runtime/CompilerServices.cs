using System.ComponentModel;

namespace System.Runtime.CompilerServices;

#if NET462 || NETSTANDARD2_0_OR_GREATER
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; }
}
#endif

#if NET462 || NETSTANDARD2_0_OR_GREATER
/// <summary>Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IsExternalInit
{
}
#endif

