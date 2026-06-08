using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Per-guild welcome-screen surface — every operation that targets
/// <c>/guilds/:id/welcome-screen*</c>.
/// </summary>
public interface IGuildWelcomeScreen
{
    /// <summary>Builds a deferred REST action that retrieves the welcome screen configuration.</summary>
    IRestAction<IWelcomeScreen> Get();

    /// <summary>Builds a deferred REST action that modifies the welcome screen configuration.</summary>
    IEditWelcomeScreenAction Edit();
}
