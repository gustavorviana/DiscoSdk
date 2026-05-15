using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="ILaunchActivityRestAction"/>. Sends a single interaction
/// callback with <see cref="InteractionCallbackType.LaunchActivity"/> (type 12) and no
/// <c>data</c> payload — Discord resolves the activity from the interaction's
/// <c>application_id</c>.
/// </summary>
internal sealed class LaunchActivityRestAction : RestAction, ILaunchActivityRestAction
{
	private readonly DiscordClient _client;
	private readonly InteractionHandle _interactionHandle;

	public LaunchActivityRestAction(DiscordClient client, InteractionHandle interactionHandle)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_interactionHandle = interactionHandle ?? throw new ArgumentNullException(nameof(interactionHandle));
	}

	public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
	{
		if (_interactionHandle.Responded)
			throw new InvalidOperationException("This interaction has already been responded to.");

		if (_interactionHandle.IsDeferred)
			throw new InvalidOperationException("Cannot launch an activity after deferring the interaction.");

		try
		{
			await _client.InteractionClient.SendCallbackAsync(
				_interactionHandle,
				data: null,
				InteractionCallbackType.LaunchActivity,
				cancellationToken);
			_interactionHandle.Responded = true;
		}
		catch (DiscordApiException ex) when (ex.DiscordCode == InteractionClient.AlreadyAcknowledgedCode)
		{
			_interactionHandle.Responded = true;
			throw;
		}
	}
}
