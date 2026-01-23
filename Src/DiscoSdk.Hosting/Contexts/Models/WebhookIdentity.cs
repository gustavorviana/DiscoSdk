using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Models; 

internal class WebhookIdentity(Snowflake id, string token)
{
    public Snowflake Id => id;
    public string Token => token;

    public static WebhookIdentity? FromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        var segments = url.Trim('/').Split('/');
        if (segments.Length < 3)
            return null;

        // Expected: api / webhooks / {id} / {token}
        // But some clients may pass without "api" depending on base
        // So we search for "webhooks" and read the next two segments
        for (var i = 0; i < segments.Length - 2; i++)
        {
            if (!segments[i].Equals("webhooks", StringComparison.OrdinalIgnoreCase))
                continue;

            var id = segments[i + 1];
            var token = segments[i + 2];
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(token))
                return null;

            return new WebhookIdentity(Snowflake.Parse(id), token);
        }

        return null;
    }
}
