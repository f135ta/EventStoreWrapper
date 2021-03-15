namespace Simple.EventStore.Attributes
{
	using System;

	/// <summary>
    /// Attribute to override the Event Name used for this object
    /// </summary>
    public class EventNameAttribute : Attribute
    {
        public string Name { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="EventNameAttribute"/>
        /// </summary>
        /// <param name="name">New Name</param>
        public EventNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
