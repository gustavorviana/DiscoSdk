using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Modifies the current user (the bot). Each setter corresponds to a single field — only the
/// fields you set are sent to Discord. Use <see cref="ClearAvatar"/> / <see cref="ClearBanner"/>
/// to explicitly remove an image instead of passing an empty string.
/// </summary>
public interface IModifyMeAction : IRestAction<IUser>
{
    /// <summary>Sets a new username.</summary>
    IModifyMeAction SetUsername(string username);

    /// <summary>Sets a new avatar as a base64-encoded data URI.</summary>
    IModifyMeAction SetAvatar(string avatarDataUri);

    /// <summary>Removes the bot's current avatar.</summary>
    IModifyMeAction ClearAvatar();

    /// <summary>Sets a new banner as a base64-encoded data URI.</summary>
    IModifyMeAction SetBanner(string bannerDataUri);

    /// <summary>Removes the bot's current banner.</summary>
    IModifyMeAction ClearBanner();
}
