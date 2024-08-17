using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic
{
    [DebuggerStepThrough]
    public static class CollectionExtensions
    {
        /// <summary>
        /// Determines whether the given array is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="self">The array to check.</param>
        /// <returns>True if the array is null or empty; otherwise, false.</returns>
        public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this T[]? self)
        {
            return self == null || self.Length == 0;
        }

        /// <summary>
        /// Determines whether the given collection is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="self">The collection to check.</param>
        /// <returns>True if the collection is null or empty; otherwise, false.</returns>
        public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this ICollection<T>? self)
        {
            return self == null || self.Count == 0;
        }

        /// <summary>
        /// Returns the elements of the list in reverse order.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="items">The list to reverse. This value cannot be null.</param>
        /// <returns>An IEnumerable containing the elements of the list in reverse order.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the items parameter is null.</exception>
        public static IEnumerable<T> FastReverse<T>(this IList<T> items)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(items);
#else
            Throw.IfNull(items);
#endif
            for (int i = items.Count - 1; i >= 0; i--)
            {
                yield return items[i];
            }
        }

        /// <summary>
        /// Performs the specified action on each element of the enumerable.
        /// </summary>
        /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
        /// <param name="source">The enumerable whose elements the action will be performed on. This value cannot be null.</param>
        /// <param name="action">The action to perform on each element. This value cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when the source or action parameter is null.</exception>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            Debug.Assert(source != null && action != null);
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(action);
#else
            Throw.IfNull(source);
            Throw.IfNull(action);
#endif
            foreach (T item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// Finds the index of the first element in the enumerable that matches the provided predicate.
        /// </summary>
        /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
        /// <param name="source">The enumerable to search. This value cannot be null.</param>
        /// <param name="match">The predicate to match elements against. This value cannot be null.</param>
        /// <returns>The index of the first matching element, or -1 if no match is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source or match parameter is null.</exception>
        public static int IndexOf<T>(this IEnumerable<T> source, Predicate<T> match)
        {
            Debug.Assert(source != null && match != null);
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(match);
#else
            Throw.IfNull(source);
            Throw.IfNull(match);
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

        /// <summary>
        /// Finds the index of the element with the maximum value as determined by the selector function.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <typeparam name="TV">The type of the value returned by the selector function, which must implement IComparable.</typeparam>
        /// <param name="source">The list to search. This value cannot be null.</param>
        /// <param name="selector">The function to select values from the elements. This value cannot be null.</param>
        /// <returns>The index of the element with the maximum value, or -1 if the list is empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source or selector parameter is null.</exception>
        public static int FindIndexOfMax<T, TV>(this IList<T> source, Func<T, TV> selector) where TV : IComparable
        {
            Debug.Assert(source != null && selector != null);
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);
#else
            Throw.IfNull(source);
            Throw.IfNull(selector);
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

        /// <summary>
        /// Disposes all elements in the collection and then clears the collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection, which must implement IDisposable.</typeparam>
        /// <param name="self">The collection whose elements will be disposed and cleared. This value cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when the self parameter is null.</exception>
        public static void DisposeAndClear<T>(this ICollection<T> self) where T : IDisposable?
        {
            Debug.Assert(self != null);
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(self);
#else
            Throw.IfNull(self);
#endif
            self.ForEach(item => item?.Dispose());
            self.Clear();
        }

        /// <summary>
        /// Asynchronously disposes all elements in the collection in the order they were added and then clears the collection.
        /// Collects and throws all exceptions that occur during disposal as an AggregateException.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection, which must implement IAsyncDisposable.</typeparam>
        /// <param name="self">The collection whose elements will be disposed and cleared. This value cannot be null.</param>
        /// <param name="continueOnCapturedContext">
        /// Whether to marshal the continuation back to the original context captured.
        /// If true, the continuation is run on the captured context; otherwise, it may run on a different context.
        /// The default value is false.
        /// </param>
        /// <returns>A ValueTask representing the asynchronous disposal and clearing operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the self parameter is null.</exception>
        /// <exception cref="AggregateException">Thrown when one or more exceptions occur during the disposal of elements.</exception>
        public static async ValueTask DisposeAndClearAsync<T>(this ICollection<T> self, bool continueOnCapturedContext = false) where T : IAsyncDisposable?
        {
            Debug.Assert(self != null);
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(self);
#else
            Throw.IfNull(self);
#endif
            List<Exception>? exceptions = null;
            foreach (var item in self)
            {
                if (item is null) continue;
                try
                {
                    await item.DisposeAsync().ConfigureAwait(continueOnCapturedContext: continueOnCapturedContext);
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(ex);
                }
            }
            if (exceptions is not null)
            {
                throw new AggregateException(exceptions);
            }
            self.Clear();
        }
    }
}
