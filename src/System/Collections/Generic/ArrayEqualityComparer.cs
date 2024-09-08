using System.Diagnostics;

namespace System.Collections.Generic
{
    [Serializable]
    public struct ArrayEqualityComparer<T> : IEqualityComparer<T[]>
    {
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
            for (int i = 0; i < x.Length; ++i)
            {
                if (!equalityComparer.Equals(x[i], y[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public readonly int GetHashCode(T[] arr)
        {
            Debug.Assert(arr is { Length: > 0 });
            var hash = new HashCode();
            for (int i = 0; i < arr.Length; ++i)
            {
                hash.Add(arr[i]);
            }
            return hash.ToHashCode();
        }
    }

}
