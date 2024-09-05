# NuExt.System

Provides various fundamental extensions and classes for .NET, simplifying tasks related to asynchronous programming, resource lifecycle management, thread synchronization, and more.

### Features:
- Simplifies asynchronous resource disposal.
- Manages the lifecycle of objects and resources.
- Provides thread synchronization tools.
- Implements property change notifications.
- Includes high-performance string manipulation utilities.

### Commonly Used Types:
- **System.AggregateDisposable**: Simplifies the disposal of multiple disposables.
- **System.AsyncDisposable**: Simplifies async resource disposal.
- **System.AsyncEventHandler**: Represents an async event handler.
- **System.AsyncLifetime**: Manages async operations lifecycle.
- **System.Disposable**: Base implementation of IDisposable.
- **System.Lifetime**: Manages the lifecycle of objects and resources.
- **System.ComponentModel.NotifyPropertyChanged**: Implementation of INotifyPropertyChanged.
- **System.Diagnostics.ProcessMonitor**: Real-time CPU, memory, thread, and ThreadPool monitoring for .NET processes.
- **System.Threading.AsyncLock**: Async lock for resource synchronization.
- **System.Threading.AsyncWaitHandle**: Async wait handle signaling with timeout and cancellation.
- **System.Threading.ReentrantAsyncLock**: Reentrant async lock.
- **System.Text.ValueStringBuilder**: High-performance string builder (originally internal in .NET runtime, made public).

### Acknowledgements
Includes code derived from the .NET Runtime, licensed under the MIT License. The `ValueStringBuilder` class was originally internal and has been adapted to be public.

### License
Licensed under the MIT License. See the LICENSE file for details.