namespace System
{
    /// <summary>
    /// The <see cref="ICloneable{T}"/> interface extends the standard <see cref="ICloneable"/>
    /// interface by providing a strongly-typed method for cloning objects.
    /// </summary>
    /// <typeparam name="T">The type of object to be cloned.</typeparam>
    public interface ICloneable<out T> : ICloneable
    {
        /// <summary>
        /// Creates and returns a new instance that is a copy of the current instance.
        /// </summary>
        /// <returns>A new instance of type <typeparamref name="T"/> that is a copy of this instance.</returns>
        new T Clone();
    }
}
