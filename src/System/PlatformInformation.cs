using System.IO;

namespace System
{
    /// <summary>
    /// This class provides simple properties for determining whether the current platform is Windows or Unix-based.
    /// </summary>
    public static class PlatformInformation
    {
        public static bool IsWindows => Path.DirectorySeparatorChar == '\\';

        public static bool IsUnix => Path.DirectorySeparatorChar == '/';
    }
}
