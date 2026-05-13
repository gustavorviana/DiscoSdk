using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.AutoModeration;

/// <summary>
/// An action executed when an <see cref="AutoModerationRule"/> is triggered. Doubles as the public
/// read surface (<see cref="IAutoModerationAction"/>).
/// </summary>
public class AutoModerationAction : IAutoModerationAction
{
	/// <summary>The type of action.</summary>
	[JsonPropertyName("type")]
	public AutoModerationActionType Type { get; set; }

	/// <summary>Additional metadata needed during execution for this action type.</summary>
	[JsonPropertyName("metadata")]
	public AutoModerationActionMetadata? Metadata { get; set; }

	IAutoModerationActionMetadata? IAutoModerationAction.Metadata => Metadata;
}
