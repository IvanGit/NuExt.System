namespace System.Threading
{
    /// <summary>
    /// Represents a synchronization context explicitly bound to a specific thread.
    /// </summary>
    public interface IThreadAffineSynchronizationContext : ISynchronizationContext, ISynchronizeInvoker
    {
        /// <summary>
        /// Gets the <see cref="System.Threading.Thread"/> to which this context is bound.
        /// </summary>
        /// <value>The thread on which work dispatched through this context executes.</value>
        Thread Thread { get; }
    }
}
