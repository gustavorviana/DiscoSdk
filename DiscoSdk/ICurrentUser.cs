namespace DiscoSdk;

public interface ICurrentUser
{
    string? Avatar { get; }
    bool Bot { get; }
    string Discriminator { get; }
    string? Email { get; }
    int Flags { get; }
    string? GlobalName { get; }
    string Id { get; }
    bool MfaEnabled { get; }
    string Username { get; }
    bool Verified { get; }
}