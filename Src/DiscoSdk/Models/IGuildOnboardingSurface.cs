using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Per-guild onboarding surface — every operation that targets <c>/guilds/:id/onboarding*</c>.
/// Suffix <c>Surface</c> avoids the name clash with the <see cref="IGuildOnboarding"/> data model.
/// </summary>
public interface IGuildOnboardingSurface
{
    /// <summary>Builds a deferred REST action that retrieves the guild's onboarding configuration.</summary>
    IRestAction<IGuildOnboarding> Get();

    /// <summary>
    /// Builds a deferred fluent action that overwrites the onboarding configuration
    /// (<c>PUT /guilds/:id/onboarding</c>) without first fetching the current state.
    /// </summary>
    IEditGuildOnboardingAction Edit();
}
