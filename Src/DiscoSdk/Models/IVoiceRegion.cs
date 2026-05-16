namespace DiscoSdk.Models;

/// <summary>Read-only Discord voice region descriptor.</summary>
public interface IVoiceRegion
{
	/// <summary>Region identifier (e.g. <c>"us-east"</c>).</summary>
	string Id { get; }

	/// <summary>Human-readable region name.</summary>
	string Name { get; }

	/// <summary>Whether this is the optimal region for the guild (only meaningful when listed per guild).</summary>
	bool Optimal { get; }

	/// <summary>Whether Discord has deprecated this region — avoid for new resources.</summary>
	bool Deprecated { get; }

	/// <summary>Whether this is a custom region (used for stage events, etc.).</summary>
	bool Custom { get; }
}
