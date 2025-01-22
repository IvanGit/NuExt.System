namespace System.Threading
{
    /// <summary>
    /// Defines a contract for a dispatcher object that manages thread access.
    /// </summary>
    public interface IDispatcherObject
    {
        /// <summary>
        /// Gets the thread associated with this dispatcher object.
        /// </summary>
        Thread Thread { get; }

        /// <summary>
        /// Determines whether the calling thread has access to this dispatcher object's thread.
        /// </summary>
        /// <returns>
        /// True if the calling thread has access to this dispatcher object's thread; otherwise, false.
        /// </returns>
        bool CheckAccess();

        /// <summary>
        /// Verifies that the calling thread has access to this dispatcher object's thread and throws an <see cref="InvalidOperationException"/> if not.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the calling thread does not have access to this dispatcher object's thread.</exception>
        void VerifyAccess();
    }
}
