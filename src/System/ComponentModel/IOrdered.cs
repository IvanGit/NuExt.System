namespace System.ComponentModel
{
    /// <summary>Represents an object with an order value.</summary>
    public interface IOrdered
    {
        /// <summary>Gets or sets an integer order value; lower means higher priority.</summary>
        int Order { get; set; }
    }
}
