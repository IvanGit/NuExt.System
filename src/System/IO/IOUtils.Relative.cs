namespace System.IO
{
    partial class IOUtils
    {
        private static int ConsumeDirectorySeparators(string path, int length, int i)
        {
            while (i < length && IsDirectorySeparator(path[i]))
            {
                i++;
            }

            return i;
        }

        /// <summary>
        /// Gets a path relative to a directory.
        /// </summary>
        public static string GetRelativePath(string directory, string fullPath)
        {
            string relativePath = string.Empty;

            directory = TrimTrailingSeparators(directory);
            fullPath = TrimTrailingSeparators(fullPath);

            if (IsChildPath(directory, fullPath))
            {
                return GetRelativeChildPath(directory, fullPath);
            }

            var directoryPathParts = GetPathParts(directory);
            var fullPathParts = GetPathParts(fullPath);

            if (directoryPathParts.Length == 0 || fullPathParts.Length == 0)
            {
                return fullPath;
            }

            int index = 0;

            // find index where full path diverges from base path
            var maxSearchIndex = Math.Min(directoryPathParts.Length, fullPathParts.Length);
            for (; index < maxSearchIndex; index++)
            {
                if (!PathEquals(directoryPathParts[index], fullPathParts[index]))
                {
                    break;
                }
            }

            // if the first part doesn't match, they don't even have the same volume
            // so there can be no relative path.
            if (index == 0)
            {
                return fullPath;
            }

            // add backup notation for remaining base path levels beyond the index
            var remainingParts = directoryPathParts.Length - index;
            if (remainingParts > 0)
            {
                for (int i = 0; i < remainingParts; i++)
                {
                    relativePath = relativePath + ParentRelativeDirectory + Path.DirectorySeparatorChar;
                }
            }

            // add the rest of the full path parts
            for (int i = index; i < fullPathParts.Length; i++)
            {
                relativePath = CombinePathsUnchecked(relativePath, fullPathParts[i]);
            }

            relativePath = TrimTrailingSeparators(relativePath);

            return relativePath;
        }

        /// <summary>
        /// True if the child path is a child of the parent path.
        /// </summary>
        public static bool IsChildPath(string parentPath, string childPath)
        {
            return parentPath.Length > 0
                   && childPath.Length > parentPath.Length
                   && PathEquals(childPath, parentPath, parentPath.Length)
#if NET462
                   && (IsDirectorySeparator(parentPath[parentPath.Length - 1])
#else
                   && (IsDirectorySeparator(parentPath[^1])
#endif
                   || IsDirectorySeparator(childPath[parentPath.Length]));
        }

        private static string GetRelativeChildPath(string parentPath, string childPath)
        {
#if NET462
            var relativePath = childPath.Substring(parentPath.Length);
#else
            var relativePath = childPath[parentPath.Length..];
#endif

            // trim any leading separators left over after removing leading directory
            int start = ConsumeDirectorySeparators(relativePath, relativePath.Length, 0);
            if (start > 0)
            {
#if NET462
                relativePath = relativePath.Substring(start);
#else
                relativePath = relativePath[start..];
#endif
            }

            return relativePath;
        }

        private static readonly char[] s_pathChars = { VolumeSeparatorChar, Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        private static string[] GetPathParts(string path)
        {
            var pathParts = path.Split(s_pathChars);

            // remove references to self directories ('.')
            if (pathParts.Contains(ThisDirectory))
            {
                pathParts = pathParts.Where(s => s != ThisDirectory).ToArray();
            }

            return pathParts;
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fromPath"/> or <paramref name="toPath"/> is <c>null</c>.</exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string GetUriRelativePath(string fromPath, string toPath)
        {
#if NET7_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(fromPath);
            ArgumentException.ThrowIfNullOrEmpty(toPath);
#else
            ThrowHelper.WhenNullOrEmpty(fromPath);
            ThrowHelper.WhenNullOrEmpty(toPath);
#endif

            var fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
            var toUri = new Uri(AppendDirectorySeparatorChar(toPath));

            if (fromUri.Scheme != toUri.Scheme)
            {
                return toPath;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!Path.HasExtension(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }
    }
}
