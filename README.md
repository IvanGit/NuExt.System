# NuExt.System

`NuExt.System` provides various fundamental extensions and classes for .NET, simplifying tasks related to asynchronous programming, resource lifecycle management, thread synchronization, and more.

### Features

- Thread synchronization tools.
- Lifecycle management of objects and resources.
- Property change notifications implementation.
- Simplified asynchronous resource disposal.
- High-performance string manipulation utilities.

### Commonly Used Types

- **`System.AggregateDisposable`**: Simplifies the disposal of multiple disposables.
- **`System.AggregateAsyncDisposable`**: The asynchronous disposal of multiple `IAsyncDisposable` instances.
- **`System.AsyncDisposable`**: Facilitates async resource disposal.
- **`System.AsyncEventHandler`**: Represents an asynchronous event handler.
- **`System.AsyncLifetime`**: Manages the lifecycle of asynchronous operations.
- **`System.Disposable`**: Base implementation of `IDisposable`.
- **`System.Lifetime`**: Manages the lifecycle of objects and resources.
- **`System.ComponentModel.NotifyPropertyChanged`**: Implementation of `INotifyPropertyChanged`.
- **`System.Diagnostics.ProcessMonitor`**: Real-time monitoring of CPU, memory and ThreadPool threads.
- **`System.Threading.AsyncLock`**: Asynchronous lock for resource synchronization.
- **`System.Threading.AsyncWaitHandle`**: Async wait handle with timeout and cancellation support.
- **`System.Threading.ReentrantAsyncLock`**: Reentrant asynchronous lock.
- **`System.Text.ValueStringBuilder`**: High-performance string builder (originally internal in .NET runtime, made public).

### Acknowledgements

Includes code derived from the .NET Runtime, licensed under the MIT License. The `ValueStringBuilder` class was originally internal and has been adapted to be public.

### Usage Examples

For comprehensive examples of how to use the package, see samples in the following repositories:

- [NuExt.System.Data.SQLite](https://github.com/IvanGit/NuExt.System.Data.SQLite)
- [NuExt.DevExpress.Mvvm](https://github.com/IvanGit/NuExt.DevExpress.Mvvm)
- [NuExt.DevExpress.Mvvm.MahApps.Metro](https://github.com/IvanGit/NuExt.DevExpress.Mvvm.MahApps.Metro)

### License

Licensed under the MIT License. See the LICENSE file for details.