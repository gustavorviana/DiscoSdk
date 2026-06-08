using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Per-guild widget surface — every operation that targets <c>/guilds/:id/widget*</c>.
/// Suffix <c>Surface</c> avoids the name clash with the <see cref="IGuildWidget"/> data model.
/// </summary>
public interface IGuildWidgetSurface
{
    /// <summary>Whether the guild widget is enabled. Defaults to <c>false</c>.</summary>
    bool IsEnabled { get; }

    /// <summary>ID of the channel the widget points at, or <c>null</c> when not configured.</summary>
    Snowflake? ChannelId { get; }

    /// <summary>Builds a deferred REST action that retrieves the widget configuration.</summary>
    IRestAction<IGuildWidget> Get();

    /// <summary>Builds a deferred REST action that modifies the widget configuration.</summary>
    IEditGuildWidgetAction Edit();

    /// <summary>Builds a deferred REST action that retrieves the widget image stream.</summary>
    /// <param name="style">Optional image style (shield / banner1 / banner2 / banner3 / banner4).</param>
    IRestAction<Stream> GetImage(string? style = null);
}
