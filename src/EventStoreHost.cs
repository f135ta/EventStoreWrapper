namespace Simple.EventStore
{
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using System.Threading.Tasks;
	using Configuration;
	using Interfaces;
	using Internal.Helpers;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.Options;
	using Models;
	using Newtonsoft.Json;

	/// <summary>
    /// Event Store Background Service
    /// </summary>
    internal class EventStoreHost : BackgroundService
    {
        private readonly EventStoreConfiguration Configuration;
        private readonly IServiceScopeFactory ServiceScopeFactory;
        private readonly IEventStoreClient Connection;
        private readonly ILogger<EventStoreHost> Logger;

        public EventStoreHost(IServiceScopeFactory serviceScopeFactory, ILoggerFactory loggerFactory, IOptions<EventStoreConfiguration> eventHubConfig, IEventStoreClient eventStoreClient)
        {
            this.Configuration = eventHubConfig.Value;
            this.ServiceScopeFactory = serviceScopeFactory;
            this.Logger = loggerFactory.CreateLogger<EventStoreHost>();
            this.Connection = eventStoreClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            await this.Connection.ConnectAsync();

            this.Connection.OnMessageReceived += async (s, message) => { await this.EventReceived(message); };
            this.Connection.OnConnected += async (s, args) => { await this.SubscribeToEventsAsync(); };

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000), stoppingToken);
            }

            this.Connection.Disconnect();
        }

        private async Task SubscribeToEventsAsync()
        {
            foreach (var registration in this.Configuration.MessageHandlerRegistrations)
            {
                await this.Connection.SubscribeToAsync(registration.EventName);
            }
        }

        private async Task EventReceived(EventMessage message)
        {
            this.Logger.LogDebug($"[IN] - {message.Metadata.EventName}");

            // Get the Handler Type from the Configuration
            var registration = this.Configuration.MessageHandlerRegistrations.FirstOrDefault(p => p.EventName == message.Metadata.EventName);
            if (registration == null)
            {
                this.Logger.LogError($"[IN] - Handler Not Found for: {message.Metadata.EventName}");
                return;
            }
            try
            {
                await this.ExecuteHandlerAsync(message, registration);
            }
            catch (Exception exception)
            {
                this.Logger.LogError($"Message Process Failed: {exception.Message}");
            }
        }

        /// <summary>
        /// Gets the Handler Registration and Executes it
        /// </summary>
        /// <param name="incomingMessage">Incoming WebSocket Message</param>
        /// <param name="registration">Handler Registration</param>
        /// <returns><see cref="Task"/></returns>
        private async Task ExecuteHandlerAsync(EventMessage incomingMessage, HandlerRegistration registration)
        {
            var handlerType = registration.HandlerInterface;
            var methodInfo = handlerType.GetMethod("ProcessMessageAsync");
            if (methodInfo == null)
            {
                return;
            }

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var handler = scope.ServiceProvider.GetService(handlerType);
                if (handler == null)
                {
                    this.Logger.LogError($"Message Handler not registered for type: {registration.MessageType}");
                    return;
                }

                // ANY CHANGES TO THE EventMessage class MUST be reflected here!
                var messageContext = Activator.CreateInstance(typeof(MessageContext<>).MakeGenericType(registration.MessageType));

                var objectValue = RuntimeHelpers.GetObjectValue(messageContext);
                objectValue.SetPrivatePropertyValue("Payload", JsonConvert.DeserializeObject(incomingMessage.Payload, registration.MessageType));
                objectValue.SetPrivatePropertyValue("Metadata", incomingMessage.Metadata);

                this.Logger.LogDebug($"Executing Handler: {registration.HandlerClass.Name}");

                await ObjectMethodExecutor
                    .Create(methodInfo, registration.HandlerClass.GetTypeInfo())
                    .ExecuteAsync(handler, new[]
                    {
                        messageContext
                    });
            }
        }
    }
}
