using DiscoSdk.Models.Activities;

namespace DiscoSdk.Hosting.Wrappers.Activities;

internal sealed class ActivityAssetsWrapper(ActivityAssets model) : IActivityAssets
{
    private readonly ActivityAssets _model = model ?? throw new ArgumentNullException(nameof(model));

    public string? LargeImage => _model.LargeImage;
    public string? LargeText => _model.LargeText;
    public string? SmallImage => _model.SmallImage;
    public string? SmallText => _model.SmallText;
}
