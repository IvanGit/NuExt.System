#if !(NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic
{
    [DebuggerStepThrough]
    public static class DictionaryExtensions
    {
        extension<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            /// <summary>Attempts to add the specified key and value to the dictionary.</summary>
            /// <param name="key">The key of the element to add.</param>
            /// <param name="value">The value of the element to add. It can be <see langword="null" />.</param>
            /// <exception cref="T:System.ArgumentNullException">
            /// <paramref name="key" /> is <see langword="null" />.</exception>
            /// <returns>
            /// <see langword="true" /> if the key/value pair was added to the dictionary successfully; otherwise, <see langword="false" />.</returns>
            public bool TryAdd(TKey key, TValue value)
            {
                ArgumentNullException.ThrowIfNull(dictionary);
                if (dictionary.ContainsKey(key))
                {
                    return false;
                }
                dictionary.Add(key, value);
                return true;
            }

            /// <summary>Removes the value with the specified key from the <see cref="T:System.Collections.Generic.Dictionary`2" />, and copies the element to the <paramref name="value" /> parameter.</summary>
            /// <param name="key">The key of the element to remove.</param>
            /// <param name="value">The removed element.</param>
            /// <exception cref="T:System.ArgumentNullException">
            /// <paramref name="key" /> is <see langword="null" />.</exception>
            /// <returns>
            /// <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.</returns>
            public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
            {
                ArgumentNullException.ThrowIfNull(dictionary);
                return dictionary.TryGetValue(key, out value) && dictionary.Remove(key);
            }
        }
    }
}
#endif