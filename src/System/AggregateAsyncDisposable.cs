namespace System
{
    /// <summary>
    /// Represents an asynchronous disposable object that aggregates multiple asynchronous disposables.
    /// When disposed, it disposes all aggregated disposables asynchronously.
    /// </summary>
    [Serializable]
    public sealed class AggregateAsyncDisposable : AsyncDisposable
    {
        /// <summary>
        /// The collection of async disposables to be managed and disposed together asynchronously.
        /// </summary>
        private readonly IEnumerable<IAsyncDisposable?> _disposables;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateAsyncDisposable"/> class with the specified async disposables and synchronization context.
        /// </summary>
        /// <param name="disposables">A collection of async disposables to aggregate.</param>
        /// <param name="synchronizationContext">An optional <see cref="SynchronizationContext"/> to use for property change notifications.</param>
        /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
        public AggregateAsyncDisposable(IEnumerable<IAsyncDisposable?> disposables, SynchronizationContext? synchronizationContext) : base(synchronizationContext)
        {
            _disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateAsyncDisposable"/> class with the specified async disposables.
        /// </summary>
        /// <param name="disposables">A collection of disposables to aggregate.</param>
        public AggregateAsyncDisposable(IEnumerable<IAsyncDisposable?> disposables) : this(disposables, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateAsyncDisposable"/> class with the specified async disposables.
        /// </summary>
        /// <param name="disposables">An array of disposables to aggregate.</param>
        public AggregateAsyncDisposable(params IAsyncDisposable?[] disposables) : this((IEnumerable<IAsyncDisposable?>)disposables)
        {
        }

        /// <summary>
        /// Disposes all aggregated disposables asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        /// <exception cref="AggregateException">Thrown if one or more exceptions occur during the disposal of the aggregated disposables.</exception>
        protected override async ValueTask OnDisposeAsync()
        {
            List<Exception>? exceptions = null;
            foreach (var disposable in _disposables)
            {
                if (disposable is null) continue;
                try
                {
                    await disposable.DisposeAsync();
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
            await base.OnDisposeAsync();
        }
    }
}
