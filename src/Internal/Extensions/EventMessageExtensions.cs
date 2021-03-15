namespace Simple.EventStore.Internal.Extensions
{
	using System.Text;
	using global::EventStore.ClientAPI;
	using Models;
	using Newtonsoft.Json;

	internal static class EventMessageExtensions
    {
        /// <summary>
        /// Converts a JSON String into a EventMessage
        /// </summary>
        /// <param name="message">JSON String</param>
        /// <returns><see cref="EventMessage"/></returns>
        public static  EventMessage ConvertToEventMessage(this string message) => JsonConvert.DeserializeObject<EventMessage>(message);

        /// <summary>
        /// Converts a JSON String into a EventMessage
        /// </summary>
        /// <param name="message">JSON String</param>
        /// <returns><see cref="EventMessage"/></returns>
        public static EventMessage ConvertToEventMessage(this RecordedEvent message)
        {
            return new EventMessage
            {
                Metadata = JsonConvert.DeserializeObject<Metadata>(Encoding.UTF8.GetString(message.Metadata)),
                Payload = Encoding.UTF8.GetString(message.Data)
            };
        }

        /// <summary>
        /// Converts a JSON String into a EventMessage{T}
        /// </summary>
        /// <param name="message">JSON String</param>
        /// <returns><see cref="EventMessage"/></returns>
        public static EventMessage<TMessageType> ConvertToEventMessage<TMessageType>(this RecordedEvent message)
        {
            return new EventMessage<TMessageType>
            {
                Metadata = JsonConvert.DeserializeObject<Metadata>(Encoding.UTF8.GetString(message.Metadata)),
                Payload = JsonConvert.DeserializeObject<TMessageType>(Encoding.UTF8.GetString(message.Data))
            };
        }

        /// <summary>
        /// Converts a <see cref="EventMessage"/> to a <see cref="EventMessage{TMessageType}"/>
        /// </summary>
        /// <typeparam name="TMessageType">Message Payload Type</typeparam>
        /// <param name="message">Web Socket Message</param>
        /// <returns><see cref="EventMessage{TMessageType}"/></returns>
        public static EventMessage<TMessageType> ConvertTo<TMessageType>(this EventMessage message)
        {
            var newMessage = new EventMessage<TMessageType>
            {
                Metadata = message.Metadata,
                Payload = JsonConvert.DeserializeObject<TMessageType>(message.Payload)
            };

            return newMessage;
        }

        /// <summary>
        /// Converts a <see cref="EventMessage"/> to a JSON String
        /// </summary>
        /// <param name="message">Web Socket Message</param>
        /// <returns>JSON String</returns>
        public static string ToJson(this EventMessage message)
        {
            return JsonConvert.SerializeObject(message, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
        }

        /// <summary>
        /// Converts a <see cref="EventMessage{TMessageType}"/> to a JSON String
        /// </summary>
        /// <param name="message">Web Socket Message</param>
        /// <returns>JSON String</returns>
        public static string ToJson<TMessageType>(this EventMessage<TMessageType> message)
        {
            return JsonConvert.SerializeObject(message, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}
