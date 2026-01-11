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
    [DebuggerStepThrough]
    [Serializable]
    public abstract partial class PropertyChangeNotifier : INotifyPropertyChanged
    {
        private readonly SendOrPostCallback? _propertyChangedCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangeNotifier"/> class 
        /// with the specified synchronization context.
        /// </summary>
        /// <param name="synchronizationContext">
        /// An optional synchronization context to use for property change notifications. 
        /// If null, no synchronization context will be used.
        /// </param>
        protected PropertyChangeNotifier(SynchronizationContext? synchronizationContext)
        {
            SynchronizationContext = synchronizationContext;
            if (!HasSynchronizationContext) return;
            _propertyChangedCallback = OnPropertyChanged;
            _state = new State(new SynchronizationContextHelper(synchronizationContext!));
        }

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
        protected virtual bool CanSetProperty<T>(T oldValue, T newValue, [CallerMemberName] string? propertyName = null)
        {
            return true;
        }

        /// <summary>
        /// Gets the current subscribers of the PropertyChanged event.
        /// </summary>
        /// <returns>
        /// An array of <see cref="PropertyChangedEventHandler"/> delegates. If there are no subscribers, returns an empty array.
        /// </returns>
        protected PropertyChangedEventHandler[] GetPropertyChangedSubscribers()
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

        private void OnPropertyChanged(object? state)
        {
            PropertyChanged?.Invoke(this, (PropertyChangedEventArgs)state!);
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
            // No need to check access because SynchronizationContext was passed to the constructor from outside.
            if (HasSynchronizationContext)
            {
                SynchronizationContext!.Send(_propertyChangedCallback!, e);
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
            // No need to check access because SynchronizationContext was passed to the constructor from outside.
            if (HasSynchronizationContext)
            {
                SynchronizationContext!.Send(_propertyChangedCallback!, args);
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
            return SetProperty(ref storage, value, out _, propertyName);
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
            if (!SetProperty(ref storage, value, out _, propertyName)) return false;
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
        /// <param name="e">The event data containing the name of the property that changed.</param>
        /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action? changedCallback, PropertyChangedEventArgs e, out T oldValue)
        {
            if (!SetProperty(ref storage, value, e, out oldValue)) return false;
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
        /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
        /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action? changedCallback, out T oldValue, [CallerMemberName] string? propertyName = null)
        {
            if (!SetProperty(ref storage, value, out oldValue, propertyName)) return false;
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
            if (!SetProperty(ref storage, value, out var oldValue, propertyName)) return false;
            changedCallback?.Invoke(oldValue);
            return true;
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the name of the property and the old value after the property is set.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="changedCallback">An optional callback to invoke with the name of the property and the old value after the property is set.</param>
        /// <param name="e">The event data containing the name of the property that changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action<string?, T>? changedCallback, PropertyChangedEventArgs e)
        {
            if (!SetProperty(ref storage, value, e, out var oldValue)) return false;
            changedCallback?.Invoke(e.PropertyName, oldValue);
            return true;
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the name of the property and the old value after the property is set.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="changedCallback">An optional callback to invoke with the name of the property and the old value after the property is set.</param>
        /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action<string?, T>? changedCallback, [CallerMemberName] string? propertyName = null)
        {
            if (!SetProperty(ref storage, value, out var oldValue, propertyName)) return false;
            changedCallback?.Invoke(propertyName, oldValue);
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
        /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action<T>? changedCallback, PropertyChangedEventArgs e, out T oldValue)
        {
            if (!SetProperty(ref storage, value, e, out oldValue)) return false;
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
        /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
        /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action<T>? changedCallback, out T oldValue, [CallerMemberName] string? propertyName = null)
        {
            if (!SetProperty(ref storage, value, out oldValue, propertyName)) return false;
            changedCallback?.Invoke(oldValue);
            return true;
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the name of the property and the old value after the property is set.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="changedCallback">An optional callback to invoke with the name of the property and the old value after the property is set.</param>
        /// <param name="e">The event data containing the name of the property that changed.</param>
        /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action<string?, T>? changedCallback, PropertyChangedEventArgs e, out T oldValue)
        {
            if (!SetProperty(ref storage, value, e, out oldValue)) return false;
            changedCallback?.Invoke(e.PropertyName, oldValue);
            return true;
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the name of the property and the old value after the property is set.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="changedCallback">An optional callback to invoke with the name of the property and the old value after the property is set.</param>
        /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
        /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action<string?, T>? changedCallback, out T oldValue, [CallerMemberName] string? propertyName = null)
        {
            if (!SetProperty(ref storage, value, out oldValue, propertyName)) return false;
            changedCallback?.Invoke(propertyName, oldValue);
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
        /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
        /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, out T oldValue, [CallerMemberName] string? propertyName = null)
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

        #endregion
    }
}
