using System.Collections.Generic;
using System.Threading;

namespace System
{
    /// <summary>
    /// Represents a disposable object that aggregates multiple disposables.
    /// When disposed, it disposes all aggregated disposables.
    /// </summary>
    [Serializable]
    public sealed class AggregateDisposable : Disposable
    {
        /// <summary>
        /// The list of disposables to be managed and disposed together.
        /// </summary>
        private readonly List<IDisposable?> _disposables;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateDisposable"/> class with the specified disposables and synchronization context.
        /// </summary>
        /// <param name="disposables">A read-only list of disposables to aggregate.</param>
        /// <param name="synchronizationContext">An optional <see cref="SynchronizationContext"/> to use for property change notifications.</param>
        /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
        public AggregateDisposable(IReadOnlyList<IDisposable?> disposables, SynchronizationContext? synchronizationContext) : base(synchronizationContext)
        {
            _disposables = [.. disposables ?? throw new ArgumentNullException(nameof(disposables))];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateDisposable"/> class with the specified disposables and synchronization context.
        /// </summary>
        /// <param name="disposables">A read-only list of disposables to aggregate.</param>
        /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
        public AggregateDisposable(params IReadOnlyList<IDisposable?> disposables) : this(disposables, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateDisposable"/> class with the specified disposables and synchronization context.
        /// </summary>
        /// <param name="disposables">A collection of disposables to aggregate.</param>
        /// <param name="synchronizationContext">An optional <see cref="SynchronizationContext"/> to use for property change notifications.</param>
        /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
        public AggregateDisposable(IEnumerable<IDisposable?> disposables, SynchronizationContext? synchronizationContext) : base(synchronizationContext)
        {
            _ = disposables ?? throw new ArgumentNullException(nameof(disposables));
            _disposables = [.. disposables];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateDisposable"/> class with the specified disposables.
        /// </summary>
        /// <param name="disposables">A collection of disposables to aggregate.</param>
        /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
        public AggregateDisposable(params IEnumerable<IDisposable?> disposables): this(disposables, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateDisposable"/> class with the specified disposables.
        /// </summary>
        /// <param name="disposables">An array of disposables to aggregate.</param>
        /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
        public AggregateDisposable(params IDisposable?[] disposables) : this(disposables, null)
        {
        }

#if NET || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateDisposable"/> class with the specified disposables.
        /// </summary>
        /// <param name="disposables">A read-only span of disposables to aggregate.</param>
        public AggregateDisposable(params ReadOnlySpan<IDisposable?> disposables)
        {
            _disposables = [.. disposables];
        }
#endif

        /// <summary>
        /// Adds a disposable object to the aggregate.
        /// </summary>
        /// <param name="disposable">The disposable object to add.</param>
        public void Add(IDisposable? disposable)
        {
            _disposables.Add(disposable);
        }

        /// <summary>
        /// Adds a range of disposable objects to the aggregate.
        /// </summary>
        /// <param name="disposables">An enumerable collection of disposable objects to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="disposables"/> is null.</exception>
        public void AddRange(params IEnumerable<IDisposable?> disposables)
        {
            Throw.IfNull(disposables);
            _disposables.AddRange(disposables);
        }

#if NET || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Adds a range of disposable objects to the aggregate from a read-only span.
        /// </summary>
        /// <param name="disposables">A read-only span of disposable objects to add.</param>
        public void AddRange(params ReadOnlySpan<IDisposable?> disposables)
        {
            _disposables.AddRange(disposables.ToArray());
        }
#endif

        /// <summary>
        /// Disposes all aggregated disposables.
        /// </summary>
        /// <exception cref="AggregateException">
        /// Thrown if one or more exceptions occur during the disposal of the aggregated disposables.
        /// </exception>
        protected override void OnDispose()
        {
            List<Exception>? exceptions = null;
            foreach (var disposable in _disposables)
            {
                try
                {
                    disposable?.Dispose();
                }
                catch (Exception ex)
                {
                    exceptions ??= [];
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
