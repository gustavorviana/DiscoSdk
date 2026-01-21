using DiscoSdk.Rest.Actions.Messages;

namespace DiscoSdk.Models.Messages.Mentions;

/// <summary>
/// Base class for builders that support semantic Discord mentions.
///
/// This type provides a high-level API for writing mentions (users, roles and
/// everyone) while tracking their intent (ping vs. silent) and automatically
/// generating a safe <see cref="AllowedMentions"/> payload.
///
/// The goal is to let derived builders express *what* is being mentioned and
/// *whether it should notify*, without exposing Discord's low-level
/// <c>allowed_mentions</c> rules. This prevents accidental mass pings and keeps
/// mention behavior consistent across message actions.
/// </summary>
public abstract class MentionBuilderBase<TSelf> where TSelf : MentionBuilderBase<TSelf>

{
    /// <summary>
    /// Tracks all semantic mentions written into the message,
    /// excluding channels (which never ping).
    /// </summary>
    protected HashSet<Mention> Mentions { get; } = [];

    /// <summary>
    /// Writes a user mention into the message.
    /// When <paramref name="silent"/> is <c>true</c>, the mention will be rendered
    /// visually but will not trigger a notification.
    /// </summary>
    /// <param name="userId">The user ID to mention.</param>
    /// <param name="silent">Whether the mention should be silent.</param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    public TSelf MentionUser(Snowflake userId, bool silent = false)
    {
        return AppendMention(new Mention(MentionType.User, userId, silent));
    }

    /// <summary>
    /// Writes a role mention into the message.
    /// When <paramref name="silent"/> is <c>true</c>, the mention will be rendered
    /// visually but will not trigger a notification.
    /// </summary>
    /// <param name="roleId">The role ID to mention.</param>
    /// <param name="silent">Whether the mention should be silent.</param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    public TSelf MentionRole(Snowflake roleId, bool silent = false)
    {
        return AppendMention(new Mention(MentionType.Role, roleId, silent));
    }

    /// <summary>
    /// Writes an <c>@everyone</c> mention into the message.
    /// When <paramref name="silent"/> is <c>true</c>, it will be rendered as text
    /// without notifying anyone.
    /// </summary>
    /// <param name="silent">Whether the mention should be silent.</param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    public TSelf MentionEveryone(bool silent = false)
    {
        return AppendMention(Mention.Everyone(silent));
    }

    /// <summary>
    /// Appends a pre-built <see cref="Mention"/> into the message content and tracks it
    /// for later generation of <see cref="AllowedMentions"/>.
    /// </summary>
    /// <param name="mention">The mention to append.</param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    public virtual TSelf AppendMention(Mention mention)
    {
        // Channels never participate in allowed_mentions; they are always silent links.
        if (mention.Type != MentionType.Channel)
            Mentions.Add(mention);

        return (TSelf)this;
    }

    protected void CopyTo<TOther>(MentionBuilderBase<TOther> builder) where TOther : MentionBuilderBase<TOther>
    {
        foreach (var mention in Mentions)
            builder.Mentions.Add(mention);
    }

    /// <summary>
    /// Builds the <see cref="AllowedMentions"/> payload based on the mentions written
    /// into the message.
    ///
    /// If no semantic mentions exist, this method returns <c>null</c>, allowing the
    /// caller to omit the field entirely.
    /// </summary>
    /// <returns>
    /// An <see cref="AllowedMentions"/> instance describing which mentions may notify,
    /// or <c>null</c> if no mention control is required.
    /// </returns>
    public AllowedMentions? BuildAllowedMentions()
    {
        if (Mentions.Count == 0)
            return null;

        var parse = new List<string>(capacity: 3);
        var userIds = new List<string>();
        var roleIds = new List<string>();

        var typedMentions = Mentions
            .GroupBy(x => x.Type, x => x)
            .ToDictionary(x => x.Key, x => x.ToArray());

        // everyone: any pinging @everyone enables global parsing
        if (typedMentions.TryGetValue(MentionType.Everyone, out var everyones) &&
            everyones.Any(x => x.Ping))
            parse.Add("everyone");

        if (ApplyMention(typedMentions, MentionType.User, userIds))
            parse.Add("users");

        if (ApplyMention(typedMentions, MentionType.Role, roleIds))
            parse.Add("roles");

        return new AllowedMentions
        {
            Parse = [.. parse],
            Roles = [.. roleIds],
            Users = [.. userIds]
        };
    }

    /// <summary>
    /// Applies the mention rules for a given <see cref="MentionType"/> and determines
    /// whether this type can be safely enabled via <c>allowed_mentions.parse</c>
    /// or must be restricted to explicit IDs.
    ///
    /// If all mentions of this type are non-silent, this method returns <c>true</c>,
    /// indicating that the caller may use <c>parse</c> (e.g. "users", "roles").
    ///
    /// If at least one silent mention exists, this method collects only the non-silent
    /// mention IDs into <paramref name="ids"/> and returns <c>false</c>, signaling that
    /// <c>parse</c> must NOT be used for this type, otherwise silent mentions would
    /// incorrectly trigger notifications.
    /// </summary>
    /// <param name="typedMentions">
    /// A dictionary grouping mentions by their <see cref="MentionType"/>.
    /// </param>
    /// <param name="type">
    /// The mention type being evaluated (User, Role, Everyone, etc.).
    /// </param>
    /// <param name="ids">
    /// A list that will receive the IDs of non-silent mentions when selective behavior
    /// is required.
    /// </param>
    /// <returns>
    /// <c>true</c> if all mentions of this type are non-silent and parsing is safe;
    /// <c>false</c> if selective ID-based behavior is required.
    /// </returns>
    private static bool ApplyMention(
        Dictionary<MentionType, Mention[]> typedMentions,
        MentionType type,
        List<string> ids)
    {
        if (!typedMentions.TryGetValue(type, out var mentions))
            return false;

        if (mentions.All(x => x.Ping))
            return true;

        ids.AddRange(mentions.Where(x => x.Ping).Select(x => x.Id.ToString()!));

        return false;
    }
}
