namespace DiscoSdk.Commands;

/// <summary>
/// Marks a method as a Discord context menu command handler. The kind (user vs message)
/// is inferred from the declaring class's base type: <see cref="UserContextMenuHandler"/>
/// produces a <see cref="ContextMenuType.User"/> command and
/// <see cref="MessageContextMenuHandler"/> produces a <see cref="ContextMenuType.Message"/>
/// command.
/// </summary>
/// <remarks>
/// Context menu names allow spaces and mixed case (per Discord's USER/MESSAGE rules) and
/// must be 1-32 characters.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ContextMenuCommandAttribute : Attribute
{
    public string Name { get; }

    public string[] GuildIds { get; set; } = [];

    public ContextMenuCommandAttribute(string name)
    {
        ContextMenuBuilder.ValidateContextMenuName(name);
        Name = name;
    }

    public bool IsGuildCommand()
    {
        return GuildIds is { Length: > 0 };
    }
}
