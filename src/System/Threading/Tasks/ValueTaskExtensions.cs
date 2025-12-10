using System.Collections.Generic;

namespace System.Threading.Tasks
{
    //Based on https://stackoverflow.com/questions/45689327/task-whenall-for-valuetask
    /// <summary>
    /// A set of extension methods for working with ValueTask objects.
    /// </summary>
    public static class ValueTaskExtensions
    {
        extension(ValueTask)
        {
#if !NET
            /// <summary>Gets a task that has already completed successfully.</summary>
            public static ValueTask CompletedTask => default;
#endif
            /// <summary>
            /// Executes a collection of ValueTasks concurrently and returns a task that completes when all the tasks have completed.
            /// </summary>
            /// <param name="tasks">An enumerable collection of ValueTasks to be executed.</param>
            /// <returns>A ValueTask that represents the completion of all provided tasks. If any of the tasks fail, an AggregateException is thrown.</returns>
            /// <exception cref="ArgumentNullException">The <paramref name="tasks"/> argument was null.</exception>
            /// <exception cref="AggregateException">One or more exceptions occurred during the invocation of tasks.</exception>
            public static ValueTask WhenAll(IEnumerable<ValueTask> tasks)
            {
                ArgumentNullException.ThrowIfNull(tasks);

                int? count = null;
                if (tasks is ICollection<ValueTask> taskCollection)
                {
                    count = taskCollection.Count;
                    if (count is > 0 && tasks is IReadOnlyList<ValueTask> taskList)
                    {
                        return WhenAll(taskList);
                    }
                }

                if (count is 0)
                {
                    return ValueTask.CompletedTask;
                }

                // Buffer the tasks into a temporary span. Small sets of tasks are common,
                // so for <= 8 we stack allocate.
                ValueListBuilder<ValueTask> builder = count is > 8 ?
                    new ValueListBuilder<ValueTask>(count.Value) :
                    new ValueListBuilder<ValueTask>([default, default, default, default, default, default, default, default]);
                foreach (ValueTask task in tasks)
                {
                    builder.Append(task);
                }

                return WhenAll((IReadOnlyList<ValueTask>)builder.ToArray());
            }

            /// <summary>
            /// Executes a variable number of ValueTasks concurrently and returns a task that completes when all the tasks have completed.
            /// </summary>
            /// <param name="tasks">A variable number of ValueTasks to be executed.</param>
            /// <returns>A ValueTask that represents the completion of all provided tasks. If any of the tasks fail, an AggregateException is thrown.</returns>
            /// <exception cref="ArgumentNullException">The <paramref name="tasks"/> argument was null.</exception>
            /// <exception cref="AggregateException">One or more exceptions occurred during the invocation of tasks.</exception>
            public static ValueTask WhenAll(params ValueTask[] tasks)
            {
                ArgumentNullException.ThrowIfNull(tasks);

                return WhenAll((IReadOnlyList<ValueTask>)tasks);
            }

            /// <summary>
            /// Executes a variable number of ValueTasks concurrently and returns a task that completes when all the tasks have completed.
            /// </summary>
            /// <param name="tasks">An array of ValueTasks to be executed.</param>
            /// <returns>A ValueTask that represents the completion of all provided tasks. If any of the tasks fail, an AggregateException is thrown.</returns>
            /// <exception cref="ArgumentNullException"> The <paramref name="tasks"/> argument was null.</exception>
            /// <exception cref="AggregateException">One or more exceptions occurred during the invocation of tasks.</exception>
            public static ValueTask WhenAll(params ReadOnlySpan<ValueTask> tasks)
            {
                if (tasks.IsEmpty)
                {
                    return ValueTask.CompletedTask;
                }
                return WhenAll((IReadOnlyList<ValueTask>)tasks.ToArray());
            }

            /// <summary>
            /// Executes a collection of ValueTasks concurrently and returns their results as an array.
            /// </summary>
            /// <typeparam name="TResult">The type of the result produced by the ValueTasks.</typeparam>
            /// <param name="tasks">An enumerable collection of ValueTasks to be executed.</param>
            /// <returns>A ValueTask that represents the completion of all provided tasks. If any of the tasks fail, an AggregateException is thrown.</returns>
            /// <exception cref="ArgumentNullException">The <paramref name="tasks"/> argument was null.</exception>
            /// <exception cref="AggregateException">One or more exceptions occurred during the invocation of tasks.</exception>
            public static ValueTask<TResult[]> WhenAll<TResult>(IEnumerable<ValueTask<TResult>> tasks)
            {
                ArgumentNullException.ThrowIfNull(tasks);

                int? count = null;
                if (tasks is ICollection<ValueTask<TResult>> taskCollection)
                {
                    count = taskCollection.Count;
                    if (count is > 0 && tasks is IReadOnlyList<ValueTask<TResult>> taskList)
                    {
                        return WhenAll(taskList);
                    }
                }

                if (count is 0)
                {
                    return new ValueTask<TResult[]>([]);
                }

                // Buffer the tasks into a temporary span. Small sets of tasks are common,
                // so for <= 8 we stack allocate.
                ValueListBuilder<ValueTask<TResult>> builder = count is > 8 ?
                    new ValueListBuilder<ValueTask<TResult>>(count.Value) :
                    new ValueListBuilder<ValueTask<TResult>>([default, default, default, default, default, default, default, default]);
                foreach (ValueTask<TResult> task in tasks)
                {
                    builder.Append(task);
                }

                return WhenAll((IReadOnlyList<ValueTask<TResult>>)builder.ToArray());
            }

