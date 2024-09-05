using System.Diagnostics;
using System.Runtime.CompilerServices;

// Based on Станислав Сидристый «Шаблон Lifetime: для сложного Disposing»
// https://www.youtube.com/watch?v=F5oOYKTFpcQ

namespace System
{
    /// <summary>
    /// Manages the lifecycle of resources and ensures that all registered cleanup actions are executed upon disposal.
    /// </summary>
    public sealed class Lifetime : ILifetime
    {
        private readonly List<Action> _actions = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Lifetime"/> class and sets up termination on disposal.
        /// </summary>
        public Lifetime()
        {
            Add(() => IsTerminated = true);
        }

        #region Properties

        /// <summary>
        /// Represents the termination status of the <see cref="Lifetime"/> instance.
        /// If true, indicates that the instance has been terminated and 
        /// all associated resources have been released.
        /// </summary>
        public bool IsTerminated { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds an action to be executed when the <see cref="Lifetime"/> instance is disposed.
        /// </summary>
        /// <param name="action">The action to add. This action will be called upon disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if the action is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add an action to a terminated <see cref="Lifetime"/> instance.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Action action)
        {
            Debug.Assert(action != null, $"{nameof(action)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(action);
#else
            Throw.IfNull(action);
#endif
            lock (_actions)
            {
                CheckTerminated();
                _actions.Add(action);
            }
        }

        /// <summary>
        /// Adds a pair of actions: one to be executed immediately (subscribe) and another to be executed during disposal (unsubscribe).
        /// </summary>
        /// <param name="subscribe">The action to execute immediately.</param>
        /// <param name="unsubscribe">The action to execute upon disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown if either subscribe or unsubscribe is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add actions to a terminated <see cref="Lifetime"/> instance.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBracket(Action subscribe, Action unsubscribe)
        {
            Debug.Assert(subscribe != null, $"{nameof(subscribe)} is null");
            Debug.Assert(unsubscribe != null, $"{nameof(unsubscribe)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(subscribe);
            ArgumentNullException.ThrowIfNull(unsubscribe);
#else
            Throw.IfNull(subscribe);
            Throw.IfNull(unsubscribe);
#endif
            subscribe();
            Add(unsubscribe);
        }

        /// <summary>
        /// Adds an <see cref="IDisposable"/> object to be disposed of when the <see cref="Lifetime"/> instance is disposed.
        /// </summary>
        /// <typeparam name="T">The type of the disposable object.</typeparam>
        /// <param name="disposable">The disposable object to add.</param>
        /// <returns>The disposable object that was added.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the disposable object is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add a disposable object to a terminated <see cref="Lifetime"/> instance.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T AddDisposable<T>(T disposable) where T : IDisposable
        {
            Debug.Assert(disposable != null, $"{nameof(disposable)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(disposable);
#else
            Throw.IfNull(disposable);
#endif
            Add(disposable.Dispose);
            return disposable;
        }

        /// <summary>
        /// Adds a reference to an object to keep it alive until the <see cref="Lifetime"/> instance is disposed.
        /// </summary>
        /// <typeparam name="T">The type of the object to keep alive. Must be a reference type.</typeparam>
        /// <param name="obj">The object to keep alive.</param>
        /// <returns>The object that was added to be kept alive.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the object is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if trying to add a reference to a terminated <see cref="Lifetime"/> instance.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T AddRef<T>(T obj) where T: class
        {
            Debug.Assert(obj != null, $"{nameof(obj)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(obj);
#else
            Throw.IfNull(obj);
#endif
            Add(() => GC.KeepAlive(obj));
            return obj;
        }

        /// <summary>
        /// Checks whether the <see cref="Lifetime"/> instance has been terminated and throws an exception if it has.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the <see cref="Lifetime"/> instance is terminated.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckTerminated()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(IsTerminated, this);
#else
            if (IsTerminated)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
#endif
        }

        /// <summary>
        /// Executes all added actions in reverse order and marks the instance as terminated.
        /// Ensures that all resources are released properly.
        /// </summary>
        public void Dispose()
        {
            if (IsTerminated)
            {
                return;
            }
            lock (_actions)
            {
                for (int i = _actions.Count - 1; i >= 0; i--)
                {
                    _actions[i]();
                }
                _actions.Clear();
            }
            Debug.Assert(IsTerminated, $"{nameof(Lifetime)} is not terminated");
        }

        #endregion
    }

}
