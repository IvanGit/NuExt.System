using System.Diagnostics;

namespace System.Reflection
{
    /// <summary>
    /// Provides extension methods for working with types and their members.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Retrieves all fields of the given <paramref name="currentType"/> and its base types up to the specified <paramref name="baseType"/>, 
        /// that match the specified condition <paramref name="match"/> and respect the provided binding flags <paramref name="flags"/>.
        /// </summary>
        /// <param name="currentType">The current type from which to start searching for fields.</param>
        /// <param name="baseType">The base type up to which fields will be retrieved. Fields of this type will also be included.</param>
        /// <param name="flags">The binding flags used to control the search for fields.</param>
        /// <param name="match">A predicate that defines the conditions that fields must satisfy to be included in the result. If null, all fields are included.</param>
        /// <returns>A read-only list of fields that match the specified criteria.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="currentType"/> or <paramref name="baseType"/> is <c>null</c>.
        /// </exception>
        public static IReadOnlyList<FieldInfo> GetAllFields(this Type currentType, Type baseType, BindingFlags flags, Predicate<FieldInfo>? match = default)
        {
            Debug.Assert(currentType != null && baseType != null);
#if NET
            ArgumentNullException.ThrowIfNull(currentType);
            ArgumentNullException.ThrowIfNull(baseType);
#else
            Throw.IfNull(currentType);
            Throw.IfNull(baseType);
#endif
            var list = new List<FieldInfo>();
            Type? type = currentType;
            while (type != null && type != typeof(object))
            {
                var fields = type.GetFields(flags | BindingFlags.DeclaredOnly);
                list.AddRange(match != null ? fields.Where(field => match(field)) : fields);
                if (type == baseType)
                    break;
                type = type.BaseType;
            }
            return list;
        }

        /// <summary>
        /// Retrieves all fields of the specified type <typeparamref name="T"/> from the given <paramref name="currentType"/> and its base types up to the specified <paramref name="baseType"/>, 
        /// that respect the provided binding flags <paramref name="flags"/>.
        /// </summary>
        /// <typeparam name="T">The type of fields to retrieve.</typeparam>
        /// <param name="currentType">The current type from which to start searching for fields.</param>
        /// <param name="baseType">The base type up to which fields will be retrieved. Fields of this type will also be included.</param>
        /// <param name="flags">The binding flags used to control the search for fields.</param>
        /// <returns>A read-only list of fields of the specified type <typeparamref name="T"/> that meet the specified criteria.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="currentType"/> or <paramref name="baseType"/> is <c>null</c>.
        /// </exception>
        public static IReadOnlyList<FieldInfo> GetAllFieldsOfType<T>(this Type currentType, Type baseType, BindingFlags flags)
        {
            var fieldType = typeof(T);
            return GetAllFields(currentType, baseType, flags, field => fieldType.IsAssignableFrom(field.FieldType));
        }

        /// <summary>
        /// Retrieves all methods of the given <paramref name="currentType"/> and its base types up to the specified <paramref name="baseType"/>, 
        /// that respect the provided binding flags <paramref name="flags"/>.
        /// </summary>
        /// <param name="currentType">The current type from which to start searching for methods.</param>
        /// <param name="baseType">The base type up to which methods will be retrieved. Methods of this type will also be included.</param>
        /// <param name="flags">The binding flags used to control the search for methods.</param>
        /// <param name="match">A predicate that defines the conditions that methods must satisfy to be included in the result. If null, all methods are included.</param>
        /// <returns>A read-only list of methods that match the specified criteria.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="currentType"/> or <paramref name="baseType"/> is <c>null</c>.
        /// </exception>
        public static IReadOnlyList<MethodInfo> GetAllMethods(this Type currentType, Type baseType, BindingFlags flags, Predicate<MethodInfo>? match = default)
        {
            Debug.Assert(currentType != null && baseType != null);
#if NET
            ArgumentNullException.ThrowIfNull(currentType);
            ArgumentNullException.ThrowIfNull(baseType);
#else
            Throw.IfNull(currentType);
            Throw.IfNull(baseType);
#endif
            var list = new List<MethodInfo>();
            Type? type = currentType;
            while (type != null && type != typeof(object))
            {
                var methods = type.GetMethods(flags | BindingFlags.DeclaredOnly);
                list.AddRange(match != null ? methods.Where(method => match(method)) : methods);
                if (type == baseType)
                    break;
                type = type.BaseType;
            }
            return list;
        }

        /// <summary>
        /// Retrieves all properties of the given <paramref name="currentType"/> and its base types up to the specified <paramref name="baseType"/>, 
        /// that match the specified condition <paramref name="match"/> and respect the provided binding flags <paramref name="flags"/>.
        /// </summary>
        /// <param name="currentType">The current type from which to start searching for properties.</param>
        /// <param name="baseType">The base type up to which properties will be retrieved. Properties of this type will also be included.</param>
        /// <param name="flags">The binding flags used to control the search for properties.</param>
        /// <param name="match">A predicate that defines the conditions that properties must satisfy to be included in the result. If null, all properties are included.</param>
        /// <returns>A read-only list of properties that match the specified criteria.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="currentType"/> or <paramref name="baseType"/> is <c>null</c>.
        /// </exception>
        public static IReadOnlyList<PropertyInfo> GetAllProperties(this Type currentType, Type baseType, BindingFlags flags, Predicate<PropertyInfo>? match = default)
        {
            Debug.Assert(currentType != null && baseType != null);
#if NET
            ArgumentNullException.ThrowIfNull(currentType);
            ArgumentNullException.ThrowIfNull(baseType);
#else
            Throw.IfNull(currentType);
            Throw.IfNull(baseType);
#endif
            var list = new List<PropertyInfo>();
            Type? type = currentType;
            while (type != null && type != typeof(object))
            {
                var props = type.GetProperties(flags | BindingFlags.DeclaredOnly);
                list.AddRange(match != null ? props.Where(prop => match(prop)) : props);
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
