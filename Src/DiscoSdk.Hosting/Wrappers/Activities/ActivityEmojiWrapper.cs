using DiscoSdk.Models;
using DiscoSdk.Models.Activities;

namespace DiscoSdk.Hosting.Wrappers.Activities;

internal sealed class ActivityEmojiWrapper(ActivityEmoji model) : IActivityEmoji
{
    private readonly ActivityEmoji _model = model ?? throw new ArgumentNullException(nameof(model));

    public string Name => _model.Name;
    public Snowflake? Id => _model.Id;
    public bool Animated => _model.Animated ?? false;
}
