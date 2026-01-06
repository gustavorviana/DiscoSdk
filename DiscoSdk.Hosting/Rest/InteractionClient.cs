using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest;

/// <summary>
/// Client for responding to Discord interactions (slash commands, buttons, etc.).
/// </summary>
public class InteractionClient
{
    private readonly IDiscordRestClientBase _client;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionClient"/> class.
    /// </summary>
    /// <param name="client">The REST client base to use for requests.</param>
    public InteractionClient(IDiscordRestClientBase client)
    {
        _client = client;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Responds to an interaction with a message.
    /// </summary>
    /// <param name="interactionId">The ID of the interaction.</param>
    /// <param name="interactionToken">The token of the interaction.</param>
    /// <param name="content">The message content.</param>
    /// <param name="ephemeral">Whether the response should be ephemeral (only visible to the user).</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RespondAsync(string interactionId, string interactionToken, string content, bool ephemeral = false, CancellationToken ct = default)
    {
        var response = new
        {
            type = 4, // CHANNEL_MESSAGE_WITH_SOURCE
            data = new
            {
                content,
                flags = ephemeral ? 64 : (int?)null // EPHEMERAL flag
            }
        };

        var path = $"interactions/{interactionId}/{interactionToken}/callback";

        await _client.SendJsonAsync<object>(path, HttpMethod.Post, response, ct);
    }

    /// <summary>
    /// Defers the response to an interaction (acknowledges it and allows up to 15 minutes to respond).
    /// </summary>
    /// <param name="interactionId">The ID of the interaction.</param>
    /// <param name="interactionToken">The token of the interaction.</param>
    /// <param name="ephemeral">Whether the response should be ephemeral (only visible to the user).</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeferAsync(string interactionId, string interactionToken, bool ephemeral = false, CancellationToken ct = default)
    {
        var response = new
        {
            type = 5, // DEFERRED_CHANNEL_MESSAGE_WITH_SOURCE
            data = ephemeral ? new { flags = 64 } : null // EPHEMERAL flag
        };

        var path = $"interactions/{interactionId}/{interactionToken}/callback";

        await _client.SendJsonAsync<object>(path, HttpMethod.Post, response, ct);
    }

    /// <summary>
    /// Follows up on a deferred interaction with a message.
    /// Use this method after calling <see cref="DeferAsync"/> to send the actual response.
    /// Note: The interaction token is valid for 15 minutes after the initial interaction.
    /// </summary>
    /// <param name="applicationId">The application ID of the bot (use interaction.ApplicationId, not interaction.Id).</param>
    /// <param name="interactionToken">The token of the interaction.</param>
    /// <param name="content">The message content.</param>
    /// <param name="ephemeral">Whether the response should be ephemeral (only visible to the user).</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task FollowUpAsync(string applicationId, string interactionToken, string content, bool ephemeral = false, CancellationToken ct = default)
    {
        var response = new
        {
            content,
            flags = ephemeral ? 64 : (int?)null // EPHEMERAL flag
        };

        var path = $"webhooks/{applicationId}/{interactionToken}";
        await _client.SendJsonAsync<object>(path, HttpMethod.Post, response, ct);
    }
}

