namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Builder that produces an <see cref="IModalComponent"/>.
/// Extends <see cref="IInteractionComponentBuilder"/> with a more specific return type.
/// </summary>
public interface IModalComponentBuilder : IInteractionComponentBuilder
{
	/// <summary>
	/// Builds and returns the modal component.
	/// </summary>
	/// <returns>The built <see cref="IModalComponent"/>.</returns>
	new IModalComponent Build();
}
