namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents role flags.
/// </summary>
[Flags]
public enum RoleFlags
{
    None = 0,
    InPrompt = 1 << 0
}
