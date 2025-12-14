using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    partial class PropertyChangeNotifier : ISynchronizeInvoker
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether a <see cref="SynchronizationContext"/> is provided.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if a <see cref="SynchronizationContext"/> is associated with this instance;
        /// otherwise, <see langword="false"/>.
        /// </value>
        [Browsable(false)]
        protected bool HasSynchronizationContext
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => SynchronizationContext is not null;
        }

        /// <summary>
        /// Gets the <see cref="SynchronizationContext"/> associated with this instance.
        /// </summary>
        /// <value>
        /// The <see cref="SynchronizationContext"/> used for marshaling operations to a specific thread or context,
        /// or <see langword="null"/> if no context was provided.
        /// </value>
        [Browsable(false)]
        public SynchronizationContext? SynchronizationContext { get; }

        /// <summary>
        /// Gets the thread on which this instance was created.
        /// <para>
        /// <b>Note:</b> This property is only relevant when no <see cref="SynchronizationContext"/> is provided.
        /// When a context is available, thread affinity is managed by the context and not by this property.
        /// </para>
        /// </summary>
        [Browsable(false)]
        public Thread Thread { get; } = Thread.CurrentThread;

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the calling thread can safely access this instance's members directly.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term><see langword="false"/></term>
        /// <description>
        /// If a <see cref="SynchronizationContext"/> is provided but it does not implement 
        /// <see cref="IThreadAffineSynchronizationContext"/>. Use <see cref="Invoke(Action)"/> or similar methods 
        /// for thread-safe access.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Context-dependent</term>
        /// <description>
        /// If the context implements <see cref="IThreadAffineSynchronizationContext"/>, returns 
        /// <see cref="IDispatcherObject.CheckAccess"/> (verifies access to the context's associated thread).
        /// </description>
        /// </item>
        /// <item>
        /// <term><see langword="true"/></term>
        /// <description>
        /// If no context is provided and the calling thread matches <see cref="Thread"/> (the thread on which this instance was created).
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>Behavior with SynchronizationContext:</b>
        /// When a context is provided, thread safety is delegated to that context. This method checks whether
        /// the calling thread can access the <i>context's associated thread</i>, not the thread on which this
        /// instance was created (<see cref="Thread"/>).
        /// </para>
        /// <para>
        /// Standard <see cref="SynchronizationContext"/> implementations cannot be queried for thread access,
        /// so they are treated as requiring explicit synchronization via <see cref="Invoke(Action)"/> methods.
        /// </para>
        /// <para>
        /// Contexts implementing <see cref="IThreadAffineSynchronizationContext"/> provide precise access verification
        /// through their <see cref="IDispatcherObject.CheckAccess"/> method, which checks access
        /// to the context's <see cref="IThreadAffineSynchronizationContext.Thread"/>.
        /// </para>
        /// <para>
        /// <b>Behavior without SynchronizationContext:</b>
        /// When no context is provided, the instance enforces strict thread affinity by requiring all access
        /// to occur on the thread where the instance was created (<see cref="Thread"/>).
        /// </para>
        /// </remarks>
        public bool CheckAccess()
        {
            if (HasSynchronizationContext)
            {
                return SynchronizationContext is IThreadAffineSynchronizationContext context && context.CheckAccess();
            }

            return Thread == Thread.CurrentThread;
        }

        /// <summary>
        /// Executes the specified delegate synchronously on the appropriate thread or context.
        /// </summary>
        /// <param name="method">A delegate to a method to invoke.</param>
        /// <param name="args">An array of objects to pass as arguments to the delegate, or <see langword="null"/>.</param>
        /// <returns>
        /// The return value from the delegate being invoked, or <see langword="null"/> if the delegate has no return value.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="method"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no <see cref="SynchronizationContext"/> is provided and the calling thread does not have access
        /// to the instance (i.e., <see cref="CheckAccess"/> returns <see langword="false"/>).
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>Behavior with SynchronizationContext:</b>
        /// The call is delegated to <see cref="SynchronizationContext"/>, which handles
        /// the thread marshaling internally. This is the safest and recommended approach for UI-bound objects.
        /// </para>
        /// <para>
        /// <b>Behavior without SynchronizationContext:</b>
        /// The method verifies thread affinity using <see cref="VerifyAccess"/> and executes the delegate directly
        /// on the calling thread if permitted.
        /// </para>
        /// </remarks>
        public object? Invoke(Delegate method, params object?[] args)
        {
            ArgumentNullException.ThrowIfNull(method);

            if (HasSynchronizationContext)
            {
                return SynchronizationContext!.Invoke(method, args);
            }

            object? result = null;
            if (CheckAccess())
            {
                result = method.Call(args);
            }
            else
            {
                ThrowCannotInvokeOnTheTargetThread();
            }
            return result;
        }

        /// <summary>
        /// Executes the specified <see cref="Action"/> synchronously on the appropriate thread or context.
        /// </summary>
        /// <param name="callback">An <see cref="Action"/> delegate to invoke.</param>
        /// <inheritdoc cref="Invoke(Delegate, object?[])" path="/exception"/>
        /// <inheritdoc cref="Invoke(Delegate, object?[])" path="/remarks"/>
        public void Invoke(Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            if (HasSynchronizationContext)
            {
                SynchronizationContext!.Invoke(callback);
                return;
            }

            if (CheckAccess())
            {
                callback();
                return;
            }

            ThrowCannotInvokeOnTheTargetThread();
        }

        /// <summary>
        /// Executes the specified <see cref="Func{TResult}"/> synchronously on the appropriate thread or context.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the delegate.</typeparam>
        /// <param name="callback">A <see cref="Func{TResult}"/> delegate to invoke.</param>
        /// <returns>The result returned by <paramref name="callback"/>.</returns>
        /// <inheritdoc cref="Invoke(Delegate, object?[])" path="/exception"/>
        /// <inheritdoc cref="Invoke(Delegate, object?[])" path="/remarks"/>
        public TResult Invoke<TResult>(Func<TResult> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            if (HasSynchronizationContext)
            {
                return SynchronizationContext!.Invoke(callback);
            }

            TResult result = default!;
            if (CheckAccess())
            {
                result = callback();
            }
            else
            {
                ThrowCannotInvokeOnTheTargetThread();
            }
            return result;
        }

        /// <summary>
        /// Executes the specified <see cref="Action"/> asynchronously on the appropriate thread or context.
        /// </summary>
        /// <param name="callback">An <see cref="Action"/> delegate to invoke.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no <see cref="SynchronizationContext"/> is provided and the calling thread does not have access
        /// to the instance (i.e., <see cref="CheckAccess"/> returns <see langword="false"/>).
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>Behavior with SynchronizationContext:</b>
        /// The call is delegated to <see cref="SynchronizationContext"/>, which posts the operation
        /// to the context's thread or message loop.
        /// </para>
        /// <para>
        /// <b>Behavior without SynchronizationContext:</b>
        /// The method verifies thread affinity using <see cref="VerifyAccess"/> and returns a completed task containing
        /// the result of executing the delegate directly. If the delegate throws, the task will be faulted.
        /// </para>
        /// </remarks>
        public Task InvokeAsync(Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            if (HasSynchronizationContext)
            {
                return SynchronizationContext!.InvokeAsync(callback);
            }

            if (CheckAccess())
            {
                try
                {
                    callback();
                }
                catch (Exception ex)
                {
                    return Task.FromException(ex);
                }
                return Task.CompletedTask;
            }

            return Task.FromException(CannotInvokeOnTheTargetThreadException());
        }

        /// <summary>
        /// Executes the specified <see cref="Func{TResult}"/> asynchronously on the appropriate thread or context.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the delegate.</typeparam>
        /// <inheritdoc cref="InvokeAsync(Action)" path="/param[@name='callback']"/>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation.</returns>
        /// <inheritdoc cref="InvokeAsync(Action)" path="/exception"/>
        /// <inheritdoc cref="InvokeAsync(Action)" path="/remarks"/>
        public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            if (HasSynchronizationContext)
            {
                return SynchronizationContext!.InvokeAsync(callback);
            }

            if (CheckAccess())
            {
                TResult result;
                try
                {
                    result = callback();
                }
                catch (Exception ex)
                {
                    return Task.FromException<TResult>(ex);
                }
                return Task.FromResult(result);
            }

            return Task.FromException<TResult>(CannotInvokeOnTheTargetThreadException());
        }

        /// <summary>
        /// Executes the specified asynchronous delegate that returns a <see cref="Task"/> on the appropriate thread or context.
        /// </summary>
        /// <param name="callback">A <see cref="Func{TResult}"/> delegate that returns a <see cref="Task"/> to invoke, where <c>TResult</c> is <see cref="Task"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the execution of the provided async delegate.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no <see cref="SynchronizationContext"/> is provided and the calling thread does not have access
        /// to the instance (i.e., <see cref="CheckAccess"/> returns <see langword="false"/>).
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>Behavior with SynchronizationContext:</b>
        /// The call is delegated to <see cref="SynchronizationContext"/>, which posts the asynchronous
        /// operation to the context's thread or message loop. The returned <see cref="Task"/> completes when the delegate's
        /// returned <see cref="Task"/> completes.
        /// </para>
        /// <para>
        /// <b>Behavior without SynchronizationContext:</b>
        /// The method verifies thread affinity (the calling thread must be <see cref="Thread"/>) and executes the delegate directly.
        /// The returned <see cref="Task"/> represents the execution of the provided async delegate. If the delegate throws
        /// synchronously, the returned task will be faulted.
        /// </para>
        /// <para>
        /// This method is suitable for invoking asynchronous methods (async/await) in a thread-safe manner.
        /// </para>
        /// </remarks>
        public Task InvokeAsync(Func<Task> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            if (HasSynchronizationContext)
            {
                return SynchronizationContext!.InvokeAsync(callback);
            }

            if (CheckAccess())
            {
                Task result;
                try
                {
                    result = callback();
                }
                catch (Exception ex)
                {
                    return Task.FromException(ex);
                }
                return result;
            }

            return Task.FromException(CannotInvokeOnTheTargetThreadException());
        }

        /// <summary>
        /// Executes the specified asynchronous delegate that returns a <see cref="Task{TResult}"/> on the appropriate thread or context.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the returned <see cref="Task{TResult}"/>.</typeparam>
        /// <param name="callback">A <see cref="Func{TResult}"/> delegate that returns a <see cref="Task{TResult}"/> to invoke, where <c>TResult</c> is <see cref="Task{TResult}"/>.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that represents the execution of the provided async delegate. Its result is the result
        /// of the <see cref="Task{TResult}"/> returned by <paramref name="callback"/>.
        /// </returns>
        /// <inheritdoc cref="InvokeAsync(Func{Task})" path="/exception"/>
        /// <remarks>
        /// <para>
        /// <b>Behavior with SynchronizationContext:</b>
        /// The call is delegated to <see cref="SynchronizationContext"/>, which posts
        /// the asynchronous operation to the context's thread or message loop.
        /// </para>
        /// <para>
        /// <b>Behavior without SynchronizationContext:</b>
        /// The method verifies thread affinity (the calling thread must be <see cref="Thread"/>) and executes the delegate directly.
        /// The returned <see cref="Task{TResult}"/> represents the execution of the provided async delegate. If the delegate throws
        /// synchronously, the returned task will be faulted.
        /// </para>
        /// <para>
        /// This method is suitable for invoking asynchronous methods (async/await) that return a value in a thread-safe manner.
        /// </para>
        /// </remarks>
        public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            if (HasSynchronizationContext)
            {
                return SynchronizationContext!.InvokeAsync(callback);
            }

            if (CheckAccess())
            {
                Task<TResult> result;
                try
                {
                    result = callback();
                }
                catch (Exception ex)
                {
                    return Task.FromException<TResult>(ex);
                }
                return result;
            }

            return Task.FromException<TResult>(CannotInvokeOnTheTargetThreadException());
        }

        /// <summary>
        /// Verifies that the calling thread can safely access this instance's members directly.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// A <see cref="SynchronizationContext"/> is provided but it does not implement 
        /// <see cref="IThreadAffineSynchronizationContext"/> (always throws, as direct access cannot be verified).
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// A <see cref="SynchronizationContext"/> implementing <see cref="IThreadAffineSynchronizationContext"/> 
        /// is provided and its <see cref="IDispatcherObject.CheckAccess"/> returns <see langword="false"/>.
        /// This verifies access to the context's associated thread, not necessarily the thread where this instance was created.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// No context is provided and the calling thread is not the same as <see cref="Thread"/> 
        /// (the thread on which this instance was created).
        /// </description>
        /// </item>
        /// </list>
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>Behavior with SynchronizationContext:</b>
        /// When a context is provided, thread safety is delegated to that context. This method verifies whether
        /// the calling thread can access the <i>context's associated thread</i>, which may differ from the thread 
        /// where this instance was created (<see cref="Thread"/>).
        /// </para>
        /// <para>
        /// <b>Important:</b> For instances with a standard <see cref="SynchronizationContext"/> (not implementing 
        /// <see cref="IThreadAffineSynchronizationContext"/>), this method will always throw. You must use the 
        /// <see cref="Invoke(Action)"/> or <see cref="InvokeAsync(Action)"/> family of methods to safely execute code.
        /// </para>
        /// <para>
        /// For contexts implementing <see cref="IThreadAffineSynchronizationContext"/>, the verification is delegated to 
        /// the context's own access validation logic.
        /// </para>
        /// <para>
        /// <b>Behavior without SynchronizationContext:</b>
        /// When no context is provided, access is restricted to the thread where this instance was created.
        /// </para>
        /// <para>
        /// This method is a stricter version of <see cref="CheckAccess"/> designed for validation in contexts where an
        /// exception is the appropriate failure mode.
        /// </para>
        /// </remarks>
        /// <seealso cref="CheckAccess"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VerifyAccess()
        {
            if (!CheckAccess())
            {
                ThrowVerifyAccess();
            }
        }

        private InvalidOperationException CannotInvokeOnTheTargetThreadException()
        {
            return new InvalidOperationException(string.Format(
                SR.InvalidOperation_CannotInvokeOnTheTargetThread,
                Environment.CurrentManagedThreadId,
                Thread.ManagedThreadId));
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowCannotInvokeOnTheTargetThread()
        {
            throw CannotInvokeOnTheTargetThreadException();
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowVerifyAccess()
        {
            throw new InvalidOperationException(string.Format(SR.InvalidOperation_ThreadAccessError, Thread.ManagedThreadId));
        }

        #endregion
    }
}
