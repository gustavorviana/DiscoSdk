using DiscoSdk.Models;
using DiscoSdk.Models.Activities;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Wrappers.Activities;

/// <summary>
/// Read-only wrapper around the gateway <see cref="Activity"/> POCO. Exposes
/// <see cref="IActivity"/> so consumers can inspect cached presence data without reaching the
/// mutable JSON model. Sub-objects are lazily wrapped on first access so repeated reads of the
/// same activity reuse the same interface instances.
/// </summary>
internal sealed class ActivityWrapper(Activity model) : IActivity
{
    private readonly Activity _model = model ?? throw new ArgumentNullException(nameof(model));

    private IActivityTimestamps? _timestamps;
    private IActivityEmoji? _emoji;
    private IActivityParty? _party;
    private IActivityAssets? _assets;
    private IActivitySecrets? _secrets;

    public string Name => _model.Name;
    public ActivityType Type => _model.Type;
    public string? Url => _model.Url;

    public DateTimeOffset? CreatedAt
        => _model.CreatedAt is long ms ? DateTimeOffset.FromUnixTimeMilliseconds(ms) : null;

    public IActivityTimestamps? Timestamps
        => _model.Timestamps is null ? null : _timestamps ??= new ActivityTimestampsWrapper(_model.Timestamps);

    public Snowflake? ApplicationId => _model.ApplicationId;
    public string? Details => _model.Details;
    public string? State => _model.State;

    public IActivityEmoji? Emoji
        => _model.Emoji is null ? null : _emoji ??= new ActivityEmojiWrapper(_model.Emoji);

    public IActivityParty? Party
        => _model.Party is null ? null : _party ??= new ActivityPartyWrapper(_model.Party);

    public IActivityAssets? Assets
        => _model.Assets is null ? null : _assets ??= new ActivityAssetsWrapper(_model.Assets);

    public IActivitySecrets? Secrets
        => _model.Secrets is null ? null : _secrets ??= new ActivitySecretsWrapper(_model.Secrets);

    public bool Instance => _model.Instance ?? false;
    public ActivityFlag Flags => (ActivityFlag)(_model.Flags ?? 0);
    public string[] Buttons => _model.Buttons ?? [];
}
