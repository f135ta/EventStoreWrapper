namespace Simple.EventStore.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Configuration;
	using Interfaces;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyInjection.Extensions;
	using Microsoft.Extensions.Hosting;

	/// <summary>
    /// Service Collection Extensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Event Store Client
        /// </summary>
        /// <param name="services">Service Collection</param>
        /// <param name="configuration">Event Store Configuration</param>
        /// <param name="eventMonitorLifeTime">Event Monitor Lifetime</param>
        public static void AddEventStoreClient<TEventMonitor>(
            this IServiceCollection services,
            Action<EventStoreConfiguration> configuration,
            ServiceLifetime eventMonitorLifeTime = ServiceLifetime.Transient) where TEventMonitor : class, IEventStoreMonitor
        {
            services.AddLogging();
            services.AddOptions();

            services.Configure(configuration);

            var config = new EventStoreConfiguration();
            configuration(config);

            foreach (var handler in config.MessageHandlerRegistrations)
            {
                services.AddScoped(handler.HandlerInterface, handler.HandlerClass);
            }

            services.AddSingleton<IEventStoreClient, EventStoreClient>();
            services.AddSingleton<IEventPublisher, EventPublisher>();

            services.TryAdd(new ServiceDescriptor(typeof(IEventStoreMonitor), typeof(TEventMonitor), eventMonitorLifeTime));
        }

        /// <summary>
        /// Adds the EventStore Host Client
        /// </summary>
        /// <param name="hostBuilder">Host Builder</param>
        public static IHostBuilder AddEventStoreHost(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(s => { s.AddHostedService<EventStoreHost>(); });
        }

        /// <summary>
        /// Adds the Event Store Client
        /// </summary>
        /// <param name="services">Service Collection</param>
        /// <param name="configuration">Event Store Configuration</param>
        public static void AddEventStoreClient(this IServiceCollection services, Action<EventStoreConfiguration> configuration)
        {
            AddEventStoreClient<DefaultEventStoreMonitor>(services, configuration, ServiceLifetime.Singleton);
        }
    }

    /// <summary>
    /// Default Implementation of the Event Store Monitor
    /// Will only persist the value of the last received event while it is active
    /// </summary>
    internal class DefaultEventStoreMonitor : IEventStoreMonitor
    {
        internal static Dictionary<string, long> SubscriptionIds { get; set; } = new Dictionary<string, long>();

        public async Task<long> GetLastEventIdAsync(string subscriptionName)
        {
            if (SubscriptionIds.ContainsKey(subscriptionName))
            {
                return SubscriptionIds.GetValueOrDefault(subscriptionName);
            }

            return await Task.FromResult(0);
        }

        public async Task SaveLastEventIdAsync(string subscriptionName, long eventId)
        {
            if (SubscriptionIds.ContainsKey(subscriptionName))
            {
                SubscriptionIds[subscriptionName] = eventId;
            }
            else
            {
                SubscriptionIds.TryAdd(subscriptionName, eventId);
            }

            await Task.CompletedTask;
        }
    }
}
