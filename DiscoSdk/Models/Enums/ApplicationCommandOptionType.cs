namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents the type of an application command option.
/// </summary>
public enum ApplicationCommandOptionType
{
    SubCommand = 1,
    SubCommandGroup = 2,
    String = 3,
    Integer = 4,
    Boolean = 5,
    User = 6,
    Channel = 7,
    Role = 8,
    Mentionable = 9,
    Number = 10,
    Attachment = 11
}
