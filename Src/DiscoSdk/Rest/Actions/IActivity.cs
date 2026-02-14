using DiscoSdk.Models.Activities;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Builder for an activity update (presence). Build() returns <see cref="ActivityUpdate"/> for use in <see cref="IUpdatePresenceAction"/>.
/// For reading activity data from Discord, use <see cref="Activity"/>.
/// </summary>
public interface IActivity
{
	/// <summary>
	/// Builds the activity into an <see cref="ActivityUpdate"/> for sending to Discord.
	/// </summary>
	/// <returns>The built <see cref="ActivityUpdate"/>.</returns>
	ActivityUpdate Build();
}

