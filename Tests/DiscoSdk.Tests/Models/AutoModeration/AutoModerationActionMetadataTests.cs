using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json;

namespace DiscoSdk.Tests.Models.AutoModeration;

public class AutoModerationActionMetadataTests
{
	private static readonly JsonSerializerOptions Options = DiscoJson.Create();

	[Fact]
	public void BlockMessage_RoundtripsCustomMessage()
	{
		var action = new AutoModerationAction
		{
			Type = AutoModerationActionType.BlockMessage,
			Metadata = new AutoModerationActionMetadata { CustomMessage = "Watch your language." }
		};

		var json = JsonSerializer.Serialize(action, Options);

		Assert.Contains("\"type\":1", json);
		Assert.Contains("\"custom_message\":\"Watch your language.\"", json);
		Assert.DoesNotContain("channel_id", json);
		Assert.DoesNotContain("duration_seconds", json);

		var deserialized = JsonSerializer.Deserialize<AutoModerationAction>(json, Options)!;
		Assert.Equal(AutoModerationActionType.BlockMessage, deserialized.Type);
		Assert.Equal("Watch your language.", deserialized.Metadata!.CustomMessage);
	}

	[Fact]
	public void BlockMessage_WithoutMetadata_SerializesWithoutMetadataField()
	{
		var action = new AutoModerationAction { Type = AutoModerationActionType.BlockMessage };

		var json = JsonSerializer.Serialize(action, Options);

		Assert.Equal("{\"type\":1}", json);
	}

	[Fact]
	public void SendAlertMessage_RoundtripsChannelId()
	{
		var action = new AutoModerationAction
		{
			Type = AutoModerationActionType.SendAlertMessage,
			Metadata = new AutoModerationActionMetadata { ChannelId = new Snowflake(987654321) }
		};

		var json = JsonSerializer.Serialize(action, Options);

		Assert.Contains("\"type\":2", json);
		Assert.Contains("\"channel_id\":\"987654321\"", json);
		Assert.DoesNotContain("duration_seconds", json);
		Assert.DoesNotContain("custom_message", json);

		var deserialized = JsonSerializer.Deserialize<AutoModerationAction>(json, Options)!;
		Assert.Equal(AutoModerationActionType.SendAlertMessage, deserialized.Type);
		Assert.Equal(new Snowflake(987654321), deserialized.Metadata!.ChannelId);
	}

	[Fact]
	public void Timeout_RoundtripsDurationSeconds()
	{
		var action = new AutoModerationAction
		{
			Type = AutoModerationActionType.Timeout,
			Metadata = new AutoModerationActionMetadata { DurationSeconds = 2419200 }
		};

		var json = JsonSerializer.Serialize(action, Options);

		Assert.Contains("\"type\":3", json);
		Assert.Contains("\"duration_seconds\":2419200", json);
		Assert.DoesNotContain("channel_id", json);
		Assert.DoesNotContain("custom_message", json);

		var deserialized = JsonSerializer.Deserialize<AutoModerationAction>(json, Options)!;
		Assert.Equal(AutoModerationActionType.Timeout, deserialized.Type);
		Assert.Equal(2419200, deserialized.Metadata!.DurationSeconds);
	}

	[Fact]
	public void BlockMemberInteraction_SerializesWithoutMetadataFields()
	{
		var action = new AutoModerationAction { Type = AutoModerationActionType.BlockMemberInteraction };

		var json = JsonSerializer.Serialize(action, Options);

		Assert.Equal("{\"type\":4}", json);

		var deserialized = JsonSerializer.Deserialize<AutoModerationAction>(json, Options)!;
		Assert.Equal(AutoModerationActionType.BlockMemberInteraction, deserialized.Type);
		Assert.Null(deserialized.Metadata);
	}

	[Theory]
	[InlineData(AutoModerationActionType.BlockMessage, 1)]
	[InlineData(AutoModerationActionType.SendAlertMessage, 2)]
	[InlineData(AutoModerationActionType.Timeout, 3)]
	[InlineData(AutoModerationActionType.BlockMemberInteraction, 4)]
	public void ActionType_MatchesDiscordEnumValue(AutoModerationActionType type, int expected)
	{
		Assert.Equal(expected, (int)type);
	}

	[Fact]
	public void RuleWithMultipleActions_DeserializesFromDiscordPayload()
	{
		const string payload = """
		{
			"id": "1",
			"guild_id": "2",
			"name": "rule",
			"creator_id": "3",
			"event_type": 1,
			"trigger_type": 1,
			"actions": [
				{ "type": 1, "metadata": { "custom_message": "blocked" } },
				{ "type": 2, "metadata": { "channel_id": "555" } },
				{ "type": 3, "metadata": { "duration_seconds": 60 } },
				{ "type": 4 }
			],
			"enabled": true,
			"exempt_roles": [],
			"exempt_channels": []
		}
		""";

		var rule = JsonSerializer.Deserialize<AutoModerationRule>(payload, Options)!;

		Assert.Equal(4, rule.Actions.Length);
		Assert.Equal(AutoModerationActionType.BlockMessage, rule.Actions[0].Type);
		Assert.Equal("blocked", rule.Actions[0].Metadata!.CustomMessage);
		Assert.Equal(AutoModerationActionType.SendAlertMessage, rule.Actions[1].Type);
		Assert.Equal(new Snowflake(555), rule.Actions[1].Metadata!.ChannelId);
		Assert.Equal(AutoModerationActionType.Timeout, rule.Actions[2].Type);
		Assert.Equal(60, rule.Actions[2].Metadata!.DurationSeconds);
		Assert.Equal(AutoModerationActionType.BlockMemberInteraction, rule.Actions[3].Type);
		Assert.Null(rule.Actions[3].Metadata);
	}
}
