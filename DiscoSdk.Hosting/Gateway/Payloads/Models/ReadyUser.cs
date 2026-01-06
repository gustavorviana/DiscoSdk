using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Gateway.Payloads.Models;

/// <summary>
/// Represents the user information received in the READY payload from the Discord Gateway.
/// </summary>
internal class ReadyUser : ICurrentUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("username")]
    public string Username { get; set; } = default!;

    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; } = default!;

    [JsonPropertyName("global_name")]
    public string? GlobalName { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("bot")]
    public bool Bot { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("mfa_enabled")]
    public bool MfaEnabled { get; set; }

    [JsonPropertyName("flags")]
    public int Flags { get; set; }
}
