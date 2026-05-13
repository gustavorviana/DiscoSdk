using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.AutoModeration;

/// <summary>
/// Read-only view of an auto-moderation action.
/// </summary>
public interface IAutoModerationAction
{
	/// <summary>The type of action.</summary>
	AutoModerationActionType Type { get; }

	/// <summary>Additional metadata needed during execution for this action type.</summary>
	IAutoModerationActionMetadata? Metadata { get; }
}
