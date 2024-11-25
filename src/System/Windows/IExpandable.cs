namespace System.Windows
{
    /// <summary>
    /// Defines a contract for objects that can be expanded and collapsed.
    /// </summary>
    public interface IExpandable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the object is expanded.
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Collapses the object by setting the <see cref="IsExpanded"/> property to <c>false</c>.
        /// </summary>

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        public void Collapse()
        {
            IsExpanded = false;
        }
#else
        public void Collapse();
#endif

        /// <summary>
        /// Expands the object by setting the <see cref="IsExpanded"/> property to <c>true</c>.
        /// </summary>

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        public void Expand()
        {
            IsExpanded = true;
        }
#else
        public void Expand();
#endif
    }
}
