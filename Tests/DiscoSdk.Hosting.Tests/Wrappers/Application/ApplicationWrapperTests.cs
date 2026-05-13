using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Application;

public class ApplicationWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task Edit_PatchesCurrentApplicationWithChangesAsync()
	{
		Http.SendAsync<DiscoSdk.Models.Applications.Application>(
				Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new DiscoSdk.Models.Applications.Application());

		var wrapper = new ApplicationWrapper(Client, new DiscoSdk.Models.Applications.Application());

		await wrapper.Edit()
			.SetDescription("new-desc")
			.SetFlags(ApplicationFlags.GatewayMessageContentLimited)
			.SetTags("a", "b")
			.ExecuteAsync();

		await Http.Received(1).SendAsync<DiscoSdk.Models.Applications.Application>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "applications/@me"),
			HttpMethod.Patch,
			Arg.Is<object?>(body =>
				BodyContains(body, "description", "new-desc") &&
				BodyContains(body, "flags", (int)ApplicationFlags.GatewayMessageContentLimited) &&
				BodyHasKey(body, "tags")),
			Arg.Any<CancellationToken>());
	}
}
