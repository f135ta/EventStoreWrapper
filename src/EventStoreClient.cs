namespace Simple.EventStore
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Attributes;
	using Configuration;
	using global::EventStore.ClientAPI;
	using global::EventStore.ClientAPI.SystemData;
	using Interfaces;
	using Internal.Extensions;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.Options;
	using Models;
	using Newtonsoft.Json;

	public class EventStoreClient : IEventStoreClient
    {
        private readonly EventStoreConfiguration Configuration;
        private readonly List<Subscription> Subscriptions = new List<Subscription>();

        private readonly ILogger<EventStoreClient> Logger;
        private readonly IEventStoreConnection Connection;
        private readonly IServiceScopeFactory ServiceScopeFactory;

        private JsonSerializerSettings JsonSerializerSettings => new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        public event EventHandler<EventMessage> OnMessageReceived;
        public event EventHandler<ClientConnectionEventArgs> OnConnected;
        public event EventHandler<ClientConnectionEventArgs> OnDisconnected;

        public EventStoreClient(ILoggerFactory loggerFactory, IOptions<EventStoreConfiguration> configuration, IServiceScopeFactory serviceScopeFactory)
        {
            this.Configuration = configuration.Value;
            this.Logger = loggerFactory.CreateLogger<EventStoreClient>();
            this.ServiceScopeFactory = serviceScopeFactory;

            var settingsBuilder = ConnectionSettings
                .Create()
                .KeepReconnecting()
                //.DisableServerCertificateValidation()
                .FailOnNoServerResponse()
                .SetHeartbeatInterval(TimeSpan.FromSeconds(5))
                .SetHeartbeatTimeout(TimeSpan.FromSeconds(10))
                .SetDefaultUserCredentials(new UserCredentials(this.Configuration.UserName, this.Configuration.Password));

            this.Connection = EventStoreConnection.Create(settingsBuilder, new Uri(this.Configuration.TcpHostAddress), this.Configuration?.ClientName);

            this.Connection.Connected += this.Connection_Connected;

            this.Connection.Disconnected += this.Connection_Disconnected;
        }

        private void Connection_Disconnected(object sender, ClientConnectionEventArgs e) => this.OnDisconnected?.Invoke(sender, e);
        private void Connection_Connected(object sender, ClientConnectionEventArgs e) => this.OnConnected?.Invoke(sender, e);

        /// <inheritdoc/>>
        public async Task ConnectAsync()
        {
            if (this.Connection != null)
            {
                try
                {
                    await this.Connection.ConnectAsync();
                }
                catch
                {
                    this.Logger.LogWarning("Already connected");
                }
            }
        }

        /// <inheritdoc/>>
        public async Task SendAsync<TMessageType>(TMessageType message)
        {
            if (message == null)
            {
                return;
            }

            this.Logger.LogDebug("[SND] => Extracting Event Name");

            var eventName = this.GetEventName<TMessageType>();

            this.Logger.LogDebug($"[SND] => Constructing Message For: => {eventName}");

            var metadataMessage = new Metadata
            {
                Sender = this.Configuration.ClientName,
                EventName = this.GetEventName<TMessageType>(),
                SentDate = DateTimeOffset.Now
            };

            this.Logger.LogDebug($"[SND] => {metadataMessage.EventName}");

            var dataJson = JsonConvert.SerializeObject(message, this.JsonSerializerSettings);
            var metadataJson = JsonConvert.SerializeObject(metadataMessage, this.JsonSerializerSettings);

            var databytes = Encoding.UTF8.GetBytes(dataJson);
            var metadatabytes = Encoding.UTF8.GetBytes(metadataJson);

            await this.Connection.AppendToStreamAsync(eventName, ExpectedVersion.Any, new EventData(Guid.NewGuid(), eventName, true, databytes, metadatabytes));
        }

        /// <inheritdoc/>>
        public async Task SubscribeToAsync(string eventName)
        {
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var eventMonitor = scope.ServiceProvider.GetService<IEventStoreMonitor>();
                if (eventMonitor == null)
                {
                    return;
                }

                var lastEventId = await eventMonitor.GetLastEventIdAsync(eventName);

                this.Subscriptions.Add(new Subscription
                {
                    ExchangeName = eventName,
                    EventStoreSubscription = this.Connection.SubscribeToStreamFrom(
                        eventName,
                        lastEventId,
                        new CatchUpSubscriptionSettings(100000, 1000, false, false, this.Configuration.ClientName),
                        this.EventReceived,
                        this.LiveProcessingStarted,
                        this.SubscriptionDropped,
                        new UserCredentials(this.Configuration.UserName, this.Configuration.Password))
                });

                this.Logger.LogDebug($"Subscribed To Event: {eventName} - Starting at Position: {lastEventId}");
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc/>>
        public void Disconnect()
        {
            this.Connection.Close();
        }

        private async Task EventReceived(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event == null)
            {
                this.Logger.LogError("[RCV] => Incoming Event Data is NULL");
                return;
            }

            this.Logger.LogDebug($"[RCV] => Converting to EventMessage");

            var incomingMessage = resolvedEvent.Event.ConvertToEventMessage();

            this.Logger.LogDebug($"[RCV] => {incomingMessage.Metadata.EventName}");

            // Get the Handler Type from the Configuration
            var registration = this.Configuration.MessageHandlerRegistrations.FirstOrDefault(p => p.EventName == incomingMessage.Metadata.EventName);
            if (registration == null)
            {
                // This should never happen since the handlers are registered for all the events that are subscribed to automatically
                this.Logger.LogError($"[RCV] => Handler Not Found for: {incomingMessage.Metadata.EventName}");
                return;
            }

            await Task.Run(() => this.OnMessageReceived?.Invoke(this, incomingMessage));

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var eventMonitor = scope.ServiceProvider.GetService<IEventStoreMonitor>();
                if (eventMonitor == null)
                {
                    return;
                }

                await eventMonitor.SaveLastEventIdAsync(incomingMessage.Metadata.EventName, resolvedEvent.OriginalEventNumber);
            }
        }

        private void SubscriptionDropped(EventStoreCatchUpSubscription subscription, SubscriptionDropReason reason, Exception exceptionMessage)
        {
            this.Logger.LogDebug($"Subscription Dropped: {reason} : {exceptionMessage}");

            var existingSubscription = this.Subscriptions.FirstOrDefault(p => p.EventStoreSubscription.StreamId == subscription.StreamId);
            if (existingSubscription != null)
            {
                this.Subscriptions.Remove(existingSubscription);
            }
        }

        private void LiveProcessingStarted(EventStoreCatchUpSubscription subscription)
        {
            this.Logger.LogDebug($"Live Processing Started: {subscription.SubscriptionName}");

            var existingSubscription = this.Subscriptions.FirstOrDefault(p => p.EventStoreSubscription.StreamId == subscription.StreamId);
            if (existingSubscription != null)
            {
                existingSubscription.IsProcessingLive = true;
            }
        }

        private string GetEventName<TMessageType>()
        {
            var messageType = typeof(TMessageType);

            // Take the Exchange Name from the Attribute - or - generate one based on the class name
            var exchangeAttribute = (EventNameAttribute)Attribute.GetCustomAttribute(messageType, typeof(EventNameAttribute));
            var exchangeName = exchangeAttribute != null ? exchangeAttribute.Name : string.Join(".", Regex.Split(messageType.Name, @"(?<!^)(?=[A-Z](?![A-Z]|$))"));

            return exchangeName.ToLower();
        }
    }

    internal class Subscription
    {
        public EventStoreStreamCatchUpSubscription EventStoreSubscription;
        public string ExchangeName;
        public bool IsProcessingLive;
        public long? CheckPointValue => this.EventStoreSubscription?.LastProcessedEventNumber;
    }
}