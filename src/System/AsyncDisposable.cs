using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// Represents a base class for asynchronous disposable objects that raises property change notifications.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="IAsyncDisposable"/> interface and provides a mechanism 
    /// for asynchronously releasing managed resources.
    /// It also includes support for property change notifications by extending the <see cref="NotifyPropertyChanged"/> class.
    /// Note that this class has a finalizer, but it is generally undesirable for the finalizer to be called. 
    /// Ensure that <see cref="DisposeAsync"/> is properly invoked to suppress finalization.
    /// </remarks>
    [Serializable]
    public abstract class AsyncDisposable : NotifyPropertyChanged, IAsyncDisposable
    {
        private bool _isDisposed;
        private bool _isDisposing;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDisposable"/> class 
        /// without a synchronization context.
        /// </summary>
        protected AsyncDisposable() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDisposable"/> class 
        /// with the specified synchronization context.
        /// </summary>
        /// <param name="synchronizationContext">
        /// An optional <see cref="SynchronizationContext"/> to use for property change notifications. 
        /// If null, no synchronization context will be used.
        /// </param>
        protected AsyncDisposable(SynchronizationContext? synchronizationContext) : base(synchronizationContext) { }

        /// <summary>
        /// Finalizes an instance of the <see cref="AsyncDisposable"/> class.
        /// This finalizer is called by the garbage collector before the object is reclaimed.
        /// It checks a condition to determine if an exception should be thrown during finalization
        /// using the overridable method <see cref="ShouldThrowFinalizerException"/>.
        /// If the method returns true, <see cref="ThrowFinalizerException"/> is invoked to generate an exception.
        /// </summary>
        ~AsyncDisposable()
        {
            if (!ShouldThrowFinalizerException()) return;
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

        /// <summary>
        /// Occurs when the object starts the disposing process asynchronously.
        /// </summary>
        public event AsyncEventHandler? Disposing;

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
        /// Asynchronously disposes of the resources used by the instance.
        /// </summary>
        public async ValueTask DisposeAsync()
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
            IsDisposing = true;
            try
            {
                await Disposing.InvokeAsync(this, EventArgs.Empty);
                await OnDisposeAsync().ConfigureAwait(false);
                IsDisposed = true;
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
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting managed resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        protected virtual ValueTask OnDisposeAsync()
        {
            return default;
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
