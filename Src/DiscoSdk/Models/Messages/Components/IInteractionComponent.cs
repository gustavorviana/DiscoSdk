using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Base contract for Discord message/modal components.
/// Per Discord API: all components have <see cref="Type"/> and optionally <see cref="Id"/>.
/// </summary>
/// <seealso href="https://docs.discord.com/developers/components/reference#anatomy-of-a-component" />
public interface IInteractionComponent
{
	/// <summary>
	/// The type of the component (required by Discord).
	/// </summary>
	ComponentType Type { get; set; }

	/// <summary>
	/// Optional 32-bit identifier for the component. Used to identify components in interaction responses.
	/// </summary>
	int? Id { get; set; }
}
