using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Activities;

internal class WatchingActivity(string name)
{
	private string? _details;
	private string? _state;
	private string? _largeImageKey;
	private string? _largeImageText;
	private string? _smallImageKey;
	private string? _smallImageText;
	private DateTimeOffset? _startTimestamp;
	private DateTimeOffset? _endTimestamp;
	private ActivityButton[]? _buttons;

	public WatchingActivity SetDetails(string? details)
	{
		_details = details;
		return this;
	}

	public WatchingActivity SetState(string? state)
	{
		_state = state;
		return this;
	}

	public WatchingActivity SetLargeImage(string? largeImageKey)
	{
		_largeImageKey = largeImageKey;
		return this;
	}

	public WatchingActivity SetLargeImageText(string? largeImageText)
	{
		_largeImageText = largeImageText;
		return this;
	}

	public WatchingActivity SetSmallImage(string? smallImageKey)
	{
		_smallImageKey = smallImageKey;
		return this;
	}

	public WatchingActivity SetSmallImageText(string? smallImageText)
	{
		_smallImageText = smallImageText;
		return this;
	}

	public WatchingActivity SetStartTimestamp(DateTimeOffset? startTimestamp)
	{
		_startTimestamp = startTimestamp;
		return this;
	}

	public WatchingActivity SetEndTimestamp(DateTimeOffset? endTimestamp)
	{
		_endTimestamp = endTimestamp;
		return this;
	}

	public WatchingActivity SetButtons(params ActivityButton[] buttons)
	{
		if (buttons.Length > 2)
			throw new ArgumentException("Maximum of 2 buttons allowed.", nameof(buttons));

		_buttons = buttons.Length > 0 ? buttons : null;
		return this;
	}

	public Activity Build()
	{
		var activity = new Activity
		{
			Name = name,
			Type = ActivityType.Watching,
			Details = _details,
			State = _state
		};

		if (_startTimestamp.HasValue || _endTimestamp.HasValue)
		{
			activity.Timestamps = new ActivityTimestamps
			{
				Start = _startTimestamp?.ToUnixTimeMilliseconds(),
				End = _endTimestamp?.ToUnixTimeMilliseconds()
			};
		}

		if (_largeImageKey != null || _smallImageKey != null || _largeImageText != null || _smallImageText != null)
		{
			activity.Assets = new ActivityAssets
			{
				LargeImage = _largeImageKey,
				LargeText = _largeImageText,
				SmallImage = _smallImageKey,
				SmallText = _smallImageText
			};
		}

		if (_buttons != null && _buttons.Length > 0)
		{
			activity.Buttons = _buttons;
		}

		return activity;
	}
}