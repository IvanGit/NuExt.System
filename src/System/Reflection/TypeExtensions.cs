using System.Diagnostics;

namespace System.Reflection
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Retrieves all properties of the given <paramref name="currentType"/> and its base types up to the specified <paramref name="baseType"/>, 
        /// that match the specified condition <paramref name="match"/> and respect the provided binding flags <paramref name="flags"/>.
        /// </summary>
        /// <param name="currentType">The current type from which to start searching for properties.</param>
        /// <param name="baseType">The base type up to which properties will be retrieved. Properties of this type will also be included.</param>
        /// <param name="flags">The binding flags used to control the search for properties.</param>
        /// <param name="match">A predicate that defines the conditions that properties must satisfy to be included in the result.</param>
        /// <returns>A read-only list of properties that match the specified criteria.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="currentType"/>, <paramref name="baseType"/>, or <paramref name="match"/> is <c>null</c>.
        /// </exception>
        public static IReadOnlyList<PropertyInfo> GetAllProperties(this Type currentType, Type baseType, BindingFlags flags, Predicate<PropertyInfo> match)
        {
            Debug.Assert(currentType != null && baseType != null && match != null);
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(currentType);
            ArgumentNullException.ThrowIfNull(baseType);
            ArgumentNullException.ThrowIfNull(match);
#else
            ThrowHelper.WhenNull(currentType);
            ThrowHelper.WhenNull(baseType);
            ThrowHelper.WhenNull(match);
#endif
            var list = new List<PropertyInfo>();
            Type? type = currentType;
            while (type != null && type != typeof(object))
            {
                list.AddRange(type.GetProperties(flags | BindingFlags.DeclaredOnly).Where(prop => match(prop)));
                if (type == baseType)
                    break;
                type = type.BaseType;
            }

            return list;
        }

        /// <summary>
        /// Retrieves all properties of the given <paramref name="currentType"/> and its base types up to the specified <paramref name="baseType"/>, 
        /// that respect the provided binding flags <paramref name="flags"/>.
        /// </summary>
        /// <param name="currentType">The current type from which to start searching for properties.</param>
        /// <param name="baseType">The base type up to which properties will be retrieved. Properties of this type will also be included.</param>
        /// <param name="flags">The binding flags used to control the search for properties.</param>
        /// <returns>A read-only list of properties that meet the specified criteria.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="currentType"/> or <paramref name="baseType"/> is <c>null</c>.
        /// </exception>
        public static IReadOnlyList<PropertyInfo> GetAllProperties(this Type currentType, Type baseType, BindingFlags flags)
        {
            Debug.Assert(currentType != null && baseType != null);
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(currentType);
            ArgumentNullException.ThrowIfNull(baseType);
#else
            ThrowHelper.WhenNull(currentType);
            ThrowHelper.WhenNull(baseType);
#endif
            var list = new List<PropertyInfo>();
            Type? type = currentType;
            while (type != null && type != typeof(object))
            {
                list.AddRange(type.GetProperties(flags | BindingFlags.DeclaredOnly));
                if (type == baseType)
                    break;
                type = type.BaseType;
            }

            return list;
        }

        /// <summary>
        /// Retrieves all properties of the specified type <typeparamref name="T"/> from the given <paramref name="currentType"/> and its base types up to the specified <paramref name="baseType"/>, 
        /// that respect the provided binding flags <paramref name="flags"/>.
        /// </summary>
        /// <typeparam name="T">The type of properties to retrieve.</typeparam>
        /// <param name="currentType">The current type from which to start searching for properties.</param>
        /// <param name="baseType">The base type up to which properties will be retrieved. Properties of this type will also be included.</param>
        /// <param name="flags">The binding flags used to control the search for properties.</param>
        /// <returns>A read-only list of properties of the specified type <typeparamref name="T"/> that meet the specified criteria.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="currentType"/> or <paramref name="baseType"/> is <c>null</c>.
        /// </exception>
        public static IReadOnlyList<PropertyInfo> GetAllPropertiesOfType<T>(this Type currentType, Type baseType, BindingFlags flags)
        {
            var propertyType = typeof(T);
            return GetAllProperties(currentType, baseType, flags, prop => propertyType.IsAssignableFrom(prop.PropertyType));
        }

    }
}
