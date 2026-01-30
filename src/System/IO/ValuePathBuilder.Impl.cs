using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.IO
{
    partial struct ValuePathBuilder
    {
        private char[]? _arrayToReturnToPool;
        private Span<char> _chars;
        private int _pos;
        private char? _directorySeparatorChar;
        private bool? _isUnixLikePlatform;

        public partial ValuePathBuilder(Span<char> initialBuffer)
        {
            _arrayToReturnToPool = null;
            _chars = initialBuffer;
            _pos = 0;
        }

        public partial ValuePathBuilder(int initialCapacity)
        {
            _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
            _chars = _arrayToReturnToPool;
            _pos = 0;
        }

        public partial ValuePathBuilder(scoped ReadOnlySpan<char> initialPath)
        {
            _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialPath.Length);
            _chars = _arrayToReturnToPool;
            initialPath.CopyTo(_chars);
            _pos = initialPath.Length;
        }

        #region Properties

        public readonly partial int Capacity => _chars.Length;

        public partial char DirectorySeparatorChar
        {
            readonly get => _directorySeparatorChar ?? Path.DirectorySeparatorChar;
            set 
            { 
                _directorySeparatorChar = value;
                _isUnixLikePlatform = PathUtilities.IsUnixLikePlatform(value);
            }
        }

        private readonly bool IsUnixLikePlatform => _isUnixLikePlatform ?? PathUtilities.IsUnixLikePlatform(DirectorySeparatorChar);

        public partial int Length
        {
            readonly get => _pos;
            set
            {
                Debug.Assert(value >= 0);
                Debug.Assert(value <= _chars.Length);
                _pos = value;
            }
        }

        public readonly partial ref char this[int index]
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
        //[OverloadResolutionPriority(-1)]
        public partial void Add(scoped ReadOnlySpan<char> s)
        {
            if (s.IsEmpty)
            {
                return;
            }

            int pos = _pos;
            if (pos > _chars.Length - s.Length)
            {
                Grow(s.Length);
            }
#if NET_OLD
            s.CopyTo(_chars.Slice(_pos));
#else
            s.CopyTo(_chars[_pos..]);
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

            s
#if !NET
                .AsSpan()
#endif
#if NET_OLD
                .CopyTo(_chars.Slice(pos));
#else
                .CopyTo(_chars[pos..]);
#endif
            _pos += s.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public partial void Clear()
        {
            _pos = 0;
            _chars.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public partial void Dispose()
        {
            char[]? toReturn = _arrayToReturnToPool;
            this = default; // Reset the struct
            if (toReturn != null)
            {
                ArrayPool<char>.Shared.Return(toReturn);
            }
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
        public readonly ref readonly char GetPinnableReference()
        {
            return ref MemoryMarshal.GetReference(_chars);
        }

        /// <summary>
        /// Get a pinnable reference to the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ref readonly char GetPinnableReference(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(_pos + 1);
                _chars[_pos] = '\0';
            }
            return ref MemoryMarshal.GetReference(_chars);
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

            // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative.
            // This could also go negative if the actual required length wraps around.
            char[] poolArray = ArrayPool<char>.Shared.Rent(newCapacity);
#if NET_OLD
            _chars.Slice(0, _pos).CopyTo(poolArray);
#else
            _chars[.._pos].CopyTo(poolArray);
#endif
            char[]? toReturn = _arrayToReturnToPool;
            _chars = _arrayToReturnToPool = poolArray;
            if (toReturn != null)
            {
                ArrayPool<char>.Shared.Return(toReturn);
            }
        }

        public override partial string ToString()
        {
#if NET_OLD
            string s = _chars.Slice(0, _pos).ToString();
#else
            string s = _chars[.._pos].ToString();
#endif
            Dispose();
            return s;
        }

        public partial ReadOnlySpan<char> AsSpan(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(_pos + 1);
                _chars[_pos] = '\0';
            }
#if NET_OLD
            return _chars.Slice(0, _pos);
#else
            return _chars[.._pos];
#endif
        }

        public readonly partial ReadOnlySpan<char> AsSpan()
        {
#if NET_OLD
            return _chars.Slice(0, _pos);
#else
            return _chars[.._pos];
#endif
        }

        public readonly partial ReadOnlySpan<char> AsSpan(int start)
        {
#if NET_OLD
            return _chars.Slice(start, _pos - start);
#else
            return _chars[start.._pos];
#endif
        }

        public readonly partial ReadOnlySpan<char> AsSpan(int start, int length) => _chars.Slice(start, length);

        public partial bool TryCopyTo(scoped Span<char> destination, out int charsWritten)
        {
            if (_chars
#if NET_OLD
                .Slice(0, _pos)
#else
                [.._pos]
#endif
                .TryCopyTo(destination))
            {
                charsWritten = _pos;
                Dispose();
                return true;
            }
            else
            {
                charsWritten = 0;
                Dispose();
                return false;
            }
        }

        #endregion

        #region Methods

        public static ValuePathBuilder operator /(ValuePathBuilder builder, scoped ReadOnlySpan<char> path)
        {
            builder.Append(path);
            return builder;
        }

        public static ValuePathBuilder operator /(ValuePathBuilder builder, string? path)
        {
            builder.Append(path);
            return builder;
        }

        public static ValuePathBuilder operator +(ValuePathBuilder builder, scoped ReadOnlySpan<char> s)
        {
            builder.Add(s);
            return builder;
        }

        public static ValuePathBuilder operator +(ValuePathBuilder builder, string? s)
        {
            builder.Add(s);
            return builder;
        }

        //[OverloadResolutionPriority(-1)]
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
                if (!PathUtilities.IsDirectorySeparator(c, IsUnixLikePlatform) && !PathUtilities.IsDirectorySeparator(path[0], IsUnixLikePlatform))
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
                if (!PathUtilities.IsDirectorySeparator(c, IsUnixLikePlatform) && !PathUtilities.IsDirectorySeparator(path![0], IsUnixLikePlatform))
                {
                    Add(DirectorySeparatorChar);
                }
            }
            Add(path);
        }

        public partial bool ChangeExtension(string? extension)
        {
            int subLength = _pos;
            if (subLength == 0)
            {
                return false;
            }

            for (int i = _pos - 1; i >= 0; i--)
            {
                char c = _chars[i];
                if (c == '.')
                {
                    subLength = i;
                    break;
                }
                if (PathUtilities.IsDirectorySeparator(c, IsUnixLikePlatform))
                {
                    break;
                }
            }

            int pos = _pos;
            _pos = subLength;

            if (extension == null)
            {
                return subLength < pos;
            }

            if (!extension.StartsWith('.'))
            {
                Add('.');
            }
            Add(extension);
            return true;
        }

        //[OverloadResolutionPriority(-1)]
        public partial bool ChangeFileName(scoped ReadOnlySpan<char> newFileName)
        {
            var fileNameLength = GetFileName().Length;

            _pos -= fileNameLength;

            if (newFileName.IsEmpty)
            {
                return fileNameLength > 0;
            }

            Append(newFileName);
            return fileNameLength > 0 || newFileName.Length > 0;
        }

        public partial bool ChangeFileName(string? newFileName)
        {
            var fileNameLength = GetFileName().Length;

            _pos -= fileNameLength;

            if (newFileName == null)
            {
                return fileNameLength > 0;
            }

            Append(newFileName);
            return fileNameLength > 0 || newFileName.Length > 0;
        }

        public readonly partial bool EndsInDirectorySeparator()
        {
            return _pos > 0 && PathUtilities.IsDirectorySeparator(_chars[_pos - 1], IsUnixLikePlatform);
        }

        public partial bool EnsureTrailingSeparator()
        {
            if (EndsInDirectorySeparator())
            {
                return false;
            }
            Add(DirectorySeparatorChar);
            return true;
        }

        public readonly partial ReadOnlySpan<char> GetDirectoryName()
        {
            return PathUtilities.GetDirectoryName(AsSpan(), IsUnixLikePlatform);
        }

        public readonly partial ReadOnlySpan<char> GetExtension()
        {
            return PathUtilities.GetExtension(AsSpan(), IsUnixLikePlatform);
        }

        public readonly partial ReadOnlySpan<char> GetFileName()
        {
            return PathUtilities.GetFileName(AsSpan(), IsUnixLikePlatform);
        }

        public readonly partial ReadOnlySpan<char> GetFileNameWithoutExtension()
        {
            return PathUtilities.GetFileNameWithoutExtension(AsSpan(), IsUnixLikePlatform);
        }

        public readonly partial ReadOnlySpan<char> GetFullPath()
        {
            return PathUtilities.GetFullPath(AsSpan(), IsUnixLikePlatform);
        }

        public readonly partial ReadOnlySpan<char> GetFullPath(ReadOnlySpan<char> basePath)
        {
            return PathUtilities.GetFullPath(AsSpan(), basePath, IsUnixLikePlatform);
        }

        public readonly partial ReadOnlySpan<char> GetPathRoot()
        {
            return PathUtilities.GetPathRoot(AsSpan(), IsUnixLikePlatform);
        }

        public readonly partial int GetPathSegments(ICollection<string>? segments)
        {
            return PathUtilities.GetPathSegments(AsSpan(), segments, IsUnixLikePlatform);
        }

        public readonly partial ReadOnlySpan<char> GetRelativePath(ReadOnlySpan<char> path, ReadOnlySpan<char> basePath)
        {
            return PathUtilities.GetRelativePath(AsSpan(), path, IsUnixLikePlatform, basePath);
        }

        public readonly partial bool HasExtension()
        {
            return PathUtilities.HasExtension(AsSpan(), IsUnixLikePlatform);
        }

        public readonly partial bool IsEffectivelyEmpty()
        {
            return PathUtilities.IsEffectivelyEmpty(AsSpan(), IsUnixLikePlatform);
        }

        public readonly partial bool IsPathFullyQualified()
        {
            return PathUtilities.IsPathFullyQualified(AsSpan(), IsUnixLikePlatform);
        }

        public readonly partial bool IsPathRooted()
        {
            return PathUtilities.IsPathRooted(AsSpan(), IsUnixLikePlatform);
        }

        public readonly partial bool IsRoot()
        {
            return _pos == PathUtilities.GetRootLength(AsSpan(), IsUnixLikePlatform);
        }

        public partial bool NormalizeDirectorySeparators()
        {
            if (_pos == 0)
            {
                return false;
            }
            ValueStringBuilder builder = default;
            var result = PathUtilities.NormalizeDirectorySeparators(AsSpan(), ref builder, IsUnixLikePlatform);
            if (result)
            {
                Debug.Assert(builder.Length > 0);
                _pos = 0;
                Add(builder.AsSpan());
            }
            builder.Dispose();
            return result;
        }

        public readonly partial bool PathEquals(scoped ReadOnlySpan<char> path)
        {
            return PathUtilities.PathEquals(AsSpan(), path, IsUnixLikePlatform);
        }

        public readonly partial bool StartsWithDirectorySeparator()
        {
            return _pos > 0 && PathUtilities.IsDirectorySeparator(_chars[0], IsUnixLikePlatform);
        }

        public partial bool TrimEndingDirectorySeparator()
        {
            if (EndsInDirectorySeparator() && !IsRoot())
            {
                _pos -= 1;
                return true;
            }
            return false;
        }

        #endregion
    }
}
