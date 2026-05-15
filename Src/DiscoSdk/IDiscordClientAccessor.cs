namespace DiscoSdk;

/// <summary>
/// Late-bound accessor for the active <see cref="IDiscordClient"/>. The client is constructed
/// <em>after</em> the DI container is built (it needs the provider itself), so services that
/// depend on it can't take it in their constructor directly — they take this accessor instead
/// and read <see cref="Client"/> on-demand.
/// </summary>
/// <remarks>
/// <para>
/// Modeled after <c>IHttpContextAccessor</c>: a single seam that hides the chicken-and-egg
/// between DI graph construction and client instantiation. Reading <see cref="Client"/>
/// before <c>DiscordClientBuilder.Build()</c> returns throws — there's no client yet.
/// </para>
/// <para>
/// The default DI registration also exposes <see cref="IDiscordClient"/> as a singleton that
/// resolves through this accessor, so most consumers can just take <see cref="IDiscordClient"/>
/// directly without ever touching the accessor.
/// </para>
/// </remarks>
public interface IDiscordClientAccessor
{
    /// <summary>The current <see cref="IDiscordClient"/>. Throws if accessed before the client is built.</summary>
    IDiscordClient Client { get; }
}
