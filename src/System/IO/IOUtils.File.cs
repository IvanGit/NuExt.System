using System.Diagnostics;
using System.Text;

namespace System.IO
{
    partial class IOUtils
    {
        /// <summary>
        /// Clears a file name by replacing invalid characters and optionally trimming its length.
        /// </summary>
        /// <param name="fileName">The original file name.</param>
        /// <param name="charToReplace">The character to replace invalid characters with (default is '_').</param>
        /// <param name="limitSize">The maximum allowed length of the file name including its extension (default is 0, meaning no limit).</param>
        public static void ClearFileName(ref string? fileName, char charToReplace = '_', int limitSize = 0)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var sb = new ValueStringBuilder(fileName!.Length);
            foreach (var c in fileName)
            {
                sb.Append(s_invalidFileNameChars.Contains(c) ? charToReplace : c);
            }
            fileName = sb.ToString();

            if (limitSize > 0 && fileName.Length > limitSize)
            {
                fileName = SmartTrimFileName(fileName, limitSize);
            }
        }

        /// <summary>
        /// Clears a file name by replacing invalid characters and trimming its length.
        /// </summary>
        /// <param name="fileName">The original file name.</param>
        /// <param name="limitSize">The maximum allowed length of the file name including its extension (default is 0, meaning no limit).</param>
        /// <returns>The cleared and possibly trimmed file name.</returns>
        public static string? ClearFileName(string? fileName, int limitSize = 0)
        {
            ClearFileName(ref fileName, limitSize: limitSize);
            return fileName;
        }

        /// <summary>
        /// Gets the last write time (UTC) of a file at the specified path.
        /// </summary>
        /// <param name="fullPath">The full path of the file.</param>
        /// <returns>The UTC date and time the file was last written to.</returns>
        /// <exception cref="IOException">Thrown when an IO error occurs while accessing the file.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided path is null or empty.</exception>
        public static DateTime GetFileTimeStamp(string fullPath)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(fullPath);
#else
            Throw.IfNullOrEmpty(fullPath);
#endif
            try
            {
                return File.GetLastWriteTimeUtc(fullPath);
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }

        /// <summary>
        /// Gets the last write time (UTC) of a file at the specified path.
        /// </summary>
        /// <param name="fullPath">The full path of the file.</param>
        /// <returns>The UTC date and time the file was last written to.</returns>
        /// <exception cref="IOException">Thrown when an IO error occurs while accessing the file.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided path is null or empty.</exception>
        public static long GetFileLength(string fullPath)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(fullPath);
#else
            Throw.IfNullOrEmpty(fullPath);
#endif
            try
            {
                var info = new FileInfo(fullPath);
                return info.Length;
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }

        /// <summary>
        /// Finds a free file name by appending a numeric suffix to the base file name if necessary.
        /// </summary>
        /// <param name="path">The original file path.</param>
        /// <returns>A free file path that does not currently exist.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided path is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when unable to find a free file name after a maximum number of attempts.</exception>
        public static string GetFreeFileName(string path)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(path);
#else
            Throw.IfNullOrEmpty(path);
#endif
            if (!File.Exists(path))
            {
                return path;
            }

            var template = Path.GetFileNameWithoutExtension(path);
            var ext = Path.GetExtension(path);
            var dir = Path.GetDirectoryName(path) ?? throw new ArgumentException("Invalid path", nameof(path));

            const int maxAttempts = 65000;
            int i = 1;
            string newPath;
            do
            {
                newPath = Path.Combine(dir, $"{template} ({i}){ext}");
            } while (File.Exists(newPath) && ++i < maxAttempts);
            if (i >= maxAttempts)
            {
                throw new InvalidOperationException($"Unable to find a free file name after {maxAttempts} attempts.");
            }
            return newPath;
        }

        /// <summary>
        /// Trims a file name intelligently to fit within the specified limit size, preserving word boundaries where possible.
        /// </summary>
        /// <param name="fileName">The original file name.</param>
        /// <param name="limitSize">The maximum allowed length of the file name including its extension.</param>
        /// <returns>The trimmed file name.</returns>
        public static string SmartTrimFileName(string fileName, int limitSize)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(fileName);
#else
            Throw.IfNull(fileName);
#endif
            if (string.IsNullOrEmpty(fileName) || limitSize <= 0)
            {
                return fileName;
            }

            var extension = Path.GetExtension(fileName);
#if NETFRAMEWORK
            var nameWithoutExtension = fileName.Substring(0, fileName.Length - extension.Length);
#else
            var nameWithoutExtension = fileName[..^extension.Length];
#endif

            if (nameWithoutExtension.Length + extension.Length > limitSize)
            {
                int count = limitSize - extension.Length;
                var index = nameWithoutExtension.LastIndexOfAny(s_smartTrimFileNameChars, count);
#if NETFRAMEWORK
                nameWithoutExtension = index > 0 ? nameWithoutExtension.Substring(0, index) : nameWithoutExtension.Substring(0, count);
#else
                nameWithoutExtension = index > 0 ? nameWithoutExtension[..index] : nameWithoutExtension[..count];
#endif
            }

            return nameWithoutExtension + extension;
        }

        /// <summary>
        /// Safely deletes a file if it exists.
        /// </summary>
        /// <param name="filePath">The path of the file to delete.</param>
        /// <returns>True if the file was successfully deleted or did not exist, false if an error occurred.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided path is null or empty.</exception>
        public static bool SafeDeleteFile(string filePath)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(filePath);
#else
            Throw.IfNullOrEmpty(filePath);
#endif
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
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
