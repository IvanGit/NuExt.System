using System.Runtime.CompilerServices;

namespace System.IO
{
    /// <summary>
    /// The <c>IOUtils</c> class provides utility methods for performing common input/output operations.
    /// </summary>
    /// <remarks>
    /// This class contains static methods that can be used without instantiating the class.
    /// It offers utility functions that are commonly needed when working with file systems.
    ///
    /// Notice: includes code derived from the Roslyn .NET compiler project, licensed under the MIT License.
    /// See LICENSE file in the project root for full license information.
    /// Original source code can be found at https://github.com/dotnet/roslyn.
    /// </remarks>
    public static partial class IOUtils
    {
        public const char VolumeSeparatorChar = ':';
        public const string ParentRelativeDirectory = "..";
        public const string ThisDirectory = ".";

        private static readonly char[] s_invalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly char[] s_smartTrimFileNameChars = [' ', '\t', '_', '-'];

        public static string CombinePathsUnchecked(string root, string? relativePath)
        {
#if NET7_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(root);
#else
            Throw.IfNullOrEmpty(root);
#endif
#if NETFRAMEWORK
            char c = root[root.Length - 1];
#else
            char c = root[^1];
#endif
            if (!IsDirectorySeparator(c) && c != VolumeSeparatorChar)
            {
                return root + Path.DirectorySeparatorChar + relativePath;
            }

            return root + relativePath;
        }

        /// <summary>
        /// Ensures that a path has the specified extension. If the path does not end with the specified extension,
        /// the method adds the specified extension.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <param name="defaultExtension">The extension to ensure (including the dot, e.g., ".dll").</param>
        /// <returns>The updated path with the specified extension.</returns>
        public static string EnsurePathHasExtension(string path, string defaultExtension)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(path);
#else
            Throw.IfNullOrEmpty(path);
#endif
            Throw.ArgumentExceptionIf(string.IsNullOrWhiteSpace(defaultExtension) ||
#if NETFRAMEWORK
                                                 !defaultExtension.StartsWith("."),
#else
                                                 !defaultExtension.StartsWith('.'),
#endif
                "Extension must start with a dot and cannot be empty", nameof(defaultExtension));

            // Check if the path already ends with the default extension
            if (!path.EndsWith(defaultExtension, StringComparison.OrdinalIgnoreCase))
            {
                // Add the default extension if it doesn't end with it
                path += defaultExtension;
            }

            return path;
        }

        /// <summary>
        /// True if the path is an absolute path (rooted to drive or network share)
        /// </summary>
        public static bool IsAbsolute(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            // "C:\"
            if (IsDriveRootedAbsolutePath(path))
            {
                // Including invalid paths (e.g. "*:\")
                return true;
            }

            // "\\machine\share"
            // Including invalid/incomplete UNC paths (e.g. "\\goo")
            return path.Length >= 2 &&
                   IsDirectorySeparator(path[0]) &&
                   IsDirectorySeparator(path[1]);
        }

        /// <summary>
        /// True if the character is any recognized directory separator character.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyDirectorySeparator(char c) => c is '\\' or '/';

        /// <summary>
        /// True if the given character is a directory separator.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDirectorySeparator(char c)
        {
            return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
        }

        /// <summary>
        /// Returns true if given path is absolute and starts with a drive specification ("C:\").
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDriveRootedAbsolutePath(string path)
        {
            return path.Length >= 3 && path[1] == VolumeSeparatorChar && IsDirectorySeparator(path[2]);
        }

        /// <summary>
        /// True if the path is a normalized.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNormalized(string path)
        {
            return Path.GetFullPath(path) == path;
        }

