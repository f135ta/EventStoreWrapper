namespace Simple.EventStore.Interfaces
{
	using System.Threading.Tasks;

	/// <summary>
    /// Event Publisher Interface
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Sends an Event to the EventStore
        /// </summary>
        /// <typeparam name="TMessageType">Message Type</typeparam>
        /// <param name="message">Message Data</param>
        /// <returns><see cref="Task"/></returns>
        Task SendAsync<TMessageType>(TMessageType message);
    }
}