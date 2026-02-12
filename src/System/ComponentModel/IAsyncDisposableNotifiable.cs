namespace System.ComponentModel
{
    /// <summary>
    /// Provides asynchronous disposal with state tracking and a disposal notification event.
    /// </summary>
    public interface IAsyncDisposableNotifiable : IAsyncDisposable, IDisposableState
    {
        /// <summary>
        /// Occurs when the object starts the disposing process asynchronously.
        /// </summary>
        /// <remarks>
        /// Exceptions thrown by event handlers propagate to the caller of <see cref="IAsyncDisposable.DisposeAsync" />.
        /// </remarks>
        event AsyncEventHandler? Disposing;
    }
}
