using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Observability;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Hosting.Tests.Gateway.Events;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Observability;

[Collection("Observability")]
public class HandlerInvocationsMetricTests : DispatcherTestBase
{
	[Fact]
	public async Task SuccessfulHandler_PublishesOkInvocationAndLatencyAsync()
	{
		using var capture = new MeterListenerCapture("discosdk.handler.invocations", "discosdk.handler.latency");
		var handler = Substitute.For<IGuildMemberAddHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.GuildMemberAdd(guildId: 100, userId: 42));

		var invocations = capture.LongFor("discosdk.handler.invocations").ToList();
		var latencies = capture.DoubleFor("discosdk.handler.latency").ToList();

		Assert.Contains(invocations, m => Equals(m.Tag(DiagnosticTags.HandlerOutcome), DiagnosticTags.OutcomeOk));
		Assert.NotEmpty(latencies);
	}

	[Fact]
	public async Task FailingHandler_PublishesErrorInvocationWithExceptionTypeAsync()
	{
		using var capture = new MeterListenerCapture("discosdk.handler.invocations");
		var handler = Substitute.For<IGuildMemberAddHandler>();
		handler.HandleAsync(Arg.Any<IGuildMemberAddContext>(), Arg.Any<IServiceProvider>())
			.Returns(_ => throw new InvalidOperationException("boom"));
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.GuildMemberAdd(guildId: 100, userId: 42));

		var errorRow = capture.LongFor("discosdk.handler.invocations")
			.FirstOrDefault(m => Equals(m.Tag(DiagnosticTags.HandlerOutcome), DiagnosticTags.OutcomeError));
		Assert.NotNull(errorRow);
		Assert.Equal(typeof(InvalidOperationException).FullName, errorRow!.Tag(DiagnosticTags.ExceptionType));
	}
}
