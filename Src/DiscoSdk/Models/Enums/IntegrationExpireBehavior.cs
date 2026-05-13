namespace DiscoSdk.Models.Enums;

/// <summary>
/// What to do when a guild integration subscription lapses.
/// </summary>
public enum IntegrationExpireBehavior
{
	/// <summary>Remove the linked role from the user when the subscription expires.</summary>
	RemoveRole = 0,

	/// <summary>Kick the user from the guild when the subscription expires.</summary>
	Kick = 1
}
