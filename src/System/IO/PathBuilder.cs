namespace System.IO
{
    /// <summary>
    /// Provides a mutable builder for constructing paths.
    /// </summary>
    public partial class PathBuilder : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathBuilder"/> class.
        /// </summary>
        public PathBuilder()
        {
            _chars = s_emptyArray;
            _pos = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathBuilder"/> class with a specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">The initial number of characters the builder can hold.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="initialCapacity"/> is less than 0.</exception>
        public PathBuilder(int initialCapacity)
        {
            Throw.ArgumentOutOfRangeExceptionIf(initialCapacity < 0, nameof(initialCapacity), "Non-negative number required.");
            _chars = initialCapacity == 0 ? s_emptyArray : new char[initialCapacity];
            _pos = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathBuilder"/> class with an initial path.
        /// </summary>
        /// <param name="initialPath">The initial path to start the builder with.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="initialPath"/> is null.</exception>
        public PathBuilder(IEnumerable<char> initialPath)
        {
#if NET
            ArgumentNullException.ThrowIfNull(initialPath);
#else
            Throw.IfNull(initialPath);
#endif
            if (initialPath is ICollection<char> chars)
            {
                int count = chars.Count;
                if (count == 0)
                {
                    _chars = s_emptyArray;
                    _pos = 0;
                }
                else
                {
                    _chars = new char[count];
                    chars.CopyTo(_chars, 0);
                    _pos = count;
                }
            }
            else
            {
                _chars = s_emptyArray;
                _pos = 0;
                foreach (char c in initialPath)
                {
                    Add(c);
                }
            }
        }

        /// <summary>
        /// Gets the capacity of the builder.
        /// </summary>
        public partial int Capacity { get; }

        /// <summary>
        /// Gets or sets the directory separator character.
        /// </summary>
        public partial char DirectorySeparatorChar { get; set; }

        /// <summary>
        /// Gets the length of the path.
        /// </summary>
        public partial int Length { get; }

        /// <summary>
        /// Gets a reference to the character at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the character to get.</param>
        /// <returns>A reference to the character at the specified index.</returns>
        public partial ref char this[int index] { get; }

        /// <summary>
        /// Appends a character to the path without introducing a directory separator.
        /// </summary>
        /// <param name="c">The character to append.</param>
        public partial void Add(char c);

        /// <summary>
        /// Adds a string segment to the path without adding a directory separator.
        /// </summary>
        /// <param name="s">The string segment to append.</param>
        public partial void Add(scoped ReadOnlySpan<char> s);

        /// <summary>
        /// Appends a string to the path without introducing a directory separator.
        /// </summary>
        /// <param name="s">The string element to append.</param>
        public partial void Add(string? s);

        /// <summary>
        /// Appends elements to the path with a directory separator specified by <see cref="DirectorySeparatorChar"/>.
        /// </summary>
        /// <param name="path">The path element to append.</param>
        public partial void Append(scoped ReadOnlySpan<char> path);

        /// <summary>
        /// Appends elements to the path with a directory separator specified by <see cref="DirectorySeparatorChar"/>.
        /// </summary>
        /// <param name="path">The path element to append.</param>
        public partial void Append(string? path);

        /// <summary>
        /// Returns a read-only span around the contents of the builder.
        /// </summary>
        /// <returns>A read-only span that represents the current contents of the builder.</returns>
        public partial ReadOnlySpan<char> AsSpan();

        /// <summary>
        /// Returns a read-only span around the contents of the builder, optionally ensuring a null character after <see cref="Length"/>.
        /// </summary>
        /// <param name="terminate">True to ensure a null character after <see cref="Length"/>; otherwise, false.</param>
        /// <returns>A read-only span that represents the current contents of the builder.</returns>
        public partial ReadOnlySpan<char> AsSpan(bool terminate);

        /// <summary>
        /// Returns a read-only span around the contents of the builder starting at a specified position.
        /// </summary>
        /// <param name="start">The zero-based index from which to begin the span.</param>
        /// <returns>A read-only span that represents the current contents of the builder starting at <paramref name="start"/>.</returns>
        public partial ReadOnlySpan<char> AsSpan(int start);

        /// <summary>
        /// Returns a read-only span around the contents of the builder with a specified starting position and length.
        /// </summary>
        /// <param name="start">The zero-based index from which to begin the span.</param>
        /// <param name="length">The number of characters to include in the span.</param>
        /// <returns>A read-only span that represents the current contents of the builder starting at <paramref name="start"/> and including <paramref name="length"/> characters.</returns>
        public partial ReadOnlySpan<char> AsSpan(int start, int length);

        /// <summary>
        /// Clears the path.
        /// </summary>
        public partial void Clear();

        /// <summary>
        /// Releases any resources used by the <see cref="PathBuilder"/> instance.
        /// </summary>
        public partial void Dispose();

        /// <summary>
        /// Ensures that the capacity of this builder is at least the specified value.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        public partial void EnsureCapacity(int capacity);

        /// <summary>
        /// Tests if the path's file name includes a file extension. A trailing period is not considered an extension.
        /// </summary>
        /// <returns>True if the path has an extension; otherwise, false.</returns>
        public partial bool HasExtension();

        /// <summary>
        /// Returns the extension of the given path.
        /// </summary>
        /// <remarks>The returned value is an empty <see cref="ReadOnlySpan{T}"/> if the given path does not include an extension.</remarks>
        /// <returns>A read-only span that represents the extension of the path.</returns>
        public partial ReadOnlySpan<char> GetExtension();

        /// <summary>
        /// Returns the path.
        /// </summary>
        /// <returns>A string that represents the path.</returns>
        public override partial string ToString();

        /// <summary>
        /// Tries to copy the contents of the builder to a destination span.
        /// </summary>
        /// <param name="destination">The destination span to copy to.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters written to the destination span.</param>
        /// <returns>True if the copy operation was successful; otherwise, false.</returns>
        public partial bool TryCopyTo(scoped Span<char> destination, out int charsWritten);
    }
}
