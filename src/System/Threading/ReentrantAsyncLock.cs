﻿#define DEBUG_WRITELINE_
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Threading
{
    /// <summary>
    /// A reentrant lock that supports asynchronous operations and mutual exclusion.
    /// This lock allows the same thread or code flow to enter it multiple times recursively without deadlocks.
    /// It provides synchronous and asynchronous methods for acquiring and releasing the lock,
    /// making it suitable for scenarios where async/await patterns or other forms of asynchronous programming are used.
    /// </summary>
    public sealed partial class ReentrantAsyncLock : Disposable
    {
        /// <summary>
        /// A static counter used to generate unique identifiers for lock instances.
        /// </summary>
        private static int s_idCounter;

        /// <summary>
        /// Semaphore to ensure mutual exclusion for synchronous and asynchronous operations.
        /// </summary>
        /// <remarks>
        /// Initialized with a count of 1, it allows only one concurrent access at a time,
        /// serving as the primary locking mechanism for this class.
        /// </remarks>
        private readonly SemaphoreSlim _syncLock = new(1, 1);
        /// <summary>
        /// An async-local storage for holding a unique identifier for each flow of execution (e.g., thread or task).
        /// This helps in managing reentrant entries specific to the current context.
        /// </summary>
        private readonly AsyncLocal<int> _localId = new();

        /// <summary>
        /// An async-local storage to hold the synchronization root (SemaphoreSlim) for reentrant lock acquisition within the same context.
        /// </summary>
        private readonly AsyncLocal<SemaphoreSlim?> _localSyncRoot = new();

        /// <summary>
        /// The current count of reentrant entries into the lock by the current flow of execution.
        /// Used to track how many times the lock has been entered without being fully released.
        /// </summary>
        private volatile int _currentCount;

        /// <summary>
        /// The identifier of the current flow of execution that holds the lock.
        /// Helps in identifying which context currently holds the lock.
        /// </summary>
        private volatile int _currentId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReentrantAsyncLock"/> class 
        /// without a synchronization context.
        /// </summary>
        public ReentrantAsyncLock()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReentrantAsyncLock"/> class 
        /// with the specified synchronization context.
        /// </summary>
        /// <param name="synchronizationContext">
        /// An optional <see cref="SynchronizationContext"/> to use for property change notifications. 
        /// If null, no synchronization context will be used.
        /// </param>
        public ReentrantAsyncLock(SynchronizationContext? synchronizationContext) : base(synchronizationContext) { }

        #region Properties

        /// <summary>
        /// Gets the current reentrant entry count for the lock.
        /// </summary>
        /// <remarks>
        /// This property indicates how many times the current flow of execution (e.g., thread or task) 
        /// has entered this reentrant lock. It is particularly useful for debugging and diagnostic purposes 
        /// to understand the depth of recursive locking.
        /// 
        /// Note:
        /// - The value represents the number of times the lock has been entered without being fully released.
        /// - This property does not trigger PropertyChanged notifications to avoid potential recursion issues.
        /// </remarks>
        public int CurrentCount => _currentCount;

        /// <summary>
        /// Gets the current identifier associated with the lock.
        /// </summary>
        /// <remarks>
        /// This property returns an identifier that is associated with the current flow of execution 
        /// (such as a thread or task) holding the lock. It is primarily used internally to manage reentrant locking.
        /// 
        /// Note:
        /// - The identifier helps in tracking which context currently holds the lock.
        /// - This value can be useful for debugging and diagnostic purposes.
        /// - This property does not trigger PropertyChanged notifications because it is intended for internal use only,
        ///   and also to avoid potential recursion issues.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal int CurrentId => _currentId;

        /// <summary>
        /// Determines whether the current context holds the lock.
        /// </summary>
        /// <remarks>
        /// This property returns true if the lock is currently held by any context, and false otherwise.
        /// Note that this check is instantaneous and does not block the calling thread.
        /// 
        /// This property does not raise PropertyChanged notifications to avoid potential recursion issues.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the ReentrantAsyncLock has been disposed.
        /// </exception>
        public bool IsEntered
        {
            get
            {
                CheckDisposed();
                return _syncLock.CurrentCount == 0;
            }
        }

        /// <summary>
        /// Gets the ambient local identifier for the current instance.
        /// </summary>
        /// <remarks>
        /// If the identifier has not been set, it generates a new identifier using the <see cref="NewId"/> method.
        /// This identifier is unique to the context and helps manage reentrant lock acquisition.
        /// Primarily intended for internal use.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal int LocalId => _localId.Value != 0 ? _localId.Value : (_localId.Value = NewId());

        /// <summary>
        /// Gets or sets the thread-local synchronization root for nested (reentrant) lock acquisition.
        /// </summary>
        /// <remarks>
        /// This property provides a thread-local <see cref="SemaphoreSlim"/> instance used to manage reentrant locks.
        /// Intended for internal use only.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal SemaphoreSlim? SyncRoot
        {
            get => _localSyncRoot.Value;
            private set => _localSyncRoot.Value = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Acquires an exclusive lock for the current context, executes the provided action,
        /// and then releases the lock. This method ensures that the specified action is executed
        /// within a mutually exclusive section, supporting reentrance within the same code flow.
        /// </summary>
        /// <param name="action">
        /// The <see cref="Action"/> to be executed while holding the lock. Must not be null.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="action"/> is null.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the object has been disposed.
        /// </exception>
        /// <exception cref="SynchronizationLockException">
        /// Thrown if there is an inconsistency in the synchronization state.
        /// </exception>
        /// <remarks>
        /// The method ensures thread-safe execution of the provided action by acquiring either an exclusive
        /// or a nested (reentrant) lock depending on the calling thread's context. For reentrant locks,
        /// the method uses a separate <see cref="SemaphoreSlim"/> instance to manage nested lock acquisition.
        /// </remarks>
        public void Acquire(Action action, CancellationToken cancellationToken = default)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(action);
#else
            ThrowHelper.WhenNull(action);
#endif
            CheckDisposed();

            bool isReentrant = LocalId == CurrentId;
            SemaphoreSlim? syncRoot = null;
            if (isReentrant)//Reentrant lock in the flow
            {
                ValidateReentrantState();

                syncRoot = SyncRoot;
                syncRoot!.Wait(cancellationToken);//Acquires a nested lock in the flow
            }
            else
            {
                _syncLock.Wait(cancellationToken);//Acquires an exclusive lock in the flow
#if DEBUG_WRITELINE
                Debug_WriteLine("Lock taken");
#endif
                ValidateExclusiveState();

                _currentId = LocalId;//The lock is obtained
            }

            Interlocked.Increment(ref _currentCount);

            using SemaphoreSlim localSyncRoot = new(1, 1);
            SyncRoot = localSyncRoot;//Sets synchronization root for a nested lock

            try
            {
#if DEBUG_WRITELINE
                Debug_WriteLine("Executing");
#endif
                action();
            }
            finally
            {
#if DEBUG_WRITELINE
                Debug_WriteLine("Executed");
#endif
                PerformValidationAfterExecution(isReentrant, localSyncRoot);

                SyncRoot = syncRoot;// Restore the previous synchronization root to maintain correct synchronization state and nested lock handling in the current flow.

                CheckSynchronizationLock();

                if (Interlocked.Decrement(ref _currentCount) == 0)
                {
                    ValidateExclusiveStateFinally(isReentrant, syncRoot);
                    _currentId = 0;//The lock is released
#if DEBUG_WRITELINE
                    Debug_WriteLine("Lock released");
#endif
                    _syncLock.Release();//Releases an exclusive lock
                }
                else
                {
                    ValidateReentrantStateFinally(isReentrant, syncRoot);
                    syncRoot!.Release();//Releases a nested lock
                }
            }
        }

        /// <summary>
        /// Acquires an exclusive lock for the current context, executes the provided function,
        /// and then releases the lock. This method ensures that the specified function is executed
        /// within a mutually exclusive section, supporting reentrance within the same code flow.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the result produced by the function.
        /// </typeparam>
        /// <param name="func">
        /// The <see cref="Func{T}"/> to be executed while holding the lock. Must not be null.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// The result of the execution of the provided function.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="func"/> is null.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the object has been disposed.
        /// </exception>
        /// <exception cref="SynchronizationLockException">
        /// Thrown if there is an inconsistency in the synchronization state.
        /// </exception>
        /// <remarks>
        /// The method ensures thread-safe execution of the provided function by acquiring either an exclusive
        /// or a nested (reentrant) lock depending on the calling thread's context. For reentrant locks,
        /// the method uses a separate <see cref="SemaphoreSlim"/> instance to manage nested lock acquisition.
        /// </remarks>
        public T Acquire<T>(Func<T> func, CancellationToken cancellationToken = default)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(func);
#else
            ThrowHelper.WhenNull(func);
#endif
            CheckDisposed();

            bool isReentrant = LocalId == CurrentId;
            SemaphoreSlim? syncRoot = null;
            if (isReentrant)//Reentrant lock in the flow
            {
                ValidateReentrantState();

                syncRoot = SyncRoot;
                syncRoot!.Wait(cancellationToken);//Acquires a nested lock in the flow
            }
            else
            {
                _syncLock.Wait(cancellationToken);//Acquires an exclusive lock in the flow
#if DEBUG_WRITELINE
                Debug_WriteLine("Lock taken");
#endif
                ValidateExclusiveState();

                _currentId = LocalId;//The lock is obtained
            }

            Interlocked.Increment(ref _currentCount);

            using SemaphoreSlim localSyncRoot = new(1, 1);
            SyncRoot = localSyncRoot;//Sets synchronization root for a nested lock

            try
            {
#if DEBUG_WRITELINE
                Debug_WriteLine("Executing");
#endif
                return func();
            }
            finally
            {
#if DEBUG_WRITELINE
                Debug_WriteLine("Executed");
#endif
                PerformValidationAfterExecution(isReentrant, localSyncRoot);

                SyncRoot = syncRoot;// Restore the previous synchronization root to maintain correct synchronization state and nested lock handling in the current flow.

                CheckSynchronizationLock();

                if (Interlocked.Decrement(ref _currentCount) == 0)
                {
                    ValidateExclusiveStateFinally(isReentrant, syncRoot);
                    _currentId = 0;//The lock is released
#if DEBUG_WRITELINE
                    Debug_WriteLine("Lock released");
#endif
                    _syncLock.Release();//Releases an exclusive lock
                }
                else
                {
                    ValidateReentrantStateFinally(isReentrant, syncRoot);
                    syncRoot!.Release();//Releases a nested lock
                }
            }
        }

        /// <summary>
        /// Asynchronously acquires an exclusive lock for the current context, executes the provided asynchronous function,
        /// and then releases the lock. This method ensures that the specified function is executed
        /// within a mutually exclusive section, supporting reentrance within the same code flow.
        /// </summary>
        /// <param name="func">
        /// The <see cref="Func{ValueTask}"/> to be executed while holding the lock. Must not be null.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask"/> representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="func"/> is null.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the object has been disposed.
        /// </exception>
        /// <exception cref="SynchronizationLockException">
        /// Thrown if there is an inconsistency in the synchronization state.
        /// </exception>
        /// <remarks>
        /// The method ensures thread-safe execution of the provided function by acquiring either an exclusive
        /// or a nested (reentrant) lock depending on the calling thread's context. For reentrant locks,
        /// the method uses a separate <see cref="SemaphoreSlim"/> instance to manage nested lock acquisition.
        /// </remarks>
        public async ValueTask AcquireAsync(Func<ValueTask> func, CancellationToken cancellationToken = default)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(func);
#else
            ThrowHelper.WhenNull(func);
#endif
            CheckDisposed();

            bool isReentrant = LocalId == CurrentId;
            SemaphoreSlim? syncRoot = null;
            if (isReentrant)//Reentrant lock in the flow
            {
                ValidateReentrantState();

                syncRoot = SyncRoot;
                await syncRoot!.WaitAsync(cancellationToken).ConfigureAwait(false);//Acquires a nested lock in the flow
            }
            else
            {
                await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);//Acquires an exclusive lock in the flow
#if DEBUG_WRITELINE
                Debug_WriteLine("Lock taken");
#endif
                ValidateExclusiveState();

                _currentId = LocalId;//The lock is obtained
            }

            Interlocked.Increment(ref _currentCount);

            using SemaphoreSlim localSyncRoot = new(1, 1);
            SyncRoot = localSyncRoot;//Sets synchronization root for a nested lock

            try
            {
#if DEBUG_WRITELINE
                Debug_WriteLine("Executing");
#endif
                await func().ConfigureAwait(false);
            }
            finally
            {
#if DEBUG_WRITELINE
                Debug_WriteLine("Executed");
#endif
                PerformValidationAfterExecution(isReentrant, localSyncRoot);

                SyncRoot = null;//Clear the synchronization root to allow garbage collection

                CheckSynchronizationLock();

                if (Interlocked.Decrement(ref _currentCount) == 0)
                {
                    ValidateExclusiveStateFinally(isReentrant, syncRoot);
                    _currentId = 0;//The lock is released
#if DEBUG_WRITELINE
                    Debug_WriteLine("Lock released");
#endif
                    _syncLock.Release();//Releases an exclusive lock
                }
                else
                {
                    ValidateReentrantStateFinally(isReentrant, syncRoot);
                    syncRoot!.Release();//Releases a nested lock
                }
            }
        }

        /// <summary>
        /// Asynchronously acquires an exclusive lock for the current context, executes the provided asynchronous function
        /// that returns a result, and then releases the lock. This method ensures that the specified function is executed
        /// within a mutually exclusive section, supporting reentrance within the same code flow.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the result produced by the function.
        /// </typeparam>
        /// <param name="func">
        /// The <see cref="ValueTask{T}"/> to be executed while holding the lock. Must not be null.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{T}"/> representing the result of the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="func"/> is null.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the object has been disposed.
        /// </exception>
        /// <exception cref="SynchronizationLockException">
        /// Thrown if there is an inconsistency in the synchronization state.
        /// </exception>
        /// <remarks>
        /// The method ensures thread-safe execution of the provided function by acquiring either an exclusive
        /// or a nested (reentrant) lock depending on the calling thread's context. For reentrant locks,
        /// the method uses a separate <see cref="SemaphoreSlim"/> instance to manage nested lock acquisition.
        /// </remarks>
        public async ValueTask<T> AcquireAsync<T>(Func<ValueTask<T>> func, CancellationToken cancellationToken = default)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(func);
