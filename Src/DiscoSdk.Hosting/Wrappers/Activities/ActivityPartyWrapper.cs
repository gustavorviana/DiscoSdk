using DiscoSdk.Models.Activities;

namespace DiscoSdk.Hosting.Wrappers.Activities;

internal sealed class ActivityPartyWrapper(ActivityParty model) : IActivityParty
{
    private readonly ActivityParty _model = model ?? throw new ArgumentNullException(nameof(model));

    public string? Id => _model.Id;

    public int? CurrentSize
        => _model.Size is { Length: >= 1 } size ? size[0] : null;

    public int? MaxSize
        => _model.Size is { Length: >= 2 } size ? size[1] : null;
}
