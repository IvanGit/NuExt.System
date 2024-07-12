using System.Diagnostics;

namespace System.Collections.Generic
{
    [DebuggerStepThrough]
    public static class CollectionExtensions
    {
        public static bool IsNullOrEmpty<T>(this T[]? self)
        {
            return self == null || self.Length == 0;
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T>? self)
        {
            return self == null || self.Count == 0;
        }

        public static IEnumerable<T> FastReverse<T>(this IList<T> items)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(items);
#else
            ThrowHelper.WhenNull(items);
#endif
            for (int i = items.Count - 1; i >= 0; i--)
            {
                yield return items[i];
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            Debug.Assert(source != null && action != null);
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(action);
#else
            ThrowHelper.WhenNull(source);
            ThrowHelper.WhenNull(action);
#endif
            foreach (T item in source)
            {
                action(item);
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> source, Predicate<T> match)
        {
            Debug.Assert(source != null && match != null);
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(match);
#else
            ThrowHelper.WhenNull(source);
            ThrowHelper.WhenNull(match);
#endif
            int i = 0;
            foreach (T item in source)
            {
                if (match(item))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public static int FindIndexOfMax<T, TV>(this IList<T> source, Func<T, TV> selector) where TV : IComparable
        {
            Debug.Assert(source != null && selector != null);
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);
#else
            ThrowHelper.WhenNull(source);
            ThrowHelper.WhenNull(selector);
#endif
            if (source.Count == 0)
            {
                return -1;
            }
            if (source.Count == 1)
            {
                return 0;
            }
            int index = 0;
            TV min = selector(source[0]);
            for (int i = 1; i < source.Count; ++i)
            {
                TV current = selector(source[i]);
                if (current.CompareTo(min) > 0)
                {
                    min = current;
                    index = i;
                }
            }
            return index;
        }

        public static void DisposeAndClear<T>(this ICollection<T> self) where T : IDisposable
        {
            Debug.Assert(self != null);
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(self);
#else
            ThrowHelper.WhenNull(self);
#endif
            self.ForEach(item => item.Dispose());
            self.Clear();
        }
    }
}
