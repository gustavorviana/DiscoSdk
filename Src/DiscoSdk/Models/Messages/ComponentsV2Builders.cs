using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Fluent builder for a <see cref="ContainerComponent"/> (type 17) — the outer wrapper for a
/// Components V2 message. Accepts an optional accent colour + spoiler and a list of inner V2
/// components.
/// </summary>
public class ContainerBuilder : IInteractionComponentBuilder
{
	private readonly ContainerComponent _container = new();
	private readonly List<IInteractionComponent> _components = [];

	/// <summary>Sets the left-edge accent colour (RGB int — e.g. <c>0x5865F2</c>).</summary>
	public ContainerBuilder WithAccentColor(int rgb) { _container.AccentColor = rgb; return this; }

	/// <summary>Renders the whole container hidden behind a spoiler overlay.</summary>
	public ContainerBuilder WithSpoiler(bool spoiler = true) { _container.Spoiler = spoiler; return this; }

	/// <summary>Adds a markdown text block.</summary>
	public ContainerBuilder AddTextDisplay(string content)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(content);
		_components.Add(new TextDisplayComponent { Content = content });
		return this;
	}

	/// <summary>Adds a separator. <paramref name="divider"/> draws a line; <paramref name="spacing"/> controls padding.</summary>
	public ContainerBuilder AddSeparator(bool divider = true, SeparatorSpacing spacing = SeparatorSpacing.Small)
	{
		_components.Add(new SeparatorComponent { Divider = divider, Spacing = spacing });
		return this;
	}

	/// <summary>Adds an arbitrary V2 component built externally (Section / MediaGallery / File / ActionRow / etc.).</summary>
	public ContainerBuilder AddComponent(IInteractionComponent component)
	{
		ArgumentNullException.ThrowIfNull(component);
		_components.Add(component);
		return this;
	}

	/// <summary>Adds a built component from a builder.</summary>
	public ContainerBuilder AddComponent(IInteractionComponentBuilder builder)
	{
		ArgumentNullException.ThrowIfNull(builder);
		_components.Add(builder.Build());
		return this;
	}

	public ContainerComponent Build()
	{
		if (_components.Count == 0)
			throw new InvalidOperationException("Container must contain at least one component.");
		_container.Components = [.. _components];
		return _container;
	}

	IInteractionComponent IInteractionComponentBuilder.Build() => Build();
}

/// <summary>
/// Fluent builder for a <see cref="SectionComponent"/> (type 9) — 1–3 <see cref="TextDisplayComponent"/>
/// paired with a <see cref="ButtonComponent"/> or <see cref="ThumbnailComponent"/> accessory.
/// </summary>
public class SectionBuilder : IInteractionComponentBuilder
{
	private readonly SectionComponent _section = new();
	private readonly List<TextDisplayComponent> _texts = [];

	/// <summary>Adds a TextDisplay block (max 3).</summary>
	public SectionBuilder AddText(string content)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(content);
		if (_texts.Count >= 3)
			throw new InvalidOperationException("Section accepts at most 3 TextDisplay components.");
		_texts.Add(new TextDisplayComponent { Content = content });
		return this;
	}

	/// <summary>Sets a Thumbnail as the accessory.</summary>
	public SectionBuilder WithThumbnail(string url, string? description = null, bool? spoiler = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(url);
		_section.Accessory = new ThumbnailComponent
		{
			Media = new UnfurledMediaItem { Url = url },
			Description = description,
			Spoiler = spoiler,
		};
		return this;
	}

	/// <summary>Sets a Button as the accessory (uses a <see cref="ButtonBuilder"/>).</summary>
	public SectionBuilder WithButton(ButtonBuilder button)
	{
		ArgumentNullException.ThrowIfNull(button);
		_section.Accessory = button.Build();
		return this;
	}

	public SectionComponent Build()
	{
		if (_texts.Count == 0)
			throw new InvalidOperationException("Section must contain at least one TextDisplay.");
		if (_section.Accessory is null)
			throw new InvalidOperationException("Section requires an accessory — a button or a thumbnail.");
		_section.Components = [.. _texts];
		return _section;
	}

	IInteractionComponent IInteractionComponentBuilder.Build() => Build();
}

/// <summary>
/// Fluent builder for a <see cref="MediaGalleryComponent"/> (type 12) — 1–10 images displayed
/// as a grid.
/// </summary>
public class MediaGalleryBuilder : IInteractionComponentBuilder
{
	private readonly List<MediaGalleryItem> _items = [];

	/// <summary>Adds an image (1–10 total).</summary>
	public MediaGalleryBuilder AddImage(string url, string? description = null, bool? spoiler = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(url);
		if (_items.Count >= 10)
			throw new InvalidOperationException("MediaGallery accepts at most 10 items.");
		_items.Add(new MediaGalleryItem
		{
			Media = new UnfurledMediaItem { Url = url },
			Description = description,
			Spoiler = spoiler,
		});
		return this;
	}

	public MediaGalleryComponent Build()
	{
		if (_items.Count == 0)
			throw new InvalidOperationException("MediaGallery must contain at least one item.");
		return new MediaGalleryComponent { Items = [.. _items] };
	}

	IInteractionComponent IInteractionComponentBuilder.Build() => Build();
}
