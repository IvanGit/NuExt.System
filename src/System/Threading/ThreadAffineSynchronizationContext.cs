using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Threading
{
    /// <summary>
    /// A <see cref="SynchronizationContext"/> implementation that adds thread affinity checking
    /// and implements <see cref="IThreadAffineSynchronizationContext"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class wraps an existing synchronization context while providing thread affinity tracking
    /// through the <see cref="CheckAccess"/> and <see cref="VerifyAccess"/> methods.
    /// </para>
    /// <para>
    /// It's particularly useful for adapting standard synchronization contexts (like those from WPF or WinForms)
    /// to work with APIs requiring <see cref="IThreadAffineSynchronizationContext"/>.
    /// </para>
    /// </remarks>
    public class ThreadAffineSynchronizationContext : SynchronizationContext, IThreadAffineSynchronizationContext
    {
        private readonly SynchronizationContext _innerContext;
        private readonly Thread _associatedThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadAffineSynchronizationContext"/> class.
        /// </summary>
        /// <param name="innerContext">The synchronization context to wrap.</param>
        /// <param name="associatedThread">The thread associated with this context.</param>
        /// <exception cref="ArgumentNullException"><paramref name="innerContext"/> is <see langword="null"/> or <paramref name="associatedThread"/> is <see langword="null"/>.</exception>
        public ThreadAffineSynchronizationContext(SynchronizationContext innerContext, Thread associatedThread)
        {
            ArgumentNullException.ThrowIfNull(innerContext);
            ArgumentNullException.ThrowIfNull(associatedThread);
            _innerContext = innerContext;
            _associatedThread = associatedThread;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadAffineSynchronizationContext"/> class.
        /// </summary>
        /// <param name="innerContext">The synchronization context to wrap.</param>
        public ThreadAffineSynchronizationContext(SynchronizationContext innerContext) : this(innerContext, Thread.CurrentThread)
        {
        }

        /// <inheritdoc/>
        public Thread Thread => _associatedThread;

        /// <summary>
        /// Gets the wrapped synchronization context.
        /// </summary>
        public SynchronizationContext InnerContext => _innerContext;

        /// <inheritdoc/>
        public bool CheckAccess()
        {
            return Thread.CurrentThread == _associatedThread;
        }

        /// <inheritdoc/>
        public void VerifyAccess()
        {
            if (!CheckAccess())
            {
                ThrowVerifyAccess();
            }
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowVerifyAccess()
        {
            throw new InvalidOperationException(string.Format(SR.InvalidOperation_SynchronizationContextAccessDenied, 
                Environment.CurrentManagedThreadId, Thread.ManagedThreadId));
        }

        /// <inheritdoc/>
        public override void Send(SendOrPostCallback d, object? state)
        {
            _innerContext.Send(d, state);
        }

        /// <inheritdoc/>
        public override void Post(SendOrPostCallback d, object? state)
        {
            _innerContext.Post(d, state);
        }

        /// <inheritdoc/>
        public override SynchronizationContext CreateCopy()
        {
            return new ThreadAffineSynchronizationContext(
                _innerContext.CreateCopy(),
                _associatedThread);
        }

        /// <inheritdoc/>
        public override int Wait(nint[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            return _innerContext.Wait(waitHandles, waitAll, millisecondsTimeout);
        }

        /// <summary>
        /// Creates a <see cref="ThreadAffineSynchronizationContext"/> from the current context.
        /// </summary>
        /// <returns>
        /// A new <see cref="ThreadAffineSynchronizationContext"/> wrapping <see cref="SynchronizationContext.Current"/>,
        /// or <see langword="null"/> if no current context exists.
        /// </returns>
        public static ThreadAffineSynchronizationContext? FromCurrent()
        {
            var current = Current;
            return current != null ? new ThreadAffineSynchronizationContext(current) : null;
        }

        /// <summary>
        /// Creates a <see cref="ThreadAffineSynchronizationContext"/> from the current context
        /// with the specified thread association.
        /// </summary>
        /// <param name="associatedThread">The thread to associate with the context</param>
        /// <returns>
        /// A new <see cref="ThreadAffineSynchronizationContext"/> wrapping <see cref="SynchronizationContext.Current"/>,
        /// or <see langword="null"/> if no current context exists.
        /// </returns>
        public static ThreadAffineSynchronizationContext? FromCurrent(Thread associatedThread)
        {
            var current = Current;
            return current != null ? new ThreadAffineSynchronizationContext(current, associatedThread) : null;
        }

        /// <summary>
        /// Creates a <see cref="ThreadAffineSynchronizationContext"/> from the current context
        /// or a default context if none exists.
        /// </summary>
        /// <param name="defaultContext">The default context to use if no current context exists.</param>
        /// <returns>A new <see cref="ThreadAffineSynchronizationContext"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="defaultContext"/> is <see langword="null"/>.
        /// </exception>
        public static ThreadAffineSynchronizationContext FromCurrentOrDefault(SynchronizationContext defaultContext)
        {
            ArgumentNullException.ThrowIfNull(defaultContext);
            var context = Current ?? defaultContext;
            return new ThreadAffineSynchronizationContext(context);
        }

        /// <summary>
        /// Creates a <see cref="ThreadAffineSynchronizationContext"/> from the current context
        /// or a default context if none exists.
        /// </summary>
        /// <param name="defaultContext">The default context to use if no current context exists.</param>
        /// <param name="associatedThread">The thread to associate with the context.</param>
        /// <returns>A new <see cref="ThreadAffineSynchronizationContext"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="defaultContext"/> or <paramref name="associatedThread"/> is <see langword="null"/>.
        /// </exception>
        public static ThreadAffineSynchronizationContext FromCurrentOrDefault(
            SynchronizationContext defaultContext,
            Thread associatedThread)
        {
            ArgumentNullException.ThrowIfNull(defaultContext);
            ArgumentNullException.ThrowIfNull(associatedThread);
            var context = Current ?? defaultContext;
            return new ThreadAffineSynchronizationContext(context, associatedThread);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ThreadAffineSynchronizationContext(Inner={_innerContext.GetType().Name}, " +
                   $"Thread={_associatedThread.ManagedThreadId})";
        }
    }
}