            /// <summary>
            /// Executes a variable number of ValueTasks concurrently and returns their results as an array.
            /// </summary>
            /// <typeparam name="TResult">The type of the result produced by the ValueTasks.</typeparam>
            /// <param name="tasks">A variable number of ValueTasks to be executed.</param>
            /// <returns>A ValueTask that represents the completion of all provided tasks. If any of the tasks fail, an AggregateException is thrown.</returns>
            /// <exception cref="ArgumentNullException">The <paramref name="tasks"/> argument was null.</exception>
            /// <exception cref="AggregateException">One or more exceptions occurred during the invocation of tasks.</exception>
            public static ValueTask<TResult[]> WhenAll<TResult>(params ValueTask<TResult>[] tasks)
            {
                ArgumentNullException.ThrowIfNull(tasks);

                return WhenAll((IReadOnlyList<ValueTask<TResult>>)tasks);
            }

            /// <summary>
            /// Executes a variable number of ValueTasks concurrently and returns their results as an array.
            /// </summary>
            /// <typeparam name="TResult">The type of the result produced by the ValueTasks.</typeparam>
            /// <param name="tasks">An array of ValueTasks to be executed.</param>
            /// <returns>A ValueTask that represents the completion of all provided tasks. If any of the tasks fail, an AggregateException is thrown.</returns>
            /// <exception cref="ArgumentNullException"> The <paramref name="tasks"/> argument was null.</exception>
            /// <exception cref="AggregateException">One or more exceptions occurred during the invocation of tasks.</exception>
            public static ValueTask<TResult[]> WhenAll<TResult>(params ReadOnlySpan<ValueTask<TResult>> tasks)
            {
                if (tasks.IsEmpty)
                {
                    return new ValueTask<TResult[]>([]);
                }
                return WhenAll((IReadOnlyList<ValueTask<TResult>>)tasks.ToArray());
            }
        }

        /// <summary>
        /// Executes a collection of ValueTasks concurrently and returns a task that completes when all the tasks have completed.
        /// </summary>
        /// <param name="tasks">A read-only list of ValueTasks to be executed.</param>
        /// <returns>A ValueTask that represents the completion of all provided tasks. If any of the tasks fail, an AggregateException is thrown.</returns>
        /// <exception cref="ArgumentNullException"> The <paramref name="tasks"/> argument was null.</exception>
        /// <exception cref="AggregateException">One or more exceptions occurred during the invocation of tasks.</exception>
        public static async ValueTask WhenAll(this IReadOnlyList<ValueTask> tasks)
        {
            ArgumentNullException.ThrowIfNull(tasks);

            if (tasks.Count == 0)
            {
                return;
            }

            // We don't allocate the list if no task throws
            List<Exception>? exceptions = null;

            for (var i = 0; i < tasks.Count; i++)
            {
                try
                {
                    await tasks[i].ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>(tasks.Count - i);
                    exceptions.Add(ex);
                }
            }

            if (exceptions is not null)
            {
                throw new AggregateException(exceptions);
            }
        }

        /// <summary>
        /// Executes a collection of ValueTasks concurrently and returns their results as an array.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the ValueTasks.</typeparam>
        /// <param name="tasks">A read-only list of ValueTasks to be executed.</param>
        /// <returns>A ValueTask that represents the completion of all provided tasks. If any of the tasks fail, an AggregateException is thrown.</returns>
        /// <exception cref="ArgumentNullException"> The <paramref name="tasks"/> argument was null.</exception>
        /// <exception cref="AggregateException">One or more exceptions occurred during the invocation of tasks.</exception>
        public static async ValueTask<TResult[]> WhenAll<TResult>(this IReadOnlyList<ValueTask<TResult>> tasks)
        {
            ArgumentNullException.ThrowIfNull(tasks);

            if (tasks.Count == 0)
            {
                return [];
            }

            // We don't allocate the list if no task throws
            List<Exception>? exceptions = null;

            var results = new TResult[tasks.Count];
            for (var i = 0; i < tasks.Count; i++)
            {
                try
                {
                    results[i] = await tasks[i].ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>(tasks.Count - i);
                    exceptions.Add(ex);
                }
            }

            return exceptions is null
                ? results
                : throw new AggregateException(exceptions);
        }
    }
}
