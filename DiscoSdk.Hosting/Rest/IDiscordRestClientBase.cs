namespace DiscoSdk.Hosting.Rest;

/// <summary>
/// Interface for making HTTP requests to the Discord REST API.
/// </summary>
public interface IDiscordRestClientBase : IDisposable
{
    /// <summary>
    /// Sends a JSON request to the Discord API and deserializes the response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="path">The API endpoint path.</param>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="body">The request body object to serialize, or null.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains the deserialized response.</returns>
    Task<T> SendJsonAsync<T>(string path, HttpMethod method, object? body, CancellationToken ct);

    /// <summary>
    /// Sends a JSON request to the Discord API and deserializes the response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="path">The API endpoint path.</param>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="body">The request body object to serialize, or null.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains the deserialized response.</returns>
    Task SendJsonAsync(string path, HttpMethod method, object? body, CancellationToken ct);

    /// <summary>
    /// Sends a request to the Discord API that expects no content in the response.
    /// </summary>
    /// <param name="path">The API endpoint path.</param>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendNoContentAsync(string path, HttpMethod method, CancellationToken ct);
}