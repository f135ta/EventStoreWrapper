namespace Simple.EventStore
{
	using System.Threading.Tasks;
	using Interfaces;

    /// <summary>
    /// Event Publisher
    /// </summary>
	public class EventPublisher : IEventPublisher
    {
        private readonly IEventStoreClient Client;

        /// <summary>
        /// Initialises a new instance of the <see cref="EventPublisher"/> class
        /// </summary>
        /// <param name="client">Event Store Client</param>
        public EventPublisher(IEventStoreClient client)
        {
            this.Client = client;
        }

        /// <summary>
        /// Sends an Event to the EventStore
        /// </summary>
        /// <typeparam name="TMessageType">Message Type</typeparam>
        /// <param name="message">Event Message Object</param>
        /// <returns><see cref="Task"/></returns>
        public async Task SendAsync<TMessageType>(TMessageType message) => await this.Client.SendAsync(message);
    }
}
