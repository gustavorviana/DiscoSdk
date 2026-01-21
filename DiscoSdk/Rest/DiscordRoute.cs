using System.Globalization;
using System.Text;

namespace DiscoSdk.Rest;

public readonly struct DiscordRoute(string template, params object[] args)
{
    private static readonly HashSet<string> MajorParameters = new(StringComparer.OrdinalIgnoreCase)
    {
        "{channel_id}","{guild_id}","{webhook_id}","{interaction_id}"
    };

    private readonly object[] _args = args ?? [];

    /// <summary>
    /// Template path, ex:
    /// "/channels/{channel_id}/messages/{message_id}"
    /// </summary>
    public string Template { get; } = template ?? throw new ArgumentNullException(nameof(template));

    public override string ToString()
    {
        if (_args.Length == 0)
            return Template;

        return ReplaceNamedParameters(Template, _args);
    }

    /// <summary>
    /// Parent bucket path derived from the first MAJOR parameter.
    /// Ex:
    /// Template: /channels/{channel_id}/messages/{message_id}
    /// Result:   /channels/123456724611559464
    /// </summary>
    public string GetBucketPath()
    {
        var segments = Template.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var allowApplicationFallback = !segments.Any(MajorParameters.Contains);
        var sb = new StringBuilder();
        var argIndex = 0;

        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            sb.Append('/');

            if (MajorParameters.Contains(segment) || (allowApplicationFallback && segment == "{application_id}"))
            {
                sb.Append(_args.Length > argIndex ? _args[argIndex] : segment);
                break;
            }

            sb.Append(segment);

            if (IsPlaceholder(segment))
                argIndex++;
        }

        return sb.Length == 0 ? "/" : sb.ToString();
    }

    private static bool IsPlaceholder(string segment)
    {
        return segment.Length >= 2 && segment[0] == '{' && segment[^1] == '}';
    }

    private static string ReplaceNamedParameters(string template, object[] args)
    {
        var sb = new StringBuilder(template.Length + 32);
        var argIndex = 0;

        for (var i = 0; i < template.Length; i++)
        {
            var c = template[i];

            if (c == '{')
            {
                var end = template.IndexOf('}', i + 1);
                if (end == -1)
                    throw new FormatException("Invalid route template");

                if (argIndex >= args.Length)
                    throw new FormatException("Not enough route arguments");

                sb.Append(Convert.ToString(args[argIndex], CultureInfo.InvariantCulture));
                argIndex++;

                i = end;
                continue;
            }

            sb.Append(c);
        }

        if (argIndex != args.Length)
            throw new FormatException("Too many route arguments");

        return sb.ToString();
    }
}