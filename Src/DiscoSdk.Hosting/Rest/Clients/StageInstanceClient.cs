using DiscoSdk.Models;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for Discord Stage Instance operations (the live "stage" attached to a Stage channel).
/// </summary>
internal class StageInstanceClient(IDiscordRestClient client)
{
	/// <summary>Creates a Stage Instance on a Stage channel.</summary>
	public Task<StageInstance> CreateAsync(object request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("stage-instances");
		return client.SendAsync<StageInstance>(route, HttpMethod.Post, request, cancellationToken);
	}

	/// <summary>Gets the Stage Instance currently associated with the given Stage channel.</summary>
	public Task<StageInstance> GetAsync(Snowflake channelId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("stage-instances/{channel_id}", channelId);
		return client.SendAsync<StageInstance>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Modifies an existing Stage Instance (topic / privacy level).</summary>
	public Task<StageInstance> ModifyAsync(Snowflake channelId, object request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("stage-instances/{channel_id}", channelId);
		return client.SendAsync<StageInstance>(route, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>Deletes the Stage Instance associated with the given Stage channel.</summary>
	public Task DeleteAsync(Snowflake channelId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("stage-instances/{channel_id}", channelId);
		return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
	}
}
