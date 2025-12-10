using System.Threading.Tasks;

namespace System.Threading
{
    /// <summary>
    /// Represents an asynchronous wait handle for a WaitHandle with support for timeouts and cancellation.
    /// </summary>
    public sealed class AsyncWaitHandle : Disposable
    {
        private readonly WaitHandle _waitObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncWaitHandle"/> class.
        /// </summary>
        /// <param name="waitObject">The <see cref="WaitHandle"/> object to be used for synchronization.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="waitObject"/> is null.</exception>
        public AsyncWaitHandle(WaitHandle waitObject)
        {
            ArgumentNullException.ThrowIfNull(waitObject);

            _waitObject = waitObject;
        }

        #region Methods

        protected override void OnDispose()
        {
            _waitObject.Dispose();
            base.OnDispose();
        }

        /// <summary>
        /// Asynchronously waits indefinitely until the wait handle is signaled.
        /// </summary>
        /// <returns>A task that represents the result of the wait operation.</returns>
        public Task<bool> WaitOneAsync()
        {
            return WaitOneAsync(Timeout.InfiniteTimeSpan, default);
        }

        /// <summary>
        /// Asynchronously waits indefinitely until the wait handle is signaled or the provided token is canceled.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token used to cancel the wait operation.</param>
        /// <returns>A task that represents the result of the wait operation.</returns>
        public Task<bool> WaitOneAsync(CancellationToken cancellationToken)
        {
            return WaitOneAsync(Timeout.InfiniteTimeSpan, cancellationToken);
        }

        /// <summary>
        /// Asynchronously waits for the specified number of milliseconds until the wait handle is signaled or the provided token is canceled.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait.</param>
        /// <param name="cancellationToken">A cancellation token used to cancel the wait operation.</param>
        /// <returns>A task that represents the result of the wait operation.</returns>
        public Task<bool> WaitOneAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return WaitOneAsync(TimeSpan.FromMilliseconds(millisecondsTimeout), cancellationToken);
        }

        /// <summary>
        /// Asynchronously waits for the specified number of milliseconds until the wait handle is signaled.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait.</param>
        /// <returns>A task that represents the result of the wait operation.</returns>
        public Task<bool> WaitOneAsync(int millisecondsTimeout)
        {
            return WaitOneAsync(TimeSpan.FromMilliseconds(millisecondsTimeout), default);
        }

        /// <summary>
        /// Asynchronously waits for the specified time span until the wait handle is signaled or the provided token is canceled.
        /// </summary>
        /// <param name="timeout">The maximum time to wait.</param>
        /// <param name="cancellationToken">A cancellation token used to cancel the wait operation.</param>
        /// <returns>A task that represents the result of the wait operation.</returns>
        public async Task<bool> WaitOneAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_waitObject.WaitOne(0))
            {
                return true;
            }
            if (timeout == TimeSpan.Zero)
            {
                return false;
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void WaitOrTimerCallback(object? state, bool timedOut)
            {
                tcs.TrySetResult(!timedOut);
            }

            var handle = ThreadPool.RegisterWaitForSingleObject(_waitObject, WaitOrTimerCallback, null, timeout, true);
            try
            {
                using (cancellationToken.CanBeCanceled
                           ? cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false)
                           : null as IDisposable)
                {
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                handle.Unregister(null);
            }
        }

        /// <summary>
        /// Asynchronously waits for the specified time span until the wait handle is signaled.
        /// </summary>
        /// <param name="timeout">The maximum time to wait.</param>
        /// <returns>A task that represents the result of the wait operation.</returns>
        public Task<bool> WaitOneAsync(TimeSpan timeout)
        {
            return WaitOneAsync(timeout, default);
        }

        #endregion
    }
}
