using DiscoSdk.Models;
using DiscoSdk.Models.Activities;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents an activity that can be built into a Discord Activity.
/// </summary>
public interface IActivity
{
	/// <summary>
	/// Builds the activity into a Discord Activity object.
	/// </summary>
	/// <returns>The built <see cref="Activity"/>.</returns>
	Activity Build();
}

