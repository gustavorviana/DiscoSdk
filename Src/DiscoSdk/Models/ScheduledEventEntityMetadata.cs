using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Extra venue information for a guild scheduled event. Currently only carries
/// <see cref="Location"/>, required for events with entity type "External".
/// Internal — consumers read the location directly from <see cref="IGuildScheduledEvent.Location"/>,
/// and write it via <c>SetLocation(...)</c> on the create/modify action builders.
/// </summary>
internal class ScheduledEventEntityMetadata
{
	/// <summary>
	/// Free-form location string shown in the event details (max 100 chars). Required when the
	/// parent event's entity type is <c>External</c>; ignored otherwise.
	/// </summary>
	[JsonPropertyName("location")]
	public string? Location { get; set; }
}
