using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using System;
using System.Linq;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper that implements <see cref="IRole"/> for a <see cref="Role"/> instance.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RoleWrapper"/> class.
/// </remarks>
/// <param name="role">The role instance to wrap.</param>
/// <param name="guild">The guild this role belongs to.</param>
/// <param name="client">The Discord client for performing operations.</param>
internal class RoleWrapper(Role role, IGuild guild, DiscordClient client) : IRole
{
    private readonly Role _role = role ?? throw new ArgumentNullException(nameof(role));
    private readonly IGuild _guild = guild ?? throw new ArgumentNullException(nameof(guild));
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    // IMentionable / IWithDiscordIdentity
    /// <inheritdoc />
    public DiscordId Id => _role.Id;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt => _role.Id.CreatedAt;

    // IRole properties
    /// <inheritdoc />
    public int Position => _role.Position;

    /// <inheritdoc />
    public int PositionRaw => _role.Position;

    /// <inheritdoc />
    public string Name => _role.Name;

    /// <inheritdoc />
    public bool IsManaged => _role.Managed;

    /// <inheritdoc />
    public bool IsHoisted => _role.Hoist;

    /// <inheritdoc />
    public bool IsMentionable => _role.Mentionable;

    /// <inheritdoc />
    public DiscordPermission Permissions
    {
        get
        {
            if (ulong.TryParse(_role.Permissions, out var permissions))
                return (DiscordPermission)permissions;
            return DiscordPermission.None;
        }
    }

    /// <inheritdoc />
    public DiscordPermission PermissionsRaw => Permissions;

    /// <inheritdoc />
    public Color Colors => _role.Color;

    /// <inheritdoc />
    public bool IsPublicRole => _role.Id == _guild.Id;

    /// <inheritdoc />
    public IGuild Guild => _guild;

	/// <inheritdoc />
	public RoleTags Tags => _role.Tags ?? new();

	/// <inheritdoc />
	public RoleIcon? Icon
	{
		get
		{
			if (!string.IsNullOrEmpty(_role.UnicodeEmoji))
				return new RoleIcon(_role.UnicodeEmoji, isEmoji: true);
			if (!string.IsNullOrEmpty(_role.Icon))
				return new RoleIcon(_role.Icon);
			return null;
		}
	}

    /// <inheritdoc />
    public int CompareTo(IRole? other)
    {
        if (other == null)
            return 1;

        if (IsPublicRole && !other.IsPublicRole)
            return -1;
        if (!IsPublicRole && other.IsPublicRole)
            return 1;

        var positionComparison = Position.CompareTo(other.Position);
        if (positionComparison != 0)
            return -positionComparison; // Higher position comes first

        return Id.CompareTo(other.Id);
    }

    /// <inheritdoc />
    public bool CanInteract(IRole role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (IsPublicRole)
            return false; // @everyone role cannot interact with any role

        if (role.IsPublicRole)
            return true; // Any role can interact with @everyone

        if (Guild.Id != role.Guild.Id)
            return false; // Roles from different guilds cannot interact

        // A role can interact with another role if it has a higher position
        return Position > role.Position;
    }

	/// <inheritdoc />
	public IRoleAction Edit()
	{
		return new RoleAction(_client, _guild, _role.Id);
	}

	/// <inheritdoc />
	public IRestAction ModifyPosition(int position)
	{
		if (IsPublicRole)
			throw new InvalidOperationException("Cannot modify position of the @everyone role.");

		return RestAction.Create(async cancellationToken =>
		{
			var request = new[]
			{
				new { id = _role.Id.ToString(), position = position }
			};

			await _client.RoleClient.ModifyPositionsAsync(_guild.Id, request, cancellationToken);
		});
	}

	/// <inheritdoc />
	public IRoleAction CreateCopy(IGuild guild)
	{
		if (guild == null)
			throw new ArgumentNullException(nameof(guild));

		var action = new RoleAction(_client, guild);
		action.SetName(_role.Name);
		action.SetPermissions(Permissions);
		action.SetColor(_role.Color);
		action.SetHoist(_role.Hoist);
		action.SetMentionable(_role.Mentionable);
		if (!string.IsNullOrEmpty(_role.Icon))
			action.SetIcon(_role.Icon);
		if (!string.IsNullOrEmpty(_role.UnicodeEmoji))
			action.SetUnicodeEmoji(_role.UnicodeEmoji);
		return action;
	}

	/// <inheritdoc />
	public IRestAction Delete()
	{
		if (IsPublicRole)
			throw new InvalidOperationException("Cannot delete the @everyone role.");

		return RestAction.Create(cancellationToken => _client.RoleClient.DeleteAsync(_guild.Id, _role.Id, cancellationToken));
	}
}

