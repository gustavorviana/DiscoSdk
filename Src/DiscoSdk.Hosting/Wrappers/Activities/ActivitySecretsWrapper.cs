using DiscoSdk.Models.Activities;

namespace DiscoSdk.Hosting.Wrappers.Activities;

internal sealed class ActivitySecretsWrapper(ActivitySecrets model) : IActivitySecrets
{
    private readonly ActivitySecrets _model = model ?? throw new ArgumentNullException(nameof(model));

    public string? Join => _model.Join;
    public string? Spectate => _model.Spectate;
    public string? Match => _model.Match;
}