#else
            ThrowHelper.WhenNull(func);
#endif
            CheckDisposed();

            bool isReentrant = LocalId == CurrentId;
            SemaphoreSlim? syncRoot = null;
            if (isReentrant)//Reentrant lock in the flow
            {
                ValidateReentrantState();

                syncRoot = SyncRoot;
                await syncRoot!.WaitAsync(cancellationToken).ConfigureAwait(false);//Acquires a nested lock in the flow
            }
            else
            {
                await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);//Acquires an exclusive lock in the flow
#if DEBUG_WRITELINE
                Debug_WriteLine("Lock taken");
#endif
                ValidateExclusiveState();

                _currentId = LocalId;//The lock is obtained
            }

            Interlocked.Increment(ref _currentCount);

            using SemaphoreSlim localSyncRoot = new(1, 1);
            SyncRoot = localSyncRoot;//Sets synchronization root for a nested lock

            try
            {
#if DEBUG_WRITELINE
                Debug_WriteLine("Executing");
#endif
                return await func().ConfigureAwait(false);
            }
            finally
            {
#if DEBUG_WRITELINE
                Debug_WriteLine("Executed");
#endif
                PerformValidationAfterExecution(isReentrant, localSyncRoot);

                SyncRoot = null;//Clear the synchronization root to allow garbage collection

                CheckSynchronizationLock();

                if (Interlocked.Decrement(ref _currentCount) == 0)
                {
                    ValidateExclusiveStateFinally(isReentrant, syncRoot);
                    _currentId = 0;//The lock is released
#if DEBUG_WRITELINE
                    Debug_WriteLine("Lock released");
#endif
                    _syncLock.Release();//Releases an exclusive lock
                }
                else
                {
                    ValidateReentrantStateFinally(isReentrant, syncRoot);
                    syncRoot!.Release();//Releases a nested lock
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckSynchronizationLock()
        {
            if (LocalId != CurrentId)
            {
                ThrowSynchronizationLockException();
            }
        }

#if DEBUG_WRITELINE
        [Conditional("DEBUG")]
        private void Debug_WriteLine(string action, [CallerMemberName]string? method = null)
        {
            Debug.WriteLine($"{action} {method} with Id={LocalId}, CurrentCount={CurrentCount}, Thread={Environment.CurrentManagedThreadId}");
        }
#endif


        /// <summary>
        /// Gets a unique ID
        /// </summary>
        internal static int NewId()
        {
            int newId;

            // We need to repeat if Interlocked.Increment wraps around and returns 0.
            // Otherwise next time this Id is queried it will get a new value
            do
            {
                newId = Interlocked.Increment(ref s_idCounter);
            }
            while (newId == 0);
            return newId;
        }

        protected override void OnDispose()
        {
            _syncLock.Dispose();
            base.OnDispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowSynchronizationLockException()
        {
            throw new SynchronizationLockException();
        }

        #endregion
    }
}
