using System.ComponentModel;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    public static class DelegateExtensions
    {
        extension(Delegate callback)
        {
            /// <summary>
            /// Dynamically invokes the specified delegate with the provided arguments.
            /// Uses direct invocation for common delegate types (Action, Func, EventHandler, etc.)
            /// falling back to <see cref="Delegate.DynamicInvoke(object[])"/> only when needed.
            /// <para>
            /// ⚠️ <b>For strongly-typed delegates (Action, Func, etc.), prefer direct invocation:</b>
            /// <code>action("arg"); // Instead of action.Call("arg")</code>
            /// This method is optimized for dynamic scenarios where only a <see cref="Delegate"/>
            /// reference is available.
            /// </para>
            /// </summary>
            /// <param name="args">An array of arguments to pass to the delegate or null, if the delegate does not require arguments.</param>
            /// <returns>The return value from the delegate, or null if the delegate returns void.</returns>
            /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
            public object? Call(params object?[]? args)
            {
                ArgumentNullException.ThrowIfNull(callback);

                args ??= [];

                try
                {
                    return args.Length switch
                    {
                        0 => callback.InvokeParameterless(),
                        1 => callback.InvokeWithOneArg(args),
                        2 => callback.InvokeWithTwoArgs(args),
                        _ => callback.DynamicInvoke(args)
                    };
                }
                catch (TargetInvocationException ex) when (ex.InnerException is not null)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                    // The above line will always throw, but the compiler requires we throw explicitly.
                    throw;
                }
            }

            private object? InvokeParameterless()
            {
                switch (callback)
                {
                    case Action action: action(); return null;
                    case Func<Task> taskFunc: return taskFunc();
                    case Func<string?> stringFunc: return stringFunc();
                    case Func<object?> func: return func();
                    case Func<int> intFunc: return intFunc();
                    case Func<bool> boolFunc: return boolFunc();
                    case Func<long> longFunc: return longFunc();
                    case Func<double> doubleFunc: return doubleFunc();
                    case Func<DateTime> dateFunc: return dateFunc();
                    default: return callback.DynamicInvoke([]);
                }
            }

            private object? InvokeWithOneArg(object?[] args)
            {
                var arg0 = args[0];
                switch (callback)
                {
                    case Action<object?> actionObj: actionObj(arg0); return null;
                    case Action<string?> actionStr when arg0 is string str: actionStr(str); return null;
                    case Action<int> actionInt when arg0 is int i: actionInt(i); return null;
                    case Action<bool> actionBool when arg0 is bool b: actionBool(b); return null;
                    case Func<object?, object?> funcObj: return funcObj(arg0);
                    case Predicate<object?> predicate: return predicate(arg0);
                    case Predicate<string?> predicateStr when arg0 is string str: return predicateStr(str);
                    case SendOrPostCallback sendOrPost: sendOrPost(arg0); return null;
                    default: return callback.DynamicInvoke(args);
                }
            }

            private object? InvokeWithTwoArgs(object?[] args)
            {
                var arg0 = args[0];
                var arg1 = args[1];
                switch (callback)
                {
                    case EventHandler eventHandler when arg1 is EventArgs e: eventHandler(arg0, e); return null;
                    case PropertyChangedEventHandler eventHandler when arg1 is PropertyChangedEventArgs e: eventHandler(arg0, e); return null;
                    case Action<object?, object?> actionObj: actionObj(arg0, arg1); return null;
                    case Action<string, string> actionStr when arg0 is string s1 && arg1 is string s2: actionStr(s1, s2); return null;
                    case Func<object?, object?, object?> funcObj: return funcObj(arg0, arg1);
                    default: return callback.DynamicInvoke(args);
                }
            }

            /// <summary>
            /// Safely attempts to dynamically invoke the delegate without throwing exceptions. 
            /// Returns false if the delegate is null or if invocation fails.
            /// <para>
            /// ⚠️ <b>For strongly-typed delegates, prefer direct invocation with try-catch:</b>
            /// <code>try { action("arg"); } catch { } // Instead of action.TryCall(...)</code>
            /// This method is designed for dynamic scenarios where exception safety is needed
            /// and only a <see cref="Delegate"/> reference is available.
            /// </para>
            /// </summary>
            /// <param name="result">The return value from the delegate, or null if the delegate returns void or invocation failed.</param>
            /// <param name="args">An array of arguments to pass to the delegate or null, if the delegate does not require arguments.</param>
            /// <returns>true if the delegate was invoked successfully; otherwise, false.</returns>
            public bool TryCall(out object? result, params object?[]? args)
            {
                try
                {
                    if (callback != null)
                    {
                        result = callback.Call(args);
                        return true;
                    }
                }
                catch
                {
                }
                result = null;
                return false;
            }

        }
    }
}
