using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SlashCommandAttribute(string name, string description) : Attribute
{
    public string Name => name;
    public string Description => description;
    public string[] GuildIds { get; set; } = [];
    public ApplicationCommandType Type { get; set; } = ApplicationCommandType.ChatInput;

    public bool IsGuildCommand()
    {
        return GuildIds?.Length > 0;
    }
}