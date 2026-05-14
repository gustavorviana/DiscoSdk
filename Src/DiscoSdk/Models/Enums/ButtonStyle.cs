namespace DiscoSdk.Models.Enums;

/// <summary>
/// Visual style of a <see cref="Messages.Components.ButtonComponent"/>. Reference:
/// https://discord.com/developers/docs/components/reference#button-button-styles
/// </summary>
public enum ButtonStyle
{
    /// <summary>Blurple — most prominent. Requires <c>custom_id</c>.</summary>
    Primary = 1,

    /// <summary>Grey neutral. Requires <c>custom_id</c>.</summary>
    Secondary = 2,

    /// <summary>Green confirm. Requires <c>custom_id</c>.</summary>
    Success = 3,

    /// <summary>Red destructive. Requires <c>custom_id</c>.</summary>
    Danger = 4,

    /// <summary>Grey link that opens a URL in the browser. Requires <c>url</c>, no <c>custom_id</c>.</summary>
    Link = 5,

    /// <summary>Premium upsell button bound to a paid SKU. Requires <c>sku_id</c>, no <c>custom_id</c>, no <c>label</c>.</summary>
    Premium = 6,
}
