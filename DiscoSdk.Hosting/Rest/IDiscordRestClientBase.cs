namespace DiscoSdk.Hosting.Rest;

public interface IDiscordRestClientBase : IDisposable
{
    Task<T> SendJsonAsync<T>(string path, HttpMethod method, object? body, CancellationToken ct);
    Task SendNoContentAsync(string path, HttpMethod method, CancellationToken ct);
}