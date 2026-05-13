namespace DiscoSdk.Models;

/// <summary>
/// Public read-only view of an external account tied to a guild integration.
/// </summary>
public interface IIntegrationAccount
{
    /// <summary>The ID of the external account.</summary>
    string Id { get; }

    /// <summary>The display name of the external account.</summary>
    string Name { get; }
}
