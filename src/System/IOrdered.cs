namespace System
{
    /// <summary>
    /// Interface representing an object with a defined order.
    /// Objects implementing this interface can be ordered based on their order value.
    /// </summary>
    public interface IOrdered
    {
        /// <summary>
        /// Gets or sets an integer value that represents the order of the object.
        /// The lower the value, the higher the object's priority in an ordered collection.
        /// </summary>
        int Order { get; set; }
    }
}
