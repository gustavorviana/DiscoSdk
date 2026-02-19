namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SlashCommandAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public string[] GuildIds { get; set; } = [];

    public SlashCommandAttribute(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Command name cannot be null, empty, or whitespace.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Command description cannot be null, empty, or whitespace.", nameof(description));

        Name = name;
        Description = description;
    }

    public bool IsGuildCommand()
    {
        return GuildIds is { Length: > 0 };
    }
}