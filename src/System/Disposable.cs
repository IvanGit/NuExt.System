#define USE_DISPOSED_EVENT_
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// An abstract base class that provides mechanisms for managing the disposal of both managed and unmanaged resources. 
    /// It ensures that resources are released appropriately by implementing the <see cref="IDisposable"/> interface.
    /// The class also extends <see cref="NotifyPropertyChanged"/> to support property change notifications.
    /// </summary>
    [Serializable]
    public abstract class Disposable : NotifyPropertyChanged, IDisposable
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
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
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
            Debug.Assert(!IsDisposed, $"{GetType().Name} ({GetHashCode()}) is already disposed");
            Debug.Assert(!IsDisposing, $"{GetType().Name} ({GetHashCode()}) is already disposing");
            Debug.Assert(disposing, $"{GetType().Name} ({GetHashCode()}) finalized");
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
                    OnDispose();
                    IsDisposed = true;
#if USE_DISPOSED_EVENT
                    Disposed?.Invoke(this, EventArgs.Empty);
                    Debug.Assert(Disposed == null, $"{nameof(Disposed)} is not null");
#endif
                    Debug.Assert(Disposing is null, $"{GetType().Name} ({GetHashCode()}): {nameof(Disposing)} is not null");
                    Debug.Assert(HasPropertyChangedSubscribers == false, $"{GetType().Name} ({GetHashCode()}): {nameof(PropertyChanged)} is not null");
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, $"{GetType().Name} ({GetHashCode()}):{Environment.NewLine}{ex.Message}");
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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting managed resources.
        /// This method is called when <see cref="Dispose(bool)"/> is invoked with a value of true.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// This method is called when <see cref="Dispose(bool)"/> is invoked with a value of false.
        /// </summary>
        protected virtual void OnDisposeUnmanaged()
        {
        }

        #endregion
    }
}
