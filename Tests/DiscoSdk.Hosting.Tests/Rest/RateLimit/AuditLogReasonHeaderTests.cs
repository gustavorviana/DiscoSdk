using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Hosting.Tests.Rest;
using DiscoSdk.Rest;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Rest.RateLimit;

/// <summary>
/// Verifies that <see cref="DiscordRestClient.SendWithReasonAsync"/> attaches the
/// <c>X-Audit-Log-Reason</c> header — URL-encoded and capped at 512 chars — to the outgoing HTTP
/// request, and that the plain <see cref="DiscordRestClient.SendAsync(DiscordRoute, HttpMethod, object?, CancellationToken)"/>
/// path never sets the header.
/// </summary>
public class AuditLogReasonHeaderTests
{
    private static DiscordRestClient NewClient(StubHttpMessageHandler handler)
    {
        // The shared SocketsHttpHandler in production is bypassed by tests via the
        // HttpMessageInvoker injected through the handler stub; here we drive the public
        // surface, so we still need a token + base URI.
        var client = new DiscordRestClient(
            botToken: "test-token",
            apiUri: new Uri("https://discord.local/"),
            jsonOptions: new JsonSerializerOptions(),
            logger: NullLogger<DiscordRestClient>.Instance,
            timeProvider: TimeProvider.System);

        // Swap the internal HttpClient. We can't reach the private field directly, so we
        // instead exercise the auditable path through a wrapping HttpClient at the test
        // boundary: the test verifies that the OUTGOING HttpRequestMessage that reaches the
        // stubbed handler carries the right headers.
        SwapHandler(client, handler);

        return client;
    }

    private static void SwapHandler(DiscordRestClient client, HttpMessageHandler handler)
    {
        // Reflect into the private _http field — the production class is sealed-ish but
        // internal, and the test assembly has InternalsVisibleTo so we use BindingFlags.
        var field = typeof(DiscordRestClient).GetField("_http",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(field);

        var existing = (HttpClient)field!.GetValue(client)!;
        existing.Dispose();
        field.SetValue(client, new HttpClient(handler) { BaseAddress = new Uri("https://discord.local/") });
    }

    [Fact]
    public async Task SendWithReasonAsync_AttachesUrlEncodedHeaderAsync()
    {
        HttpRequestMessage? captured = null;
        var handler = new StubHttpMessageHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        });
        using var client = NewClient(handler);

        await client.SendWithReasonAsync(
            new DiscordRoute("guilds/{guild_id}/bans/{user_id}", 100UL, 42UL),
            HttpMethod.Put,
            new { delete_message_days = 0 },
            "Spamming in #general — non-ASCII: ção",
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.True(captured!.Headers.TryGetValues(AuditLogReason.HeaderName, out var values));
        var value = Assert.Single(values!);
        // URL-encoded — non-ASCII characters round-trip
        Assert.Equal(Uri.EscapeDataString("Spamming in #general — non-ASCII: ção"), value);
    }

    [Fact]
    public async Task SendWithReasonAsync_TruncatesAt512CharsAsync()
    {
        HttpRequestMessage? captured = null;
        var handler = new StubHttpMessageHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        });
        using var client = NewClient(handler);

        var oversized = new string('x', 600);
        await client.SendWithReasonAsync(
            new DiscordRoute("guilds/{guild_id}", 100UL),
            HttpMethod.Delete,
            body: null,
            oversized,
            CancellationToken.None);

        Assert.NotNull(captured);
        var headerValue = captured!.Headers.GetValues(AuditLogReason.HeaderName).Single();
        // After URL-encoding 'x' stays 'x' so the encoded length equals the trimmed source length (512).
        Assert.Equal(512, headerValue.Length);
    }

    [Fact]
    public async Task PlainSendAsync_DoesNotAttachHeaderAsync()
    {
        HttpRequestMessage? captured = null;
        var handler = new StubHttpMessageHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        });
        using var client = NewClient(handler);

        await client.SendAsync(
            new DiscordRoute("guilds/{guild_id}/bans/{user_id}", 100UL, 42UL),
            HttpMethod.Put,
            new { delete_message_days = 0 },
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.False(captured!.Headers.Contains(AuditLogReason.HeaderName));
    }
}
