namespace DiscoSdk.Rest.Actions;

/// <summary>
/// REST action that responds to an interaction by launching the bot's embedded Discord Activity
/// in the voice channel where the invoking user currently is. Maps to
/// <see cref="Models.Enums.InteractionCallbackType.LaunchActivity"/> (type 12).
/// </summary>
/// <remarks>
/// Only meaningful for applications flagged as <c>EMBEDDED</c> in the Discord Developer Portal —
/// for regular bots without an activity registered, Discord rejects the callback. The user must
/// be in a voice channel; otherwise Discord rejects the call.
/// </remarks>
public interface ILaunchActivityRestAction : IRestAction
{
}