        //https://referencesource.microsoft.com/#mscorlib/system/io/pathinternal.cs,465
        /// <summary>
        /// Returns true if the path specified is relative to the current drive or working directory.
        /// Returns false if the path is fixed to a specific drive or UNC path.  This method does no
        /// validation of the path (URIs will be returned as relative as a result).
        /// </summary>
        /// <remarks>
        /// Handles paths that use the alternate directory separator.  It is a frequent mistake to
        /// assume that rooted paths (Path.IsPathRooted) are not relative.  This isn't the case.
        /// "C:a" is drive relative- meaning that it will be resolved against the current directory
        /// for C: (rooted, but relative). "C:\a" is rooted and not relative (the current directory
        /// will not be used to modify the path).
        /// </remarks>
        public static bool IsPartiallyQualified(string path)
        {
            if (path.Length < 2)
            {
                // It isn't fixed, it must be relative.  There is no way to specify a fixed
                // path with one character (or less).
                return true;
            }

            if (IsDirectorySeparator(path[0]))
            {
                // There is no valid way to specify a relative path with two initial slashes or
                // \? as ? isn't valid for drive relative paths and \??\ is equivalent to \\?\
                return !(path[1] == '?' || IsDirectorySeparator(path[1]));
            }

            // The only way to specify a fixed path that doesn't begin with two slashes
            // is the drive, colon, slash format- i.e. C:\
            return !((path.Length >= 3)
                && (path[1] == VolumeSeparatorChar)
                && IsDirectorySeparator(path[2])
                // To match old behavior we'll check the drive character for validity as the path is technically
                // not qualified if you don't have a valid drive. "=:\" is the "=" file's default data stream.
                && IsValidDriveChar(path[0]));
        }

        /// <summary>
        /// Returns true if the given character is a valid drive letter
        /// </summary>
        /// <summary>
        /// Returns true if the given character is a valid drive letter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidDriveChar(char value)
        {
            return value is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
        }

        /// <summary>
        /// Compares two file or directory paths for equality after normalizing them.
        /// </summary>
        /// <param name="path1">The first path to compare. This can be null.</param>
        /// <param name="path2">The second path to compare. This can be null.</param>
        /// <returns>
        /// true if both paths are considered equal after normalization; otherwise, false. 
        /// If either path is null, the method returns false.
        /// </returns>
        public static bool NormalizedPathEquals(string? path1, string? path2)
        {
            if (path1 == null || path2 == null)
            {
                return false;
            }
            return PathEquals(TrimTrailingSeparators(Path.GetFullPath(path1)), TrimTrailingSeparators(Path.GetFullPath(path2)));
        }

        /// <summary>
        /// Normalizes the specified path by converting it to its absolute form and removing any trailing directory separators.
        /// </summary>
        /// <param name="path">The path to normalize. This cannot be null or empty.</param>
        /// <returns>The normalized absolute path without trailing directory separators.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided path is null or empty.</exception>
        /// <remarks>
        /// This method uses <see cref="Path.GetFullPath(string)"/> to convert the given path to an absolute path
        /// and removes any trailing directory separators.
        /// </remarks>
        public static string PathNormalize(string path)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(path);
#else
            Throw.IfNullOrEmpty(path);
#endif
            var fullPath = Path.GetFullPath(path);
            return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        /// <summary>
        /// True if the two paths are the same (compare chars). Use <see cref="NormalizedPathEquals"/> to compare paths with normalization.
        /// </summary>
        /// <returns>
        /// If either path is null, the method returns false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool PathEquals(string? path1, string? path2)
        {
            if (path1 == null || path2 == null)
            {
                return false;
            }
            return PathEquals(path1, path2, Math.Max(path1.Length, path2.Length));
        }

        /// <summary>
        /// True if the two paths are the same.  (but only up to the specified length)
        /// </summary>
        private static bool PathEquals(string path1, string path2, int length)
        {
            if (path1.Length < length || path2.Length < length)
            {
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                if (!PathCharEqual(path1[i], path2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool PathCharEqual(char x, char y)
        {
            if (IsDirectorySeparator(x) && IsDirectorySeparator(y))
            {
                return true;
            }

            return char.ToUpperInvariant(x) == char.ToUpperInvariant(y);
        }

        /// <summary>
        /// Removes trailing directory separator characters
        /// </summary>
        /// <remarks>
        /// This will trim the root directory separator:
        /// "C:\" maps to "C:", and "/" maps to ""
        /// </remarks>
        public static string TrimTrailingSeparators(string s)
        {
            int lastSeparator = s.Length;
            while (lastSeparator > 0 && IsDirectorySeparator(s[lastSeparator - 1]))
            {
                lastSeparator --;
            }

            if (lastSeparator != s.Length)
            {
#if NETFRAMEWORK
                s = s.Substring(0, lastSeparator);
#else
                s = s[..lastSeparator];
#endif
            }

            return s;
        }
    }
}
