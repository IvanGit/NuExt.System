namespace System.Windows
{
    /// <summary>
    /// Defines methods and properties for drag-and-drop functionality.
    /// </summary>
    public interface IDragDrop
    {
        /// <summary>
        /// Gets a value indicating whether the object can be dragged.
        /// </summary>
        bool CanDrag { get; }

        /// <summary>
        /// Determines whether a specified object can be dropped onto this object.
        /// </summary>
        /// <param name="draggedObject">The object being dragged.</param>
        /// <returns>true if the object can be dropped; otherwise, false.</returns>
        bool CanDrop(IDragDrop draggedObject);

        /// <summary>
        /// Executes the drop action for a specified object.
        /// </summary>
        /// <param name="draggedObject">The object being dragged.</param>
        /// <returns>true if the drop operation was successful; otherwise, false.</returns>
        bool Drop(IDragDrop draggedObject);
    }
}
