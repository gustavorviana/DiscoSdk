using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Activities;

public class ListeningActivity(string name) : IActivity
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

	public ListeningActivity SetDetails(string? details)
	{
		_details = details;
		return this;
	}

	public ListeningActivity SetState(string? state)
	{
		_state = state;
		return this;
	}

	public ListeningActivity SetLargeImage(string? largeImageKey)
	{
		_largeImageKey = largeImageKey;
		return this;
	}

	public ListeningActivity SetLargeImageText(string? largeImageText)
	{
		_largeImageText = largeImageText;
		return this;
	}

	public ListeningActivity SetSmallImage(string? smallImageKey)
	{
		_smallImageKey = smallImageKey;
		return this;
	}

	public ListeningActivity SetSmallImageText(string? smallImageText)
	{
		_smallImageText = smallImageText;
		return this;
	}

	public ListeningActivity SetStartTimestamp(DateTimeOffset? startTimestamp)
	{
		_startTimestamp = startTimestamp;
		return this;
	}

	public ListeningActivity SetEndTimestamp(DateTimeOffset? endTimestamp)
	{
		_endTimestamp = endTimestamp;
		return this;
	}

	public ListeningActivity SetButtons(params ActivityButton[] buttons)
	{
		if (buttons.Length > 2)
			throw new ArgumentException("Maximum of 2 buttons allowed.", nameof(buttons));

		_buttons = buttons.Length > 0 ? buttons : null;
		return this;
	}

	public ActivityUpdate Build()
	{
		var activity = new ActivityUpdate
		{
			Name = name,
			Type = ActivityType.Listening,
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
			activity.Buttons = _buttons;

		return activity;
	}
}