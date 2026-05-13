namespace DiscoSdk.Models;

/// <summary>
/// Read-only view of a selectable option inside an <see cref="IOnboardingPrompt"/>.
/// </summary>
public interface IOnboardingPromptOption
{
	/// <summary>The ID of the option.</summary>
	Snowflake Id { get; }

	/// <summary>IDs of channels a member is opted into when selecting this option.</summary>
	IReadOnlyList<Snowflake> ChannelIds { get; }

	/// <summary>IDs of roles assigned to a member when selecting this option.</summary>
	IReadOnlyList<Snowflake> RoleIds { get; }

	/// <summary>The emoji of the option.</summary>
	IEmoji? Emoji { get; }

	/// <summary>The title of the option.</summary>
	string Title { get; }

	/// <summary>The description of the option.</summary>
	string? Description { get; }
}
