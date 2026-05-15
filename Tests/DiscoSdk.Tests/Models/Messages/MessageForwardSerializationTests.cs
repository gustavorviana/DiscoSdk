using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using DiscoSdk.Models.Messages;
using System.Text.Json;

namespace DiscoSdk.Tests.Models.Messages;

/// <summary>
/// JSON roundtrip tests for the Forward message-reference type and the message_snapshots
/// payload Discord attaches to forwarded messages.
/// </summary>
public class MessageForwardSerializationTests
{
	private static readonly JsonSerializerOptions Options = DiscoJson.Create();

	[Fact]
	public void MessageReference_ForwardType_SerializesAsIntegerOne()
	{
		var reference = new MessageReference
		{
			Type = MessageReferenceType.Forward,
			MessageId = "100",
			ChannelId = "200",
			GuildId = "300",
		};

		var json = JsonSerializer.Serialize(reference, Options);

		Assert.Contains("\"type\":1", json);
		Assert.Contains("\"message_id\":\"100\"", json);
		Assert.Contains("\"channel_id\":\"200\"", json);
		Assert.Contains("\"guild_id\":\"300\"", json);
	}

	[Fact]
	public void MessageReference_DefaultType_OmittedOnWire()
	{
		var reference = new MessageReference
		{
			Type = null,
			MessageId = "100",
		};

		var json = JsonSerializer.Serialize(reference, Options);

		// Default ignores when writing null, and historical Discord replies don't include type.
		Assert.DoesNotContain("\"type\"", json);
		Assert.Contains("\"message_id\":\"100\"", json);
	}

	[Fact]
	public void MessageReference_DeserializesTypeFromPayload()
	{
		const string payload = """
		{
		  "type": 1,
		  "message_id": "100",
		  "channel_id": "200",
		  "guild_id": "300"
		}
		""";

		var reference = JsonSerializer.Deserialize<MessageReference>(payload, Options)!;

		Assert.Equal(MessageReferenceType.Forward, reference.Type);
		Assert.Equal("100", reference.MessageId);
	}

	[Fact]
	public void Message_WithForwardSnapshots_DeserializesPayloadFromDiscord()
	{
		const string payload = """
		{
		  "id": "555",
		  "channel_id": "200",
		  "guild_id": "300",
		  "author": { "id": "1", "username": "bot" },
		  "content": "",
		  "timestamp": "2024-01-01T00:00:00+00:00",
		  "type": 0,
		  "message_reference": {
		    "type": 1,
		    "message_id": "100",
		    "channel_id": "999",
		    "guild_id": "300"
		  },
		  "message_snapshots": [
		    {
		      "message": {
		        "type": 0,
		        "content": "Hello from the source channel.",
		        "embeds": [],
		        "attachments": [],
		        "timestamp": "2023-12-31T23:00:00+00:00",
		        "flags": 0,
		        "mentions": [],
		        "mention_roles": []
		      }
		    }
		  ]
		}
		""";

		var message = JsonSerializer.Deserialize<Message>(payload, Options)!;

		Assert.NotNull(message.MessageReference);
		Assert.Equal(MessageReferenceType.Forward, message.MessageReference!.Type);
		Assert.Equal("100", message.MessageReference.MessageId);

		Assert.NotNull(message.MessageSnapshots);
		Assert.Single(message.MessageSnapshots!);

		var snapshot = message.MessageSnapshots![0];
		Assert.NotNull(snapshot.Message);
		Assert.Equal("Hello from the source channel.", snapshot.Message.Content);
		Assert.Equal(MessageType.Default, snapshot.Message.Type);
		Assert.Empty(snapshot.Message.Embeds);
		Assert.Empty(snapshot.Message.Attachments);
	}

	[Fact]
	public void Message_WithoutForwardSnapshots_HasNullField()
	{
		const string payload = """
		{
		  "id": "555",
		  "channel_id": "200",
		  "author": { "id": "1", "username": "bot" },
		  "content": "Hello",
		  "timestamp": "2024-01-01T00:00:00+00:00",
		  "type": 0
		}
		""";

		var message = JsonSerializer.Deserialize<Message>(payload, Options)!;

		Assert.Null(message.MessageSnapshots);
	}

	[Theory]
	[InlineData(MessageReferenceType.Default, 0)]
	[InlineData(MessageReferenceType.Forward, 1)]
	public void MessageReferenceType_MatchesDiscordWireValue(MessageReferenceType type, int expected)
	{
		Assert.Equal(expected, (int)type);
	}
}
