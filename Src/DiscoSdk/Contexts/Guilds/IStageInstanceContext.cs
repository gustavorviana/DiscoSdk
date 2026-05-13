using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for <c>STAGE_INSTANCE_CREATE</c>, <c>STAGE_INSTANCE_UPDATE</c>, and
/// <c>STAGE_INSTANCE_DELETE</c> Gateway events.
/// </summary>
public interface IStageInstanceContext : IContext
{
	/// <summary>The stage instance payload.</summary>
	StageInstance Instance { get; }

	/// <summary>The guild containing the stage instance.</summary>
	IGuild Guild { get; }
}
