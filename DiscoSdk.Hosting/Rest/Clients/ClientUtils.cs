using DiscoSdk.Models.Messages;
using DiscoSdk.Rest;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DiscoSdk.Hosting.Rest.Clients;

internal static class ClientUtils
{
    public static async Task<TResponse> SendMultipartAsync<TResponse>(this IDiscordRestClient client,
        DiscordRoute route,
        HttpMethod method,
        object payload,
        IReadOnlyList<MessageFile> files,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(files);

        using var content = client.BuildMultipartContent(payload, files);
        return await client.SendAsync<TResponse>(route, method, content, cancellationToken);
    }

    public static MultipartFormDataContent BuildMultipartContent(this IDiscordRestClient client, object payload, IReadOnlyList<MessageFile> files)
    {
        var form = new MultipartFormDataContent();

        var payloadJson = JsonSerializer.Serialize(payload, client.JsonOptions);
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