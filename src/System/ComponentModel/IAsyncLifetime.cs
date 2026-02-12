using System.Threading.Tasks;

namespace System.ComponentModel
{
    /// <summary>
    /// Defines a mechanism for managing dependent resources that are disposed asynchronously together.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Registered callbacks execute during asynchronous disposal in reverse (LIFO) order.
    /// </para>
    /// <para>
    /// Implementations of this interface should be thread-safe for concurrent calls to its methods
    /// and the <see cref="IAsyncDisposable.DisposeAsync"/> method.
    /// </para>
    /// </remarks>
    public interface IAsyncLifetime : IAsyncDisposable
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
        /// Adds an action to be executed when the lifetime instance is disposed asynchronously.
        /// </summary>
        /// <param name="action">The action to add. This action will be called upon asynchronous disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if the action is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add an action to a terminated lifetime instance.</exception>
        void Add(Action action);

        /// <summary>
        /// Adds an asynchronous action to be executed when the lifetime instance is disposed asynchronously.
        /// </summary>
        /// <param name="callback">The asynchronous callback to add. This callback will be called upon asynchronous disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if the callback is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add a callback to a terminated lifetime instance.</exception>
        void AddAsync(Func<Task> callback);

        /// <summary>
        /// Adds an asynchronous action to be executed when the lifetime instance is disposed asynchronously.
        /// </summary>
        /// <param name="callback">The asynchronous callback to add. This callback will be called upon asynchronous disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if the callback is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add a callback to a terminated lifetime instance.</exception>
        void AddAsync(Func<ValueTask> callback);

        /// <summary>
        /// Adds an <see cref="IAsyncDisposable"/> object to be disposed of asynchronously when the lifetime instance is disposed.
        /// </summary>
        /// <typeparam name="T">The type of the asynchronous disposable object.</typeparam>
        /// <param name="disposable">The asynchronous disposable object to add.</param>
        /// <returns>The asynchronous disposable object that was added.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the disposable object is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add a disposable object to a terminated lifetime instance.</exception>
        T AddAsyncDisposable<T>(T disposable) where T: IAsyncDisposable;

        /// <summary>
        /// Adds a pair of actions: one to be executed immediately (subscribe) and another to be executed during asynchronous disposal (unsubscribe).
        /// </summary>
        /// <param name="subscribe">The action to execute immediately.</param>
        /// <param name="unsubscribe">The action to execute upon asynchronous disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if either subscribe or unsubscribe is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add actions to a terminated lifetime instance.</exception>
        void AddBracket(Action subscribe, Action unsubscribe);

        /// <summary>
        /// Adds a pair of asynchronous actions: one to be executed immediately (subscribe) 
        /// and another to be executed during asynchronous disposal (unsubscribe).
        /// </summary>
        /// <param name="subscribe">The action to execute immediately.</param>
        /// <param name="unsubscribe">The action to execute upon asynchronous disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if either subscribe or unsubscribe is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add actions to a terminated lifetime instance.</exception>
        ValueTask AddBracketAsync(Func<ValueTask> subscribe, Func<ValueTask> unsubscribe);

        /// <summary>
        /// Adds an <see cref="IDisposable"/> object to be disposed of when the lifetime instance is disposed asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the disposable object.</typeparam>
        /// <param name="disposable">The disposable object to add.</param>
        /// <returns>The disposable object that was added.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the disposable object is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add a disposable object to a terminated lifetime instance.</exception>
        T AddDisposable<T>(T disposable) where T : IDisposable;

        /// <summary>
        /// Adds a reference to an object to keep it alive until the lifetime instance is disposed asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the object to keep alive. Must be a reference type.</typeparam>
        /// <param name="obj">The object to keep alive.</param>
        /// <returns>The object that was added to be kept alive.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the object is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add a reference to a terminated lifetime instance.</exception>
        T AddRef<T>(T obj) where T : class;
    }
}
