namespace System.Threading
{
    /// <summary>
    /// Defines a contract for objects that are associated with a specific execution context
    /// and can verify access to that context.
    /// </summary>
    public interface IDispatcherObject
    {
        /// <summary>
        /// Determines whether the calling thread has access to this object's associated execution context.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the calling thread has access; otherwise, <see langword="false"/>.
        /// </returns>
        bool CheckAccess();

        /// <summary>
        /// Verifies that the calling thread has access to this object's associated execution context.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the calling thread does not have access.
        /// </exception>
        void VerifyAccess();
    }
}
