namespace Simple.EventStore.Models
{
	using Newtonsoft.Json;

	/// <summary>
    /// Event Message
    /// </summary>
    [JsonObject]
    public class EventMessage
    {
        /// <summary>
        /// Gets or sets the Metadata
        /// </summary>
        [JsonProperty("M")]
        public Metadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets the Message Payload
        /// </summary>
        [JsonProperty("P")]
        public string Payload { get; set; }
    }

    /// <summary>
    /// Event Message
    /// </summary>
    [JsonObject]
    internal class EventMessage<TMessageType>
    {
        /// <summary>
        /// Gets or sets the Metadata
        /// </summary>
        [JsonProperty("M")]
        public Metadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets the Message Payload
        /// </summary>
        [JsonProperty("P")]
        public TMessageType Payload { get; set; }
    }
}