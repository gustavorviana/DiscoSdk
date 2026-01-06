using DiscoSdk.Models.Enums;

namespace DiscoSdk;

/// <summary>
/// Represents the current authenticated user in the Discord application.
/// </summary>
public interface ICurrentUser
{
    string? Avatar { get; }
    bool Bot { get; }
    string Discriminator { get; }
    string? Email { get; }
    UserFlags Flags { get; }
    string? GlobalName { get; }
    string Id { get; }
    bool MfaEnabled { get; }
    string Username { get; }
    bool Verified { get; }
}