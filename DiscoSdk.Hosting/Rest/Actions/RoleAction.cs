using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IRoleAction"/> for creating or editing Discord roles.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RoleAction"/> class.
/// </remarks>
/// <param name="client">The Discord client.</param>
/// <param name="guildId">The ID of the guild to create/edit the role in.</param>
/// <param name="roleId">The ID of the role to edit, or null if creating a new role.</param>
internal class RoleAction(DiscordClient client, IGuild guild, DiscordId? roleId = null) : RestAction<IRole>, IRoleAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private readonly DiscordId? _roleId = roleId;

	private string? _name;
	private DiscordPermission? _permissions;
	private Color? _color;
	private bool? _hoist;
	private bool? _mentionable;
	private int? _position;
	private byte[]? _iconBytes;
	private string? _iconHash;
	private string? _unicodeEmoji;

	/// <inheritdoc />
	public IRoleAction SetName(string name)
	{
		_name = name;
		return this;
	}

	/// <inheritdoc />
	public IRoleAction SetPermissions(DiscordPermission permissions)
	{
		_permissions = permissions;
		return this;
	}

	/// <inheritdoc />
	public IRoleAction SetColor(Color color)
	{
		_color = color;
		return this;
	}

	/// <inheritdoc />
	public IRoleAction SetHoist(bool hoist)
	{
		_hoist = hoist;
		return this;
	}

	/// <inheritdoc />
	public IRoleAction SetMentionable(bool mentionable)
	{
		_mentionable = mentionable;
		return this;
	}

	/// <inheritdoc />
	public IRoleAction SetPosition(int? position)
	{
		_position = position;
		return this;
	}

	/// <inheritdoc />
	public IRoleAction SetIcon(byte[]? icon)
	{
		_iconBytes = icon;
		_iconHash = null;
		_unicodeEmoji = null;
		return this;
	}

	/// <inheritdoc />
	public IRoleAction SetIcon(string? iconHash)
	{
		_iconHash = iconHash;
		_iconBytes = null;
		_unicodeEmoji = null;
		return this;
	}

	/// <inheritdoc />
	public IRoleAction SetUnicodeEmoji(string? unicodeEmoji)
	{
		_unicodeEmoji = unicodeEmoji;
		_iconBytes = null;
		_iconHash = null;
		return this;
	}

	/// <inheritdoc />
	public override async Task<IRole> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var request = new Dictionary<string, object?>();

		if (_name != null)
			request["name"] = _name;

		if (_permissions.HasValue)
			request["permissions"] = ((ulong)_permissions.Value).ToString();

		if (_color.HasValue)
			request["color"] = _color.Value.Value;

		if (_hoist.HasValue)
			request["hoist"] = _hoist.Value;

		if (_mentionable.HasValue)
			request["mentionable"] = _mentionable.Value;

		if (_position.HasValue)
			request["position"] = _position.Value;

		if (_iconBytes != null)
		{
			// Convert icon bytes to base64 data URI (Discord expects data:image/{type};base64,{base64})
			var base64 = Convert.ToBase64String(_iconBytes);
			// Try to detect image type, default to png
			var imageType = "png";
			if (_iconBytes.Length >= 2)
			{
				// Check for JPEG signature
				if (_iconBytes[0] == 0xFF && _iconBytes[1] == 0xD8)
					imageType = "jpeg";
				// Check for GIF signature
				else if (_iconBytes[0] == 0x47 && _iconBytes[1] == 0x49 && _iconBytes[2] == 0x46)
					imageType = "gif";
			}
			request["icon"] = $"data:image/{imageType};base64,{base64}";
		}
		else if (_iconHash != null)
		{
			request["icon"] = _iconHash;
		}
		else if (_unicodeEmoji != null)
		{
			request["unicode_emoji"] = _unicodeEmoji;
		}

		Role role;
		if (_roleId.HasValue)
		{
			// Editing existing role
			role = await _client.RoleClient.EditAsync(guild.Id, _roleId.Value, request, cancellationToken);
		}
		else
		{
			// Creating new role
			role = await _client.RoleClient.CreateAsync(guild.Id, request, cancellationToken);
		}

		return new RoleWrapper(role, guild, _client);
	}
}

