namespace System
{
    /// <summary>
    /// Provides a mechanism to query the disposal state of an object.
    /// </summary>
    public interface IDisposableState
    {
        /// <summary>
        /// Gets a value indicating whether the object is currently being disposed.
        /// </summary>
        bool IsDisposing { get; }

        /// <summary>
        /// Gets a value indicating whether the object has been disposed.
        /// </summary>
        bool IsDisposed { get; }
    }
}
