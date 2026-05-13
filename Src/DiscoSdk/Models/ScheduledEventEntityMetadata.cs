using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Extra venue information for a guild scheduled event. Currently only carries
/// <see cref="Location"/>, which is required for events with entity type "External".
/// </summary>
public class ScheduledEventEntityMetadata
{
	/// <summary>
	/// Free-form location string shown in the event details (max 100 chars). Required when the
	/// parent event's entity type is <c>External</c>; ignored otherwise.
	/// </summary>
	[JsonPropertyName("location")]
	public string? Location { get; set; }
}
