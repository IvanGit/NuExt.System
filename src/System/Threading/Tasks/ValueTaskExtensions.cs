namespace System.Threading.Tasks
{
    //https://stackoverflow.com/questions/45689327/task-whenall-for-valuetask
    public static class ValueTaskExtensions
    {
        // There are some collections (e.g. hash-sets, queues/stacks,
        // linked lists, etc) that only implement I*Collection interfaces
        // and not I*List ones, but A) we're not likely to have our tasks
        // in them and B) even if we do, IEnumerable accepting overload
        // below should handle them. Allocation-wise; it's a ToList there
        // vs GetEnumerator here.
        public static async ValueTask<T[]> WhenAll<T>(this IReadOnlyList<ValueTask<T>> tasks, bool continueOnCapturedContext = false)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(tasks);
#else
            ThrowHelper.WhenNull(tasks);
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

        // ToList call below ensures that all tasks are initialized, so
        // calling this with an iterator wouldn't cause the tasks to run
        // sequentially.
        public static ValueTask<T[]> WhenAll<T>(this IEnumerable<ValueTask<T>> tasks, bool continueOnCapturedContext = false)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(tasks);
#else
            ThrowHelper.WhenNull(tasks);
#endif
            return WhenAll(tasks.ToList(), continueOnCapturedContext);
        }

        // Arrays already implement IReadOnlyList<T>, but this overload
        // is still useful because the `params` keyword allows callers 
        // to pass individual tasks like they are different arguments.
        public static ValueTask<T[]> WhenAll<T>(bool continueOnCapturedContext = false, params ValueTask<T>[] tasks)
        {
            return WhenAll(tasks, continueOnCapturedContext);
        }
    }
}
