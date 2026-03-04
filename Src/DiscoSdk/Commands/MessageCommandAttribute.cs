namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class MessageCommandAttribute : Attribute
{
    public string Name { get; }

    public string[] GuildIds { get; set; } = [];

    public MessageCommandAttribute(string name)
    {
        UserCommandBuilder.ValidateContextMenuName(name);
        Name = name;
    }

    public bool IsGuildCommand()
    {
        return GuildIds is { Length: > 0 };
    }
}
