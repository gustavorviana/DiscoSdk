using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Activities;

public class StreamingActivity(string name, string url) : IActivity
{
	private string? _details;
	private string? _state;

	public StreamingActivity SetDetails(string? details)
	{
		_details = details;
		return this;
	}

	public StreamingActivity SetState(string? state)
	{
		_state = state;
		return this;
	}

	public Activity Build()
	{
		return new Activity
		{
			Name = name,
			Type = ActivityType.Streaming,
			Url = url,
			Details = _details,
			State = _state
		};
	}
}