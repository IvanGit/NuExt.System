#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

using System.Diagnostics;

namespace System
{
    /// <summary>
    /// Provides utility methods for working with enum types.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    public static class EnumHelper<TEnum> where TEnum : struct, Enum
    {
#if NET8_0_OR_GREATER
        private static readonly FrozenDictionary<string, TEnum> s_valuesCaseInsensitive;
        private static readonly FrozenDictionary<string, TEnum> s_valuesCaseSensitive;
#else
        private static readonly Dictionary<string, TEnum> s_valuesCaseInsensitive;
        private static readonly Dictionary<string, TEnum> s_valuesCaseSensitive;
#endif

        static EnumHelper()
        {
#if NET
            var names = Enum.GetNames<TEnum>();
            var values = Enum.GetValues<TEnum>();
#else
            var names = Enum.GetNames(typeof(TEnum));
            var values = (TEnum[])Enum.GetValues(typeof(TEnum));
#endif
            Dictionary<string, TEnum> valuesCaseInsensitive = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, TEnum> valuesCaseSensitive = new(StringComparer.Ordinal);

            for (int i = 0; i < names.Length; i++)
            {
                valuesCaseInsensitive.TryAdd(names[i], values[i]);
                valuesCaseSensitive.Add(names[i], values[i]);
            }
#if NET8_0_OR_GREATER
            s_valuesCaseInsensitive = valuesCaseInsensitive.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
            s_valuesCaseSensitive = valuesCaseSensitive.ToFrozenDictionary(StringComparer.Ordinal);
#else
            s_valuesCaseInsensitive = valuesCaseInsensitive;
            s_valuesCaseSensitive = valuesCaseSensitive;
#endif
        }

        /// <summary>
        /// Retrieves the enum value associated with the specified string name.
        /// </summary>
        /// <param name="name">The string name of the enum value to retrieve.</param>
        /// <returns>The enum value associated with the specified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty or the enum value is not found.</exception>
        [DebuggerHidden]
        public static TEnum GetValue(string name) =>
            GetValue(name, ignoreCase: false);

        /// <summary>
        /// Retrieves the enum value associated with the specified string name.
        /// </summary>
        /// <param name="name">The string name of the enum value to retrieve.</param>
        /// <param name="ignoreCase">Whether to ignore case when matching the name.</param>
        /// <returns>The enum value associated with the specified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty or the enum value is not found.</exception>
        [DebuggerHidden]
        public static TEnum GetValue(string name, bool ignoreCase)
        {
            Throw.IfNullOrWhiteSpace(name);

            if (ignoreCase)
            {
                if (s_valuesCaseInsensitive.TryGetValue(name, out var value))
                {
                    return value;
                }
            }
            else
            {
                if (s_valuesCaseSensitive.TryGetValue(name, out var value))
                {
                    return value;
                }
            }

            throw new ArgumentException($"Requested value '{name}' was not found.", nameof(name));
        }

        /// <summary>
        /// Tries to get the enum value associated with the specified string name.
        /// </summary>
        /// <param name="name">The string name of the enum value to retrieve.</param>
        /// <param name="value">When this method returns, contains the enum value associated with the specified name, if the name is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the enum value was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetValue(string? name, out TEnum value) =>
            TryGetValue(name, ignoreCase: false, out value);

        /// <summary>
        /// Tries to get the enum value associated with the specified string name.
        /// </summary>
        /// <param name="name">The string name of the enum value to retrieve.</param>
        /// <param name="ignoreCase">Whether to ignore case when matching the name.</param>
        /// <param name="value">When this method returns, contains the enum value associated with the specified name, if the name is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the enum value was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetValue(string? name, bool ignoreCase, out TEnum value)
        {
            if (name?.Length > 0)
            {
                if (ignoreCase)
                {
                    return s_valuesCaseInsensitive.TryGetValue(name, out value);
                }
                return s_valuesCaseSensitive.TryGetValue(name, out value);
            }

            value = default;
            return false;
        }
    }
}
