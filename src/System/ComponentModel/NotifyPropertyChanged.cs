using System.Runtime.CompilerServices;

namespace System.ComponentModel
{
    /// <summary>
    /// An abstract base class that provides property change notification functionality by implementing the 
    /// <see cref="INotifyPropertyChanged"/> interface. It also supports thread-safe property updates and 
    /// synchronization with a specified <see cref="SynchronizationContext"/> for UI-bound operations.
    /// </summary>
    [Serializable]
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged, IDispatcher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChanged"/> class 
        /// without a synchronization context.
        /// </summary>
        protected NotifyPropertyChanged() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChanged"/> class 
        /// with the specified synchronization context.
        /// </summary>
        /// <param name="synchronizationContext">
        /// An optional synchronization context to use for property change notifications. 
        /// If null, no synchronization context will be used.
        /// </param>
        protected NotifyPropertyChanged(SynchronizationContext? synchronizationContext)
        {
            SynchronizationContext = synchronizationContext;
            Thread = Thread.CurrentThread;
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
        public SynchronizationContext? SynchronizationContext { get; }

        /// <summary>
        /// Gets the thread on which the current instance was created.
        /// </summary>
        [Browsable(false)]
        public Thread Thread { get; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Methods

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
        /// Compares two values for equality using the default equality comparer.
        /// </summary>
        /// <typeparam name="T">The type of the values to compare.</typeparam>
        /// <param name="storage">The first value to compare.</param>
        /// <param name="value">The second value to compare.</param>
        /// <returns>True if the values are equal; otherwise, false.</returns>
        private static bool CompareValues<T>(T storage, T value) => EqualityComparer<T>.Default.Equals(storage, value);

        /// <summary>
        /// Gets the current subscribers of the PropertyChanged event.
        /// </summary>
        /// <returns>
        /// An array of <see cref="PropertyChangedEventHandler"/> delegates, or <see langword="null"/> if there are no subscribers.
        /// </returns>
        protected PropertyChangedEventHandler[]? GetPropertyChangedEventHandlers()
        {
            // eventDelegate will be null if no listeners are attached to the event
            var eventDelegate = PropertyChanged;
            if (eventDelegate is null)
            {
                return null;
            }

            var subscribers = Array.ConvertAll(eventDelegate.GetInvocationList(), del => (PropertyChangedEventHandler)del);
            return subscribers;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for a specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                if (HasSynchronizationContext)
                {
                    SynchronizationContext!.Send(x =>
                    {
#if DEBUG_PROPERTIES
                        Debug.WriteLine($"DEBUG - Property sending: {propertyName}");
#endif
                        PropertyChanged?.Invoke(this, (PropertyChangedEventArgs)x!);
#if DEBUG_PROPERTIES
                        Debug.WriteLine($"DEBUG - Property send: {propertyName}");
#endif
                    }, args);
                    return;
                }
                handler(this, args);
            }
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
            return SetProperty(ref storage, value, propertyName, (Action?)null);
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, 
        /// and invokes an optional callback after the property is set.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
        /// <param name="changedCallback">An optional callback to invoke after the property is set.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, string? propertyName, Action? changedCallback)
        {
            if (CompareValues(storage, value))
            {
                return false;
            }
            storage = value;
#if DEBUG_PROPERTIES
            Debug.WriteLine($"DEBUG - Property changing: {propertyName}");
#endif
            OnPropertyChanged(propertyName);
#if DEBUG_PROPERTIES
            Debug.WriteLine($"DEBUG - Property changed: {propertyName}");
#endif
            changedCallback?.Invoke();
            return true;
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed,
        /// and invokes an optional callback with the old value after the property is set.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
        /// <param name="changedCallback">An optional callback to invoke with the old value after the property is set.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, string? propertyName, Action<T>? changedCallback)
        {
            if (CompareValues(storage, value))
            {
                return false;
            }
            T oldValue = storage;
            storage = value;
#if DEBUG_PROPERTIES
            Debug.WriteLine($"DEBUG - Property changing: {propertyName}");
#endif
            OnPropertyChanged(propertyName);
#if DEBUG_PROPERTIES
            Debug.WriteLine($"DEBUG - Property changed: {propertyName}");
#endif
            changedCallback?.Invoke(oldValue);
            return true;
        }

        #endregion
    }
}
