using DiscoSdk.Models.Activities;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Builder for an activity update (presence). <see cref="Build"/> returns an
/// <see cref="ActivityUpdate"/> for use in <see cref="IUpdatePresenceAction"/>. For reading the
/// activities a user is currently broadcasting, consume <see cref="IActivity"/> from
/// <see cref="DiscoSdk.Models.IMember.Activities"/> or
/// <see cref="DiscoSdk.Models.Presences.IPresence.Activities"/>.
/// </summary>
public interface IActivityBuilder
{
    /// <summary>
    /// Builds the activity into an <see cref="ActivityUpdate"/> for sending to Discord.
    /// </summary>
    /// <returns>The built <see cref="ActivityUpdate"/>.</returns>
    ActivityUpdate Build();
}
