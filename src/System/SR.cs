namespace System
{
    internal static class SR
    {
        public const string Argument_EmptyString = "The value cannot be an empty string.";
        public const string Argument_EmptyOrWhiteSpaceString = "The value cannot be an empty string or composed entirely of whitespace.";
        public const string Argument_WhiteSpaceString = "The value cannot be a white-space string.";

        public const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";

        public const string Format_IndexOutOfRange = "Index (zero based) must be greater than or equal to zero and less than the size of the argument list.";
        public const string Format_InvalidStringWithOffsetAndReason = "Input string was not in a correct format. Failure to parse near offset {0}. {1}";

        public const string InvalidOperation_CannotInvokeOnTheTargetThread =
            "Cannot invoke on the target thread because no SynchronizationContext is available " +
            "and the calling thread (ID: {0}) is not thread #{1} where the object was created.";
        public const string InvalidOperation_ThreadAccessError =
            "Cannot access this object from the current thread. " +
            "If this object has a SynchronizationContext, use the Invoke or InvokeAsync methods for thread-safe access. " +
            "Otherwise, ensure all access is made from the thread on which the object was created (Thread ID: {0}).";
        public const string InvalidOperation_SynchronizationContextAccessDenied =
            "Thread access violation. The calling thread (ID: {0}) attempted to access " +
            "a synchronization context bound to thread (ID: {1}). Use the Send or Post methods " +
            "for cross-thread operations.";

        public const string OutOfMemory_StringTooLong = "String length exceeded supported range.";

        public const string VerifyAccess = "The calling thread cannot access this object because a different thread owns it.";
    }
}
