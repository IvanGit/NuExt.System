using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// Represents a base class for asynchronous disposable objects that raises property change notifications.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="IAsyncDisposable"/> interface and provides a mechanism 
    /// for asynchronously releasing both managed (<see cref="DisposeAsyncCore"/>) and unmanaged (<see cref="Dispose(bool)"/>) resources.
    /// It also includes support for property change notifications by extending the <see cref="PropertyChangeNotifier"/> class.
    ///
    /// Note that this class has a finalizer, but it is generally undesirable for the finalizer to be called. 
    /// Ensure that <see cref="DisposeAsync"/> is properly invoked to suppress finalization.
    ///
    /// <para>
    /// This class is designed to have its <see cref="DisposeAsync"/> method called only once. Multiple calls to <see cref="DisposeAsync"/> are thread safe.
    /// </para>
    /// </remarks>
    [DebuggerStepThrough]
    [Serializable]
    public abstract class AsyncDisposable : PropertyChangeNotifier, IAsyncDisposable
    {
#if NET9_0_OR_GREATER
        private enum States
        {
            NotDisposed,// default value of _state
            Disposing,
            Disposed
        }

        private volatile States _state;
#else
        private class States
        {
            public const int NotDisposed = 0;// default value of _state
            public const int Disposing = 1;
            public const int Disposed = 2;
        }

        private volatile int _state;
#endif

        private readonly bool _continueOnCapturedContext;

        /// <summary>
        /// Initializes a new instance with the specified continuation behavior.
        /// </summary>
        /// <param name="continueOnCapturedContext">
        /// Whether to continue on the captured synchronization context during disposal.
        /// </param>
        protected AsyncDisposable(bool continueOnCapturedContext)
        {
            _continueOnCapturedContext = continueOnCapturedContext;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDisposable"/> class 
        /// without a synchronization context.
        /// </summary>
        protected AsyncDisposable(): this(continueOnCapturedContext: false) { }

        /// <summary>
        /// Initializes a new instance with the specified synchronization context and continuation behavior.
        /// </summary>
        /// <param name="synchronizationContext">
        /// An optional <see cref="SynchronizationContext"/> to use for property change notifications. 
        /// If null, no synchronization context will be used.
        /// </param>
        /// <param name="continueOnCapturedContext">
        /// Whether to continue on the captured synchronization context during disposal.
        /// </param>
        protected AsyncDisposable(SynchronizationContext? synchronizationContext, bool continueOnCapturedContext)
            : base(synchronizationContext)
        {
            _continueOnCapturedContext = continueOnCapturedContext;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDisposable"/> class 
        /// with the specified synchronization context.
        /// </summary>
        /// <param name="synchronizationContext">
        /// An optional <see cref="SynchronizationContext"/> to use for property change notifications. 
        /// If null, no synchronization context will be used.
        /// </param>
        protected AsyncDisposable(SynchronizationContext? synchronizationContext) : this(synchronizationContext, continueOnCapturedContext: false) { }

        /// <summary>
        /// Finalizes an instance of the <see cref="AsyncDisposable"/> class.
        /// This method is invoked by the garbage collector if explicit disposal did not occur.
        /// It releases unmanaged resources by calling <see cref="Dispose(bool)"/> with disposing set to <see langword="false"/>.
        /// </summary>
        ~AsyncDisposable()
        {
            Dispose(false);
            string message = $"{GetType().FullName} ({GetHashCode()}) was finalized without proper disposal.";
            Trace.WriteLine(message);
            Debug.Fail(message);
        }

        #region Properties

        /// <summary>
        /// Gets a value indicating whether to capture the current synchronization context 
        /// and use it to execute asynchronous continuations.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to execute continuations on the captured synchronization context;
        /// <see langword="false"/> to execute continuations on a thread pool thread. 
        /// The default is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// Controls the flow of the synchronization context during asynchronous operations.
        /// </para>
        /// <para>
        /// Set to <see langword="true"/> when the instance must interact with thread-affine objects
        /// (e.g., UI controls in WPF/WinForms). Keep as <see langword="false"/> for library code.
        /// </para>
        /// </remarks>
        public bool ContinueOnCapturedContext => _continueOnCapturedContext;

        /// <summary>
        /// Gets a value indicating whether the object has been disposed.
        /// </summary>
        [Browsable(false)]
        public bool IsDisposed => _state == States.Disposed;

        /// <summary>
        /// Gets a value indicating whether the object is currently in the process of being disposed.
        /// </summary>
        [Browsable(false)]
        public bool IsDisposing => _state == States.Disposing;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the object starts the disposing process asynchronously.
        /// </summary>
        /// <remarks>
        /// <para>
        /// ⚠️ <b>Important:</b> Subscribers are responsible for unsubscribing from this event
        /// to prevent memory leaks. The event is not automatically cleared during disposal.
        /// </para>
        /// <para>
        /// Event handlers should be designed to complete quickly and avoid throwing exceptions.
        /// If an exception is thrown, it may interrupt the disposal process.
        /// </para>
        /// </remarks>
        public event AsyncEventHandler? Disposing;

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the object has been disposed and throws an <see cref="ObjectDisposedException"/> if it has.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckDisposed()
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
        }

        /// <summary>
        /// Checks if the object has been disposed or is in the process of disposing.
        /// Throws an <see cref="ObjectDisposedException"/> if either condition is true.
        /// </summary>
        /// <remarks>
        /// This method provides a stronger guarantee than <see cref="CheckDisposed"/> by also
        /// validating that the object is not currently in the disposal process, which helps
        /// prevent race conditions during asynchronous disposal.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the object is either already disposed or currently being disposed.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckDisposedOrDisposing()
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ObjectDisposedException.ThrowIf(IsDisposing, this);
        }

        /// <summary>
        /// Asynchronously releases all resources used by this instance. This method is idempotent and thread-safe.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object is already disposed and
        /// <see cref="ShouldThrowAlreadyDisposedException"/> returns <see langword="true"/>.</exception>
        /// <remarks>
        /// Invokes the <see cref="Disposing"/> event, calls <see cref="DisposeAsyncCore"/> for managed resources
        /// and calls <see cref="Dispose(bool)"/> with <see langword="false"/> for unmanaged resources.
        /// The disposal operation is performed only once. Subsequent calls are thread-safe and have no effect.
        /// The object transitions to the disposed state even if an exception occurs during cleanup.
        /// </remarks>
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _state, States.Disposing, States.NotDisposed) != States.NotDisposed)
            {
                // Already disposing or disposed

                if (IsDisposed)
                {
                    string message = $"{GetType().FullName} ({GetHashCode()}) is already disposed.";
                    Trace.WriteLine(message);
                    Debug.Fail(message);
                }
                if (IsDisposing)
                {
                    string message = $"{GetType().FullName} ({GetHashCode()}) is already disposing.";
                    Trace.WriteLine(message);
                    Debug.Fail(message);
                }

                if (ShouldThrowAlreadyDisposedException())
                {
                    ObjectDisposedException.ThrowIf(IsDisposed, this);
                }
                return;
            }

            OnPropertyChanged(EventArgsCache.IsDisposingPropertyChanged);
            try
            {
                await Disposing.InvokeAsync(this, EventArgs.Empty, continueOnCapturedContext: ContinueOnCapturedContext);//no ConfigureAwait needed
                //https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync
                await DisposeAsyncCore().ConfigureAwait(continueOnCapturedContext: ContinueOnCapturedContext);
                Dispose(false);

                if (Disposing != null)
                {
                    var message = $"{GetType().FullName} ({GetHashCode()}): {nameof(Disposing)} is not null";
                    Trace.WriteLine(message);
                    Debug.Fail(message);
                }
                if (HasPropertyChangedSubscribers)
                {
                    var message = $"{GetType().FullName} ({GetHashCode()}): {nameof(PropertyChanged)} is not null";
                    Trace.WriteLine(message);
                    Debug.Fail(message);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{GetType().FullName} ({GetHashCode()}):{Environment.NewLine}{ex.Message}";
                Trace.WriteLine(errorMessage);
                Debug.Fail(errorMessage);
                throw;
            }
            finally
            {
                _state = States.Disposed;
                OnPropertyChanged(EventArgsCache.IsDisposingPropertyChanged);
                OnPropertyChanged(EventArgsCache.IsDisposedPropertyChanged);
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Override to asynchronously release managed resources.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous cleanup operation.</returns>
        /// <remarks>
        /// This method is called by <see cref="DisposeAsync"/> before releasing unmanaged resources.
        /// The base implementation returns a completed task. Derived classes should override this method to release managed resources
        /// and should call the base implementation to ensure proper cleanup in the inheritance chain.
        /// </remarks>
        protected virtual ValueTask DisposeAsyncCore()
        {
            return default;
        }

        /// <summary>
        /// Releases the unmanaged resources used by this instance.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// In this asynchronous disposal pattern, the parameter is always <see langword="false"/>.
        /// </param>
        /// <remarks>
        /// This method is called by <see cref="DisposeAsync"/> and the finalizer (if required).
        /// This method is always called with <paramref name="disposing"/> set to <see langword="false"/> in this
        /// asynchronous disposal pattern. It must be idempotent and must not throw exceptions.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Determines whether an <see cref="ObjectDisposedException"/> should be thrown when
        /// <see cref="DisposeAsync"/> is called on an already disposed object.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if an exception should be thrown on redundant <see cref="DisposeAsync"/> calls;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The base implementation returns <see langword="false"/>. Override this method in derived classes
        /// to customize disposal behavior.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool ShouldThrowAlreadyDisposedException()
        {
            return false;
        }

        #endregion
    }
}
