namespace System.Threading.Tasks
{
    //Based on https://stackoverflow.com/questions/45689327/task-whenall-for-valuetask
    /// <summary>
    /// A set of extension methods for working with ValueTask objects.
    /// </summary>
    public static class ValueTaskExtensions
    {
        /// <summary>
        /// Executes a variable number of ValueTasks concurrently and returns their results as an array.
        /// </summary>
        /// <typeparam name="T">The type of the result produced by the ValueTasks.</typeparam>
        /// <param name="continueOnCapturedContext">
        /// Whether to marshal the continuation back to the original context captured when tasks complete.
        /// </param>
        /// <param name="tasks">A variable number of ValueTasks to be executed.</param>
        /// <returns>A task that represents the completion of all provided tasks. If any of the tasks fail, an AggregateException is thrown.</returns>
        public static ValueTask<T[]> WhenAll<T>(bool continueOnCapturedContext, params ValueTask<T>[] tasks)
        {
            return WhenAll(tasks, continueOnCapturedContext);
        }

        /// <summary>
        /// Executes a collection of ValueTasks concurrently and returns their results as an array.
        /// </summary>
        /// <typeparam name="T">The type of the result produced by the ValueTasks.</typeparam>
        /// <param name="tasks">An enumerable collection of ValueTasks to be executed.</param>
        /// <param name="continueOnCapturedContext">
        /// Whether to marshal the continuation back to the original context captured when tasks complete.
        /// </param>
        /// <returns>A task that represents the completion of all provided tasks. If any of the tasks fail, an AggregateException is thrown.</returns>
        public static ValueTask<T[]> WhenAll<T>(this IEnumerable<ValueTask<T>> tasks, bool continueOnCapturedContext)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(tasks);
#else
            Throw.IfNull(tasks);
#endif
            return WhenAll(tasks.ToList(), continueOnCapturedContext);
        }

        /// <summary>
        /// Executes a collection of ValueTasks concurrently and returns their results as an array.
        /// </summary>
        /// <typeparam name="T">The type of the result produced by the ValueTasks.</typeparam>
        /// <param name="tasks">An enumerable collection of ValueTasks to be executed.</param>
        /// <returns>
        /// A task that represents the completion of all provided tasks. If any of the tasks fail,
        /// an AggregateException is thrown containing all exceptions thrown by the tasks.
        /// </returns>
        /// <remarks>
        /// This method does not marshal the continuation back to the original context captured when tasks complete.
        /// To configure this behavior, use the overload with the 'continueOnCapturedContext' parameter.
        /// </remarks>
        public static ValueTask<T[]> WhenAll<T>(this IEnumerable<ValueTask<T>> tasks)
        {
            return WhenAll(tasks, false);
        }

        /// <summary>
        /// Executes a collection of ValueTasks concurrently and returns their results as an array.
        /// </summary>
        /// <typeparam name="T">The type of the result produced by the ValueTasks.</typeparam>
        /// <param name="tasks">A read-only list of ValueTasks to be executed.</param>
        /// <param name="continueOnCapturedContext">
        /// Whether to marshal the continuation back to the original context captured when tasks complete.
        /// </param>
        /// <returns>A task that represents the completion of all provided tasks. If any of the tasks fail, an AggregateException is thrown.</returns>
        public static async ValueTask<T[]> WhenAll<T>(this IReadOnlyList<ValueTask<T>> tasks, bool continueOnCapturedContext)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(tasks);
#else
            Throw.IfNull(tasks);
#endif

            if (tasks.Count == 0)
                return Array.Empty<T>();

            // We don't allocate the list if no task throws
            List<Exception>? exceptions = null;

            var results = new T[tasks.Count];
            for (var i = 0; i < tasks.Count; i++)
            {
                try
                {
                    results[i] = await tasks[i].ConfigureAwait(continueOnCapturedContext);
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>(tasks.Count);
                    exceptions.Add(ex);
                }
            }

            return exceptions is null
                ? results
                : throw new AggregateException(exceptions);
        }

        /// <summary>
        /// Executes a collection of ValueTasks concurrently and returns their results as an array.
        /// </summary>
        /// <typeparam name="T">The type of the result produced by the ValueTasks.</typeparam>
        /// <param name="tasks">A read-only list of ValueTasks to be executed.</param>
        /// <returns>
        /// A task that represents the completion of all provided tasks. If any of the tasks fail,
        /// an AggregateException is thrown containing all exceptions thrown by the tasks.
        /// </returns>
        /// <remarks>
        /// This method does not marshal the continuation back to the original context captured when tasks complete.
        /// To configure this behavior, use the overload with the 'continueOnCapturedContext' parameter.
        /// </remarks>
        public static ValueTask<T[]> WhenAll<T>(this IReadOnlyList<ValueTask<T>> tasks)
        {
            return WhenAll(tasks, false);
        }

        /// <summary>
        /// Executes a variable number of ValueTasks concurrently and returns their results as an array.
        /// </summary>
        /// <typeparam name="T">The type of the result produced by the ValueTasks.</typeparam>
        /// <param name="tasks">A variable number of ValueTasks to be executed.</param>
        /// <returns>
        /// A task that represents the completion of all provided tasks. If any of the tasks fail,
        /// an AggregateException is thrown containing all exceptions thrown by the tasks.
        /// </returns>
        /// <remarks>
        /// This method does not marshal the continuation back to the original context captured when tasks complete.
        /// To configure this behavior, use the overload with the 'continueOnCapturedContext' parameter.
        /// </remarks>
        public static ValueTask<T[]> WhenAll<T>(params ValueTask<T>[] tasks)
        {
            return WhenAll(tasks, false);
        }
    }
}
