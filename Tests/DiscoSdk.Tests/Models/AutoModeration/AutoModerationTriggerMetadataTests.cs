using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json;

namespace DiscoSdk.Tests.Models.AutoModeration;

public class AutoModerationTriggerMetadataTests
{
	private static readonly JsonSerializerOptions Options = DiscoJson.Create();

	[Fact]
	public void Keyword_RoundtripsKeywordFilterRegexAndAllowList()
	{
		var metadata = new AutoModerationTriggerMetadata
		{
			KeywordFilter = ["badword", "*phrase*"],
			RegexPatterns = ["^block.*$"],
			AllowList = ["safeword"]
		};

		var json = JsonSerializer.Serialize(metadata, Options);

		Assert.Contains("\"keyword_filter\":[\"badword\",\"*phrase*\"]", json);
		Assert.Contains("\"regex_patterns\":[\"^block.*$\"]", json);
		Assert.Contains("\"allow_list\":[\"safeword\"]", json);
		Assert.DoesNotContain("presets", json);
		Assert.DoesNotContain("mention_total_limit", json);
		Assert.DoesNotContain("mention_raid_protection_enabled", json);

		var deserialized = JsonSerializer.Deserialize<AutoModerationTriggerMetadata>(json, Options)!;
		Assert.Equal(metadata.KeywordFilter, deserialized.KeywordFilter);
		Assert.Equal(metadata.RegexPatterns, deserialized.RegexPatterns);
		Assert.Equal(metadata.AllowList, deserialized.AllowList);
	}

	[Fact]
	public void Spam_SerializesAsEmptyObject()
	{
		var metadata = new AutoModerationTriggerMetadata();

		var json = JsonSerializer.Serialize(metadata, Options);

		Assert.Equal("{}", json);
	}

	[Fact]
	public void KeywordPreset_RoundtripsPresetsAndAllowList()
	{
		var metadata = new AutoModerationTriggerMetadata
		{
			Presets =
			[
				AutoModerationKeywordPresetType.Profanity,
				AutoModerationKeywordPresetType.SexualContent,
				AutoModerationKeywordPresetType.Slurs
			],
			AllowList = ["allowed"]
		};

		var json = JsonSerializer.Serialize(metadata, Options);

		Assert.Contains("\"presets\":[1,2,3]", json);
		Assert.Contains("\"allow_list\":[\"allowed\"]", json);
		Assert.DoesNotContain("keyword_filter", json);
		Assert.DoesNotContain("regex_patterns", json);

		var deserialized = JsonSerializer.Deserialize<AutoModerationTriggerMetadata>(json, Options)!;
		Assert.Equal(metadata.Presets, deserialized.Presets);
		Assert.Equal(metadata.AllowList, deserialized.AllowList);
	}

	[Fact]
	public void MentionSpam_RoundtripsMentionLimitAndRaidProtection()
	{
		var metadata = new AutoModerationTriggerMetadata
		{
			MentionTotalLimit = 5,
			MentionRaidProtectionEnabled = true
		};

		var json = JsonSerializer.Serialize(metadata, Options);

		Assert.Contains("\"mention_total_limit\":5", json);
		Assert.Contains("\"mention_raid_protection_enabled\":true", json);
		Assert.DoesNotContain("keyword_filter", json);
		Assert.DoesNotContain("presets", json);

		var deserialized = JsonSerializer.Deserialize<AutoModerationTriggerMetadata>(json, Options)!;
		Assert.Equal(5, deserialized.MentionTotalLimit);
		Assert.True(deserialized.MentionRaidProtectionEnabled);
	}

	[Fact]
	public void MemberProfile_RoundtripsKeywordFilterRegexAndAllowList()
	{
		var metadata = new AutoModerationTriggerMetadata
		{
			KeywordFilter = ["nickbad"],
			RegexPatterns = [".*scam.*"],
			AllowList = ["nickok"]
		};

		var json = JsonSerializer.Serialize(metadata, Options);

		Assert.Contains("\"keyword_filter\":[\"nickbad\"]", json);
		Assert.Contains("\"regex_patterns\":[\".*scam.*\"]", json);
		Assert.Contains("\"allow_list\":[\"nickok\"]", json);

		var deserialized = JsonSerializer.Deserialize<AutoModerationTriggerMetadata>(json, Options)!;
		Assert.Equal(metadata.KeywordFilter, deserialized.KeywordFilter);
		Assert.Equal(metadata.RegexPatterns, deserialized.RegexPatterns);
		Assert.Equal(metadata.AllowList, deserialized.AllowList);
	}

	[Theory]
	[InlineData(AutoModerationTriggerType.Keyword, 1)]
	[InlineData(AutoModerationTriggerType.Spam, 3)]
	[InlineData(AutoModerationTriggerType.KeywordPreset, 4)]
	[InlineData(AutoModerationTriggerType.MentionSpam, 5)]
	[InlineData(AutoModerationTriggerType.MemberProfile, 6)]
	public void TriggerType_MatchesDiscordEnumValue(AutoModerationTriggerType type, int expected)
	{
		Assert.Equal(expected, (int)type);
	}

	[Fact]
	public void RuleWithKeywordTrigger_DeserializesFromDiscordPayload()
	{
		const string payload = """
		{
			"id": "1",
			"guild_id": "2",
			"name": "rule",
			"creator_id": "3",
			"event_type": 1,
			"trigger_type": 1,
			"trigger_metadata": {
				"keyword_filter": ["x"],
				"regex_patterns": ["^y$"],
				"allow_list": ["z"]
			},
			"actions": [],
			"enabled": true,
			"exempt_roles": [],
			"exempt_channels": []
		}
		""";

		var rule = JsonSerializer.Deserialize<AutoModerationRule>(payload, Options)!;

		Assert.Equal(AutoModerationTriggerType.Keyword, rule.TriggerType);
		Assert.NotNull(rule.TriggerMetadata);
		Assert.Equal(["x"], rule.TriggerMetadata!.KeywordFilter);
		Assert.Equal(["^y$"], rule.TriggerMetadata.RegexPatterns);
		Assert.Equal(["z"], rule.TriggerMetadata.AllowList);
	}
}
