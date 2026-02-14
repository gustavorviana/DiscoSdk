namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Builder that produces an <see cref="IInteractionComponent"/>.
/// </summary>
public interface IInteractionComponentBuilder
{
	/// <summary>
	/// Builds and returns the component.
	/// </summary>
	/// <returns>The built <see cref="IInteractionComponent"/>.</returns>
	IInteractionComponent Build();
}
