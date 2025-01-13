using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.IO
{
    partial class PathBuilder
    {
        private char[] _chars;
        private int _pos;
        private char? _directorySeparatorChar;

        private static readonly char[] s_emptyArray = [];

        #region Properties

        public partial int Capacity => _chars.Length;

        public partial char DirectorySeparatorChar
        {
            get => _directorySeparatorChar ?? PathUtilities.DirectorySeparatorChar;
            set => _directorySeparatorChar = value;
        }

        public partial int Length => _pos;

        public partial ref char this[int index]
        {
            get
            {
                Debug.Assert(index < _pos);
                return ref _chars[index];
            }
        }

        #endregion

        #region Utility Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public partial void Add(char c)
        {
            int pos = _pos;
            var chars = _chars;
            if ((uint)pos < (uint)chars.Length)
            {
                chars[pos] = c;
                _pos = pos + 1;
            }
            else
            {
                AddWithResize(c);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AddWithResize(char c)
        {
            int pos = _pos;
            Grow(1);
            _chars[pos] = c;
            _pos = pos + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public partial void Add(scoped ReadOnlySpan<char> s)
        {
            int pos = _pos;
            if (pos > _chars.Length - s.Length)
            {
                Grow(s.Length);
            }
#if NET_OLD
            s.CopyTo(_chars.AsSpan().Slice(_pos));
#else
            s.CopyTo(_chars.AsSpan()[_pos..]);
#endif
            _pos += s.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public partial void Add(string? s)
        {
            if (s == null)
            {
                return;
            }

            int pos = _pos;
            if (s.Length == 1 && (uint)pos < (uint)_chars.Length) // common case
            {
                _chars[pos] = s[0];
                _pos = pos + 1;
            }
            else
            {
                AddSlow(s);
            }
        }

        private void AddSlow(string s)
        {
            int pos = _pos;
            if (pos > _chars.Length - s.Length)
            {
                Grow(s.Length);
            }

            s.CopyTo(0, _chars, _pos, s.Length);
            _pos += s.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public partial void Clear()
        {
            _pos = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public partial void Dispose()
        {
            Clear();
            _chars = s_emptyArray;
        }

        public partial void EnsureCapacity(int capacity)
        {
            // This is not expected to be called this with negative capacity
            Debug.Assert(capacity >= 0);

            // If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
            if ((uint)capacity > (uint)_chars.Length)
            {
                Grow(capacity - _pos);
            }
        }

        /// <summary>
        /// Get a pinnable reference to the builder.
        /// Does not ensure there is a null char after <see cref="Length"/>
        /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
        /// the explicit method call, and write eg "fixed (char* c = builder)"
        /// </summary>
        public ref readonly char GetPinnableReference()
        {
            return ref MemoryMarshal.GetReference(_chars.AsSpan());
        }

        /// <summary>
        /// Get a pinnable reference to the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ref readonly char GetPinnableReference(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
            return ref MemoryMarshal.GetReference(_chars.AsSpan());
        }

        /// <summary>
        /// Resize the internal buffer either by doubling current buffer size or
        /// by adding <paramref name="additionalCapacityBeyondPos"/> to
        /// <see cref="_pos"/> whichever is greater.
        /// </summary>
        /// <param name="additionalCapacityBeyondPos">
        /// Number of chars requested beyond current position.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int additionalCapacityBeyondPos)
        {
            Debug.Assert(additionalCapacityBeyondPos > 0);
            Debug.Assert(_pos > _chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

            const uint arrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

            // Increase to at least the required size (_pos + additionalCapacityBeyondPos), but try
            // to double the size if possible, bounding the doubling to not go beyond the max array length.
            int newCapacity = (int)Math.Max(
                (uint)(_pos + additionalCapacityBeyondPos),
                Math.Min((uint)_chars.Length * 2, arrayMaxLength));

            var destinationArray = new char[newCapacity];
            _chars.CopyTo(destinationArray, 0);
            _chars = destinationArray;
        }

        public override partial string ToString()
        {
            return _pos == 0 ? string.Empty : new string(_chars, 0, _pos);
        }

        public partial ReadOnlySpan<char> AsSpan(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
#if NET_OLD
            return _chars.AsSpan().Slice(0, _pos);
#else
            return _chars.AsSpan()[.._pos];
#endif
        }

        public partial ReadOnlySpan<char> AsSpan()
        {
#if NET_OLD
            return _chars.AsSpan().Slice(0, _pos);
#else
            return _chars.AsSpan()[.._pos];
#endif
        }

        public partial ReadOnlySpan<char> AsSpan(int start)
        {
#if NET_OLD
            return _chars.AsSpan().Slice(start, _pos - start);
#else
            return _chars.AsSpan()[start.._pos];
#endif
        }

        public partial ReadOnlySpan<char> AsSpan(int start, int length)
        {
            return _chars.AsSpan().Slice(start, length);
        }

        public partial bool TryCopyTo(scoped Span<char> destination, out int charsWritten)
        {
            if (_chars.AsSpan()
#if NET_OLD
                .Slice(0, _pos)
#else
                [.._pos]
#endif
                .TryCopyTo(destination))
            {
                charsWritten = _pos;
                return true;
            }
            else
            {
                charsWritten = 0;
                return false;
            }
        }

#endregion

        #region Methods

        public static PathBuilder operator /(PathBuilder builder, scoped ReadOnlySpan<char> path)
        {
            builder.Append(path);
            return builder;
        }

        public static PathBuilder operator /(PathBuilder builder, string? path)
        {
            builder.Append(path);
            return builder;
        }

        public static PathBuilder operator +(PathBuilder builder, scoped ReadOnlySpan<char> s)
        {
            builder.Add(s);
            return builder;
        }

        public static PathBuilder operator +(PathBuilder builder, string? s)
        {
            builder.Add(s);
            return builder;
        }

        public partial void Append(scoped ReadOnlySpan<char> path)
        {
            if (path.IsEmpty)
            {
                return;
            }
            int pos = _pos;
            if (pos > 0)
            {
                var c = _chars[pos - 1];
                if (!PathUtilities.IsDirectorySeparator(c) && !PathUtilities.IsDirectorySeparator(path[0]))
                {
                    Add(DirectorySeparatorChar);
                }
            }
            Add(path);
        }

        public partial void Append(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            if (_pos > 0)
            {
                var c = _chars[_pos - 1];
                if (!PathUtilities.IsDirectorySeparator(c) && !PathUtilities.IsDirectorySeparator(path![0]))
                {
                    Add(DirectorySeparatorChar);
                }
            }
            Add(path);
        }

        public partial bool HasExtension()
        {
            for (int i = Length - 1; i >= 0; i--)
            {
                char c = _chars[i];
                if (c == '.')
                {
                    return i != Length - 1;
                }
                if (PathUtilities.IsDirectorySeparator(c))
                {
                    break;
                }
            }
            return false;
        }

        public partial ReadOnlySpan<char> GetExtension()
        {
            for (int i = Length - 1; i >= 0; i--)
            {
                char c = _chars[i];
                if (c == '.')
                {
                    return i != Length - 1 ?
                        _chars.AsSpan()
#if NET_OLD
                        .Slice(i, Length - i)
#else
                        [i..Length]
#endif
                        : [];
                }
                if (PathUtilities.IsDirectorySeparator(c))
                {
                    break;
                }
            }
            return [];
        }

#endregion
    }
}
