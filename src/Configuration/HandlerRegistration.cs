namespace Simple.EventStore.Configuration
{
	using System;

	internal class HandlerRegistration
    {
        public string EventName { get; set; }
        public Type HandlerClass { get; set; }
        public Type HandlerInterface { get; set; }
        public Type MessageType { get; set; }
    }
}