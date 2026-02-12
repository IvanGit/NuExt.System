// Notice: includes code derived from the .NET Runtime, licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    internal partial class SpanHelpers
    {
        public static bool Contains<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>?
        {
            Debug.Assert(length >= 0);

            nint index = 0; // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations

            if (default(T) != null || (object?)value != null)
            {
                Debug.Assert(value is not null);

                while (length >= 8)
                {
                    length -= 8;

                    if (value!.Equals(Unsafe.Add(ref searchSpace, index + 0)) ||
                        value.Equals(Unsafe.Add(ref searchSpace, index + 1)) ||
                        value.Equals(Unsafe.Add(ref searchSpace, index + 2)) ||
                        value.Equals(Unsafe.Add(ref searchSpace, index + 3)) ||
                        value.Equals(Unsafe.Add(ref searchSpace, index + 4)) ||
                        value.Equals(Unsafe.Add(ref searchSpace, index + 5)) ||
                        value.Equals(Unsafe.Add(ref searchSpace, index + 6)) ||
                        value.Equals(Unsafe.Add(ref searchSpace, index + 7)))
                    {
                        goto Found;
                    }

                    index += 8;
                }

                if (length >= 4)
                {
                    length -= 4;

                    if (value!.Equals(Unsafe.Add(ref searchSpace, index + 0)) ||
                        value.Equals(Unsafe.Add(ref searchSpace, index + 1)) ||
                        value.Equals(Unsafe.Add(ref searchSpace, index + 2)) ||
                        value.Equals(Unsafe.Add(ref searchSpace, index + 3)))
                    {
                        goto Found;
                    }

                    index += 4;
                }

                while (length > 0)
                {
                    length--;

                    if (value!.Equals(Unsafe.Add(ref searchSpace, index)))
                        goto Found;

                    index += 1;
                }
            }
            else
            {
                nint len = length;
                for (index = 0; index < len; index++)
                {
                    if ((object?)Unsafe.Add(ref searchSpace, index) is null)
                    {
                        goto Found;
                    }
                }
            }

            return false;

        Found:
            return true;
        }

        //1059
        internal static int IndexOfAnyExcept<T>(ref T searchSpace, T value0, int length)
        {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (int i = 0; i < length; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(Unsafe.Add(ref searchSpace, i), value0))
                {
                    return i;
                }
            }

            return -1;
        }

        //1074
        internal static int LastIndexOfAnyExcept<T>(ref T searchSpace, T value0, int length)
        {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (int i = length - 1; i >= 0; i--)
            {
                if (!EqualityComparer<T>.Default.Equals(Unsafe.Add(ref searchSpace, i), value0))
                {
                    return i;
                }
            }

            return -1;
        }

        //1089
        internal static int IndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, int length)
        {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (int i = 0; i < length; i++)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, i);
                if (!EqualityComparer<T>.Default.Equals(current, value0) && !EqualityComparer<T>.Default.Equals(current, value1))
                {
                    return i;
                }
            }

            return -1;
        }

        //1105
        internal static int LastIndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, int length)
        {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (int i = length - 1; i >= 0; i--)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, i);
                if (!EqualityComparer<T>.Default.Equals(current, value0) && !EqualityComparer<T>.Default.Equals(current, value1))
                {
                    return i;
                }
            }

            return -1;
        }

        //1137
        internal static int IndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, T value2, int length)
        {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (int i = 0; i < length; i++)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, i);
                if (!EqualityComparer<T>.Default.Equals(current, value0)
                    && !EqualityComparer<T>.Default.Equals(current, value1)
                    && !EqualityComparer<T>.Default.Equals(current, value2))
                {
                    return i;
                }
            }

            return -1;
        }

        //1139
        internal static int LastIndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, T value2, int length)
        {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (int i = length - 1; i >= 0; i--)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, i);
                if (!EqualityComparer<T>.Default.Equals(current, value0)
                    && !EqualityComparer<T>.Default.Equals(current, value1)
                    && !EqualityComparer<T>.Default.Equals(current, value2))
                {
                    return i;
                }
            }

            return -1;
        }

        //1157
        internal static int IndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, T value2, T value3, int length)
        {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (int i = 0; i < length; i++)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, i);
                if (!EqualityComparer<T>.Default.Equals(current, value0)
                    && !EqualityComparer<T>.Default.Equals(current, value1)
                    && !EqualityComparer<T>.Default.Equals(current, value2)
                    && !EqualityComparer<T>.Default.Equals(current, value3))
                {
                    return i;
                }
            }

            return -1;
        }

        //1176
        internal static int LastIndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, T value2, T value3, int length)
        {
            Debug.Assert(length >= 0, "Expected non-negative length");

            for (int i = length - 1; i >= 0; i--)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, i);
                if (!EqualityComparer<T>.Default.Equals(current, value0)
                    && !EqualityComparer<T>.Default.Equals(current, value1)
                    && !EqualityComparer<T>.Default.Equals(current, value2)
                    && !EqualityComparer<T>.Default.Equals(current, value3))
                {
                    return i;
                }
            }

            return -1;
        }

        //1306
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool ContainsValueType<T>(ref T searchSpace, T value, int length) where T : struct, IEquatable<T>
        {
            return IndexOfValueType(ref searchSpace, value, length) >= 0;
        }

        //1464
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int IndexOfChar(ref char searchSpace, char value, int length)
            => IndexOfValueType(ref Unsafe.As<char, short>(ref searchSpace), (short)value, length);

        //1476
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int IndexOfValueType<T>(ref T searchSpace, T value, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value is byte or short or int or long, "Expected caller to normalize to one of these types");

            nint index = 0;
            while (length > 0)
            {
                if (value.Equals(Unsafe.Add(ref searchSpace, index)))
                    goto Found;
                index += 1;
                length--;
            }

            return -1;
            Found:
            return (int)index;
        }

        //1480
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int IndexOfAnyExceptValueType<T>(ref T searchSpace, T value, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value is byte or short or int or long, "Expected caller to normalize to one of these types");

            nint index = 0;
            while (length > 0)
            {
                if (!value.Equals(Unsafe.Add(ref searchSpace, index)))
                    goto Found;
                index += 1;
                length--;
            }

            return -1;
            Found:
            return (int)index;
        }

        //1667
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int IndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value0 is byte or short or int or long, "Expected caller to normalize to one of these types");

            // Fast-path: both needles are the same -> fallback to single-needle search
            if (value0.Equals(value1))
                return IndexOfValueType(ref searchSpace, value0, length);

            nint index = 0;
            while (length > 0)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, index);
                if (value0.Equals(current) || value1.Equals(current))
                    goto Found;
                index += 1;
                length--;
            }

            return -1;
            Found:
            return (int)index;
        }

        //1670
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int IndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value0 is byte or short or int or long, "Expected caller to normalize to one of these types");

            // Fast-path: both needles are the same -> fallback to single-needle search
            if (value0.Equals(value1))
                return IndexOfAnyExceptValueType(ref searchSpace, value0, length);

            nint index = 0;
            while (length > 0)
            {
                ref T candidate = ref Unsafe.Add(ref searchSpace, index);
                if (!(value0.Equals(candidate) || value1.Equals(candidate)))
                    goto Found;

                index += 1;
                length--;
            }

            return -1;
            Found:
            return (int)index;
        }

        //1893
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int IndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value0 is byte or short or int or long, "Expected caller to normalize to one of these types");

            // Deduplicate needles to avoid redundant comparisons
            if (value0.Equals(value1) && value0.Equals(value2))
                return IndexOfValueType(ref searchSpace, value0, length);

            // Pair-equals -> reduce to two-needle overload
            if (value0.Equals(value1))
                return IndexOfAnyValueType(ref searchSpace, value0, value2, length);
            if (value0.Equals(value2))
                return IndexOfAnyValueType(ref searchSpace, value0, value1, length);
            if (value1.Equals(value2))
                return IndexOfAnyValueType(ref searchSpace, value0, value1, length);

            nint index = 0;
            while (length > 0)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, index);
                if (value0.Equals(current) || value1.Equals(current) || value2.Equals(current))
                    goto Found;
                index += 1;
                length--;
            }

            return -1;
            Found:
            return (int)index;
        }

        //1897
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int IndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value0 is byte or short or int or long, "Expected caller to normalize to one of these types");

            // Deduplicate needles to avoid redundant comparisons
            if (value0.Equals(value1) && value0.Equals(value2))
                return IndexOfAnyExceptValueType(ref searchSpace, value0, length);

            // Pair-equals -> reduce to two-needle overload
            if (value0.Equals(value1))
                return IndexOfAnyExceptValueType(ref searchSpace, value0, value2, length);
            if (value0.Equals(value2))
                return IndexOfAnyExceptValueType(ref searchSpace, value0, value1, length);
            if (value1.Equals(value2))
                return IndexOfAnyExceptValueType(ref searchSpace, value0, value1, length);

            nint index = 0;
            while (length > 0)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, index);
                if (!(value0.Equals(current) || value1.Equals(current) || value2.Equals(current)))
                    goto Found;

                index += 1;
                length -= 1;
            }

            return -1;
            Found:
            return (int)index;
        }

        //2104
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int IndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0);
            /*
            // Deduplicate needles to avoid redundant comparisons
            if (value0.Equals(value1) && value0.Equals(value2) && value0.Equals(value3))
                return IndexOfAnyExceptValueType(ref searchSpace, value0, length);
            */
            nint index = 0;
            while (length > 0)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, index);
                if (!(value0.Equals(current) || value1.Equals(current) || value2.Equals(current) || value3.Equals(current)))
                    goto Found;

                index += 1;
                length -= 1;
            }

            return -1;
        Found:
            return (int)index;
        }

       //2266
       [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int IndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, T value4, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0);
            /*
            // Deduplicate needles to avoid redundant comparisons
            if (value0.Equals(value1) && value0.Equals(value2) && value0.Equals(value3) && value0.Equals(value4))
                return IndexOfAnyExceptValueType(ref searchSpace, value0, length);
            */
            nint index = 0;
            while (length > 0)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, index);
                if (!(value0.Equals(current) || value1.Equals(current) || value2.Equals(current) || value3.Equals(current) || value4.Equals(current)))
                    goto Found;
                index += 1;
                length -= 1;
            }

            return -1;
            Found:
            return (int)index;
        }

        //2428
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LastIndexOfValueType<T>(ref T searchSpace, T value, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value is byte or short or int or long, "Expected caller to normalize to one of these types");

            // Scan from the end and return the first element that is equal to 'value'.
            nint index = (nint)length;
            while (index > 0)
            {
                index -= 1;
                if (value.Equals(Unsafe.Add(ref searchSpace, index)))
                    goto Found;
            }

            return -1;
            Found:
            return (int)index;
        }

        //2432
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LastIndexOfAnyExceptValueType<T>(ref T searchSpace, T value, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value is byte or short or int or long, "Expected caller to normalize to one of these types");

            // Scan from the end and return the first element that is not equal to 'value'.
            nint index = (nint)length;
            while (index > 0)
            {
                index -= 1;
                if (!value.Equals(Unsafe.Add(ref searchSpace, index)))
                    goto Found;
            }

            return -1;
            Found:
            return (int)index;
        }

        //2548
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LastIndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value0 is byte or short or int or long, "Expected caller to normalize to one of these types");

            // Fast-path: both values are the same -> reduce to single-value overload
            if (value0.Equals(value1))
                return LastIndexOfValueType(ref searchSpace, value0, length);

            // Scan from the end; return the last index equal to either value0 or value1.
            nint index = (nint)length;
            while (index > 0)
            {
                index -= 1;

                ref T current = ref Unsafe.Add(ref searchSpace, index);
                if (value0.Equals(current) || value1.Equals(current))
                    goto Found;
            }

            return -1;
            Found:
            return (int)index;
        }

        //2552
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LastIndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value0 is byte or short or int or long, "Expected caller to normalize to one of these types");

            // Fast-path: both values are the same -> reduce to single-value overload
            if (value0.Equals(value1))
                return LastIndexOfAnyExceptValueType(ref searchSpace, value0, length);

            nint index = (nint)length;
            while (index > 0)
            {
                index -= 1;
                ref T current = ref Unsafe.Add(ref searchSpace, index);
                if (!(value0.Equals(current) || value1.Equals(current)))
                    goto Found;
            }

            return -1;
            Found:
            return (int)index;
        }

        //2733
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LastIndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value0 is byte or short or int or long, "Expected caller to normalize to one of these types");

            // dedup fast-paths
            if (value0.Equals(value1) && value0.Equals(value2))
                return LastIndexOfValueType(ref searchSpace, value0, length);

            if (value0.Equals(value1))
                return LastIndexOfAnyValueType(ref searchSpace, value0, value2, length);
            if (value0.Equals(value2))
                return LastIndexOfAnyValueType(ref searchSpace, value0, value1, length);
            if (value1.Equals(value2))
                return LastIndexOfAnyValueType(ref searchSpace, value0, value1, length);

            // Scan from the end; return the last index equal to any of {value0, value1, value2}.
            nint index = (nint)length;
            while (index > 0)
            {
                index -= 1;

                ref T current = ref Unsafe.Add(ref searchSpace, index);
                if (value0.Equals(current) || value1.Equals(current) || value2.Equals(current))
                    goto Found;
            }

            return -1;
            Found:
            return (int)index;
        }

        //2737
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LastIndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value0 is byte or short or int or long, "Expected caller to normalize to one of these types");

            // dedup fast-paths
            if (value0.Equals(value1) && value0.Equals(value2))
                return LastIndexOfAnyExceptValueType(ref searchSpace, value0, length);

            if (value0.Equals(value1))
                return LastIndexOfAnyExceptValueType(ref searchSpace, value0, value2, length);
            if (value0.Equals(value2))
                return LastIndexOfAnyExceptValueType(ref searchSpace, value0, value1, length);
            if (value1.Equals(value2))
                return LastIndexOfAnyExceptValueType(ref searchSpace, value0, value1, length);

            // Scan from the end; return the last index whose element is NOT any of {value0, value1, value2}.
            nint index = (nint)length;
            while (index > 0)
            {
                index -= 1;

                ref T current = ref Unsafe.Add(ref searchSpace, index);
                if (!(value0.Equals(current) || value1.Equals(current) || value2.Equals(current)))
                    goto Found;
            }

            return -1;
            Found:
            return (int)index;
        }

        //2919
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LastIndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value0 is byte or short or int or long, "Expected caller to normalize to one of these types");

            // Scan from the end; return the last index equal to any of {value0, value1, value2, value3}.
            nint index = (nint)length;
            while (index > 0)
            {
                index -= 1;

                ref T current = ref Unsafe.Add(ref searchSpace, index);
                if (value0.Equals(current) || value1.Equals(current) || value2.Equals(current) || value3.Equals(current))
                    goto Found;
            }

            return -1;
        Found:
            return (int)index;
        }

        //2923
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LastIndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value0 is byte or short or int or long, "Expected caller to normalize to one of these types");

            // Scan from the end; return the last index whose element is NOT any of {value0, value1, value2, value3}.
            nint index = (nint)length;
            while (index > 0)
            {
                index -= 1;

                ref T current = ref Unsafe.Add(ref searchSpace, index);
                if (!(value0.Equals(current) || value1.Equals(current) || value2.Equals(current) || value3.Equals(current)))
                    goto Found;
            }

            return -1;
        Found:
            return (int)index;
        }

        //3068
        public static void Replace<T>(ref T src, ref T dst, T oldValue, T newValue, nuint length) where T : IEquatable<T>?
        {
            if (default(T) is not null || oldValue is not null)
            {
                Debug.Assert(oldValue is not null);

                for (nuint idx = 0; idx < length; ++idx)
                {
                    T original = Unsafe.Add(ref src, idx);
                    Unsafe.Add(ref dst, idx) = oldValue!.Equals(original) ? newValue : original;
                }
            }
            else
            {
                for (nuint idx = 0; idx < length; ++idx)
                {
                    T original = Unsafe.Add(ref src, idx);
                    Unsafe.Add(ref dst, idx) = original is null ? newValue : original;
                }
            }
        }

        //3098
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReplaceValueType<T>(ref T src, ref T dst, T oldValue, T newValue, nuint length) where T : struct
        {
            if (length == 0) return;
            var comparer = EqualityComparer<T>.Default;
            for (nuint idx = 0; idx < length; ++idx)
            {
                T original = Unsafe.Add(ref src, idx);
                Unsafe.Add(ref dst, idx) = comparer.Equals(original, oldValue) ? newValue : original;
            }
        }

        //3186
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LastIndexOfAnyValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, T value4, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value0 is byte or short or int or long, "Expected caller to normalize to one of these types");

            // Scan from the end; return the last index equal to any of {value0..value4}.
            nint index = (nint)length;
            while (index > 0)
            {
                index -= 1;

                ref T current = ref Unsafe.Add(ref searchSpace, index);
                if (value0.Equals(current) || value1.Equals(current) || value2.Equals(current) || value3.Equals(current) || value4.Equals(current))
                    goto Found;
            }

            return -1;
        Found:
            return (int)index;
        }

        //3190
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LastIndexOfAnyExceptValueType<T>(ref T searchSpace, T value0, T value1, T value2, T value3, T value4, int length) where T : struct, IEquatable<T>
        {
            Debug.Assert(length >= 0, "Expected non-negative length");
            Debug.Assert(value0 is byte or short or int or long, "Expected caller to normalize to one of these types");

            // Scan from the end; return the last index whose element is NOT any of {value0..value4}.
            nint index = (nint)length;
            while (index > 0)
            {
                index -= 1;

                ref T current = ref Unsafe.Add(ref searchSpace, index);
                if (!(value0.Equals(current) || value1.Equals(current) || value2.Equals(current) || value3.Equals(current) || value4.Equals(current)))
                    goto Found;
            }

            return -1;
        Found:
            return (int)index;
        }

        //3446
        internal static int IndexOfAnyInRange<T>(ref T searchSpace, T lowInclusive, T highInclusive, int length)
            where T : IComparable<T>
        {
            for (int i = 0; i < length; i++)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, i);
                if ((lowInclusive.CompareTo(current) <= 0) && (highInclusive.CompareTo(current) >= 0))
                {
                    return i;
                }
            }

            return -1;
        }

        //3461
        internal static int IndexOfAnyExceptInRange<T>(ref T searchSpace, T lowInclusive, T highInclusive, int length)
            where T : IComparable<T>
        {
            for (int i = 0; i < length; i++)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, i);
                if ((lowInclusive.CompareTo(current) > 0) || (highInclusive.CompareTo(current) < 0))
                {
                    return i;
                }
            }

            return -1;
        }

        //3612
        internal static int LastIndexOfAnyInRange<T>(ref T searchSpace, T lowInclusive, T highInclusive, int length)
            where T : IComparable<T>
        {
            for (int i = length - 1; i >= 0; i--)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, i);
                if ((lowInclusive.CompareTo(current) <= 0) && (highInclusive.CompareTo(current) >= 0))
                {
                    return i;
                }
            }

            return -1;
        }

        //3627
        internal static int LastIndexOfAnyExceptInRange<T>(ref T searchSpace, T lowInclusive, T highInclusive, int length)
            where T : IComparable<T>
        {
            for (int i = length - 1; i >= 0; i--)
            {
                ref T current = ref Unsafe.Add(ref searchSpace, i);
                if ((lowInclusive.CompareTo(current) > 0) || (highInclusive.CompareTo(current) < 0))
                {
                    return i;
                }
            }

            return -1;
        }

        //3753
        public static int Count<T>(ref T current, T value, int length) where T : IEquatable<T>?
        {
            int count = 0;

            ref T end = ref Unsafe.Add(ref current, length);
            if (value is not null)
            {
                while (Unsafe.IsAddressLessThan(ref current, ref end))
                {
                    if (value.Equals(current))
                    {
                        count++;
                    }

                    current = ref Unsafe.Add(ref current, 1);
                }
            }
            else
            {
                while (Unsafe.IsAddressLessThan(ref current, ref end))
                {
                    if (current is null)
                    {
                        count++;
                    }

                    current = ref Unsafe.Add(ref current, 1);
                }
            }

            return count;
        }

        //3786
        public static unsafe int CountValueType<T>(ref T current, T value, int length) where T : struct, IEquatable<T>
        {
            int count = 0;
            ref T end = ref Unsafe.Add(ref current, length);

            while (Unsafe.IsAddressLessThan(ref current, ref end))
            {
                if (current.Equals(value))
                {
                    count++;
                }

                current = ref Unsafe.Add(ref current, 1);
            }

            return count;
        }
    }
}
