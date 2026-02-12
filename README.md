
# NuExt.System

`NuExt.System` is a **lightweight, production-ready foundation** for everyday .NET development. It brings together high-quality utilities for **asynchrony**, **lifetime management**, **threading primitives**, **high-performance spans/memory helpers**, **diagnostics**, and **collection helpers** — all with a strong focus on **performance**, **correctness**, and **developer ergonomics**.  
Use it to reduce boilerplate, standardize common patterns across projects, and keep code fast and clean across modern .NET and .NET Framework.

## Features

- **Asynchrony & lifetime**
  - Async locks and wait handles (`AsyncLock`, `ReentrantAsyncLock`, `AsyncWaitHandle`)
  - Disposable tooling (`Disposable`, `AggregateDisposable`, `AsyncDisposable`, `AggregateAsyncDisposable`, `AsyncLifetime`)
- **Threading & synchronization**
  - Reentrant async lock (context-aware), synchronization context helpers, thread-affine context
- **High-performance spans & memory**
  - Backports/polyfills for `Span` / `MemoryExtensions` APIs (see below)
  - `ValueStringBuilder` / `ValueListBuilder<T>` for zero-allocation building
- **Collections & equality**
  - `ObservableDictionary<TKey, TValue>`, `ReferenceEqualityComparer` (polyfill), ArrayEqualityComparer, plus **many useful extension helpers**
- **I/O & paths (cross-platform)**
  - `PathBuilder` (class, `IDisposable`) and `ValuePathBuilder` (ref struct) — platform-independent path builders
  - `PathUtilities` — static helpers for common path operations
- **Diagnostics & utilities**
  - `ProcessMonitor`, `PerformanceMonitor`, `EnumHelper<T>`, `TypeExtensions`, and more

### Span / MemoryExtensions Polyfills

This package includes **polyfills (API backports)** for selected `Span` / `MemoryExtensions`-style APIs from newer .NET versions.  
On modern runtimes, it transparently uses the inbox implementations; on older runtimes, it provides compatible behavior with the same semantics.

**What you get (highlights)**
- Search & comparison: `Contains`, `SequenceEqual` (+ `IEqualityComparer<T>`)
- Indexing: `IndexOf`, `LastIndexOf` (element / sequence)
- Set-based search: `IndexOfAny`, `LastIndexOfAny`, `IndexOfAnyExcept`, `LastIndexOfAnyExcept`
- Range-based: `IndexOfAnyInRange`, `IndexOfAnyExceptInRange`, `LastIndexOfAnyInRange`, `LastIndexOfAnyExceptInRange`
- Utilities: `StartsWith`, `EndsWith`, `Replace` (in-place / copy), `Count`, `CountAny`

**Notes**
- Allocation-free, performance-oriented; value types use bitwise fast paths where applicable.
- Semantics match the .NET runtime APIs; custom comparers are honored when provided.

## Why NuExt.System?

- **Practical**: battle-tested building blocks you use every day  
- **Fast**: zero-alloc paths, tight loops, and careful branching profiles  
- **Consistent**: same behavior across modern .NET and .NET Framework  
- **Focused**: no heavy external dependencies or configuration

### Compatibility

- **.NET Standard 2.0+, .NET 8/9/10 and .NET Framework 4.6.2+**
- **Works across desktop, web, and services** (Console, ASP.NET Core, Avalonia, WinUI, WPF, WinForms)

## Commonly Used Types

- **Asynchrony & lifetime**
  - `System.Threading.AsyncLock`, `System.Threading.ReentrantAsyncLock`, `System.Threading.AsyncWaitHandle`
  - `System.ComponentModel.Disposable`, `AggregateDisposable`, `AsyncDisposable`, `AggregateAsyncDisposable`, `AsyncLifetime`
- **Collections & equality**
  - `System.Collections.ObjectModel.ObservableDictionary<TKey, TValue>`
  - `System.Collections.Generic.ValueListBuilder<T>`
  - `System.Collections.Generic.ReferenceEqualityComparer` *(polyfill)*
  - `System.Collections.Generic.ArrayEqualityComparer<T>`
  - **Various useful extension helpers** (collections, delegates, enums, strings, exceptions)
- **Strings, spans, and memory**
  - `System.Text.ValueStringBuilder`
  - `System.CompatMemoryExtensions` (polyfills/backports)
- **I/O & paths (cross-platform)**
  - `System.IO.PathBuilder` *(class, `IDisposable` — mutable path builder)*
  - `System.IO.ValuePathBuilder` *(ref struct — high-performance mutable path builder)*
  - `System.IO.PathUtilities` *(static common path operations)*  
- **Diagnostics & helpers**
  - `System.Diagnostics.ProcessMonitor`, `PerformanceMonitor`
  - `System.EnumHelper<T>`, `System.FormatUtils`, `System.HexConverter`

### Installation

Via [NuGet](https://www.nuget.org/):

```sh
dotnet add package NuExt.System
```

Or via Visual Studio:

1. Go to `Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution...`.
2. Search for `NuExt.System`.
3. Click "Install".

### ReentrantAsyncLock Internals

The `ReentrantAsyncLock` provides a context-aware reentrant async lock. It uses `AsyncLocal` to track ownership across awaits, allowing the same logical flow to re-enter without deadlocks.

In scenarios that capture **ExecutionContext** (e.g., `CancellationToken.Register`), prefer suppressing the flow to avoid leaking `AsyncLocal` state into callbacks:

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

### Usage Examples

#### `ValueListBuilder<T>` and `ValueTask.WhenAll`

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

#### `ValueListBuilder<T>` and `ValueTask.WhenAll<TResult>`

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

For comprehensive examples of how to use the package, see:

- [NuExt.System.Data.SQLite](https://github.com/IvanGit/NuExt.System.Data.SQLite)
- [NuExt.DevExpress.Mvvm](https://github.com/IvanGit/NuExt.DevExpress.Mvvm)
- [NuExt.DevExpress.Mvvm.MahApps.Metro](https://github.com/IvanGit/NuExt.DevExpress.Mvvm.MahApps.Metro)
- [NuExt.Minimal.Mvvm.Wpf](https://github.com/IvanGit/NuExt.Minimal.Mvvm.Wpf)
- [NuExt.Minimal.Mvvm.MahApps.Metro](https://github.com/IvanGit/NuExt.Minimal.Mvvm.MahApps.Metro)

### Acknowledgements

Includes code derived from the .NET Runtime, licensed under the MIT License. The original source code can be found in the [.NET Runtime GitHub repository](https://github.com/dotnet/runtime).

### Contributing

Issues and PRs are welcome. Keep changes minimal and performance-conscious.

### License

MIT. See LICENSE.