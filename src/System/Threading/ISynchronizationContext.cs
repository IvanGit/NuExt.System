namespace System.Threading
{
    /// <summary>
    /// Provides a basic abstraction for dispatching work to synchronization contexts.
    /// </summary>
    public interface ISynchronizationContext
    {
        /// <summary>
        /// Synchronously executes a delegate on the synchronization context.
        /// </summary>
        /// <param name="d">The delegate to execute.</param>
        /// <param name="state">An optional state object passed to the delegate.</param>
        /// <remarks>
        /// This method blocks the calling thread until the delegate completes execution.
        /// It is typically used for operations that require immediate execution on the target context,
        /// such as reading properties from UI controls.
        /// </remarks>
        void Send(SendOrPostCallback d, object? state);

        /// <summary>
        /// Asynchronously executes a delegate on the synchronization context.
        /// </summary>
        /// <param name="d">The delegate to execute.</param>
        /// <param name="state">An optional state object passed to the delegate.</param>
        /// <remarks>
        /// This method queues the delegate for execution and returns immediately.
        /// It is suitable for operations that can complete in the background, such as updating UI controls.
        /// </remarks>
        void Post(SendOrPostCallback d, object? state);
    }
}
