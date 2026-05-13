using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for registering and managing Discord application commands.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ApplicationCommandClient"/> class.
/// </remarks>
/// <param name="client">The REST client base to use for requests.</param>
internal sealed class ApplicationCommandClient(IDiscordRestClient client)
{
    /// <summary>
    /// Registers global application commands, replacing all existing global commands.
    /// </summary>
    /// <param name="applicationId">The application ID of the bot.</param>
    /// <param name="commands">The commands to register.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains the registered commands.</returns>
    public async Task<List<SlashCommand>> RegisterGlobalCommandsAsync(Snowflake applicationId, List<SlashCommand> commands, CancellationToken ct = default)
    {
        var route = new DiscordRoute("applications/{application_id}/commands", applicationId);
        return await client.SendAsync<List<SlashCommand>>(route, HttpMethod.Put, commands, ct);
    }

    /// <summary>
    /// Registers guild-specific application commands, replacing all existing guild commands.
    /// </summary>
    /// <param name="applicationId">The application ID of the bot.</param>
    /// <param name="guildId">The guild ID where to register the commands.</param>
    /// <param name="commands">The commands to register.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains the registered commands.</returns>
    public async Task<List<SlashCommand>> RegisterGuildCommandsAsync(Snowflake applicationId, Snowflake guildId, List<SlashCommand> commands, CancellationToken ct = default)
    {
        var route = new DiscordRoute("applications/{application_id}/guilds/{guild_id}/commands", applicationId, guildId);
        return await client.SendAsync<List<SlashCommand>>(route, HttpMethod.Put, commands, ct);
    }

    /// <summary>
    /// Gets all global application commands.
    /// </summary>
    /// <param name="applicationId">The application ID of the bot.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains all global commands.</returns>
    public async Task<List<SlashCommand>> GetGlobalCommandsAsync(Snowflake applicationId, CancellationToken ct = default)
    {
        var route = new DiscordRoute("applications/{application_id}/commands", applicationId);
        return await client.SendAsync<List<SlashCommand>>(route, HttpMethod.Get, null, ct);
    }

    /// <summary>
    /// Gets all guild-specific application commands.
    /// </summary>
    /// <param name="applicationId">The application ID of the bot.</param>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains all guild commands.</returns>
    public async Task<List<SlashCommand>> GetGuildCommandsAsync(Snowflake applicationId, Snowflake guildId, CancellationToken ct = default)
    {
        var route = new DiscordRoute("applications/{application_id}/guilds/{guild_id}/commands", applicationId, guildId);
        return await client.SendAsync<List<SlashCommand>>(route, HttpMethod.Get, null, ct);
    }

    /// <summary>
    /// Deletes a global application command.
    /// </summary>
    /// <param name="applicationId">The application ID of the bot.</param>
    /// <param name="commandId">The ID of the command to delete.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteGlobalCommandAsync(Snowflake applicationId, string commandId, CancellationToken ct = default)
    {
        var route = new DiscordRoute("applications/{application_id}/commands/{command_id}", applicationId, commandId);
        await client.SendAsync(route, HttpMethod.Delete, ct);
    }

    /// <summary>
    /// Deletes a guild-specific application command.
    /// </summary>
    /// <param name="applicationId">The application ID of the bot.</param>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="commandId">The ID of the command to delete.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteGuildCommandAsync(Snowflake applicationId, string guildId, string commandId, CancellationToken ct = default)
    {
        var route = new DiscordRoute("applications/{application_id}/guilds/{guild_id}/commands/{command_id}", applicationId, guildId, commandId);
        await client.SendAsync(route, HttpMethod.Delete, ct);
    }

    /// <summary>Gets a single global application command by ID.</summary>
    public Task<SlashCommand> GetGlobalCommandAsync(Snowflake applicationId, Snowflake commandId, CancellationToken ct = default)
    {
        var route = new DiscordRoute("applications/{application_id}/commands/{command_id}", applicationId, commandId);
        return client.SendAsync<SlashCommand>(route, HttpMethod.Get, null, ct);
    }

    /// <summary>Creates a single new global application command (does not bulk-overwrite).</summary>
    public Task<SlashCommand> CreateGlobalCommandAsync(Snowflake applicationId, SlashCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var route = new DiscordRoute("applications/{application_id}/commands", applicationId);
        return client.SendAsync<SlashCommand>(route, HttpMethod.Post, command, ct);
    }

    /// <summary>Edits an existing global application command. Only the supplied fields will be updated.</summary>
    public Task<SlashCommand> EditGlobalCommandAsync(Snowflake applicationId, Snowflake commandId, object request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var route = new DiscordRoute("applications/{application_id}/commands/{command_id}", applicationId, commandId);
        return client.SendAsync<SlashCommand>(route, HttpMethod.Patch, request, ct);
    }

    /// <summary>Gets a single guild-scoped application command by ID.</summary>
    public Task<SlashCommand> GetGuildCommandAsync(Snowflake applicationId, Snowflake guildId, Snowflake commandId, CancellationToken ct = default)
    {
        var route = new DiscordRoute("applications/{application_id}/guilds/{guild_id}/commands/{command_id}", applicationId, guildId, commandId);
        return client.SendAsync<SlashCommand>(route, HttpMethod.Get, null, ct);
    }

    /// <summary>Creates a single new guild-scoped application command (does not bulk-overwrite).</summary>
    public Task<SlashCommand> CreateGuildCommandAsync(Snowflake applicationId, Snowflake guildId, SlashCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var route = new DiscordRoute("applications/{application_id}/guilds/{guild_id}/commands", applicationId, guildId);
        return client.SendAsync<SlashCommand>(route, HttpMethod.Post, command, ct);
    }

    /// <summary>Edits an existing guild-scoped application command. Only the supplied fields will be updated.</summary>
    public Task<SlashCommand> EditGuildCommandAsync(Snowflake applicationId, Snowflake guildId, Snowflake commandId, object request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var route = new DiscordRoute("applications/{application_id}/guilds/{guild_id}/commands/{command_id}", applicationId, guildId, commandId);
        return client.SendAsync<SlashCommand>(route, HttpMethod.Patch, request, ct);
    }
}

