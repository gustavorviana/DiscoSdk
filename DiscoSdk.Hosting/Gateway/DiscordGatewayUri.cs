using System.Text;

namespace DiscoSdk.Hosting.Gateway;

internal readonly struct DiscordGatewayUri(
    string @base = "wss://gateway.discord.gg/",
    int version = 10,
    string encoding = "json",
    string? compress = null)
{
    public string Base => @base.TrimEnd('/');
    public int Version => version;
    public string Encoding => encoding;
    public string? Compress => compress;

    public Uri ToUri()
    {
        var sb = new StringBuilder();
        sb.Append(Base);
        sb.Append("/?v=").Append(Version);
        sb.Append("&encoding=").Append(Uri.EscapeDataString(Encoding));

        if (!string.IsNullOrWhiteSpace(Compress))
            sb.Append("&compress=").Append(Uri.EscapeDataString(Compress));

        return new Uri(sb.ToString(), UriKind.Absolute);
    }

    public override string ToString()
        => ToUri().ToString();
}
