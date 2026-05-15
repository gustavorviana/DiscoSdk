using DiscoSdk.Commands;
using DiscoSdk.Commands.Localization;
using DiscoSdk.Hosting.Commands.Localization;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
using Microsoft.Extensions.Logging;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Internal implementation of <see cref="ICommandUpdateFactory"/> (and therefore also
/// <see cref="IGuildCommandUpdateFactory"/>). Resolved by DI: takes <see cref="IDiscordClient"/>
/// (via accessor, late-bound) and reads <see cref="IDiscordClient.ApplicationId"/> at the moment
/// a scope is opened — by then the gateway READY phase has populated it. Pre-READY callers get
/// a clear <see cref="InvalidOperationException"/>.
/// </summary>
internal sealed class CommandUpdateFactory : ICommandUpdateFactory
{
    private readonly IDiscordClient _discord;
    private readonly ApplicationCommandClient _client;
    private readonly SlashCommandLocalizer? _slashLocalizer;
    private readonly ContextCommandLocalizer? _contextLocalizer;
    private readonly ICommandRegistry? _registry;
    private readonly ILogger? _logger;

    public CommandUpdateFactory(
        IDiscordClient discord,
        IDiscordRestClient rest,
        ICommandLocalizationProvider? localizationProvider = null,
        IContextCommandLocalizationProvider? contextLocalizationProvider = null,
        ICommandRegistry? registry = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(discord);
        ArgumentNullException.ThrowIfNull(rest);
        _discord = discord;
        _client = new ApplicationCommandClient(rest);
        _logger = logger;
        _slashLocalizer = localizationProvider is null ? null : new SlashCommandLocalizer(localizationProvider, logger);
        _contextLocalizer = contextLocalizationProvider is null ? null : new ContextCommandLocalizer(contextLocalizationProvider, logger);
        _registry = registry;
    }

    public ICommandUpdateScope OpenForGlobal(bool overwrite)
        => new CommandUpdateScope(_client, RequireApplicationId(), guildId: null, overwrite, _slashLocalizer, _contextLocalizer, _registry, _logger);

    public ICommandUpdateScope OpenForGuild(Snowflake guildId, bool overwrite)
        => new CommandUpdateScope(_client, RequireApplicationId(), guildId, overwrite, _slashLocalizer, _contextLocalizer, _registry, _logger);

    private Snowflake RequireApplicationId()
        => _discord.ApplicationId
            ?? throw new InvalidOperationException(
                "Cannot open a command update scope before the gateway READY phase — ApplicationId is not yet available.");
}
