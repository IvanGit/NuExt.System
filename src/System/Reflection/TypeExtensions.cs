using System.Diagnostics;

namespace System.Reflection
{
    public static class TypeExtensions
    {
        public static IReadOnlyList<PropertyInfo> GetAllProperties<T>(this Type type, BindingFlags flags)
        {
            Debug.Assert(type != null);
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(type);
#else
            ThrowHelper.WhenNull(type);
#endif
            var list = new List<PropertyInfo>();
            list.AddRange(type.GetProperties(flags | BindingFlags.DeclaredOnly));
            while (type != typeof(object) && type != typeof(T))
            {
                type = type.BaseType!;
                list.AddRange(type.GetProperties(flags | BindingFlags.DeclaredOnly));
            }

            return list;
        }
    }
}
