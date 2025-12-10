using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// Represents an asynchronous and synchronous disposable object that aggregates multiple disposables.
    /// When disposed, it disposes all aggregated disposables either asynchronously or synchronously.
    /// </summary>
    [Serializable]
    public sealed class AggregateAsyncDisposable : AsyncDisposable
    {
        /// <summary>
        /// The list of disposables to be managed and disposed together asynchronously.
        /// </summary>
        private readonly List<(IAsyncDisposable?, IDisposable?)> _disposables;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateAsyncDisposable"/> class with the specified async disposables and synchronization context.
        /// </summary>
        /// <param name="disposables">A read-only list of async disposables to aggregate.</param>
        /// <param name="synchronizationContext">An optional <see cref="SynchronizationContext"/> to use for property change notifications.</param>
        /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
        public AggregateAsyncDisposable(IReadOnlyList<IAsyncDisposable?> disposables, SynchronizationContext? synchronizationContext) : base(synchronizationContext)
        {
            _  = disposables ?? throw new ArgumentNullException(nameof(disposables));
            List<(IAsyncDisposable?, IDisposable?)> list = new(disposables.Count);
            foreach (var disposable in disposables)
            {
                list.Add((disposable, null));
            }
            _disposables = list;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateAsyncDisposable"/> class with the specified async disposables and synchronization context.
        /// </summary>
        /// <param name="disposables">A read-only list of async disposables to aggregate.</param>
        /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
        public AggregateAsyncDisposable(params IReadOnlyList<IAsyncDisposable?> disposables) : this(disposables, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateAsyncDisposable"/> class with the specified async disposables and synchronization context.
        /// </summary>
        /// <param name="disposables">A collection of async disposables to aggregate.</param>
        /// <param name="synchronizationContext">An optional <see cref="SynchronizationContext"/> to use for property change notifications.</param>
        /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
        public AggregateAsyncDisposable(IEnumerable<IAsyncDisposable?> disposables, SynchronizationContext? synchronizationContext) : base(synchronizationContext)
        {
            _ = disposables ?? throw new ArgumentNullException(nameof(disposables));
            List<(IAsyncDisposable?, IDisposable?)> list = [];
            foreach (var disposable in disposables)
            {
                list.Add((disposable, null));
            }
            _disposables = list;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateAsyncDisposable"/> class with the specified async disposables.
        /// </summary>
        /// <param name="disposables">A collection of disposables to aggregate.</param>
        /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
        public AggregateAsyncDisposable(params IEnumerable<IAsyncDisposable?> disposables) : this(disposables, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateAsyncDisposable"/> class with the specified async disposables.
        /// </summary>
        /// <param name="disposables">An array of disposables to aggregate.</param>
        /// <exception cref="ArgumentNullException">Thrown when disposables is null.</exception>
        public AggregateAsyncDisposable(params IAsyncDisposable?[] disposables) : this(disposables, null)
        {
        }

#if NET || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateAsyncDisposable"/> class with the specified async disposables.
        /// </summary>
        /// <param name="disposables">A read-only span of disposables to aggregate.</param>
        public AggregateAsyncDisposable(params ReadOnlySpan<IAsyncDisposable?> disposables)
        {
            List<(IAsyncDisposable?, IDisposable?)> list = new(disposables.Length);
            foreach (var disposable in disposables)
            {
                list.Add((disposable, null));
            }
            _disposables = list;
        }
#endif

        /// <summary>
        /// Adds a disposable object to the aggregate.
        /// </summary>
        /// <param name="disposable">The disposable object to add.</param>
        public void Add(IAsyncDisposable? disposable)
        {
            _disposables.Add((disposable, null));
        }

        /// <summary>
        /// Adds a disposable object to the aggregate.
        /// </summary>
        /// <param name="disposable">The disposable object to add.</param>
        [OverloadResolutionPriority(-1)]
        public void Add(IDisposable? disposable)
        {
            if (disposable is IAsyncDisposable asyncDisposable)
            {
                _disposables.Add((asyncDisposable, null));
            }
            else
            {
                _disposables.Add((null, disposable));
            }
        }

        /// <summary>
        /// Adds a range of disposable objects to the aggregate.
        /// </summary>
        /// <param name="disposables">An enumerable collection of disposable objects to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="disposables"/> is null.</exception>
        public void AddRange(params IEnumerable<IAsyncDisposable?> disposables)
        {
            Throw.IfNull(disposables);
            foreach (var disposable in disposables)
            {
                Add(disposable);
            }
        }

        /// <summary>
        /// Adds a range of disposable objects to the aggregate.
        /// </summary>
        /// <param name="disposables">An enumerable collection of disposable objects to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="disposables"/> is null.</exception>
        [OverloadResolutionPriority(-1)]
        public void AddRange(params IEnumerable<IDisposable?> disposables)
        {
            Throw.IfNull(disposables);
            foreach (var disposable in disposables)
            {
                Add(disposable);
            }
        }

#if NET || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Adds a range of disposable objects to the aggregate from a read-only span.
        /// </summary>
        /// <param name="disposables">A read-only span of disposable objects to add.</param>
        public void AddRange(params ReadOnlySpan<IAsyncDisposable?> disposables)
        {
            foreach (var disposable in disposables)
            {
                Add(disposable);
            }
        }

        /// <summary>
        /// Adds a range of disposable objects to the aggregate from a read-only span.
        /// </summary>
        /// <param name="disposables">A read-only span of disposable objects to add.</param>
        [OverloadResolutionPriority(-1)]
        public void AddRange(params ReadOnlySpan<IDisposable?> disposables)
        {
            foreach (var disposable in disposables)
            {
                Add(disposable);
            }
        }
#endif

        /// <summary>
        /// Disposes all aggregated disposables asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        /// <exception cref="AggregateException">Thrown if one or more exceptions occur during the disposal of the aggregated disposables.</exception>
        protected override async ValueTask OnDisposeAsync()
        {
            List<Exception>? exceptions = null;
            foreach (var (asyncDisposable, disposable) in _disposables)
            {
                try
                {
                    if (asyncDisposable != null)
                    {
                        await asyncDisposable.DisposeAsync();
                        continue;
                    }
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
            await base.OnDisposeAsync();
        }
    }
}
