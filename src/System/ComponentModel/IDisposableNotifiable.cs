namespace System.ComponentModel
{
    /// <summary>
    /// Provides disposal with state tracking and a disposal notification event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="Disposing"/> event is raised synchronously during the call to <see cref="IDisposable.Dispose"/>.
    /// Event handlers must be fast and non-blocking to avoid deadlocks.
    /// </para>
    /// </remarks>
    public interface IDisposableNotifiable : IDisposable, IDisposableState
    {
        /// <summary>
        /// Occurs when the object starts the disposing process.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event is raised synchronously during the call to <see cref="IDisposable.Dispose"/>.
        /// Prefer <see cref="IAsyncDisposableNotifiable.Disposing"/> for asynchronous scenarios.
        /// </para>
        /// </remarks>
        event EventHandler? Disposing;
    }
}