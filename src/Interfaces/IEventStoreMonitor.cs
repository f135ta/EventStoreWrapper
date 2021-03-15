namespace Simple.EventStore.Interfaces
{
	using System.Threading.Tasks;

	/// <summary>
    /// Event Store Monitor
    /// </summary>
    /// <remarks>
    /// Supports getting and setting the last Received Event Id from Custom Storage
    /// </remarks>
    public interface IEventStoreMonitor
    {
        /// <summary>
        /// Gets the Last Stored Event Id
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task<long> GetLastEventIdAsync(string subscriptionName);

        /// <summary>
        /// Saves the Last Stored Event Id
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task SaveLastEventIdAsync(string subscriptionName, long eventId);
    }
}