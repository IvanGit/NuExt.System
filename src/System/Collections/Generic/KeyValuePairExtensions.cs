#if NETFRAMEWORK
using System.ComponentModel;
using System.Diagnostics;

namespace System.Collections.Generic
{
    [DebuggerStepThrough]
    public static class KeyValuePairExtensions
    {

        //https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> keyValuePair, out TKey key, out TValue value)
        {
            key = keyValuePair.Key;
            value = keyValuePair.Value;
        }
    }
}
#endif
