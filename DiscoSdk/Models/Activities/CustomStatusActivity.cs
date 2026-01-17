using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Activities;

internal class CustomStatusActivity(string? state)
{
	private string? _emojiName;
	private Snowflake? _emojiId;
	private bool _emojiAnimated;

    public CustomStatusActivity SetEmoji(IEmoji emoji)
    {
		_emojiName = emoji.Name;
		_emojiId = emoji.Id;
		_emojiAnimated = emoji.IsAnimated;
		return this;
    }

    public Activity Build()
	{
		var activity = new Activity
		{
			Type = ActivityType.Custom,
			State = state
		};

		if (_emojiName != null)
		{
			activity.Emoji = new ActivityEmoji
			{
				Name = _emojiName,
				Id = _emojiId,
				Animated = _emojiId.HasValue ? _emojiAnimated : null
			};
		}

		return activity;
	}
}