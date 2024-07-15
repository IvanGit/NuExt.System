namespace System
{
    /// <summary>
    /// Represents a disposable object that aggregates multiple disposables.
    /// When disposed, it disposes all aggregated disposables.
    /// </summary>
    [Serializable]
    public class AggregateDisposable : Disposable
    {
        /// <summary>
        /// The collection of disposables to be managed and disposed together.
        /// </summary>
        private readonly IEnumerable<IDisposable> _disposables;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateDisposable"/> class with the specified disposables and synchronization context.
        /// </summary>
        /// <param name="disposables">A collection of disposables to aggregate.</param>
        /// <param name="synchronizationContext">An optional <see cref="SynchronizationContext"/> to use for property change notifications.</param>
        /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
        public AggregateDisposable(IEnumerable<IDisposable> disposables, SynchronizationContext? synchronizationContext) : base(synchronizationContext)
        {
            _disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateDisposable"/> class with the specified disposables.
        /// </summary>
        /// <param name="disposables">A collection of disposables to aggregate.</param>
        public AggregateDisposable(IEnumerable<IDisposable> disposables): this(disposables, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateDisposable"/> class with the specified disposables.
        /// </summary>
        /// <param name="disposables">An array of disposables to aggregate.</param>
        public AggregateDisposable(params IDisposable[] disposables) : this((IEnumerable<IDisposable>)disposables)
        {
        }

        /// <summary>
        /// Disposes all aggregated disposables.
        /// </summary>
        /// <exception cref="AggregateException">
        /// Thrown if one or more exceptions occur during the disposal of the aggregated disposables.
        /// </exception>
        protected override void OnDispose()
        {
            List<Exception>? exceptions = null;
            foreach (var disposer in _disposables)
            {
                try
                {
                    disposer?.Dispose();
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
            base.OnDispose();
        }
    }
}
