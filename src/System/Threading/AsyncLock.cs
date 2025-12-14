using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

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

        private sealed class Releaser(AsyncLock asyncLock) : IDisposable
        {
            private bool _disposed;

            public void Dispose()
            {
                if (!_disposed)
                {
                    asyncLock.Exit();
                    _disposed = true;
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
        /// Gets a value indicating whether the lock is currently held by any execution context.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property returns true if the lock is currently held by any context, and false otherwise.
        /// Note that this check is instantaneous and does not block the calling thread.
        /// It should not be used for making synchronization decisions, as this will introduce race conditions.
        /// </para>
        /// <para>
        /// This property is exposed internally for debugging and state validation only.
        /// </para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal bool IsEntered
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
        /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
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
        /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask"/> representing the asynchronous operation of acquiring the lock.
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
        /// Acquires an exclusive lock for the current context and returns an <see cref="IDisposable"/> 
        /// that will release the lock upon disposal.
        /// This method combines lock acquisition and release management into a single, convenient API. 
        /// It ensures that the lock is properly released when the returned <see cref="IDisposable"/> is disposed, 
        /// typically using a 'using' statement.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// An <see cref="IDisposable"/> that releases the acquired lock when disposed.
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
        /// Asynchronously acquires an exclusive lock for the current context and returns an <see cref="IDisposable"/> 
        /// that will release the lock upon disposal.
        /// This method combines lock acquisition and release management into a single, convenient API. 
        /// It ensures that the lock is properly released when the returned <see cref="IDisposable"/> is disposed, 
        /// typically using a 'using' statement or pattern.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation of acquiring the lock, which upon completion 
        /// provides an <see cref="IDisposable"/> that releases the acquired lock when disposed.
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

        protected override void DisposeCore()
        {
            _syncLock.Dispose();
            base.DisposeCore();
        }

        /// <summary>
        /// Attempts to acquire an exclusive lock for the current context without blocking.
        /// This method returns immediately, indicating whether the lock was successfully acquired or not.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the lock was successfully acquired; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        public bool TryEnter()
        {
            CheckDisposed();
            return _syncLock.Wait(0, CancellationToken.None);
        }

        /// <summary>
        /// Attempts to acquire an exclusive lock for the current context, observing a cancellation token.
        /// This method blocks until the lock is acquired or the cancellation token is canceled.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the lock was successfully acquired; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        public bool TryEnter(CancellationToken cancellationToken)
        {
            CheckDisposed();
            return _syncLock.Wait(Timeout.InfiniteTimeSpan, cancellationToken);
        }

        /// <summary>
        /// Attempts to acquire an exclusive lock for the current context within the specified timeout.
        /// This method blocks until the lock is acquired or the timeout expires.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> representing the maximum time to wait for the lock.
        /// To wait indefinitely, use <see cref="Timeout.InfiniteTimeSpan"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the lock was successfully acquired; 
        /// <see langword="false"/> if the timeout expired before the lock could be acquired.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="timeout"/> is a negative time other than <see cref="Timeout.InfiniteTimeSpan"/>.
        /// </exception>
        public bool TryEnter(TimeSpan timeout)
        {
            CheckDisposed();
            return _syncLock.Wait(timeout, CancellationToken.None);
        }

        /// <summary>
        /// Attempts to acquire an exclusive lock for the current context within the specified timeout.
        /// This method blocks until the lock is acquired, the timeout expires, or the cancellation token is canceled.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> representing the maximum time to wait for the lock.
        /// To wait indefinitely, use <see cref="Timeout.InfiniteTimeSpan"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the lock was successfully acquired; 
        /// <see langword="false"/> if the timeout expired before the lock could be acquired.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="timeout"/> is a negative time other than <see cref="Timeout.InfiniteTimeSpan"/>.
        /// </exception>
        public bool TryEnter(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            return _syncLock.Wait(timeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously attempts to acquire an exclusive lock for the current context without blocking.
        /// This method returns immediately with a result indicating whether the lock was successfully acquired or not.
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that completes with <see langword="true"/> if the lock was successfully acquired;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        public async ValueTask<bool> TryEnterAsync()
        {
            CheckDisposed();
            return await _syncLock.WaitAsync(0, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously attempts to acquire an exclusive lock for the current context, observing a cancellation token.
        /// This method asynchronously blocks until the lock is acquired or the cancellation token is canceled.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that completes with <see langword="true"/> if the lock was successfully acquired;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        public async ValueTask<bool> TryEnterAsync(CancellationToken cancellationToken)
        {
            CheckDisposed();
            return await _syncLock.WaitAsync(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously attempts to acquire an exclusive lock for the current context within the specified timeout.
        /// This method asynchronously blocks until the lock is acquired or the timeout expires.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> representing the maximum time to wait for the lock.
        /// To wait indefinitely, use <see cref="Timeout.InfiniteTimeSpan"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that completes with <see langword="true"/> if the lock was successfully acquired;
        /// <see langword="false"/> if the timeout expired before the lock could be acquired.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="timeout"/> is a negative time other than <see cref="Timeout.InfiniteTimeSpan"/>.
        /// </exception>
        public async ValueTask<bool> TryEnterAsync(TimeSpan timeout)
        {
            CheckDisposed();
            return await _syncLock.WaitAsync(timeout, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously attempts to acquire an exclusive lock for the current context within the specified timeout.
        /// This method asynchronously blocks until the lock is acquired, the timeout expires, or the cancellation token is canceled.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> representing the maximum time to wait for the lock.
        /// To wait indefinitely, use <see cref="Timeout.InfiniteTimeSpan"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting to acquire the lock.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that completes with <see langword="true"/> if the lock was successfully acquired;
        /// <see langword="false"/> if the timeout expired before the lock could be acquired.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the AsyncLock has been disposed.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="timeout"/> is a negative time other than <see cref="Timeout.InfiniteTimeSpan"/>.
        /// </exception>
        public async ValueTask<bool> TryEnterAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            return await _syncLock.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
        }
        #endregion
    }
}
