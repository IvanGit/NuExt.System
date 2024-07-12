namespace System.Threading
{
    /// <summary>
    /// Defines methods for sending and posting asynchronous messages to a synchronization context.
    /// </summary>
    public interface ISynchronizationContext
    {
        /// <summary>
        /// Dispatches a synchronous message to the synchronization context.
        /// </summary>
        /// <param name="d">The delegate to call.</param>
        /// <param name="state">An object passed to the delegate.</param>
        /// <remarks>
        /// The method blocks until the delegate has run. This is typically used for operations that need 
        /// to run synchronously on a specific thread or context.
        /// </remarks>
        void Send(SendOrPostCallback d, object? state);

        /// <summary>
        /// Dispatches an asynchronous message to the synchronization context.
        /// </summary>
        /// <param name="d">The delegate to call.</param>
        /// <param name="state">An object passed to the delegate.</param>
        /// <remarks>
        /// The method queues the delegate to be run asynchronously on the synchronization context.
        /// This is typically used for operations that can run asynchronously without blocking the calling thread.
        /// </remarks>
        void Post(SendOrPostCallback d, object? state);
    }
}
