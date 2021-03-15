namespace Simple.EventStore.Interfaces
{
	using System.Threading.Tasks;

	/// <summary>
    /// Message Consumer Interface
    /// </summary>
    public interface IMessageConsumer<TMessageType>
    {
        Task ProcessMessageAsync(IMessageContext<TMessageType> context);
    }
}