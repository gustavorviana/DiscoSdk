using DiscoSdk.Rest;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest;

public class DiscordRestClientBase : IDisposable, IDiscordRestClientBase
{
    private readonly HttpClient _http = new();
    private readonly string _botToken;
    private readonly JsonSerializerOptions _json;
    private bool _disposed;

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
