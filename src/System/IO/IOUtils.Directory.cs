using System.Diagnostics;

namespace System.IO
{
    partial class IOUtils
    {
        /// <summary>
        /// Checks if a directory exists, and creates it if it does not.
        /// Throws an exception if the directory cannot be created and throwError is true.
        /// </summary>
        /// <param name="path">The directory path to check or create.</param>
        /// <param name="throwError">Indicates whether to throw an exception if the directory cannot be created.</param>
        public static void CheckDirectory(string path, bool throwError)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(path);
#else
            ThrowHelper.WhenNullOrEmpty(path);
#endif
            if (Directory.Exists(path))
            {
                return;
            }
            try
            {
                Directory.CreateDirectory(path);
            }
            catch
            {
                if (throwError)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the directory name from a full path.
        /// </summary>
        /// <param name="fullPath">The full path to the directory.</param>
        /// <returns>The directory name if it exists; otherwise, returns the original path.</returns>
        public static string GetDirectoryName(string fullPath)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(fullPath);
#else
            ThrowHelper.WhenNullOrEmpty(fullPath);
#endif
            if (fullPath.Length <= 3)
            {
                return fullPath;
            }

            string trimmedPath = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return Path.GetFileName(trimmedPath);
        }

        /// <summary>
        /// Finds a free directory name by appending a numeric suffix if the directory already exists.
        /// </summary>
        /// <param name="path">The base path to check and modify if necessary.</param>
        /// <returns>A free directory name that does not currently exist on the file system.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided path is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when unable to find a free directory name after a maximum number of attempts.</exception>
        public static string GetFreeDirectoryName(string path)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(path);
#else
            ThrowHelper.WhenNullOrEmpty(path);
#endif
            const int maxAttempts = 65000;
            var originalPath = path;
            int i = 0;
            while (Directory.Exists(path) && i++ < maxAttempts)
            {
                path = $"{originalPath} ({i})";
            }
            if (i >= maxAttempts)
            {
                throw new InvalidOperationException($"Unable to find a free directory name after {maxAttempts} attempts.");
            }
            return path;
        }

        /// <summary>
        /// Safely deletes a directory if it exists.
        /// </summary>
        /// <param name="path">The path of the directory to delete.</param>
        /// <returns>True if the directory was successfully deleted or did not exist; false if an exception occurred.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided path is null or empty.</exception>
        public static bool SafeDeleteDirectory(string path)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(path);
#else
            ThrowHelper.WhenNullOrEmpty(path);
#endif
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
                return false;
            }
        }
    }
}
