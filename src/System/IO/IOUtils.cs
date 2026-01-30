using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace System.IO
{
    /// <summary>
    /// The <c>IOUtils</c> class provides utility methods for performing common input/output operations.
    /// </summary>
    /// <remarks>
    /// This class contains static methods that can be used without instantiating the class.
    /// It offers utility functions that are commonly needed when working with file systems.
    ///
    /// Notice: based on original code derived from the Roslyn .NET compiler project, licensed under the MIT License.
    /// See LICENSE file in the project root for full license information.
    /// Original source code can be found at https://github.com/dotnet/roslyn.
    /// </remarks>
    public static partial class IOUtils
    {
        public static char DirectorySeparatorChar => Path.DirectorySeparatorChar;
        public const char AltDirectorySeparatorChar = '/';
        public const string ParentRelativeDirectory = "..";
        public const string ThisDirectory = ".";

        /// <summary>
        /// Returns true if the platform uses case-sensitive file system.
        /// </summary>
        public static bool IsCaseSensitiveFileSystem => !(PlatformInformation.IsWindows || PlatformInformation.IsMacOS);

        /// <summary>
        /// Ensures that a path has the specified extension. If the path does not end with the specified extension,
        /// the method adds the specified extension.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <param name="defaultExtension">The extension to ensure (including the dot, e.g., ".dll").</param>
        /// <returns>The updated path with the specified extension.</returns>
        public static string EnsurePathHasExtension(string path, string defaultExtension)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            Throw.ArgumentExceptionIf(string.IsNullOrWhiteSpace(defaultExtension) || !defaultExtension.StartsWith('.'),
                "Extension must start with a dot and cannot be empty", nameof(defaultExtension));

            var comparison = IsCaseSensitiveFileSystem ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            // Check if the path already ends with the default extension
            if (!path.EndsWith(defaultExtension, comparison))
            {
                // Add the default extension if it doesn't end with it
                path += defaultExtension;
            }

            return path;
        }

        /// <summary>
        /// True if the given character is a directory separator.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDirectorySeparator(char c)
        {
            return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
        }

        /// <summary>
        /// True if the path is normalized.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNormalized(string path)
        {
            try
            {
                return Path.GetFullPath(path) == path;
            }
            catch (Exception ex) when (ex is ArgumentException or SecurityException or NotSupportedException)
            {
                return false;
            }
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
            try
            {
                var normalized1 = TrimTrailingSeparators(Path.GetFullPath(path1));
                var normalized2 = TrimTrailingSeparators(Path.GetFullPath(path2));
                return PathEquals(normalized1, normalized2);
            }
            catch (Exception ex) when (ex is ArgumentException or SecurityException or NotSupportedException)
            {
                return false;
            }
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
        public static string GetNormalizedPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            string normalized = Path.GetFullPath(path);
            if (PlatformInformation.IsWindows)
            {
                normalized = normalized.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
            return TrimTrailingSeparators(normalized);
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

            if (path1.Length != path2.Length)
            {
                return false;
            }

            for (int i = 0; i < path1.Length; i++)
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

            if (!IsCaseSensitiveFileSystem)
            {
                return char.ToUpperInvariant(x) == char.ToUpperInvariant(y);
            }
            else
            {
                return x == y;
            }
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
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            if (IsRootPath(s))
            {
                return s;
            }

            int lastSeparator = s.Length;
            while (lastSeparator > 0 && IsDirectorySeparator(s[lastSeparator - 1]))
            {
                lastSeparator --;
            }

            if (lastSeparator != s.Length)
            {
#if NET || NETSTANDARD2_1_OR_GREATER
                s = s[..lastSeparator];
#else
                s = s.Substring(0, lastSeparator);
#endif
            }

            return s;
        }

        private static bool IsRootPath(string path)
        {
            if (!PlatformInformation.IsWindows && path == "/")
            {
                return true;
            }

            if (PlatformInformation.IsWindows && path.Length >= 2 && path[1] == ':')
            {
                // "C:" or "C:\" or "C:/"
                if (path.Length == 2)
                    return true;

                if (path.Length == 3 && IsDirectorySeparator(path[2]))
                    return true;
            }

            // Windows UNC path: \\server\share
            if (PlatformInformation.IsWindows && path.StartsWith(@"\\", StringComparison.Ordinal))
            {
                // Count path segments after the initial "\\"
                int segmentCount = 0;
                int i = 2; // Skip initial "\\"

                while (i < path.Length)
                {
                    // Skip consecutive separators
                    while (i < path.Length && IsDirectorySeparator(path[i]))
                        i++;

                    // If we have a non-separator character, it's a segment
                    int segmentStart = i;
                    while (i < path.Length && !IsDirectorySeparator(path[i]))
                        i++;

                    if (i > segmentStart)
                        segmentCount++;

                    // If we already have more than 2 segments, it's not a root
                    if (segmentCount > 2)
                        return false;
                }

                // Root UNC has exactly 2 segments: server and share
                return segmentCount == 2;
            }

            return false;
        }

        /// <summary>
        /// Gets a path relative to a directory.
        /// </summary>
        public static string GetRelativePath(string directory, string fullPath)
        {
            ArgumentNullException.ThrowIfNull(directory);
            ArgumentNullException.ThrowIfNull(fullPath);

            var builder = new PathBuilder(directory) { DirectorySeparatorChar = Path.DirectorySeparatorChar };
            return builder.GetRelativePath(fullPath, Environment.CurrentDirectory).ToString();
        }
    }
}
