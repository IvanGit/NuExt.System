// Notice: includes code derived from the .NET Runtime, licensed under the MIT License.
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    partial class SpanHelpers
    {
        public static int IndexOf(ref char searchSpace, int searchSpaceLength, ref char value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return 0;  // A zero-length sequence is always treated as "found" at the start of the search space.

            int valueTailLength = valueLength - 1;
            if (valueTailLength == 0)
            {
                // for single-char values use plain IndexOf
                return IndexOfChar(ref searchSpace, value, searchSpaceLength);
            }

            nint offset = 0;
            char valueHead = value;
            int searchSpaceMinusValueTailLength = searchSpaceLength - valueTailLength;

            ref byte valueTail = ref Unsafe.As<char, byte>(ref Unsafe.Add(ref value, 1));
            int remainingSearchSpaceLength = searchSpaceMinusValueTailLength;

            while (remainingSearchSpaceLength > 0)
            {
                // Do a quick search for the first element of "value".
                int relativeIndex = IndexOfChar(ref Unsafe.Add(ref searchSpace, offset), valueHead, remainingSearchSpaceLength);
                if (relativeIndex < 0)
                    break;

                remainingSearchSpaceLength -= relativeIndex;
                offset += relativeIndex;

                if (remainingSearchSpaceLength <= 0)
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(
                        ref Unsafe.As<char, byte>(ref Unsafe.Add(ref searchSpace, offset + 1)),
                        ref valueTail,
                        (nuint)(uint)valueTailLength * 2))
                {
                    return (int)offset;  // The tail matched. Return a successful find.
                }

                remainingSearchSpaceLength--;
                offset++;
            }
            return -1;
        }

        //264
        public static int LastIndexOf(ref char searchSpace, int searchSpaceLength, ref char value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return searchSpaceLength;  // A zero-length sequence is always treated as "found" at the end of the search space.

            int valueTailLength = valueLength - 1;
            if (valueTailLength == 0)
                return LastIndexOfValueType(ref Unsafe.As<char, short>(ref searchSpace), (short)value, searchSpaceLength); // for single-char values use plain LastIndexOf

            int offset = 0;
            char valueHead = value;

            ref byte valueTail = ref Unsafe.As<char, byte>(ref Unsafe.Add(ref value, 1));

            while (true)
            {
                Debug.Assert(0 <= offset && offset <= searchSpaceLength); // Ensures no deceptive underflows in the computation of "remainingSearchSpaceLength".
                int remainingSearchSpaceLength = searchSpaceLength - offset - valueTailLength;
                if (remainingSearchSpaceLength <= 0)
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Do a quick search for the first element of "value".
                int relativeIndex = LastIndexOfValueType(ref Unsafe.As<char, short>(ref searchSpace), (short)valueHead, remainingSearchSpaceLength);
                if (relativeIndex == -1)
                    break;

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(
                        ref Unsafe.As<char, byte>(ref Unsafe.Add(ref searchSpace, relativeIndex + 1)),
                        ref valueTail, (nuint)(uint)valueTailLength * 2))
                {
                    return relativeIndex; // The tail matched. Return a successful find.
                }

                offset += remainingSearchSpaceLength - relativeIndex;
            }
            return -1;
        }

        public static int IndexOfAnyExcept(ref char searchSpace, int searchSpaceLength, ref char values, int valuesLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valuesLength >= 0);

            // Empty search space -> not found
            if (searchSpaceLength == 0)
                return -1;

            // Empty set -> the first element is not excluded by definition
            if (valuesLength == 0)
                return 0;

            // Build a 64K bitmap (8 KiB) for O(1) membership checks of UTF-16 code units [0..65535].
            Span<byte> bitmap = stackalloc byte[8192]; // 65536 / 8
            bitmap.Clear();

            // Populate bitmap; duplicates are harmless
            for (int i = 0; i < valuesLength; i++)
            {
                int code = Unsafe.Add(ref values, i);             // char -> int promotion
                bitmap[code >> 3] |= (byte)(1 << (code & 7));     // set bit
            }

            // Scan search space and return the first char NOT present in the bitmap (bit == 0).
            nint index = 0;
            int remaining = searchSpaceLength;
            while (remaining > 0)
            {
                int code = Unsafe.Add(ref searchSpace, index);
                if ((bitmap[code >> 3] & (1 << (code & 7))) == 0)
                    goto Found;

                index += 1;
                remaining -= 1;
            }

            return -1;
            Found:
            return (int)index;
        }

        public static int LastIndexOfAnyExcept(ref char searchSpace, int searchSpaceLength, ref char values, int valuesLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valuesLength >= 0);

            // Empty search space -> not found
            if (searchSpaceLength == 0)
                return -1;

            // Empty set -> the last element is not excluded by definition
            if (valuesLength == 0)
                return searchSpaceLength - 1;

            // Build a 64K bitmap (8 KiB) for O(1) membership checks of UTF-16 code units [0..65535].
            Span<byte> bitmap = stackalloc byte[8192]; // 65536 / 8
            bitmap.Clear();

            // Populate bitmap; duplicates are harmless
            for (int i = 0; i < valuesLength; i++)
            {
                int code = Unsafe.Add(ref values, i);             // char -> int promotion
                bitmap[code >> 3] |= (byte)(1 << (code & 7));     // set bit
            }

            // Scan from the end and return the last char NOT present in the bitmap (bit == 0).
            nint index = (nint)searchSpaceLength;
            while (index > 0)
            {
                index -= 1;

                int code = Unsafe.Add(ref searchSpace, index);
                if ((bitmap[code >> 3] & (1 << (code & 7))) == 0)
                    goto Found;
            }

            return -1;
            Found:
            return (int)index;
        }


    }
}
