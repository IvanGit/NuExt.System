using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System
{
    /// <summary>
    /// Represents a base class for synchronous disposable objects that raises property change notifications.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="IDisposable"/> interface and provides a mechanism 
    /// for synchronously releasing both managed (<see cref="DisposeCore"/>) and unmanaged (<see cref="Dispose(bool)"/>) resources. 
    /// It also includes support for property change notifications by extending the <see cref="PropertyChangeNotifier"/> class.
    /// 
    /// Note that this class has a finalizer, but it is generally undesirable for the finalizer to be called. 
    /// Ensure that <see cref="Dispose()"/> is properly invoked to suppress finalization.
    ///
    /// <para>
    /// This class is designed to have its <see cref="Dispose()"/> method called only once. Multiple calls to <see cref="Dispose()"/> are thread safe.
    /// </para>
    /// </remarks>
    [DebuggerStepThrough]
    [Serializable]
    public abstract class Disposable : PropertyChangeNotifier, IDisposable
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Disposable"/> class 
        /// without a synchronization context.
        /// </summary>
        protected Disposable() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Disposable"/> class 
        /// with the specified synchronization context.
        /// </summary>
        /// <param name="synchronizationContext">
        /// An optional <see cref="SynchronizationContext"/> to use for property change notifications. 
        /// If null, no synchronization context will be used.
        /// </param>
        protected Disposable(SynchronizationContext? synchronizationContext) : base(synchronizationContext) { }

        /// <summary>
        /// Finalizes an instance of the <see cref="Disposable"/> class.
        /// This method is invoked by the garbage collector if explicit disposal did not occur.
        /// It releases unmanaged resources by calling <see cref="Dispose(bool)"/> with disposing set to <see langword="false"/>.
        /// </summary>
        ~Disposable()
        {
            Dispose(false);
            string message = $"{GetType().FullName} ({GetHashCode()}) was finalized without proper disposal.";
            Trace.WriteLine(message);
            Debug.Fail(message);
        }

        #region Properties

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
        /// Occurs when the object starts the disposing process.
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
        public event EventHandler? Disposing;

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
        /// Releases all resources used by this instance. This method is idempotent and thread-safe.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is already disposed and
        /// <see cref="ShouldThrowAlreadyDisposedException"/> returns <see langword="true"/>.</exception>
        /// <remarks>
        /// Invokes the <see cref="Disposing"/> event and calls <see cref="Dispose(bool)"/> with disposing set to <see langword="true"/>,
        /// which in turn invokes <see cref="DisposeCore"/> for managed resources.
        /// The disposal operation is performed only once. Subsequent calls are thread-safe and have no effect.
        /// The object transitions to the disposed state even if an exception occurs during cleanup.
        /// </remarks>
        public void Dispose()
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
                Disposing?.Invoke(this, EventArgs.Empty);
                //https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
                Dispose(true);

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
        /// Override to release managed resources.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called from <see cref="Dispose(bool)"/> when the disposing parameter is <see langword="true"/>.
        /// It is not called during finalization.
        /// </para>
        /// <para>
        /// The base implementation does nothing. Derived classes should override this method to release managed resources
        /// and should call the base implementation to ensure proper cleanup in the inheritance chain.
        /// </para>
        /// </remarks>
        protected virtual void DisposeCore()
        {
        }

        /// <summary>
        /// Releases the unmanaged resources used by this instance and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources (when called from <see cref="Dispose()"/>);
        /// <see langword="false"/> to release only unmanaged resources (when called from a finalizer).
        /// </param>
        /// <remarks>
        /// <para>
        /// This method is called by <see cref="Dispose()"/> (with <paramref name="disposing"/> set to <see langword="true"/>) and by the finalizer (with <paramref name="disposing"/> set to <see langword="false"/>).
        /// </para>
        /// <para>
        /// When overriding this method in a derived class, release unmanaged resources regardless of the value of <paramref name="disposing"/>.
        /// Release managed resources only when <paramref name="disposing"/> is <see langword="true"/>, preferably by overriding <see cref="DisposeCore"/>.
        /// </para>
        /// <para>
        /// This method must be idempotent and safe to call multiple times. It must not throw exceptions when <paramref name="disposing"/> is <see langword="false"/>.
        /// </para>
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCore();
            }
        }

        /// <summary>
        /// Disposes of the specified disposable object and sets the reference to null.
        /// </summary>
        /// <typeparam name="T">The type of the disposable object.</typeparam>
        /// <param name="disposable">The disposable object to dispose of.</param>
        /// <returns>The original disposable object, or null if it was already null.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable? DisposeAndNull<T>(ref T? disposable) where T : class, IDisposable
        {
            var original = disposable;
            original?.Dispose();
            disposable = null;
            return original;
        }

        /// <summary>
        /// Disposes of the specified disposable object and sets the reference to null.
        /// Returns a value indicating whether the object was disposed.
        /// </summary>
        /// <typeparam name="T">The type of the disposable object.</typeparam>
        /// <param name="disposable">The disposable object to dispose of.</param>
        /// <returns><c>true</c> if the disposable was disposed; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryDisposeAndNull<T>(ref T? disposable) where T : class, IDisposable
        {
            var original = disposable;
            if (original == null) return false;

            original.Dispose();
            disposable = null;
            return true;
        }

        /// <summary>
        /// Determines whether an <see cref="ObjectDisposedException"/> should be thrown when
        /// <see cref="Dispose()"/> is called on an already disposed object.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if an exception should be thrown on redundant <see cref="Dispose()"/> calls; 
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

    internal static partial class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs IsDisposedPropertyChanged = new(nameof(Disposable.IsDisposed));
        internal static readonly PropertyChangedEventArgs IsDisposingPropertyChanged = new(nameof(Disposable.IsDisposing));
    }
}
