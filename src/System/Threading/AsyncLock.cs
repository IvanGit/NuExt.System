using System.Diagnostics;

namespace System.Threading
{
    /// <summary>
    /// A simple lock that supports asynchronicity and mutual exclusion.
    /// This lock ensures that only one thread can enter the critical section at a time,
    /// and provides both synchronous and asynchronous methods for acquiring and releasing the lock.
    /// Note that this lock is <b>not</b> recursive, meaning that if a thread tries to re-enter 
    /// the lock it already holds, it will result in a deadlock.
    /// If you need reentrance, consider using the <see cref="ReentrantAsyncLock"/> class instead.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class AsyncLock : Disposable
    {
        #region Internal classes

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock _lock;
            private bool _disposed;

            public Releaser(AsyncLock recursiveLock)
            {
                _lock = recursiveLock;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    try
                    {
                        _lock.Exit();
                    }
                    finally
                    {
                        _disposed = true;
                    }
                }
            }
        }

        #endregion

        private readonly SemaphoreSlim _syncLock = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock"/> class 
        /// without a synchronization context.
        /// </summary>
        public AsyncLock()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock"/> class 
        /// with the specified synchronization context.
        /// </summary>
        /// <param name="synchronizationContext">
        /// An optional <see cref="SynchronizationContext"/> to use for property change notifications. 
        /// If null, no synchronization context will be used.
        /// </param>
        public AsyncLock(SynchronizationContext? synchronizationContext) : base(synchronizationContext) { }

        #region Properties

        /// <summary>
        /// Determines whether the current context holds the lock.
        /// </summary>
        /// <remarks>
        /// This property returns true if the lock is currently held by any context, and false otherwise.
        /// Note that this check is instantaneous and does not block the calling thread.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        public bool IsEntered
        {
            get
            {
                CheckDisposed();
                return _syncLock.CurrentCount == 0;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Acquires an exclusive lock for the current context.
        /// This method will block until the lock is available or the specified 
        /// cancellation token is canceled. It ensures that only one thread can 
        /// enter the critical section protected by this lock at a time.
        /// </summary>
        /// <param name="cancellationToken">
        /// A CancellationToken to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        public void Enter(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            _syncLock.Wait(cancellationToken);
        }

        /// <summary>
        /// Asynchronously acquires an exclusive lock for the current context.
        /// This method will asynchronously block until the lock is available or the specified 
        /// cancellation token is canceled. It ensures that only one thread can enter the critical 
        /// section protected by this lock at a time.
        /// </summary>
        /// <param name="cancellationToken">
        /// A CancellationToken to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A ValueTask representing the asynchronous operation of acquiring the lock.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        public async ValueTask EnterAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Releases the exclusive lock for the current context.
        /// This method should be called to release a previously acquired lock. 
        /// If the lock has been entered multiple times, it will only be fully released 
        /// after the corresponding number of calls to <see cref="Enter"/> or <see cref="EnterAsync"/> have been balanced by calls to <see cref="Exit"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        public void Exit()
        {
            CheckDisposed();
            _syncLock.Release();
        }

        /// <summary>
        /// Acquires an exclusive lock for the current context and returns an IDisposable 
        /// that will release the lock upon disposal.
        /// This method combines lock acquisition and release management into a single, convenient API. 
        /// It ensures that the lock is properly released when the returned IDisposable is disposed, 
        /// typically using a 'using' statement.
        /// </summary>
        /// <param name="cancellationToken">
        /// A CancellationToken to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// An IDisposable that releases the acquired lock when disposed.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        public IDisposable Lock(CancellationToken cancellationToken = default)
        {
            Enter(cancellationToken);
            return new Releaser(this);
        }

        /// <summary>
        /// Asynchronously acquires an exclusive lock for the current context and returns an IDisposable 
        /// that will release the lock upon disposal.
        /// This method combines lock acquisition and release management into a single, convenient API. 
        /// It ensures that the lock is properly released when the returned IDisposable is disposed, 
        /// typically using a 'using' statement or pattern.
        /// </summary>
        /// <param name="cancellationToken">
        /// A CancellationToken to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A ValueTask representing the asynchronous operation of acquiring the lock, which upon completion 
        /// provides an IDisposable that releases the acquired lock when disposed.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        public async ValueTask<IDisposable> LockAsync(CancellationToken cancellationToken = default)
        {
            await EnterAsync(cancellationToken).ConfigureAwait(false);
            return new Releaser(this);
        }

        protected override void OnDispose()
        {
            _syncLock.Dispose();
            base.OnDispose();
        }

        /// <summary>
        /// Attempts to acquire an exclusive lock for the current context without blocking.
        /// This method returns immediately, indicating whether the lock was successfully acquired or not.
        /// </summary>
        /// <returns>
        /// True if the lock was successfully acquired; otherwise, false.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        public bool TryEnter()
        {
            CheckDisposed();
            return _syncLock.Wait(0);
        }

        #endregion
    }
}
