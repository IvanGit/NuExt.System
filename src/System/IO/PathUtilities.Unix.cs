using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.IO
{
    partial class PathUtilities
    {
        private static class Unix
        {
            internal const char DirectorySeparatorChar = '/';
            private const char AltDirectorySeparatorChar = '/';
            internal const string DirectorySeparatorCharAsString = "/";

            public static char[] GetInvalidFileNameChars() => ['\0', '/'];

            public static char[] GetInvalidPathChars() => ['\0'];

            // Expands the given path to a fully qualified path.
            internal static ReadOnlySpan<char> GetFullPath(ReadOnlySpan<char> path)
            {
                if (path.Contains('\0'))
                    throw new ArgumentException(SR.Argument_NullCharInPath, nameof(path));

                return GetFullPathInternal(path);
            }

            internal static ReadOnlySpan<char> GetFullPath(ReadOnlySpan<char> path, ReadOnlySpan<char> basePath)
            {
                if (!IsPathFullyQualified(basePath))
                    throw new ArgumentException(SR.Arg_BasePathNotFullyQualified, nameof(basePath));

                if (basePath.Contains('\0') || path.Contains('\0'))
                    throw new ArgumentException(SR.Argument_NullCharInPath);

                if (IsPathFullyQualified(path))
                    return GetFullPathInternal(path);

                return GetFullPathInternal(CombineInternal(basePath, path, true));
            }

            // Gets the full path without argument validation
            private static ReadOnlySpan<char> GetFullPathInternal(ReadOnlySpan<char> path)
            {
                Debug.Assert(!path.IsEmpty);
                Debug.Assert(!path.Contains('\0'));

                // Expand with current directory if necessary
                /*if (!IsPathRooted(path))
                {
                    //path = Combine(Interop.Sys.GetCwd(), path);
                    var sb = new ValueStringBuilder(1 +  path.Length);
                    sb.Append(DirectorySeparatorChar);
                    sb.Append(path);
                    path = sb.ToString().AsSpan();
                }*/

                // We would ideally use realpath to do this, but it resolves symlinks and requires that the file actually exist.
                var collapsedString = RemoveRelativeSegments(path, GetRootLength(path), true);

                Debug.Assert(collapsedString.Length < path.Length || collapsedString.ToString() == path.ToString(),
                    "Either we've removed characters, or the string should be unmodified from the input path.");

                var result = collapsedString.Length == 0 ? DirectorySeparatorCharAsString.AsSpan() : collapsedString;

                return result;
            }

            internal static int GetRootLength(scoped ReadOnlySpan<char> path)
            {
                return path.Length > 0 && IsDirectorySeparator(path[0]) ? 1 : 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool IsDirectorySeparator(char c)
            {
                // The alternate directory separator char is the same as the directory separator,
                // so we only need to check one.
                Debug.Assert(DirectorySeparatorChar == AltDirectorySeparatorChar);
                return c == DirectorySeparatorChar;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool IsEffectivelyEmpty(scoped ReadOnlySpan<char> path)
            {
                return path.IsEmpty;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool IsPathFullyQualified(scoped ReadOnlySpan<char> path)
            {
                return !IsPartiallyQualified(path);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool IsPartiallyQualified(scoped ReadOnlySpan<char> path)
            {
                // This is much simpler than Windows where paths can be rooted, but not fully qualified (such as Drive Relative)
                // As long as the path is rooted in Unix it doesn't use the current directory and therefore is fully qualified.
                return !IsPathRooted(path);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool IsPathRooted(scoped ReadOnlySpan<char> path)
            {
                return path.StartsWith(DirectorySeparatorChar);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ReadOnlySpan<char> GetPathRoot(scoped ReadOnlySpan<char> path)
            {
                return IsPathRooted(path) ? DirectorySeparatorCharAsString.AsSpan() : [];
            }

            /// <summary>
            /// Normalize separators in the given path. Compresses forward slash runs.
            /// </summary>
            internal static bool NormalizeDirectorySeparators(scoped ReadOnlySpan<char> path, ref ValueStringBuilder builder)
            {
                if (path.IsEmpty)
                    return false;

                // Make a pass to see if we need to normalize so we can potentially skip allocating
                bool normalized = true;

                for (int i = 0; i < path.Length; i++)
                {
                    if (IsDirectorySeparator(path[i])
                        && (i + 1 < path.Length && IsDirectorySeparator(path[i + 1])))
                    {
                        normalized = false;
                        break;
                    }
                }

                if (normalized)
                    return false;

                builder = new ValueStringBuilder(path.Length);

                for (int i = 0; i < path.Length; i++)
                {
                    char current = path[i];

                    // Skip if we have another separator following
                    if (IsDirectorySeparator(current)
                        && (i + 1 < path.Length && IsDirectorySeparator(path[i + 1])))
                        continue;

                    builder.Append(current);
                }

                return true;
            }

            internal static bool PathEquals(scoped ReadOnlySpan<char> path1, scoped ReadOnlySpan<char> path2)
            {
                path1 = TrimTrailingSeparators(path1, true);
                path2 = TrimTrailingSeparators(path2, true);

                if (path1.Length != path2.Length)
                {
                    return false;
                }

                var length = path1.Length;
                for (int i = 0; i < length; i++)
                {
                    if (!PathCharEquals(path1[i], path2[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            private static bool PathCharEquals(char x, char y)
            {
                if (IsDirectorySeparator(x) && IsDirectorySeparator(y))
                {
                    return true;
                }

                return x == y;
            }
        }
    }
}
