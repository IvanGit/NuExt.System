namespace System.ComponentModel
{
    /// <summary>
    /// Defines a contract for managing the lifecycle of resources and actions that should be executed upon disposal.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Registered operations execute during disposal in reverse (LIFO) order.
    /// </para>
    /// <para>
    /// Implementations of this interface should be thread-safe for concurrent calls to its methods
    /// and the <see cref="IDisposable.Dispose"/> method.
    /// </para>
    /// </remarks>
    public interface ILifetime : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the lifetime instance has been terminated.
        /// </summary>
        /// <value>
        /// <see langword="true"/> only after all registered actions have been executed during disposal;
        /// otherwise, <see langword="false"/>.
        /// </value>
        bool IsTerminated { get; }

        /// <summary>
        /// Adds an action to be executed when the lifetime instance is disposed.
        /// </summary>
        /// <param name="action">The action to add. This action will be called upon disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if the action is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add an action to a terminated lifetime instance.</exception>
        void Add(Action action);

        /// <summary>
        /// Adds a pair of actions: one to be executed immediately (subscribe) and another to be executed during disposal (unsubscribe).
        /// </summary>
        /// <param name="subscribe">The action to execute immediately.</param>
        /// <param name="unsubscribe">The action to execute upon disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if either subscribe or unsubscribe is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add actions to a terminated lifetime instance.</exception>
        void AddBracket(Action subscribe, Action unsubscribe);

        /// <summary>
        /// Adds an <see cref="IDisposable"/> object to be disposed of when the lifetime instance is disposed.
        /// </summary>
        /// <typeparam name="T">The type of the disposable object.</typeparam>
        /// <param name="disposable">The disposable object to add.</param>
        /// <returns>The disposable object that was added.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the disposable object is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add a disposable object to a terminated lifetime instance.</exception>
        T AddDisposable<T>(T disposable) where T: IDisposable;

        /// <summary>
        /// Adds a reference to an object to keep it alive until the lifetime instance is disposed.
        /// </summary>
        /// <typeparam name="T">The type of the object to keep alive. Must be a reference type.</typeparam>
        /// <param name="obj">The object to keep alive.</param>
        /// <returns>The object that was added to be kept alive.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the object is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add a reference to a terminated lifetime instance.</exception>
        T AddRef<T>(T obj) where T : class;
    }
}
