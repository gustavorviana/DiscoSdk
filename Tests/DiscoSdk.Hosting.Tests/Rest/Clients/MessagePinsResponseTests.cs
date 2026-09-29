using DiscoSdk.Models;
using DiscoSdk.Models.JsonConverters;
using DiscoSdk.Models.Messages;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class MessagePinsResponseTests
{
	[Fact]
	public void Deserialize_MapsDiscordPinsPayload()
	{
		const string json = """
			{
			  "items": [
			    {
			      "pinned_at": "2026-09-01T12:00:00.000000+00:00",
			      "message": { "id": "300", "channel_id": "200", "content": "hi", "timestamp": "2026-08-31T10:00:00.000000+00:00" }
			    }
			  ],
			  "has_more": true
			}
			""";

		var response = JsonSerializer.Deserialize<MessagePinsResponse>(json, DiscoJson.Create());

		Assert.NotNull(response);
		Assert.True(response!.HasMore);
		var pin = Assert.Single(response.Items);
		Assert.Equal(new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero), pin.PinnedAt);
		Assert.Equal(new Snowflake(300), pin.Message.Id);
	}
}
