namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Contract for components that can appear in messages and thread (forum) posts.
/// Extends <see cref="IInteractionComponent"/>. Used by message/thread APIs only.
/// </summary>
public interface IMessageComponent : IInteractionComponent
{
	/// <summary>
	/// Developer-defined identifier for interactive components (1-100 characters).
	/// </summary>
	string? CustomId { get; set; }

	/// <summary>
	/// Whether the component is disabled.
	/// </summary>
	bool? Disabled { get; set; }
}
