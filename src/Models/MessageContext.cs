namespace Simple.EventStore.Models
{
	using Interfaces;

	/// <summary>
    /// Message Context for handling incoming messages
    /// </summary>
    public class MessageContext<TMessageType> : IMessageContext<TMessageType>
    {
        /// <summary>
        /// Gets or sets the Message Payload
        /// </summary>
        public TMessageType Payload { get; set; }

        /// <summary>
        /// Gets or sets the Message Metadata
        /// </summary>
        public Metadata Metadata { get; set; }
    }
}
