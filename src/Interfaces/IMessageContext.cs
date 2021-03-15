namespace Simple.EventStore.Interfaces
{
	using Models;

	/// <summary>
    /// Standard Message Interface
    /// </summary>
    public interface IMessageContext<TMessageType>
    {
        /// <summary>
        /// Gets or sets the Message Payload
        /// </summary>
        TMessageType Payload { get; set; }

        /// <summary>
        /// Gets or sets the Message Metadata
        /// </summary>
        Metadata Metadata { get; set; }
    }
}
