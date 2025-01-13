namespace System
{
    /// <summary>
    /// Provides utility methods for working with enum types.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    public static class EnumHelper<TEnum> where TEnum : struct, Enum
    {
        private static readonly Dictionary<string, TEnum> s_values = new(StringComparer.OrdinalIgnoreCase);

        static EnumHelper()
        {
#if NET
            var values = Enum.GetValues<TEnum>();
#else
            var values = (TEnum[])Enum.GetValues(typeof(TEnum));
#endif
            foreach (var value in values)
            {
                s_values.Add(value.ToString(), value);
            }
        }

        /// <summary>
        /// Retrieves the enum value associated with the specified string name.
        /// </summary>
        /// <param name="name">The string name of the enum value to retrieve.</param>
        /// <returns>The enum value associated with the specified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty.</exception>
        public static TEnum GetValue(string name)
        {
            Throw.IfNullOrEmpty(name);
            return s_values[name];
        }

        /// <summary>
        /// Tries to get the enum value associated with the specified string name.
        /// </summary>
        /// <param name="name">The string name of the enum value to retrieve.</param>
        /// <param name="value">When this method returns, contains the enum value associated with the specified name, if the name is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the enum value was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetValue(string? name, out TEnum value)
        {
            if (name != null && s_values.TryGetValue(name, out value))
            {
                return true;
            }
            value = default;
            return false;
        }
    }
}
