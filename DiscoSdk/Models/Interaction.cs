using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using DiscoSdk.Models.Messages;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord interaction.
/// </summary>
public class Interaction
{
	/// <summary>
	/// Gets or sets the ID of the interaction.
	/// </summary>
	[JsonPropertyName("id")]
    public string Id { get; set; } = default!;

	/// <summary>
	/// Gets or sets the application ID.
	/// </summary>
	[JsonPropertyName("application_id")]
	public string ApplicationId { get; set; } = default!;

	/// <summary>
	/// Gets or sets the type of interaction.
	/// </summary>
	[JsonPropertyName("type")]
	public InteractionType Type { get; set; }

	/// <summary>
	/// Gets or sets the interaction data, if the interaction is of type APPLICATION_COMMAND.
	/// </summary>
	[JsonPropertyName("data")]
	public InteractionData? Data { get; set; }

	/// <summary>
	/// Gets or sets the guild ID where the interaction was triggered.
	/// </summary>
	[JsonPropertyName("guild_id")]
	public string? GuildId { get; set; }

	/// <summary>
	/// Gets or sets the channel ID where the interaction was triggered.
	/// </summary>
	[JsonPropertyName("channel_id")]
	public string? ChannelId { get; set; }

	/// <summary>
	/// Gets or sets the member who triggered the interaction.
	/// </summary>
	[JsonPropertyName("member")]
	public GuildMember? Member { get; set; }

	/// <summary>
	/// Gets or sets the user who triggered the interaction.
	/// </summary>
	[JsonPropertyName("user")]
	public User? User { get; set; }

	/// <summary>
	/// Gets or sets the continuation token for responding to the interaction.
	/// </summary>
	[JsonPropertyName("token")]
	public string Token { get; set; } = default!;

	/// <summary>
	/// Gets or sets the read-only property, always 1.
	/// </summary>
	[JsonPropertyName("version")]
	public int Version { get; set; }

	/// <summary>
	/// Gets or sets the message this button was attached to.
	/// </summary>
	[JsonPropertyName("message")]
	public Message? Message { get; set; }

	/// <summary>
	/// Gets or sets the selected language of the invoking user.
	/// </summary>
	[JsonPropertyName("locale")]
	public string? Locale { get; set; }

	/// <summary>
	/// Gets or sets the guild's preferred locale.
	/// </summary>
	[JsonPropertyName("guild_locale")]
	public string? GuildLocale { get; set; }
}

/// <summary>
/// Represents interaction data for application commands.
/// </summary>
public class InteractionData
{
	/// <summary>
	/// Gets or sets the ID of the invoked command.
	/// </summary>
	[JsonPropertyName("id")]
    [JsonConverter(typeof(SafeStringConverter))]
    public string? Id { get; set; }

	/// <summary>
	/// Gets or sets the name of the invoked command.
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>
	/// Gets or sets the type of the invoked command.
	/// </summary>
	[JsonPropertyName("type")]
	public ApplicationCommandType Type { get; set; }

	/// <summary>
	/// Gets or sets the converted users.
	/// </summary>
	[JsonPropertyName("resolved")]
	public InteractionResolved? Resolved { get; set; }

	/// <summary>
	/// Gets or sets the parameters and values from the user.
	/// </summary>
	[JsonPropertyName("options")]
	public InteractionOption[]? Options { get; set; }

	/// <summary>
	/// Gets or sets the custom ID of the component.
	/// </summary>
	[JsonPropertyName("custom_id")]
	public string? CustomId { get; set; }

	/// <summary>
	/// Gets or sets the type of component.
	/// </summary>
	[JsonPropertyName("component_type")]
	public ComponentType? ComponentType { get; set; }

	/// <summary>
	/// Gets or sets the values the user selected.
	/// </summary>
	[JsonPropertyName("values")]
	public string[]? Values { get; set; }

	/// <summary>
	/// Gets or sets the components submitted in a modal.
	/// </summary>
	[JsonPropertyName("components")]
	public ModalComponent[]? Components { get; set; }
}

/// <summary>
/// Represents a component submitted in a modal.
/// </summary>
public class ModalComponent
{
	/// <summary>
	/// Gets or sets the type of component. Must be ActionRow (1) for modal submissions.
	/// </summary>
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.ActionRow;

	/// <summary>
	/// Gets or sets the components within this action row (text inputs).
	/// </summary>
	[JsonPropertyName("components")]
	public ModalTextInput[]? Components { get; set; }
}

/// <summary>
/// Represents a text input component submitted in a modal.
/// </summary>
public class ModalTextInput
{
	/// <summary>
	/// Gets or sets the type of component. Must be TextInput (4).
	/// </summary>
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.TextInput;

	/// <summary>
	/// Gets or sets the custom ID of the text input.
	/// </summary>
	[JsonPropertyName("custom_id")]
	public string CustomId { get; set; } = default!;

	/// <summary>
	/// Gets or sets the value entered by the user.
	/// </summary>
	[JsonPropertyName("value")]
	public string Value { get; set; } = default!;
}

/// <summary>
/// Represents resolved data from an interaction.
/// </summary>
public class InteractionResolved
{
	/// <summary>
	/// Gets or sets the resolved users.
	/// </summary>
	[JsonPropertyName("users")]
	public Dictionary<string, User>? Users { get; set; }

	/// <summary>
	/// Gets or sets the resolved members.
	/// </summary>
	[JsonPropertyName("members")]
	public Dictionary<string, GuildMember>? Members { get; set; }

	/// <summary>
	/// Gets or sets the resolved roles.
	/// </summary>
	[JsonPropertyName("roles")]
	public Dictionary<string, Role>? Roles { get; set; }

	/// <summary>
	/// Gets or sets the resolved channels.
	/// </summary>
	[JsonPropertyName("channels")]
	public Dictionary<string, Channel>? Channels { get; set; }

	/// <summary>
	/// Gets or sets the resolved messages.
	/// </summary>
	[JsonPropertyName("messages")]
	public Dictionary<string, Message>? Messages { get; set; }
}

