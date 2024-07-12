namespace System
{
    /// <summary>
    /// Defines a contract for managing the lifecycle of resources and actions that should be executed upon disposal.
    /// </summary>
    public interface ILifeTime : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the lifetime instance has been terminated.
        /// </summary>
        bool IsTerminated { get; }

        /// <summary>
        /// Adds an action to be executed when the lifetime instance is disposed.
        /// </summary>
        /// <param name="action">The action to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the action is null.</exception>
        void Add(Action action);

        /// <summary>
        /// Adds a pair of actions: one to execute immediately (setup) and another to execute on disposal (cleanup).
        /// </summary>
        /// <param name="action">The setup action to execute immediately.</param>
        /// <param name="release">The cleanup action to execute upon disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if either action or release is null.</exception>
        void AddBracket(Action action, Action release);

        /// <summary>
        /// Adds an IDisposable object to be disposed of when the lifetime instance is disposed.
        /// </summary>
        /// <param name="disposable">The disposable object to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the disposable object is null.</exception>
        void AddDisposable(IDisposable disposable);

        /// <summary>
        /// Adds a reference to an object to keep it alive until the lifetime instance is disposed.
        /// </summary>
        /// <param name="obj">The object to keep alive.</param>
        /// <exception cref="ArgumentNullException">Thrown if the object is null.</exception>
        void AddRef(object obj);
    }
}
