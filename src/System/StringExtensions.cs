using System.Globalization;

namespace System
{
    public static class StringExtensions
    {
#if NET_OLD
        /// <summary>
        /// Determines whether this string instance contains a specified character.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="value">The character to seek.</param>
        /// <returns>true if the value parameter occurs within this string; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
        public static bool Contains(this string str, char value)
        {
            Throw.IfNull(str);

            return str.IndexOf(value) >= 0;
        }

        /// <summary>
        /// Determines whether the end of this string instance matches a specified character.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="value">The character to compare to the end of this string.</param>
        /// <returns>true if value matches the end of this string; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
        public static bool EndsWith(this string str, char value)
        {
            Throw.IfNull(str);

            if (str.Length == 0)
                return false;

            return str[str.Length - 1] == value;
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified Unicode character in this string instance.
        /// The search starts at a specified character position and examines a specified number of character positions.
        /// </summary>
        /// <param name="str">The string to search.</param>
        /// <param name="value">The character to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <returns>The zero-based index position of value if that character is found, or -1 if it is not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when startIndex or count is less than 0, or startIndex plus count indicates a position not within this instance.
        /// </exception>
        public static int IndexOf(this string str, char value, int startIndex, int count)
        {
            Throw.IfNull(str);

            return str.Substring(startIndex, count).IndexOf(value);
        }

        /// <summary>
        /// Reports the zero-based index position of the last occurrence of a specified Unicode character in this string instance.
        /// The search starts at a specified character position and proceeds backward for a specified number of character positions.
        /// </summary>
        /// <param name="str">The string to search.</param>
        /// <param name="value">The character to seek.</param>
        /// <param name="startIndex">
        /// The starting position of the search. The search proceeds from startIndex toward the beginning of this instance.
        /// </param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <returns>The zero-based index position of value if that character is found, or -1 if it is not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when startIndex or count is less than 0, or startIndex minus count indicates a position not within this instance.
        /// </exception>
        public static int LastIndexOf(this string str, char value, int startIndex, int count)
        {
            Throw.IfNull(str);

            if (startIndex < 0 || count < 0 || startIndex - count < 0)
                throw new ArgumentOutOfRangeException();

            return str.Substring(startIndex - count + 1, count).LastIndexOf(value);
        }

        /// <summary>
        /// Determines whether the beginning of this string instance matches a specified character.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="value">The character to compare to the start of this string.</param>
        /// <returns>true if value matches the beginning of this string; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
        public static bool StartsWith(this string str, char value)
        {
            Throw.IfNull(str);

            if (str.Length == 0)
                return false;

            return str[0] == value;
        }
#endif

        /// <summary>
        /// Capitalizes the first letter of the given string using the specified culture.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="culture">The culture to use for case conversion. If null, the current culture is used.</param>
        /// <returns>The string with the first letter capitalized.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
        public static string CapitalizeFirstLetter(this string str, CultureInfo? culture = null)
        {
#if NET
            ArgumentNullException.ThrowIfNull(str);
#else
            Throw.IfNull(str);
#endif
            if (str.Length == 0) return str;
            culture ??= CultureInfo.CurrentCulture;
            return char.ToUpper(str[0], culture) +
#if NET_OLD
                str.Substring(1);
#else
                str[1..];
#endif
        }

        /// <summary>
        /// Removes all whitespace characters from the string.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <returns>The string without any whitespace characters.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
        public static string RemoveWhiteSpace(this string str)
        {
#if NET
            ArgumentNullException.ThrowIfNull(str);
#else
            Throw.IfNull(str);
#endif

            return string.Concat(str.Where(c => !char.IsWhiteSpace(c)));
        }

        /// <summary>
        /// Reverses the characters in the string.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <returns>The reversed string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
        public static string Reverse(this string str)
        {
#if NET
            ArgumentNullException.ThrowIfNull(str);
#else
            Throw.IfNull(str);
#endif
            if (str.Length == 0) return str;
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Converts the first letter of each word to uppercase using the specified culture.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="culture">The culture to use for case conversion. If null, the current culture is used.</param>
        /// <returns>The string with the first letter of each word converted to uppercase.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
        public static string ToTitleCase(this string str, CultureInfo? culture = null)
        {
#if NET
            ArgumentNullException.ThrowIfNull(str);
#else
            Throw.IfNull(str);
#endif
            if (str.Length == 0) return str;
            culture ??= CultureInfo.CurrentCulture;
            return culture.TextInfo.ToTitleCase(str.ToLower(culture));
        }
    }
}