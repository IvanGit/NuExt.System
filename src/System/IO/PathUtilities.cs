using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO
{
    /// <summary>
    /// The <c>PathUtilities</c> class provides utility methods for performing common path operations.
    /// </summary>
    /// <remarks>
    /// This class contains static methods that can be used without instantiating the class.
    /// It offers utility functions that are commonly needed when working with file systems.
    ///
    /// Notice: includes code derived from the Roslyn .NET compiler project, licensed under the MIT License.
    /// See LICENSE file in the project root for full license information.
    /// Original source code can be found at https://github.com/dotnet/roslyn.
    /// </remarks>
    internal static class PathUtilities
    {
        // We consider '/' a directory separator on Unix like systems. 
        // On Windows both / and \ are equally accepted.
        internal static char DirectorySeparatorChar => Path.DirectorySeparatorChar;
        internal const char AltDirectorySeparatorChar = '/';
        internal const char VolumeSeparatorChar = ':';

        /// <summary>
        /// True if the character is the platform directory separator character or the alternate directory separator.
        /// </summary>
        public static bool IsDirectorySeparator(char c) => c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;
    }
}
