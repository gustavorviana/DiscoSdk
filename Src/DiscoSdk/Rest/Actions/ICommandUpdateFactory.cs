namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Combined entry point to the command registration system. Inherits the guild-only contract
/// and adds <see cref="OpenForGlobal(bool)"/> for the global scope. Passed to the
/// <c>CommandsUpdateWindowOpened</c> event during startup and also resolvable from DI.
/// </summary>
public interface ICommandUpdateFactory : IGuildCommandUpdateFactory
{
    /// <summary>
    /// Opens a registration scope for the application's global commands.
    /// </summary>
    /// <param name="overwrite">
    /// <c>true</c> = bulk PUT (Discord replaces every global command).
    /// <c>false</c> = append/upsert (reads existing first; POST new, PATCH changed, no-op equal; never deletes).
    /// </param>
    ICommandUpdateScope OpenForGlobal(bool overwrite);
}
