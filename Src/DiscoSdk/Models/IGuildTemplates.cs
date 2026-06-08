using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Per-guild template surface — every operation that targets <c>/guilds/:id/templates*</c>.
/// </summary>
public interface IGuildTemplates
{
    /// <summary>Builds a deferred REST action that lists the templates owned by this guild.</summary>
    IRestAction<IReadOnlyList<IGuildTemplate>> GetAll();

    /// <summary>
    /// Builds a deferred REST action that creates a template from this guild's current configuration.
    /// </summary>
    IRestAction<IGuildTemplate> Create(string name, string? description = null);
}
