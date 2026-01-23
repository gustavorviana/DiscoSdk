namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents the type of client a user is using.
/// </summary>
[Flags]
public enum ClientType
{
	/// <summary>
	/// No client type.
	/// </summary>
	None = 0,

	/// <summary>
	/// Desktop client.
	/// </summary>
	Desktop = 1 << 0,

	/// <summary>
	/// Mobile client.
	/// </summary>
	Mobile = 1 << 1,

	/// <summary>
	/// Web client.
	/// </summary>
	Web = 1 << 2
}

