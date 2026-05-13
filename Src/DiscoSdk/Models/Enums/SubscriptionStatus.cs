namespace DiscoSdk.Models.Enums;

/// <summary>
/// The status of a Discord subscription.
/// </summary>
public enum SubscriptionStatus
{
	/// <summary>The subscription is active and scheduled to renew.</summary>
	Active = 0,

	/// <summary>The subscription is active but will not renew.</summary>
	Ending = 1,

	/// <summary>The subscription is inactive and not being charged.</summary>
	Inactive = 2
}
