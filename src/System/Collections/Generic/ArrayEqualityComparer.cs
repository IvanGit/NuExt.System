using System.Diagnostics;

namespace System.Collections.Generic
{
    /// <summary>
    /// Provides a comparer to determine equality between two arrays of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the arrays.</typeparam>
    [Serializable]
    public struct ArrayEqualityComparer<T> : IEqualityComparer<T[]>
    {
        /// <summary>
        /// Determines whether the specified arrays are equal.
        /// </summary>
        /// <param name="x">The first array to compare.</param>
        /// <param name="y">The second array to compare.</param>
        /// <returns>true if the specified arrays are equal; otherwise, false.</returns>
        public readonly bool Equals(T[]? x, T[]? y)
        {
            if (x == y)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            if (x.Length != y.Length)
            {
                return false;
            }
            var equalityComparer = EqualityComparer<T>.Default;
            for (uint i = 0; i < x.Length; ++i)
            {
                if (!equalityComparer.Equals(x[i], y[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a hash code for the specified array.
        /// </summary>
        /// <param name="arr">The array for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified array.</returns>
        public readonly int GetHashCode(T[] arr)
        {
            Debug.Assert(arr is { Length: > 0 });
            var hash = new HashCode();
            for (uint i = 0; i < arr.Length; ++i)
            {
                hash.Add(arr[i]);
            }
            return hash.ToHashCode();
        }
    }
}
