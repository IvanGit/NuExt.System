// Notice: includes code derived from the .NET Runtime, licensed under the MIT License.
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace System
{
    partial class SpanHelpers
    {
        public static int IndexOf(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return 0;  // A zero-length sequence is always treated as "found" at the start of the search space.

            int valueTailLength = valueLength - 1;
            if (valueTailLength == 0)
                return IndexOfValueType(ref searchSpace, value, searchSpaceLength); // for single-byte values use plain IndexOf

            nint offset = 0;
            byte valueHead = value;
            int searchSpaceMinusValueTailLength = searchSpaceLength - valueTailLength;

            ref byte valueTail = ref Unsafe.Add(ref value, 1);
            int remainingSearchSpaceLength = searchSpaceMinusValueTailLength;

            while (remainingSearchSpaceLength > 0)
            {
                // Do a quick search for the first element of "value".
                int relativeIndex = IndexOfValueType(ref Unsafe.Add(ref searchSpace, offset), valueHead, remainingSearchSpaceLength);
                if (relativeIndex < 0)
                    break;

                remainingSearchSpaceLength -= relativeIndex;
                offset += relativeIndex;

                if (remainingSearchSpaceLength <= 0)
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(
                        ref Unsafe.Add(ref searchSpace, offset + 1),
                        ref valueTail, (nuint)(uint)valueTailLength))  // The (nuint)-cast is necessary to pick the correct overload
                    return (int)offset;  // The tail matched. Return a successful find.

                remainingSearchSpaceLength--;
                offset++;
            }
            return -1;
        }

        //252
        public static int LastIndexOf(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return searchSpaceLength;  // A zero-length sequence is always treated as "found" at the end of the search space.

            int valueTailLength = valueLength - 1;
            if (valueTailLength == 0)
                return LastIndexOfValueType(ref searchSpace, value, searchSpaceLength); // for single-byte values use plain LastIndexOf

            int offset = 0;
            byte valueHead = value;

            ref byte valueTail = ref Unsafe.Add(ref value, 1);

            while (true)
            {
                Debug.Assert(0 <= offset && offset <= searchSpaceLength); // Ensures no deceptive underflows in the computation of "remainingSearchSpaceLength".
                int remainingSearchSpaceLength = searchSpaceLength - offset - valueTailLength;
                if (remainingSearchSpaceLength <= 0)
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Do a quick search for the first element of "value".
                int relativeIndex = LastIndexOfValueType(ref searchSpace, valueHead, remainingSearchSpaceLength);
                if (relativeIndex < 0)
                    break;

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(
                        ref Unsafe.Add(ref searchSpace, relativeIndex + 1),
                        ref valueTail, (nuint)(uint)valueTailLength)) // The (nuint)-cast is necessary to pick the correct overload
                    return relativeIndex;  // The tail matched. Return a successful find.

                offset += remainingSearchSpaceLength - relativeIndex;
            }
            return -1;

        }

        //760
        public static bool SequenceEqual(ref byte first, ref byte second, nuint length)
        {
            if (length == 0) return true;

            nuint index = 0;
            while (length > 0)
            {
                if (Unsafe.Add(ref first, index) != Unsafe.Add(ref second, index))
                    goto Fail;
                index += 1;
                length--;
            }

            return true;
            Fail:
            return false;
        }

        public static nuint CommonPrefixLength(ref byte first, ref byte second, nuint length)
        {
            nuint i;
            // To have kind of fast path for small inputs, we handle as much elements needed
            // so that either we are done or can use the unrolled loop below.
            i = length % 4;

            if (i > 0)
            {
                if (first != second)
                {
                    return 0;
                }

                if (i > 1)
                {
                    if (Unsafe.Add(ref first, 1) != Unsafe.Add(ref second, 1))
                    {
                        return 1;
                    }

                    if (i > 2 && Unsafe.Add(ref first, 2) != Unsafe.Add(ref second, 2))
                    {
                        return 2;
                    }
                }
            }

            for (; (nint)i <= (nint)length - 4; i += 4)
            {
                if (Unsafe.Add(ref first, i + 0) != Unsafe.Add(ref second, i + 0)) goto Found0;
                if (Unsafe.Add(ref first, i + 1) != Unsafe.Add(ref second, i + 1)) goto Found1;
                if (Unsafe.Add(ref first, i + 2) != Unsafe.Add(ref second, i + 2)) goto Found2;
                if (Unsafe.Add(ref first, i + 3) != Unsafe.Add(ref second, i + 3)) goto Found3;
            }

            return length;
        Found0:
            return i;
        Found1:
            return i + 1;
        Found2:
            return i + 2;
        Found3:
            return i + 3;
        }

    }
}
