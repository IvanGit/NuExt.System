using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.IO
{
    partial class PathUtilities
    {
        private static class Windows
        {
            internal const char DirectorySeparatorChar = '\\';
            internal const char AltDirectorySeparatorChar = '/';
            private const char VolumeSeparatorChar = ':';

            internal const string DirectorySeparatorCharAsString = "\\";

            private const string UncExtendedPathPrefix = @"\\?\UNC\";

            private const int MaxShortPath = 260;
            // \\?\, \\.\, \??\
            private const int DevicePrefixLength = 4;
            // \\
            private const int UncPrefixLength = 2;
            // \\?\UNC\, \\.\UNC\
            private const int UncExtendedPrefixLength = 8;

            // Expands the given path to a fully qualified path.
            internal static ReadOnlySpan<char> GetFullPath(ReadOnlySpan<char> path)
            {
                // If the path would normalize to string empty, we'll consider it empty
                if (IsEffectivelyEmpty(path))
                    throw new ArgumentException("The path is empty.", nameof(path));

                // Embedded null characters are the only invalid character case we truly care about.
                // This is because the nulls will signal the end of the string to Win32 and therefore have
                // unpredictable results.
                if (path.Contains('\0'))
                    throw new ArgumentException("Null character in path.", nameof(path));

                return GetFullPathInternal(path);
            }

            internal static ReadOnlySpan<char> GetFullPath(ReadOnlySpan<char> path, ReadOnlySpan<char> basePath)
            {
                if (!IsPathFullyQualified(basePath))
                {
                    throw new ArgumentException("Basepath argument is not fully qualified.", nameof(basePath));
                }

                if (basePath.Contains('\0') || path.Contains('\0'))
                    throw new ArgumentException("Null character in path.");

                if (IsPathFullyQualified(path))
                    return GetFullPathInternal(path);

                if (IsEffectivelyEmpty(path))
                {
                    return basePath;
                }

                int length = path.Length;
                ReadOnlySpan<char> combinedPath;
                if (length >= 1 && IsDirectorySeparator(path[0]))
                {
                    // Path is current drive rooted i.e. starts with \:
                    // "\Foo" and "C:\Bar" => "C:\Foo"
                    // "\Foo" and "\\?\C:\Bar" => "\\?\C:\Foo"
                    combinedPath = Join(GetPathRoot(basePath), path.Slice(1), false); // Cut the separator to ensure we don't end up with two separators when joining with the root.
                }
                else if (length >= 2 && IsValidDriveChar(path[0]) && path[1] == VolumeSeparatorChar)
                {
                    // Drive relative paths
                    Debug.Assert(length == 2 || !IsDirectorySeparator(path[2]));

                    if (GetVolumeName(path).EqualsOrdinalIgnoreCase(GetVolumeName(basePath)))
                    {
                        // Matching root
                        // "C:Foo" and "C:\Bar" => "C:\Bar\Foo"
                        // "C:Foo" and "\\?\C:\Bar" => "\\?\C:\Bar\Foo"
                        combinedPath = Join(basePath, path.Slice(2), false);
                    }
                    else
                    {
                        // No matching root, root to specified drive
                        // "D:Foo" and "C:\Bar" => "D:Foo"
                        // "D:Foo" and "\\?\C:\Bar" => "\\?\D:\Foo"
                        combinedPath = !IsDevice(basePath)
                            ? path.ToString().Insert(2, @"\").AsSpan()
                            : length == 2
                                ? JoinInternal(basePath.Slice(0, 4), path, @"\".AsSpan(), false)
                                : JoinInternal(basePath.Slice(0, 4), path.Slice(0, 2), @"\".AsSpan(), path.Slice(2), false);
                    }
                }
                else
                {
                    // "Simple" relative path
                    // "Foo" and "C:\Bar" => "C:\Bar\Foo"
                    // "Foo" and "\\?\C:\Bar" => "\\?\C:\Bar\Foo"
                    combinedPath = JoinInternal(basePath, path, false);
                }

                // Device paths are normalized by definition, so passing something of this format (i.e. \\?\C:\.\tmp, \\.\C:\foo)
                // to Windows APIs won't do anything by design. Additionally, GetFullPathName() in Windows doesn't root
                // them properly. As such we need to manually remove segments and not use GetFullPath().

                return IsDevice(combinedPath)
                    ? RemoveRelativeSegments(combinedPath, GetRootLength(combinedPath), false)
                    : GetFullPathInternal(combinedPath);
            }

            // Gets the full path without argument validation
            private static ReadOnlySpan<char> GetFullPathInternal(ReadOnlySpan<char> path)
            {
                Debug.Assert(!path.IsEmpty);
                Debug.Assert(!path.Contains('\0'));

                if (IsExtended(path))
                {
                    // \\?\ paths are considered normalized by definition. Windows doesn't normalize \\?\
                    // paths and neither should we. Even if we wanted to GetFullPathName does not work
                    // properly with device paths. If one wants to pass a \\?\ path through normalization
                    // one can chop off the prefix, pass it to GetFullPath and add it again.
                    return path;
                }

                //return Normalize(path);
                return RemoveRelativeSegments(path, GetRootLength(path), false);
            }


            /// <summary>
            /// Returns true if the given character is a valid drive letter
            /// </summary>
            private static bool IsValidDriveChar(char value)
            {
                return (uint)((value | 0x20) - 'a') <= (uint)('z' - 'a');
            }

            /// <summary>
            /// Returns true if the path uses any of the DOS device path syntaxes. ("\\.\", "\\?\", or "\??\")
            /// </summary>
            private static bool IsDevice(scoped ReadOnlySpan<char> path)
            {
                // If the path begins with any two separators is will be recognized and normalized and prepped with
                // "\??\" for internal usage correctly. "\??\" is recognized and handled, "/??/" is not.
                return IsExtended(path)
                       ||
                       (
                           path.Length >= DevicePrefixLength
                           && IsDirectorySeparator(path[0])
                           && IsDirectorySeparator(path[1])
                           && (path[2] == '.' || path[2] == '?')
                           && IsDirectorySeparator(path[3])
                       );
            }

            /// <summary>
            /// Returns true if the path is a device UNC (\\?\UNC\, \\.\UNC\)
            /// </summary>
            private static bool IsDeviceUNC(scoped ReadOnlySpan<char> path)
            {
                return path.Length >= UncExtendedPrefixLength
                       && IsDevice(path)
                       && IsDirectorySeparator(path[7])
                       && path[4] == 'U'
                       && path[5] == 'N'
                       && path[6] == 'C';
            }

            /// <summary>
            /// Returns true if the path uses the canonical form of extended syntax ("\\?\" or "\??\"). If the
            /// path matches exactly (cannot use alternate directory separators) Windows will skip normalization
            /// and path length checks.
            /// </summary>
            private static bool IsExtended(scoped ReadOnlySpan<char> path)
            {
                // While paths like "//?/C:/" will work, they're treated the same as "\\.\" paths.
                // Skipping of normalization will *only* occur if backslashes ('\') are used.
                return path.Length >= DevicePrefixLength
                       && path[0] == '\\'
                       && (path[1] == '\\' || path[1] == '?')
                       && path[2] == '?'
                       && path[3] == '\\';
            }

            /// <summary>
            /// Gets the length of the root of the path (drive, share, etc.).
            /// </summary>
            internal static int GetRootLength(scoped ReadOnlySpan<char> path)
            {
                int pathLength = path.Length;
                int i = 0;

                bool deviceSyntax = IsDevice(path);
                bool deviceUnc = deviceSyntax && IsDeviceUNC(path);

                if ((!deviceSyntax || deviceUnc) && pathLength > 0 && IsDirectorySeparator(path[0]))
                {
                    // UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
                    if (deviceUnc || (pathLength > 1 && IsDirectorySeparator(path[1])))
                    {
                        // UNC (\\?\UNC\ or \\), scan past server\share

                        // Start past the prefix ("\\" or "\\?\UNC\")
                        i = deviceUnc ? UncExtendedPrefixLength : UncPrefixLength;

                        // Skip two separators at most
                        int n = 2;
                        while (i < pathLength && (!IsDirectorySeparator(path[i]) || --n > 0))
                            i++;
                    }
                    else
                    {
                        // Current drive rooted (e.g. "\foo")
                        i = 1;
                    }
                }
                else if (deviceSyntax)
                {
                    // Device path (e.g. "\\?\.", "\\.\")
                    // Skip any characters following the prefix that aren't a separator
                    i = DevicePrefixLength;
                    while (i < pathLength && !IsDirectorySeparator(path[i]))
                        i++;

                    // If there is another separator take it, as long as we have had at least one
                    // non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
                    if (i < pathLength && i > DevicePrefixLength && IsDirectorySeparator(path[i]))
                        i++;
                }
                else if (pathLength >= 2
                    && path[1] == VolumeSeparatorChar
                    && IsValidDriveChar(path[0]))
                {
                    // Valid drive specified path ("C:", "D:", etc.)
                    i = 2;

                    // If the colon is followed by a directory separator, move past it (e.g "C:\")
                    if (pathLength > 2 && IsDirectorySeparator(path[2]))
                        i++;
                }

                return i;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool IsPathFullyQualified(scoped ReadOnlySpan<char> path)
            {
                return !IsPartiallyQualified(path);
            }

            /// <summary>
            /// Returns true if the path specified is relative to the current drive or working directory.
            /// Returns false if the path is fixed to a specific drive or UNC path.  This method does no
            /// validation of the path (URIs will be returned as relative as a result).
            /// </summary>
            /// <remarks>
            /// Handles paths that use the alternate directory separator.  It is a frequent mistake to
            /// assume that rooted paths (Path.IsPathRooted) are not relative.  This isn't the case.
            /// "C:a" is drive relative-meaning that it will be resolved against the current directory
            /// for C: (rooted, but relative). "C:\a" is rooted and not relative (the current directory
            /// will not be used to modify the path).
            /// </remarks>
            private static bool IsPartiallyQualified(scoped ReadOnlySpan<char> path)
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
            /// True if the given character is a directory separator.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool IsDirectorySeparator(char c)
            {
                return c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;
            }

            /// <summary>
            /// Returns true if the path is effectively empty for the current OS.
            /// For unix, this is empty or null. For Windows, this is empty, null, or
            /// just spaces ((char)32).
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool IsEffectivelyEmpty(scoped ReadOnlySpan<char> path)
            {
                if (path.IsEmpty)
                    return true;

                foreach (char c in path)
                {
                    if (c != ' ')
                        return false;
                }
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsPathRooted(ReadOnlySpan<char> path)
            {
                int length = path.Length;
                return (length >= 1 && IsDirectorySeparator(path[0]))
                       || (length >= 2 && IsValidDriveChar(path[0]) && path[1] == VolumeSeparatorChar);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
            {
                if (IsEffectivelyEmpty(path))
                    return [];

                int pathRoot = GetRootLength(path);
                return pathRoot <= 0 ? [] : path.Slice(0, pathRoot);
            }

            /// <summary>
            /// Returns the volume name for dos, UNC and device paths.
            /// </summary>
            private static ReadOnlySpan<char> GetVolumeName(ReadOnlySpan<char> path)
            {
                // 3 cases: UNC ("\\server\share"), Device ("\\?\C:\"), or Dos ("C:\")
                var root = GetPathRoot(path);
                if (root.Length == 0)
                    return root;

                // Cut from "\\?\UNC\Server\Share" to "Server\Share"
                // Cut from  "\\Server\Share" to "Server\Share"
                int startOffset = GetUncRootLength(path);
                if (startOffset == -1)
                {
                    if (IsDevice(path))
                    {
                        startOffset = 4; // Cut from "\\?\C:\" to "C:"
                    }
                    else
                    {
                        startOffset = 0; // e.g. "C:"
                    }
                }

                var pathToTrim = root.Slice(startOffset);
                return EndsInDirectorySeparator(pathToTrim, true) ? pathToTrim.Slice(0, pathToTrim.Length - 1) : pathToTrim;
            }

            /// <summary>
            /// Returns offset as -1 if the path is not in Unc format, otherwise returns the root length.
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            private static int GetUncRootLength(scoped ReadOnlySpan<char> path)
            {
                bool isDevice = IsDevice(path);

                if (!isDevice && path.Slice(0, 2).EqualsOrdinal(@"\\".AsSpan()))
                    return 2;
                else if (isDevice && path.Length >= 8
                                  && (path.Slice(0, 8).EqualsOrdinalIgnoreCase(UncExtendedPathPrefix.AsSpan())
                                      || path.Slice(5, 4).EqualsOrdinalIgnoreCase(@"UNC\".AsSpan())))
                    return 8;

                return -1;
            }

            /// <summary>
            /// Normalize separators in the given path. Converts forward slashes into back slashes and compresses slash runs, keeping initial 2 if present.
            /// Also trims initial whitespace in front of "rooted" paths (see PathStartSkip).
            ///
            /// This effectively replicates the behavior of the legacy NormalizePath when it was called with fullCheck=false and expandShortpaths=false.
            /// The current NormalizePath gets directory separator normalization from Win32's GetFullPathName(), which will resolve relative paths and as
            /// such can't be used here (and is overkill for our uses).
            ///
            /// Like the current NormalizePath this will not try and analyze periods/spaces within directory segments.
            /// </summary>
            /// <remarks>
            /// The only callers that used to use Path.Normalize(fullCheck=false) were Path.GetDirectoryName() and Path.GetPathRoot(). Both usages do
            /// not need trimming of trailing whitespace here.
            ///
            /// GetPathRoot() could technically skip normalizing separators after the second segment- consider as a future optimization.
            ///
            /// For legacy .NET Framework behavior with ExpandShortPaths:
            ///  - It has no impact on GetPathRoot() so doesn't need consideration.
            ///  - It could impact GetDirectoryName(), but only if the path isn't relative (C:\ or \\Server\Share).
            ///
            /// In the case of GetDirectoryName() the ExpandShortPaths behavior was undocumented and provided inconsistent results if the path was
            /// fixed/relative. For example: "C:\PROGRA~1\A.TXT" would return "C:\Program Files" while ".\PROGRA~1\A.TXT" would return ".\PROGRA~1". If you
            /// ultimately call GetFullPath() this doesn't matter, but if you don't or have any intermediate string handling could easily be tripped up by
            /// this undocumented behavior.
            ///
            /// We won't match this old behavior because:
            ///
            ///   1. It was undocumented
            ///   2. It was costly (extremely so if it actually contained '~')
            ///   3. Doesn't play nice with string logic
            ///   4. Isn't a cross-plat friendly concept/behavior
            /// </remarks>
            internal static bool NormalizeDirectorySeparators(scoped ReadOnlySpan<char> path, ref ValueStringBuilder builder)
            {
                if (path.IsEmpty)
                    return false;

                char current;

                // Make a pass to see if we need to normalize so we can potentially skip allocating
                bool normalized = true;

                for (int i = 0; i < path.Length; i++)
                {
                    current = path[i];
                    if (IsDirectorySeparator(current)
                        && (current != DirectorySeparatorChar
                            // Check for sequential separators past the first position (we need to keep initial two for UNC/extended)
                            || (i > 0 && i + 1 < path.Length && IsDirectorySeparator(path[i + 1]))))
                    {
                        normalized = false;
                        break;
                    }
                }

                if (normalized)
                    return false;

                builder = new ValueStringBuilder(MaxShortPath);

                int start = 0;
                if (IsDirectorySeparator(path[start]))
                {
                    start++;
                    builder.Append(DirectorySeparatorChar);
                }

                for (int i = start; i < path.Length; i++)
                {
                    current = path[i];

                    // If we have a separator
                    if (IsDirectorySeparator(current))
                    {
                        // If the next is a separator, skip adding this
                        if (i + 1 < path.Length && IsDirectorySeparator(path[i + 1]))
                        {
                            continue;
                        }

                        // Ensure it is the primary separator
                        current = DirectorySeparatorChar;
                    }

                    builder.Append(current);
                }

                return true;
            }

            internal static bool PathEquals(scoped ReadOnlySpan<char> path1, scoped ReadOnlySpan<char> path2)
            {
                path1 = TrimTrailingSeparators(path1, false);
                path2 = TrimTrailingSeparators(path2, false);

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

                return char.ToUpperInvariant(x) == char.ToUpperInvariant(y);
            }
        }
    }
}
