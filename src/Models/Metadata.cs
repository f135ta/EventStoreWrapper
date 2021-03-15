namespace Simple.EventStore.Models
{
	using System;
	using Newtonsoft.Json;

	/// <summary>
	/// Metadata
	/// </summary>
	[JsonObject]
	public class Metadata
	{
		/// <summary>
		/// Gets or sets the Sent Date
		/// </summary>
		[JsonProperty("Sent")]
		public DateTimeOffset SentDate { get; set; } = DateTimeOffset.Now;

		/// <summary>
		/// Gets or sets the Original Sender Id
		/// </summary>
		[JsonProperty("Sender")]
		public string Sender { get; set; }

		/// <summary>
		/// Gets or sets the Event Name
		/// </summary>
		[JsonProperty("Event")]
		public string EventName { get; set; }
	}
}