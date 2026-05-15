using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SlashCommandAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public string[] GuildIds { get; set; } = [];

    /// <summary>
    /// Installation contexts where the command is available
    /// (<see cref="ApplicationIntegrationType.GuildInstall"/>, <see cref="ApplicationIntegrationType.UserInstall"/>).
    /// When empty, Discord's default is applied.
    /// </summary>
    public ApplicationIntegrationType[] IntegrationTypes { get; set; } = [];

    /// <summary>
    /// Interaction contexts where the command can be used
    /// (<see cref="InteractionContextType.Guild"/>, <see cref="InteractionContextType.BotDm"/>,
    /// <see cref="InteractionContextType.PrivateChannel"/>). When empty, Discord's default is applied.
    /// </summary>
    public InteractionContextType[] Contexts { get; set; } = [];

    public SlashCommandAttribute(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Command name cannot be null, empty, or whitespace.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Command description cannot be null, empty, or whitespace.", nameof(description));

        SlashCommandBuilder.ValidateCommandName(name);
        SlashCommandBuilder.ValidateCommandDescription(description);

        Name = name;
        Description = description;
    }

    public bool IsGuildCommand()
    {
        return GuildIds is { Length: > 0 };
    }
}