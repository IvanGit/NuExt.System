#if NET_OLD
using System.Diagnostics;

namespace System.Collections.Generic
{
    [DebuggerStepThrough]
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Tries to add the specified key and value to the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to add the key and value to.</param>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>true if the key/value pair was added to the dictionary successfully; otherwise, false.</returns>
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            Throw.IfNull(dictionary);
            if (dictionary.ContainsKey(key))
            {
                return false;
            }
            dictionary.Add(key, value);
            return true;
        }
    }
}
#endif