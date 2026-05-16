using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Partial emoji reference — the minimal shape Discord accepts in component payloads
/// (<c>button.emoji</c>, <c>select_option.emoji</c>), reaction endpoints, and onboarding prompts.
/// </summary>
/// <remarks>
/// <para>
/// Construct one of these whenever you need to attach an emoji to a button, select option, poll
/// answer, or reaction:
/// </para>
/// <list type="bullet">
///   <item><term>Unicode</term><description><c>new Emoji { Name = "✅" }</c></description></item>
///   <item><term>Custom</term><description><c>new Emoji { Id = 123, Name = "smile", Animated = false }</c></description></item>
/// </list>
/// <para>
/// The richer guild-emoji shape (with creator, allowed roles, managed/available flags) is
/// represented by <see cref="IEmoji"/> and is only returned from guild emoji listings.
/// </para>
/// </remarks>
public class Emoji
{
    /// <summary>The emoji's id when it's a custom guild emoji, otherwise <c>null</c> (Unicode).</summary>
    [JsonPropertyName("id")]
    public Snowflake? Id { get; set; }

    /// <summary>
    /// The emoji's name — the literal Unicode character for built-in emojis, or the alias for
    /// custom emojis. <c>null</c> only inside deserialized reaction payloads where Discord omits
    /// it for deleted custom emojis.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Whether the custom emoji is animated. Always <c>false</c> for Unicode emojis.</summary>
    [JsonPropertyName("animated")]
    public bool? Animated { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Id.HasValue)
            return $"{Name}:{Id.Value}";

        return Name ?? string.Empty;
    }
}
