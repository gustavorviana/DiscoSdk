namespace DiscoSdk.Commands;

/// <summary>
/// Marks a discovered slash command or context menu command as <em>on-demand</em>: the assembly
/// scan will add the built command to <see cref="ICommandCatalog"/>, but the auto-registration
/// pipeline will <strong>not</strong> register it globally during startup.
/// </summary>
/// <remarks>
/// <para>
/// Use this for commands that should be activated for specific guilds at arbitrary moments —
/// for example, a "premium feature" command granted after a payment flow completes, or a
/// per-guild ops command toggled by an external admin tool. The caller resolves
/// <see cref="DiscoSdk.Rest.Actions.IGuildCommandUpdateFactory"/> from DI and calls
/// <see cref="DiscoSdk.Rest.Actions.ICommandUpdateScope.AddFromCatalog(string, DiscoSdk.Models.Enums.ApplicationCommandType)"/>
/// to pull the command by name + type and register it on the target guild.
/// </para>
/// <para>
/// Combination with <c>GuildIds</c> (on <see cref="SlashCommandAttribute"/>, <see cref="UserCommandAttribute"/>,
/// or <see cref="MessageCommandAttribute"/>) is allowed: the auto-register will still target the
/// listed guilds, and the command remains available via the catalog for additional manual registrations.
/// What <c>[OnDemand]</c> always suppresses is the <strong>global</strong> auto-register.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class OnDemandAttribute : Attribute
{
}
