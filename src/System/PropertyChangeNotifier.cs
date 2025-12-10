using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System
{
    /// <summary>
    /// An abstract base class that provides property change notification functionality by implementing the 
    /// <see cref="INotifyPropertyChanged"/> interface. It also supports thread-safe property updates and 
    /// synchronization with a specified <see cref="SynchronizationContext"/> for UI-bound operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PropertyChangeNotifier"/> class 
    /// with the specified synchronization context.
    /// </remarks>
    /// <param name="synchronizationContext">
    /// An optional synchronization context to use for property change notifications. 
    /// If null, no synchronization context will be used.
    /// </param>
    [DebuggerStepThrough]
    [Serializable]
    public abstract class PropertyChangeNotifier(SynchronizationContext? synchronizationContext) : INotifyPropertyChanged, IDispatcherObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangeNotifier"/> class 
        /// without a synchronization context.
        /// </summary>
        protected PropertyChangeNotifier() : this(null)
        {
        }

        #region Properties

        /// <summary>
        /// Gets a value indicating whether there are any subscribers to the PropertyChanged event.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool HasPropertyChangedSubscribers
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => PropertyChanged is not null;
        }

        /// <summary>
        /// Gets a value indicating whether a SynchronizationContext is provided.
        /// </summary>
        [Browsable(false)]
        protected bool HasSynchronizationContext
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => SynchronizationContext is not null;
        }

        /// <summary>
        /// Gets the SynchronizationContext associated with this instance.
        /// </summary>
        [Browsable(false)]
        public SynchronizationContext? SynchronizationContext { get; } = synchronizationContext;

        /// <summary>
        /// Gets the thread on which the current instance was created.
        /// </summary>
        [Browsable(false)]
        public Thread Thread { get; } = Thread.CurrentThread;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the property can be set. Can be overridden in derived classes for custom logic.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="oldValue">The current value of the property.</param>
        /// <param name="newValue">The new value to set.</param>
        /// <param name="propertyName">The name of the property being updated. This is optional and can be automatically provided by the compiler.</param>
        /// <returns>True if the property can be set; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual bool CanSetProperty<T>(T oldValue, T newValue, [CallerMemberName] string? propertyName = null)
        {
            return true;
        }

        /// <summary>
        /// Checks if the current thread is the same as the thread on which this instance was created.
        /// </summary>
        /// <returns>True if the current thread is the same as the creation thread; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAccess()
        {
            return Thread == Thread.CurrentThread;
        }

        /// <summary>
        /// Gets the current subscribers of the PropertyChanged event.
        /// </summary>
        /// <returns>
        /// An array of <see cref="PropertyChangedEventHandler"/> delegates. If there are no subscribers, returns an empty array.
        /// </returns>
        protected PropertyChangedEventHandler[] GetPropertyChangedEventHandlers()
        {
            // eventDelegate will be null if no listeners are attached to the event
            var eventDelegate = PropertyChanged;
            if (eventDelegate is null)
            {
                return [];
            }

            var subscribers = Array.ConvertAll(eventDelegate.GetInvocationList(), del => (PropertyChangedEventHandler)del);
            return subscribers;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        /// <param name="e">The event data containing the name of the property that changed.</param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler == null)
            {
                return;
            }
            if (HasSynchronizationContext/* && !CheckAccess()*/)
            {
                SynchronizationContext!.Send(x =>
                {
                    PropertyChanged?.Invoke(this, (PropertyChangedEventArgs)x!);
                }, e);
                return;
            }
            handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for a specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler == null)
            {
                return;
            }
            var args = new PropertyChangedEventArgs(propertyName);
            if (HasSynchronizationContext/* && !CheckAccess()*/)
            {
                SynchronizationContext!.Send(x =>
                {
                    PropertyChanged?.Invoke(this, (PropertyChangedEventArgs)x!);
                }, args);
                return;
            }
            handler(this, args);
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="e">The event data containing the name of the property that changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, PropertyChangedEventArgs e)
        {
            return SetProperty(ref storage, value, e, out _);
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            return SetProperty(ref storage, value, propertyName, out _);
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback after the property is set.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="changedCallback">An optional callback to invoke after the property is set.</param>
        /// <param name="e">The event data containing the name of the property that changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action? changedCallback, PropertyChangedEventArgs e)
        {
            if (!SetProperty(ref storage, value, e, out _)) return false;
            changedCallback?.Invoke();
            return true;
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback after the property is set.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="changedCallback">An optional callback to invoke after the property is set.</param>
        /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action? changedCallback, [CallerMemberName] string? propertyName = null)
        {
            if (!SetProperty(ref storage, value, propertyName, out _)) return false;
            changedCallback?.Invoke();
            return true;
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the old value after the property is set.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="changedCallback">An optional callback to invoke with the old value after the property is set.</param>
        /// <param name="e">The event data containing the name of the property that changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action<T>? changedCallback, PropertyChangedEventArgs e)
        {
            if (!SetProperty(ref storage, value, e, out var oldValue)) return false;
            changedCallback?.Invoke(oldValue);
            return true;
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the old value after the property is set.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="changedCallback">An optional callback to invoke with the old value after the property is set.</param>
        /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action<T>? changedCallback, [CallerMemberName] string? propertyName = null)
        {
            if (!SetProperty(ref storage, value, propertyName, out var oldValue)) return false;
            changedCallback?.Invoke(oldValue);
            return true;
        }

        /// <summary>
        /// Sets the property, raises the <see cref="PropertyChanged"/> event, and allows cancellation through the <see cref="CanSetProperty{T}"/> method, and outputs the old value if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="e">The event data containing the name of the property that changed.</param>
        /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, PropertyChangedEventArgs e, out T oldValue)
        {
            oldValue = storage;
            if (EqualityComparer<T>.Default.Equals(storage, value) || !CanSetProperty(oldValue, value, e.PropertyName))
            {
                return false;
            }
            storage = value;
            OnPropertyChanged(e);
            return true;
        }

        /// <summary>
        /// Sets the property, raises the <see cref="PropertyChanged"/> event, and allows cancellation through the <see cref="CanSetProperty{T}"/> method, and outputs the old value if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, string? propertyName, out T oldValue)
        {
            oldValue = storage;
            if (EqualityComparer<T>.Default.Equals(storage, value) || !CanSetProperty(oldValue, value, propertyName))
            {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidThreadAccess()
        {
            throw new InvalidOperationException(SR.VerifyAccess);
        }

        /// <summary>
        /// Checks if the current thread is the same as the thread on which this instance was created and throws an <see cref="InvalidOperationException"/> if not.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the current thread is not the same as the thread on which this instance was created.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VerifyAccess()
        {
            if (!CheckAccess())
            {
                ThrowInvalidThreadAccess();
            }
        }

        #endregion
    }
}
