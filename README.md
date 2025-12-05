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
- **`System.PropertyChangeNotifier`**: Implementation of `INotifyPropertyChanged`.
- **`System.Diagnostics.ProcessMonitor`**: Real-time monitoring of CPU, memory and ThreadPool threads.
- **`System.Threading.AsyncLock`**: Asynchronous lock for resource synchronization.
- **`System.Threading.AsyncWaitHandle`**: Async wait handle with timeout and cancellation support.
- **`System.Threading.ReentrantAsyncLock`**: Reentrant asynchronous lock.
- **`System.Threading.Tasks.ValueTaskExtensions`**: `ValueTask.WhenAll` and `ValueTask.WhenAll<TResult>` implementations.
- **`System.IO.PathBuilder`**: Builder for constructing paths.
- **`System.IO.ValuePathBuilder`**: High-performance builder for constructing paths.
- **`System.Text.ValueStringBuilder`**: High-performance string builder (originally internal in .NET runtime).
- **`System.Collections.Generic.ValueListBuilder<T>`**: High-performance list builder (originally internal in .NET runtime).

### Installation

You can install `NuExt.System` via [NuGet](https://www.nuget.org/):

```sh
dotnet add package NuExt.System
```

Or via the Visual Studio package manager:

1. Go to `Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution...`.
2. Search for `NuExt.System`.
3. Click "Install".

### ReentrantAsyncLock Internals

The `ReentrantAsyncLock` class provides a reentrant (re-enterable) asynchronous lock. This means that the same thread can acquire the lock multiple times without blocking itself. It is particularly useful for complex asynchronous scenarios where recursive calls are expected.

`ReentrantAsyncLock` relies on `AsyncLocal` to manage the lock's state across asynchronous method calls, ensuring that the lock is associated with the correct execution context. `AsyncLocal` variables store data that is unique to a particular asynchronous control flow, allowing different asynchronous operations to have their own distinct contexts.

In most cases, you won't encounter any issues. However, in specific scenarios where certain methods, such as `CancellationToken.Register`, might capture the `ExecutionContext`, here's an example demonstrating a preferred usage. Instead of:

```csharp
var asyncLock = new ReentrantAsyncLock();
var cts = new CancellationTokenSource();

asyncLock.Acquire(() => cts.Token.Register(() => asyncLock.Acquire(() =>
{
    // user code
})));

asyncLock.Acquire(() => cts.Cancel());
```

It is preferable to do:

```csharp
var asyncLock = new ReentrantAsyncLock();
var cts = new CancellationTokenSource();

asyncLock.Acquire(() => 
{
    //Don't capture the current ExecutionContext and its AsyncLocals for CancellationToken.Register
    using (ExecutionContext.SuppressFlow())
    {
        cts.Token.Register(() => asyncLock.Acquire(() =>
        {
            // user code
        }));
    }
    //The current ExecutionContext is restored after exiting the using block
});

asyncLock.Acquire(() => cts.Cancel());
```

This ensures that the `AsyncLocal` values do not unintentionally flow into the registered callbacks, maintaining the intended behavior of your application.

### Usage Examples

#### Examples Using `ValueListBuilder<T>` and `ValueTask.WhenAll` extensions

##### Using `ValueListBuilder<T>` and `ValueTask.WhenAll`

```csharp
public class Example
{
    public static async Task Main()
    {
        int failed = 0;
        String[] urls = [ "www.adatum.com", "www.cohovineyard.com",
                        "www.cohowinery.com", "www.northwindtraders.com",
                        "www.contoso.com" ];
        var tasks = new ValueListBuilder<ValueTask>(urls.Length);

        foreach (var value in urls)
        {
            var url = value;
            tasks.Append(new ValueTask(Task.Run(() =>
            {
                var png = new Ping();
                try
                {
                    var reply = png.Send(url);
                    if (reply.Status != IPStatus.Success)
                    {
                        Interlocked.Increment(ref failed);
                        throw new TimeoutException("Unable to reach " + url + ".");
                    }
                }
                catch (PingException)
                {
                    Interlocked.Increment(ref failed);
                    throw;
                }
            })));
        }
        ValueTask t = ValueTask.WhenAll(tasks.ToArray());
        try
        {
            await t;
        }
        catch { }

        if (t.IsCompletedSuccessfully)
            Console.WriteLine("All ping attempts succeeded.");
        else if (t.IsFaulted)
            Console.WriteLine("{0} ping attempts failed", failed);
    }
}
```

##### Using `ValueListBuilder<T>` and `ValueTask.WhenAll<T>`

```csharp
public class Example
{
    public static async Task Main()
    {
        int failed = 0;
        String[] urls = [ "www.adatum.com", "www.cohovineyard.com",
                        "www.cohowinery.com", "www.northwindtraders.com",
                        "www.contoso.com" ];
        var tasks = new ValueListBuilder<ValueTask<PingReply>>(urls.Length);

        foreach (var value in urls)
        {
            var url = value;
            tasks.Append(new ValueTask<PingReply>(Task.Run(() =>
            {
                var png = new Ping();
                try
                {
                    var reply = png.Send(url);
                    if (reply.Status != IPStatus.Success)
                    {
                        Interlocked.Increment(ref failed);
                        throw new TimeoutException("Unable to reach " + url + ".");
                    }
                    return reply;
                }
                catch (PingException)
                {
                    Interlocked.Increment(ref failed);
                    throw;
                }
            })));
        }
        try
        {
            PingReply[] replies = await ValueTask.WhenAll(tasks.ToArray());
            Console.WriteLine("{0} ping attempts succeeded:", replies.Length);
            for (int i = 0; i < replies.Length; i++)
            {
                var reply = replies[i];
                Console.WriteLine($"Reply from {reply.Address}: bytes={reply.Buffer.Length} time={reply.RoundtripTime}ms TTL={reply.Options?.Ttl} [{urls[i]}]");
            }
        }
        catch (AggregateException)
        {
            Console.WriteLine("{0} ping attempts failed", failed);
        }
    }
}
```

For comprehensive examples of how to use the package, see samples in the following repositories:

- [NuExt.System.Data.SQLite](https://github.com/IvanGit/NuExt.System.Data.SQLite)
- [NuExt.DevExpress.Mvvm](https://github.com/IvanGit/NuExt.DevExpress.Mvvm)
- [NuExt.DevExpress.Mvvm.MahApps.Metro](https://github.com/IvanGit/NuExt.DevExpress.Mvvm.MahApps.Metro)
- [NuExt.Minimal.Mvvm.Windows](https://github.com/IvanGit/NuExt.Minimal.Mvvm.Windows)
- [NuExt.Minimal.Mvvm.MahApps.Metro](https://github.com/IvanGit/NuExt.Minimal.Mvvm.MahApps.Metro)

### Acknowledgements

Includes code derived from the .NET Runtime, licensed under the MIT License. The original source code can be found in the [.NET Runtime GitHub repository](https://github.com/dotnet/runtime).

### License

Licensed under the MIT License. See the LICENSE file for details.