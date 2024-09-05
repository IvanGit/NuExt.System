namespace System
{
    /// <summary>
    /// Defines a contract for managing the asynchronous lifecycle of resources and actions that should be executed upon asynchronous disposal.
    /// </summary>
    public interface IAsyncLifetime : IAsyncDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the lifetime instance has been terminated.
        /// </summary>
        bool IsTerminated { get; }

        /// <summary>
        /// Adds an action to be executed when the lifetime instance is disposed asynchronously.
        /// </summary>
        /// <param name="action">The action to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the action is null.</exception>
        void Add(Action action);

        /// <summary>
        /// Adds an asynchronous action to be executed when the lifetime instance is disposed asynchronously.
        /// </summary>
        /// <param name="action">A function returning a <see cref="ValueTask"/> representing the asynchronous action to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the action is null.</exception>
        void AddAsync(Func<ValueTask> action);

        /// <summary>
        /// Adds an <see cref="IAsyncDisposable"/> object to be disposed of asynchronously when the <see cref="AsyncLifetime"/> instance is disposed.
        /// </summary>
        /// <typeparam name="T">The type of the asynchronous disposable object.</typeparam>
        /// <param name="disposable">The asynchronous disposable object to add.</param>
        /// <returns>The asynchronous disposable object that was added.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the disposable object is null.</exception>
        T AddAsyncDisposable<T>(T disposable) where T: IAsyncDisposable;

        /// <summary>
        /// Adds a pair of actions: one to execute immediately (setup) and another to execute on asynchronous disposal (cleanup).
        /// </summary>
        /// <param name="action">The setup action to execute immediately.</param>
        /// <param name="release">The cleanup action to execute upon asynchronous disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if either action or release is null.</exception>
        void AddBracket(Action action, Action release);

        /// <summary>
        /// Adds an <see cref="IDisposable"/> object to be disposed of when the <see cref="AsyncLifetime"/> instance is disposed asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the disposable object.</typeparam>
        /// <param name="disposable">The disposable object to add.</param>
        /// <returns>The disposable object that was added.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the disposable object is null.</exception>
        T AddDisposable<T>(T disposable) where T : IDisposable;

        /// <summary>
        /// Adds a reference to an object to keep it alive until the <see cref="AsyncLifetime"/> instance is disposed asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the object to keep alive. Must be a reference type.</typeparam>
        /// <param name="obj">The object to keep alive.</param>
        /// <returns>The object that was added to be kept alive.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the object is null.</exception>
        T AddRef<T>(T obj) where T : class;
    }
}
