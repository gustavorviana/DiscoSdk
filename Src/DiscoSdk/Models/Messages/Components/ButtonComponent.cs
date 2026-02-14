using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Button component for messages and threads.
/// </summary>
public class ButtonComponent : IMessageComponent
{
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.Button;

	[JsonPropertyName("id")]
	public int? Id { get; set; }

	[JsonPropertyName("style")]
	public ButtonStyle Style { get; set; }

	[JsonPropertyName("label")]
	public string? Label { get; set; }

	[JsonPropertyName("emoji")]
	public Emoji? Emoji { get; set; }

	[JsonPropertyName("custom_id")]
	public string? CustomId { get; set; }

	[JsonPropertyName("url")]
	public string? Url { get; set; }

	[JsonPropertyName("disabled")]
	public bool? Disabled { get; set; }

	[JsonPropertyName("sku_id")]
	public Snowflake? SkuId { get; set; }
}
