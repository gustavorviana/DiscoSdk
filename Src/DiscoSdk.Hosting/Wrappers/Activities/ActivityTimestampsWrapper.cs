using DiscoSdk.Models.Activities;

namespace DiscoSdk.Hosting.Wrappers.Activities;

internal sealed class ActivityTimestampsWrapper(ActivityTimestamps model) : IActivityTimestamps
{
    private readonly ActivityTimestamps _model = model ?? throw new ArgumentNullException(nameof(model));

    public DateTimeOffset? Start
        => _model.Start is long ms ? DateTimeOffset.FromUnixTimeMilliseconds(ms) : null;

    public DateTimeOffset? End
        => _model.End is long ms ? DateTimeOffset.FromUnixTimeMilliseconds(ms) : null;
}
