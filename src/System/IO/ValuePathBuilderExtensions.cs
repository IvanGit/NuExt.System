using System.Diagnostics;

namespace System.IO
{
    public static class ValuePathBuilderExtensions
    {
        /// <summary>
        /// Splits the specified path into its individual segments.
        /// </summary>
        /// <returns>A list where each item represents a segment of the path.</returns>
        public static List<string> GetPathSegments(this ref ValuePathBuilder builder)
        {
            var list = new List<string>();
            var count = builder.GetPathSegments(list);
            Debug.Assert(list.Count == count);
            return list;
        }
    }
}
