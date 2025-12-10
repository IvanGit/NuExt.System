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

        public const string OutOfMemory_StringTooLong = "String length exceeded supported range.";

        public const string VerifyAccess = "The calling thread cannot access this object because a different thread owns it.";
    }
}
