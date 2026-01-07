using DiscoSdk.Rest;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Base implementation for making HTTP requests to the Discord REST API.
/// </summary>
public class DiscordRestClientBase : IDisposable, IDiscordRestClientBase
{
    private readonly HttpClient _http = new();
    private readonly string _botToken;
    private readonly JsonSerializerOptions _json;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscordRestClientBase"/> class.
    /// </summary>
    /// <param name="botToken">The bot token for authentication.</param>
    /// <param name="apiUri">The base URI of the Discord API.</param>
    /// <exception cref="ArgumentException">Thrown when the bot token is null or whitespace.</exception>
    public DiscordRestClientBase(string botToken, Uri apiUri)
    {
        if (string.IsNullOrWhiteSpace(botToken))
            throw new ArgumentException("Bot token is required.", nameof(botToken));

        _botToken = botToken;

        _http.BaseAddress = apiUri;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd($"{DeviceInfo.SdkName}/1.0");
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", _botToken);

        _json = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Sends a JSON request to the Discord API and deserializes the response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="path">The API endpoint path.</param>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="body">The request body object to serialize, or null.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains the deserialized response.</returns>
    /// <exception cref="DiscordApiException">Thrown when the API request fails.</exception>
    public async Task<T> SendJsonAsync<T>(string path, HttpMethod method, object? body, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(method, path);

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, _json);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        using var res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

        if (res.IsSuccessStatusCode)
        {
            if (res.StatusCode == HttpStatusCode.NoContent)
                return default!;

            await using var stream = await res.Content.ReadAsStreamAsync(ct);
            var data = await JsonSerializer.DeserializeAsync<T>(stream, _json, ct);
            return data ?? throw new DiscordApiException("Discord API returned empty JSON.", res.StatusCode, null);
        }

        var error = await TryReadDiscordErrorAsync(res, ct);
        throw new DiscordApiException(
            error?.Message ?? $"Discord API error ({(int)res.StatusCode} {res.ReasonPhrase}).",
            res.StatusCode,
            error?.Code);
    }

    public async Task SendJsonAsync(string path, HttpMethod method, object? body, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(method, path);

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, _json);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        using var res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

        if (res.IsSuccessStatusCode || res.StatusCode == HttpStatusCode.NoContent)
            return;

        var error = await TryReadDiscordErrorAsync(res, ct);
        throw new DiscordApiException(
            error?.Message ?? $"Discord API error ({(int)res.StatusCode} {res.ReasonPhrase}).",
            res.StatusCode,
            error?.Code);
    }

    /// <summary>
    /// Sends a request to the Discord API that expects no content in the response.
    /// </summary>
    /// <param name="path">The API endpoint path.</param>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="DiscordApiException">Thrown when the API request fails.</exception>
    public async Task SendNoContentAsync(string path, HttpMethod method, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(method, path);
        using var res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

        if (res.IsSuccessStatusCode)
            return;

        var error = await TryReadDiscordErrorAsync(res, ct);
        throw new DiscordApiException(
            error?.Message ?? $"Discord API error ({(int)res.StatusCode} {res.ReasonPhrase}).",
            res.StatusCode,
            error?.Code);
    }

    private async Task<DiscordErrorResponse?> TryReadDiscordErrorAsync(HttpResponseMessage res, CancellationToken ct)
    {
        try
        {
            var raw = await res.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            return JsonSerializer.Deserialize<DiscordErrorResponse>(raw, _json);
        }
        catch
        {
            return null;
        }
    }

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _http.Dispose();

        _disposed = true;
    }

    ~DiscordRestClientBase()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
