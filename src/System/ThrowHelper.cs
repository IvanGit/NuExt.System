using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System
{
    [StackTraceHidden]

    internal static class ThrowHelper
    {
        [DoesNotReturn]
        internal static void ThrowFormatIndexOutOfRange()
        {
            throw new FormatException(SR.Format_IndexOutOfRange);
        }

        [DoesNotReturn]
        internal static void ThrowFormatInvalidString(int offset, string resourceString)
        {
            throw new FormatException(string.Format(SR.Format_InvalidStringWithOffsetAndReason, offset, resourceString));
        }

        [DoesNotReturn]
        internal static void ThrowOutOfMemoryException_StringTooLong()
        {
            throw new OutOfMemoryException(SR.OutOfMemory_StringTooLong);
        }

        [DoesNotReturn]
        internal static void ThrowObjectDisposedException(object? instance)
        {
            throw new ObjectDisposedException(instance?.GetType().FullName);
        }

        [DoesNotReturn]
        internal static void ThrowObjectDisposedException(Type? type)
        {
            throw new ObjectDisposedException(type?.FullName);
        }
    }
}
