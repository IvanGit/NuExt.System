# NuExt.System

Provides various fundamental extensions and classes for .NET, simplifying tasks related to asynchronous programming, resource lifecycle management, thread synchronization, and more.

### Features:
- Simplifies asynchronous resource disposal
- Manages the lifecycle of objects and resources
- Provides thread synchronization tools
- Implements property change notifications
- Includes high-performance string manipulation utilities

### Commonly Used Types:
- **System.AsyncDisposable:** Simplifies async resource disposal
- **System.AsyncEventHandler:** Represents an async event handler
- **System.AsyncLifeTime:** Manages async operations lifecycle
- **System.Disposable:** Base implementation of IDisposable
- **System.LifeTime:** Manages the lifecycle of objects and resources
- **System.ComponentModel.NotifyPropertyChanged:** Implementation of INotifyPropertyChanged
- **System.Threading.AsyncLock:** Async lock for resource synchronization
- **System.Threading.ReentrantAsyncLock:** Reentrant async lock
- **System.Text.ValueStringBuilder:** High-performance string builder (originally internal in .NET runtime, made public)

### License
Licensed under the MIT License. See the LICENSE file for details.

### Notice
Includes code derived from the .NET Runtime, licensed under the MIT License. The `ValueStringBuilder` class was originally internal and has been adapted to be public.