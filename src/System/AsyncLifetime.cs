using System.Diagnostics;
using System.Runtime.CompilerServices;

// Based on Станислав Сидристый «Шаблон Lifetime: для сложного Disposing»
// https://www.youtube.com/watch?v=F5oOYKTFpcQ

namespace System
{
    /// <summary>
    /// Manages the asynchronous lifecycle of resources, ensuring that all registered cleanup actions are executed upon disposal.
    /// </summary>
    public sealed class AsyncLifetime : IAsyncLifetime
    {
        private readonly List<Func<ValueTask>> _actions = new();
        private readonly SemaphoreSlim _syncLock = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLifetime"/> class with the default setting 
        /// for ContinueOnCapturedContext, which is false.
        /// </summary>
        /// <remarks>
        /// This constructor initializes the <see cref="AsyncLifetime"/> instance with 
        /// <c>ContinueOnCapturedContext</c> set to <c>false</c>. By default, the current execution context 
        /// will not be captured to continue asynchronous operations in the <see cref="DisposeAsync"/> method.
        /// </remarks>
        public AsyncLifetime() : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLifetime"/> class with the specified settings.
        /// </summary>
        /// <param name="continueOnCapturedContext">Determines whether the current execution context should be captured 
        /// and used to continue asynchronous operations in the <see cref="DisposeAsync"/> method.</param>
        /// <remarks>
        /// When an instance of <see cref="AsyncLifetime"/> is disposed asynchronously, the actions added to it will 
        /// be executed. If <paramref name="continueOnCapturedContext"/> is set to true, the original synchronization context 
        /// will be used to continue the asynchronous operations.
        /// </remarks>
        public AsyncLifetime(bool continueOnCapturedContext)
        {
            ContinueOnCapturedContext = continueOnCapturedContext;
            Add(() => IsTerminated = true);
        }

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the current execution context should be captured 
        /// and used to continue asynchronous operations in the DisposeAsync method.
        /// </summary>
        public bool ContinueOnCapturedContext { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has been terminated. 
        /// When true, no more actions can be added, and disposal is either in progress or completed.
        /// </summary>
        public bool IsTerminated { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds an action to be executed when the <see cref="AsyncLifetime"/> instance is disposed asynchronously.
        /// </summary>
        /// <param name="action">The action to add. This action will be called upon asynchronous disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if the action is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add an action to a terminated <see cref="AsyncLifetime"/> instance.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Action action)
        {
            Debug.Assert(action != null, $"{nameof(action)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(action);
#else
            Throw.IfNull(action);
#endif
            AddAsync(() =>
            {
                action();
                return default;
            });
        }

        /// <summary>
        /// Adds an asynchronous action to be executed when the <see cref="AsyncLifetime"/> instance is disposed asynchronously.
        /// </summary>
        /// <param name="action">The asynchronous action to add. This action will be called upon asynchronous disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if the action is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add an action to a terminated <see cref="AsyncLifetime"/> instance.</exception>
        public void AddAsync(Func<ValueTask> action)
        {
            Debug.Assert(action != null, $"{nameof(action)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(action);
#else
            Throw.IfNull(action);
#endif
            _syncLock.Wait();
            try
            {
                CheckTerminated();
                _actions.Add(action);
            }
            finally
            {
                _syncLock.Release();
            }
        }

        /// <summary>
        /// Adds an IAsyncDisposable object to be disposed of asynchronously when the <see cref="AsyncLifetime"/> instance is disposed.
        /// </summary>
        /// <param name="disposable">The asynchronous disposable object to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the disposable object is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add a disposable object to a terminated <see cref="AsyncLifetime"/> instance.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddAsyncDisposable(IAsyncDisposable disposable)
        {
            Debug.Assert(disposable != null, $"{nameof(disposable)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(disposable);
#else
            Throw.IfNull(disposable);
#endif
            AddAsync(disposable.DisposeAsync);
        }

        /// <summary>
        /// Adds a pair of actions: one to be executed immediately (subscribe) and another to be executed during asynchronous disposal (unsubscribe).
        /// </summary>
        /// <param name="subscribe">The action to execute immediately.</param>
        /// <param name="unsubscribe">The action to execute upon asynchronous disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if either subscribe or unsubscribe is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add actions to a terminated <see cref="AsyncLifetime"/> instance.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBracket(Action subscribe, Action unsubscribe)
        {
            Debug.Assert(subscribe != null, $"{nameof(subscribe)} is null");
            Debug.Assert(unsubscribe != null, $"{nameof(unsubscribe)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(subscribe);
            ArgumentNullException.ThrowIfNull(unsubscribe);
#else
            Throw.IfNull(subscribe);
            Throw.IfNull(unsubscribe);
#endif
            subscribe();
            Add(unsubscribe);
        }

        /// <summary>
        /// Adds an IDisposable object to be disposed of when the <see cref="AsyncLifetime"/> instance is disposed asynchronously.
        /// </summary>
        /// <param name="disposable">The disposable object to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the disposable object is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add a disposable object to a terminated <see cref="AsyncLifetime"/> instance.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddDisposable(IDisposable disposable)
        {
            Debug.Assert(disposable != null, $"{nameof(disposable)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(disposable);
#else
            Throw.IfNull(disposable);
#endif
            Add(disposable.Dispose);
        }

        /// <summary>
        /// Adds a reference to an object to keep it alive until the <see cref="AsyncLifetime"/> instance is disposed asynchronously.
        /// </summary>
        /// <param name="obj">The object to keep alive.</param>
        /// <exception cref="ArgumentNullException">Thrown if the object is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add a reference to a terminated <see cref="AsyncLifetime"/> instance.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRef(object obj)
        {
            Debug.Assert(obj != null, $"{nameof(obj)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(obj);
#else
            Throw.IfNull(obj);
#endif
            Add(() => GC.KeepAlive(obj));
        }

        /// <summary>
        /// Checks whether the <see cref="AsyncLifetime"/> instance has been terminated and throws an exception if it has.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the <see cref="AsyncLifetime"/> instance is terminated.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckTerminated()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(IsTerminated, this);
#else
            if (IsTerminated)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
#endif
        }

        /// <summary>
        /// Executes all added asynchronous actions in reverse order and marks the instance as terminated.
        /// Ensures that all resources are released asynchronously.
        /// </summary>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (IsTerminated)
            {
                return;
            }
            await _syncLock.WaitAsync().ConfigureAwait(ContinueOnCapturedContext);
            try
            {
                for (int i = _actions.Count - 1; i >= 0; i--)
                {
                    await _actions[i]().ConfigureAwait(ContinueOnCapturedContext);
                }
                _actions.Clear();
            }
            finally
            {
                _syncLock.Release();
            }
            Debug.Assert(IsTerminated, $"{nameof(AsyncLifetime)} is not terminated");
        }

        #endregion
    }
}
