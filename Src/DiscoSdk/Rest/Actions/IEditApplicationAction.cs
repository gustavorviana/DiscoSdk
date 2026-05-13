using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// A REST action that edits the current application. Every field is optional — only the ones that are
/// set are sent.
/// </summary>
public interface IEditApplicationAction : IRestAction<IApplication>
{
	/// <summary>Sets the default custom authorization URL.</summary>
	IEditApplicationAction SetCustomInstallUrl(string? url);

	/// <summary>Sets the application description.</summary>
	IEditApplicationAction SetDescription(string description);

	/// <summary>Sets the role-connection verification URL.</summary>
	IEditApplicationAction SetRoleConnectionsVerificationUrl(string? url);

	/// <summary>Sets the default in-app authorization link parameters.</summary>
	IEditApplicationAction SetInstallParams(ApplicationInstallParams? installParams);

	/// <summary>Sets the public application flags (only the GATEWAY_* limited flags are editable).</summary>
	IEditApplicationAction SetFlags(ApplicationFlags flags);

	/// <summary>Sets the application icon (base64 image data URI), or null to remove it.</summary>
	IEditApplicationAction SetIcon(string? icon);

	/// <summary>Sets the default rich-presence cover image (base64 image data URI), or null to remove it.</summary>
	IEditApplicationAction SetCoverImage(string? coverImage);

	/// <summary>Sets the interactions endpoint URL.</summary>
	IEditApplicationAction SetInteractionsEndpointUrl(string? url);

	/// <summary>Sets up to 5 tags describing the application.</summary>
	IEditApplicationAction SetTags(params string[] tags);

	/// <summary>Sets the event webhooks URL.</summary>
	IEditApplicationAction SetEventWebhooksUrl(string? url);
}
