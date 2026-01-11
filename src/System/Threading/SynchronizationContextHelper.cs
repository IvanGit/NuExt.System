using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Threading
{
    public class SynchronizationContextHelper(SynchronizationContext synchronizationContext)
    {
        private readonly SynchronizationContext _synchronizationContext = synchronizationContext ?? throw new ArgumentNullException(nameof(synchronizationContext));
        private static readonly ConcurrentDictionary<string, Func<SynchronizationContext, Func<bool>?>> s_checkAccessFactories = new(StringComparer.Ordinal);
        private static readonly Func<bool> s_falseFunc = () => false;

        private Func<bool>? CheckAccessDelegate
        {
            get => Interlocked.CompareExchange(ref field, null, null);
            set => Interlocked.Exchange(ref field, value);
        }

        public bool IsThreadAffineKnown { get; } = synchronizationContext.IsThreadAffineKnown;

        public bool CheckAccess()
        {
            if (!IsThreadAffineKnown) return false;

            if (ReferenceEquals(SynchronizationContext.Current, _synchronizationContext))
            {
                return true;
            }

            CheckAccessDelegate ??= GetCheckAccessDelegate();

            Debug.Assert(CheckAccessDelegate != null);

            return CheckAccessDelegate?.Invoke() == true;
        }

        private Func<bool>? GetCheckAccessDelegate()
        {
            if (_synchronizationContext is IThreadAffineSynchronizationContext context)
            {
                return context.CheckAccess;
            }

            var type = _synchronizationContext.GetType();
            return s_checkAccessFactories.GetOrAdd(type.FullName!,
#if NET_OLD
                fullName => CreateCheckAccessDelegate(fullName, type)
#else
                CreateCheckAccessDelegate, type
#endif
                ).Invoke(_synchronizationContext);
        }

        private static Func<SynchronizationContext, Func<bool>?> CreateCheckAccessDelegate(string fullName, Type type)
        {
            string message;
            switch (fullName)
            {
                case "Avalonia.Threading.AvaloniaSynchronizationContext":
                case "System.Windows.Threading.DispatcherSynchronizationContext":
                    var dispatcherTypeFullName = fullName.AsSpan(0, fullName.LastIndexOf('.') + 1).ToString() + "Dispatcher";
                    Debug.Assert(dispatcherTypeFullName == "System.Windows.Threading.Dispatcher" ||
                                 dispatcherTypeFullName == "Avalonia.Threading.Dispatcher");
                    var dispatcherFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    FieldInfo? dispatcherField = null;
                    foreach (var field in dispatcherFields)
                    {
                        if (field.FieldType.FullName != dispatcherTypeFullName) continue;
                        dispatcherField = field;
                        break;
                    }
                    Debug.Assert(dispatcherField != null);
                    if (dispatcherField == null) break;
                    var checkAccessMethod = dispatcherField.FieldType.GetMethod("CheckAccess");
                    Debug.Assert(checkAccessMethod != null);
                    if (checkAccessMethod == null) break;

                    try
                    {
                        var contextParam = Expression.Parameter(typeof(SynchronizationContext), "context");
                        var castContext = Expression.Convert(contextParam, type);
                        var fieldAccess = Expression.Field(castContext, dispatcherField);
                        var methodCall = Expression.Call(fieldAccess, checkAccessMethod);

                        var nullCheck = Expression.NotEqual(fieldAccess, Expression.Constant(null, dispatcherField.FieldType));
                        var conditional = Expression.Condition(nullCheck, methodCall, Expression.Constant(false));
                        var lambda = Expression.Lambda<Func<SynchronizationContext, bool>>(conditional, contextParam);
                        var compiledFunc = lambda.Compile();

                        return ctx => () => compiledFunc(ctx);
                    }
                    catch (Exception ex)
                    {
                        message = $"Expression compilation failed for {fullName}, using reflection fallback: {ex.Message}";
                        Debug.Fail(message);
                        Trace.WriteLine(message);
                        return ctx =>
                        {
                            var dispatcher = dispatcherField.GetValue(ctx);
                            Debug.Assert(dispatcher != null);
                            return dispatcher != null
                                ? Delegate.CreateDelegate(typeof(Func<bool>), dispatcher, checkAccessMethod) as Func<bool>
                                : s_falseFunc;
                        };
                    }
                case "System.Windows.Forms.WindowsFormsSynchronizationContext":
                    var controlFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    FieldInfo? controlField = null;
                    foreach (var field in controlFields)
                    {
                        if (field.FieldType.FullName != "System.Windows.Forms.Control") continue;
                        controlField = field;
                        break;
                    }
                    Debug.Assert(controlField != null);
                    if (controlField == null) break;
                    var invokeRequiredProperty = controlField.FieldType.GetProperty("InvokeRequired");
                    Debug.Assert(invokeRequiredProperty != null);
                    if (invokeRequiredProperty == null) break;

                    try
                    {
                        var contextParam = Expression.Parameter(typeof(SynchronizationContext), "context");
                        var castContext = Expression.Convert(contextParam, type);
                        var fieldAccess = Expression.Field(castContext, controlField);
                        var getProperty = Expression.Property(fieldAccess, invokeRequiredProperty);
                        var invert = Expression.Not(getProperty);

                        var nullCheck = Expression.NotEqual(fieldAccess, Expression.Constant(null, controlField.FieldType));
                        var conditional = Expression.Condition(nullCheck, invert, Expression.Constant(false));
                        var lambda = Expression.Lambda<Func<SynchronizationContext, bool>>(conditional, contextParam);
                        var compiledFunc = lambda.Compile();

                        return ctx => () => compiledFunc(ctx);
                    }
                    catch (Exception ex)
                    {
                        message = $"Expression compilation failed for {fullName}, using reflection fallback: {ex.Message}";
                        Debug.Fail(message);
                        Trace.WriteLine(message);

                        return ctx =>
                        {
                            var control = controlField.GetValue(ctx);
                            Debug.Assert(control != null);
                            return control != null
                                ? () => !(bool)invokeRequiredProperty.GetValue(control)!
                                : s_falseFunc;
                        };
                    }
            }

            message = $"Unexpected delegate resolution for {fullName}";
            Debug.Fail(message);
            Trace.WriteLine(message);
            return ctx => () => ctx.Invoke(() => Thread.CurrentThread) == Thread.CurrentThread;
        }
    }
}
