using DiscoSdk.Models.Messages;
using DiscoSdk.Rest;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DiscoSdk.Hosting.Rest.Clients;

internal static class ClientUtils
{
    public static Task<TResponse> SendMultipartAsync<TResponse>(this IDiscordRestClient client,
        DiscordRoute route,
        HttpMethod method,
        object payload,
        IReadOnlyList<MessageFile> files,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(files);

        var jsonOptions = client.JsonOptions;
        // Pass a factory so the multipart body can be rebuilt on rate-limit retries
        // (HttpContent streams are not re-readable once sent).
        Func<HttpContent> factory = () => BuildMultipartContent(payload, files, jsonOptions);
        return client.SendAsync<TResponse>(route, method, factory, cancellationToken);
    }

    private static MultipartFormDataContent BuildMultipartContent(object payload, IReadOnlyList<MessageFile> files, JsonSerializerOptions jsonOptions)
    {
        var form = new MultipartFormDataContent();

        var payloadJson = JsonSerializer.Serialize(payload, jsonOptions);
        var payloadContent = new StringContent(payloadJson, Encoding.UTF8, "application/json");
        form.Add(payloadContent, "payload_json");

        for (var i = 0; i < files.Count; i++)
        {
            var f = files[i];

            var fileContent = new ByteArrayContent(f.Buffer);
            if (!string.IsNullOrWhiteSpace(f.ContentType))
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(f.ContentType);

            form.Add(fileContent, $"files[{i}]", f.FileName);
        }

        return form;
    }
}
