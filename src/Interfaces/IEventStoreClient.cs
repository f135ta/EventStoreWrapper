namespace Simple.EventStore.Interfaces
{
	using System;
	using System.Threading.Tasks;
	using global::EventStore.ClientAPI;
	using Models;

	public interface IEventStoreClient
    {
        event EventHandler<EventMessage> OnMessageReceived;
        event EventHandler<ClientConnectionEventArgs> OnConnected;
        event EventHandler<ClientConnectionEventArgs> OnDisconnected;

        /// <summary>
        /// Connects to the EventStore
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task ConnectAsync();

        /// <summary>
        /// Sends an Event to the EventStore
        /// </summary>
        /// <typeparam name="TMessageType">Message Type</typeparam>
        /// <param name="message">Message Data</param>
        /// <returns><see cref="Task"/></returns>
        Task SendAsync<TMessageType>(TMessageType message);

        /// <summary>
        /// Subscribes to an Event
        /// </summary>
        /// <param name="eventName">Event Name</param>
        Task SubscribeToAsync(string eventName);

        /// <summary>
        /// Disconnects the Client
        /// </summary>
        void Disconnect();
    }
}