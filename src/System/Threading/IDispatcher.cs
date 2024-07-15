namespace System.Threading
{
    /// <summary>
    /// Defines a contract for a dispatcher that manages thread access.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Gets the thread associated with this dispatcher.
        /// </summary>
        Thread Thread { get; }

        /// <summary>
        /// Determines whether the calling thread has access to this dispatcher's thread.
        /// </summary>
        /// <returns>
        /// True if the calling thread has access to this dispatcher's thread; otherwise, false.
        /// </returns>
        bool CheckAccess();
    }
}
