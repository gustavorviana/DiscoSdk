using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Fluent builder for message-side <see cref="ButtonComponent"/> (type 2). Buttons are always
/// added inside an <see cref="MessageActionRowComponent"/> (the message builder's
/// <c>AddActionRow</c> handles that automatically).
/// </summary>
public class ButtonBuilder : IInteractionComponentBuilder
{
	private readonly ButtonComponent _button = new();

	/// <summary>Starts a non-link button.</summary>
	/// <param name="style">Visual style (Primary, Secondary, Success, Danger).</param>
	/// <param name="customId">Identifier sent back on click (1–100 chars).</param>
	public ButtonBuilder(ButtonStyle style, string customId)
	{
		if (style == ButtonStyle.Link)
			throw new ArgumentException("Use the Link constructor (URL) for link buttons.", nameof(style));
		if (style == ButtonStyle.Premium)
			throw new ArgumentException("Use the Premium constructor (skuId) for premium buttons.", nameof(style));
		if (string.IsNullOrWhiteSpace(customId))
			throw new ArgumentException("Custom ID cannot be null or empty.", nameof(customId));
		if (customId.Length > 100)
			throw new ArgumentException("Custom ID cannot exceed 100 characters.", nameof(customId));

		_button.Style = style;
		_button.CustomId = customId;
	}

	private ButtonBuilder()
	{
	}

	/// <summary>Creates a Link button (no custom_id, no click event — opens a URL).</summary>
	public static ButtonBuilder Link(string url, string label)
	{
		if (string.IsNullOrWhiteSpace(url))
			throw new ArgumentException("URL cannot be null or empty.", nameof(url));

		var builder = new ButtonBuilder
		{
			_button = { Style = ButtonStyle.Link, Url = url },
		};
		return builder.WithLabel(label);
	}

	/// <summary>Creates a Premium button that prompts the user to buy the given SKU.</summary>
	public static ButtonBuilder Premium(Snowflake skuId)
	{
		return new ButtonBuilder
		{
			_button = { Style = ButtonStyle.Premium, SkuId = skuId },
		};
	}

	/// <summary>Sets the button label (≤ 80 chars). Required unless an emoji is provided.</summary>
	public ButtonBuilder WithLabel(string label)
	{
		if (string.IsNullOrWhiteSpace(label))
			throw new ArgumentException("Label cannot be null or empty.", nameof(label));
		if (label.Length > 80)
			throw new ArgumentException("Button label cannot exceed 80 characters.", nameof(label));
		_button.Label = label;
		return this;
	}

	/// <summary>Sets a leading emoji.</summary>
	public ButtonBuilder WithEmoji(Emoji emoji)
	{
		ArgumentNullException.ThrowIfNull(emoji);
		_button.Emoji = emoji;
		return this;
	}

	/// <summary>Sets the disabled state.</summary>
	public ButtonBuilder WithDisabled(bool disabled = true)
	{
		_button.Disabled = disabled;
		return this;
	}

	/// <summary>Builds the button. Validates that label or emoji is present (Discord requires one).</summary>
	public ButtonComponent Build()
	{
		if (_button.Style == ButtonStyle.Link && string.IsNullOrWhiteSpace(_button.Url))
			throw new InvalidOperationException("Link button requires a URL.");
		if (_button.Style != ButtonStyle.Link && _button.Style != ButtonStyle.Premium && string.IsNullOrWhiteSpace(_button.CustomId))
			throw new InvalidOperationException("Non-link, non-premium button requires a custom_id.");
		if (_button.Style != ButtonStyle.Premium && string.IsNullOrWhiteSpace(_button.Label) && _button.Emoji is null)
			throw new InvalidOperationException("Button requires either a label or an emoji.");

		return _button;
	}

	IInteractionComponent IInteractionComponentBuilder.Build() => Build();
}
