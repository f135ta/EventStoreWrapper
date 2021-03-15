namespace Simple.EventStore.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using Attributes;
	using Interfaces;

	/// <summary>
    /// Event Store Configuration
    /// </summary>
    public class EventStoreConfiguration
    {
        internal string ClientName { get; set; }
        internal string TcpHostAddress { get; set; } = "tcp://localhost:1113";
        internal string UserName { get; set; } = "admin";
        internal string Password { get; set; } = "changeit";

        internal List<HandlerRegistration> MessageHandlerRegistrations = new List<HandlerRegistration>();

        /// <summary>
        /// Consumer Name
        /// </summary>
        /// <remarks>
        /// Note: This value is prepended to queue names to provide a unique queue name. Eg: [queueName]:[eventName]
        /// </remarks>
        public EventStoreConfiguration SetConsumerName(string value)
        {
            this.ClientName = value;
            return this;
        }

        /// <summary>
        /// TCP Endpoint Address
        /// </summary>
        public EventStoreConfiguration SetHostAddress(string value)
        {
            this.TcpHostAddress = value;
            return this;
        }

        /// <summary>
        /// UserName that connects to the server.
        /// </summary>
        public EventStoreConfiguration WithUserName(string value)
        {
            this.UserName = value;
            return this;
        }

        /// <summary>
        /// Password of the chosen user.
        /// </summary>
        public EventStoreConfiguration SetPassword(string value)
        {
            this.Password = value;
            return this;
        }

        /// <summary>
        /// Adds a Message Handler
        /// </summary>
        /// <returns><see cref="EventStoreConfiguration"/></returns>
        public EventStoreConfiguration AddHandlersFromAssembly(Assembly assembly)
        {
            var handlers = assembly.GetTypes().SelectMany(x => x.GetInterfaces(), (x, z) => new { x, z })
                .Select(t => new { t, y = t.x.BaseType })
                .Where(t => t.y != null && t.y.IsGenericType && typeof(IMessageConsumer<>)
                                .IsAssignableFrom(t.y.GetGenericTypeDefinition()) || t.t.z.IsGenericType && typeof(IMessageConsumer<>)
                                .IsAssignableFrom(t.t.z.GetGenericTypeDefinition())).Select(t => t.t.x);

            foreach (var handlerType in handlers)
            {
                this.AddHandler(handlerType);
            }

            return this;
        }

        /// <summary>
        /// Adds a Message Handler
        /// </summary>
        /// <returns><see cref="EventStoreConfiguration"/></returns>
        public EventStoreConfiguration AddHandler<THandler>() where THandler : class
        {
            return this.AddHandler(typeof(THandler));
        }

        /// <summary>
        /// Adds a Message Handler
        /// </summary>
        /// <returns><see cref="EventStoreConfiguration"/></returns>
        public EventStoreConfiguration AddHandler(Type handlerType)
        {
            var messageHandlerInterface = handlerType.GetInterfaces().FirstOrDefault(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IMessageConsumer<>));
            var messageType = messageHandlerInterface?.GenericTypeArguments.FirstOrDefault();

            var handlerName = messageType?.Name;
            if (handlerName == null)
            {
                return this;
            }

            // Take the Exchange Name from the Attribute - or - generate one based on the class name
            var eventNameAttribute = (EventNameAttribute)Attribute.GetCustomAttribute(messageType, typeof(EventNameAttribute));
            var eventName = eventNameAttribute != null ? eventNameAttribute.Name : string.Join(".", Regex.Split(handlerName, @"(?<!^)(?=[A-Z](?![A-Z]|$))")).ToLower();

            var registration = new HandlerRegistration
            {
                HandlerClass = handlerType,
                MessageType = messageType,
                HandlerInterface = messageHandlerInterface,
                EventName = eventName,
            };

            this.MessageHandlerRegistrations.Add(registration);

            return this;
        }
    }
}