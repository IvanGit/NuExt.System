#define USE_DISPOSED_EVENT_
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// Represents a base class for synchronous disposable objects that raises property change notifications.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="IDisposable"/> interface and provides a mechanism 
    /// for synchronously releasing both managed and unmanaged resources. 
    /// It also includes support for property change notifications by extending the <see cref="PropertyChangeNotifier"/> class.
    /// 
    /// Note that this class has a finalizer, but it is generally undesirable for the finalizer to be called. 
    /// Ensure that <see cref="Dispose()"/> is properly invoked to suppress finalization.
    ///
    /// <para>
    /// This class is designed to have its <see cref="Dispose()"/> method called only once. 
    /// Calling <see cref="Dispose()"/> multiple times or attempting to use the object after it has been disposed 
    /// may result in undefined behavior or exceptions.
    /// </para>
    /// </remarks>
    [Serializable]
    public abstract class Disposable : PropertyChangeNotifier, IDisposable
    {
        private bool _isDisposed;
        private bool _isDisposing;

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
        protected Disposable(SynchronizationContext? synchronizationContext) :base(synchronizationContext) { }

        ~Disposable()
        {
            Dispose(false);
            if (!ShouldThrowFinalizerException())
            {
                Debug.Fail($"{GetType().FullName} ({GetHashCode()}) was finalized without proper disposal.");
                return;
            }
            ThrowFinalizerException();
        }

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the object has been disposed.
        /// </summary>
        [Browsable(false)]
        public bool IsDisposed { get => _isDisposed; private set => SetProperty(ref _isDisposed, value); }

        /// <summary>
        /// Gets a value indicating whether the object is currently in the process of being disposed.
        /// </summary>
        [Browsable(false)]
        public bool IsDisposing { get => _isDisposing; private set => SetProperty(ref _isDisposing, value); }

        #endregion

        #region Events

#if USE_DISPOSED_EVENT
        /// <summary>
        /// Occurs when the object has been disposed.
        /// </summary>
        [Browsable(false)]
        public event EventHandler? Disposed;
#endif

        /// <summary>
        /// Occurs when the object starts the disposing process.
        /// </summary>
        public event EventHandler? Disposing;

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the object has been disposed and throws an <see cref="ObjectDisposedException"/> if it has.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckDisposed()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(_isDisposed, this);
#else
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
#endif
        }

        /// <summary>
        /// Disposes of the resources used by the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the unmanaged resources used by the instance and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            Debug.Assert(!IsDisposed, $"{GetType().FullName} ({GetHashCode()}) is already disposed");
            Debug.Assert(!IsDisposing, $"{GetType().FullName} ({GetHashCode()}) is already disposing");
            if (ShouldThrowAlreadyDisposedException())
            {
                CheckDisposed();
            }
            if (IsDisposed || IsDisposing)
            {
                return;
            }
            if (disposing)
            {
                IsDisposing = true;
                try
                {
                    Disposing?.Invoke(this, EventArgs.Empty);
                    //https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
                    OnDispose();
                    OnDisposeUnmanaged();
                    IsDisposed = true;
#if USE_DISPOSED_EVENT
                    Disposed?.Invoke(this, EventArgs.Empty);
                    Debug.Assert(Disposed == null, $"{nameof(Disposed)} is not null");
#endif
                    Debug.Assert(Disposing is null, $"{GetType().FullName} ({GetHashCode()}): {nameof(Disposing)} is not null");
                    Debug.Assert(HasPropertyChangedSubscribers == false, $"{GetType().FullName} ({GetHashCode()}): {nameof(PropertyChanged)} is not null");
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, $"{GetType().FullName} ({GetHashCode()}):{Environment.NewLine}{ex.Message}");
                    throw;
                }
                finally
                {
                    IsDisposing = false;
                }
            }
            else
            {
                OnDisposeUnmanaged();
            }
        }

        /// <summary>
        /// Disposes of the specified disposable object and sets the reference to null.
        /// </summary>
        /// <typeparam name="T">The type of the disposable object.</typeparam>
        /// <param name="disposable">The disposable object to dispose of.</param>
        /// <returns>The original disposable object, or null if it was already null.</returns>
        public static IDisposable? DisposeAndNull<T>(ref T? disposable) where T : class, IDisposable
        {
            var original = disposable;
            original?.Dispose();
            disposable = null;
            return original;
        }

        /// <summary>
        /// Override this method to release managed resources.
        /// This method is called when <see cref="Dispose()"/> is invoked.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        /// <summary>
        /// Override this method to release unmanaged resources.
        /// This method is called when <see cref="Dispose()"/> is invoked or when the finalizer of <see cref="Disposable"/> is executed.
        /// </summary>
        protected virtual void OnDisposeUnmanaged()
        {
        }

        /// <summary>
        /// Determines whether an exception should be thrown when Dispose is called 
        /// on an already disposed object. By default, returns false.
        /// </summary>
        /// <returns>
        /// True if an exception should be thrown on redundant Dispose calls; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Override this method in derived classes to customize disposal behavior.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool ShouldThrowAlreadyDisposedException()
        {
            return false;
        }

        /// <summary>
        /// Overridable method that determines whether an exception should be thrown during finalization.
        /// By default, it returns true. Subclasses can override this method to change the behavior.
        /// </summary>
        /// <returns>Returns true if an exception should be thrown; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool ShouldThrowFinalizerException()
        {
            return true;
        }

        /// <summary>
        /// Method invoked by the finalizer to generate an exception.
        /// It outputs debug messages and throws an exception with information about the type and hash code of the object.
        /// This method can be used for debugging and diagnosing issues related to improper object usage.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowFinalizerException()
        {
            string message = $"{GetType().FullName} ({GetHashCode()}) was finalized without proper disposal.";
            Debug.WriteLine(message);
            Debug.Fail(message);
            throw new InvalidOperationException(message);
        }

        #endregion
    }
}
