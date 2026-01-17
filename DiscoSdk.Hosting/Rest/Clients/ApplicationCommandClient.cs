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
internal sealed class ApplicationCommandClient(IDiscordRestClientBase client)
{
    /// <summary>
    /// Registers global application commands, replacing all existing global commands.
    /// </summary>
    /// <param name="applicationId">The application ID of the bot.</param>
    /// <param name="commands">The commands to register.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains the registered commands.</returns>
    public async Task<List<ApplicationCommand>> RegisterGlobalCommandsAsync(string applicationId, List<ApplicationCommand> commands, CancellationToken ct = default)
    {
        var path = $"applications/{applicationId}/commands";
        return await client.SendAsync<List<ApplicationCommand>>(path, HttpMethod.Put, commands, ct);
    }

    /// <summary>
    /// Registers guild-specific application commands, replacing all existing guild commands.
    /// </summary>
    /// <param name="applicationId">The application ID of the bot.</param>
    /// <param name="guildId">The guild ID where to register the commands.</param>
    /// <param name="commands">The commands to register.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains the registered commands.</returns>
    public async Task<List<ApplicationCommand>> RegisterGuildCommandsAsync(string applicationId, Snowflake guildId, List<ApplicationCommand> commands, CancellationToken ct = default)
    {
        var path = $"applications/{applicationId}/guilds/{guildId}/commands";
        return await client.SendAsync<List<ApplicationCommand>>(path, HttpMethod.Put, commands, ct);
    }

    /// <summary>
    /// Gets all global application commands.
    /// </summary>
    /// <param name="applicationId">The application ID of the bot.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains all global commands.</returns>
    public async Task<List<ApplicationCommand>> GetGlobalCommandsAsync(string applicationId, CancellationToken ct = default)
    {
        var path = $"applications/{applicationId}/commands";
        return await client.SendAsync<List<ApplicationCommand>>(path, HttpMethod.Get, null, ct);
    }

    /// <summary>
    /// Gets all guild-specific application commands.
    /// </summary>
    /// <param name="applicationId">The application ID of the bot.</param>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains all guild commands.</returns>
    public async Task<List<ApplicationCommand>> GetGuildCommandsAsync(string applicationId, Snowflake guildId, CancellationToken ct = default)
    {
        var path = $"applications/{applicationId}/guilds/{guildId}/commands";
        return await client.SendAsync<List<ApplicationCommand>>(path, HttpMethod.Get, null, ct);
    }

    /// <summary>
    /// Deletes a global application command.
    /// </summary>
    /// <param name="applicationId">The application ID of the bot.</param>
    /// <param name="commandId">The ID of the command to delete.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteGlobalCommandAsync(string applicationId, string commandId, CancellationToken ct = default)
    {
        var path = $"applications/{applicationId}/commands/{commandId}";
        await client.SendAsync(path, HttpMethod.Delete, ct);
    }

    /// <summary>
    /// Deletes a guild-specific application command.
    /// </summary>
    /// <param name="applicationId">The application ID of the bot.</param>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="commandId">The ID of the command to delete.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteGuildCommandAsync(string applicationId, string guildId, string commandId, CancellationToken ct = default)
    {
        var path = $"applications/{applicationId}/guilds/{guildId}/commands/{commandId}";
        await client.SendAsync(path, HttpMethod.Delete, ct);
    }
}

