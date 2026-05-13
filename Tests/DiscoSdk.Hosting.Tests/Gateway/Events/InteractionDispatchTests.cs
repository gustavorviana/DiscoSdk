using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

/// <summary>
/// Verifies the INTERACTION_CREATE sub-dispatch in <c>DiscordEventDispatcher</c> routes each
/// interaction type to the correct handler interface.
/// <para>
/// Frames are dispatched with <c>channelId: null</c> so the dispatcher skips the REST-bound
/// <c>GetChannel</c> call (the dispatcher only fetches the channel when channelId is non-null).
/// </para>
/// </summary>
public class InteractionDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task InteractionCreate_ChatInputCommand_InvokesApplicationCommandHandlerAsync()
	{
		var commandHandler = Substitute.For<IApplicationCommandHandler>();
		var genericHandler = Substitute.For<IInteractionCreateHandler>();
		AddHandler(commandHandler);
		AddHandler(genericHandler);

		await DispatchAsync(DispatchFrames.InteractionCreate(type: 2, commandType: 1, channelId: null));

		await commandHandler.Received(1).HandleAsync(Arg.Any<ICommandContext>(), Arg.Any<IServiceProvider>());
		await genericHandler.Received(1).HandleAsync(Arg.Any<IInteractionContext>(), Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task InteractionCreate_UserCommand_InvokesUserCommandHandlerAsync()
	{
		var userHandler = Substitute.For<IUserCommandHandler>();
		var commandHandler = Substitute.For<IApplicationCommandHandler>();
		AddHandler(userHandler);
		AddHandler(commandHandler);

		await DispatchAsync(DispatchFrames.InteractionCreate(type: 2, commandType: 2, channelId: null));

		await userHandler.Received(1).HandleAsync(Arg.Any<IUserCommandContext>(), Arg.Any<IServiceProvider>());
		// IApplicationCommandHandler should NOT fire for User/Message context-menu commands.
		await commandHandler.DidNotReceive().HandleAsync(Arg.Any<ICommandContext>(), Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task InteractionCreate_MessageCommand_InvokesMessageCommandHandlerAsync()
	{
		var messageHandler = Substitute.For<IMessageCommandHandler>();
		var commandHandler = Substitute.For<IApplicationCommandHandler>();
		AddHandler(messageHandler);
		AddHandler(commandHandler);
		// MessageCommandContext wraps a target message in a MessageWrapper, which requires a
		// non-null channel — make the dispatcher's REST channel resolution succeed.
		SeedTextChannel(channelId: 200, guildId: 100);

		await DispatchAsync(DispatchFrames.InteractionCreate(type: 2, commandType: 3, channelId: 200));

		await messageHandler.Received(1).HandleAsync(Arg.Any<IMessageCommandContext>(), Arg.Any<IServiceProvider>());
		await commandHandler.DidNotReceive().HandleAsync(Arg.Any<ICommandContext>(), Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task InteractionCreate_MessageComponent_InvokesComponentInteractionHandlerAsync()
	{
		var componentHandler = Substitute.For<IComponentInteractionHandler>();
		var genericHandler = Substitute.For<IInteractionCreateHandler>();
		AddHandler(componentHandler);
		AddHandler(genericHandler);

		await DispatchAsync(DispatchFrames.InteractionCreate(type: 3, channelId: null));

		await componentHandler.Received(1).HandleAsync(Arg.Any<IInteractionContext>(), Arg.Any<IServiceProvider>());
		await genericHandler.Received(1).HandleAsync(Arg.Any<IInteractionContext>(), Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task InteractionCreate_Autocomplete_InvokesAutocompleteHandlerAsync()
	{
		var autoHandler = Substitute.For<IAutocompleteHandler>();
		AddHandler(autoHandler);
		// AutocompleteContext reads options through InteractionWrapper.Data, which is null unless
		// the wrapper sees a non-null channel — seed the REST resolution so it succeeds.
		SeedTextChannel(channelId: 200, guildId: 100);

		await DispatchAsync(DispatchFrames.InteractionCreate(type: 4, channelId: 200));

		await autoHandler.Received(1).HandleAsync(Arg.Any<IAutocompleteContext>(), Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task InteractionCreate_ModalSubmit_InvokesModalSubmitHandlerAsync()
	{
		var modalHandler = Substitute.For<IModalSubmitHandler>();
		var genericHandler = Substitute.For<IInteractionCreateHandler>();
		AddHandler(modalHandler);
		AddHandler(genericHandler);

		await DispatchAsync(DispatchFrames.InteractionCreate(type: 5, channelId: null));

		await modalHandler.Received(1).HandleAsync(Arg.Any<IModalContext>(), Arg.Any<IServiceProvider>());
		await genericHandler.Received(1).HandleAsync(Arg.Any<IInteractionContext>(), Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task InteractionCreate_DoesNotInvokeUnrelatedHandlersAsync()
	{
		var commandHandler = Substitute.For<IApplicationCommandHandler>();
		var modalHandler = Substitute.For<IModalSubmitHandler>();
		var autoHandler = Substitute.For<IAutocompleteHandler>();
		var componentHandler = Substitute.For<IComponentInteractionHandler>();
		AddHandler(commandHandler);
		AddHandler(modalHandler);
		AddHandler(autoHandler);
		AddHandler(componentHandler);

		// MessageComponent (type 3) — only the component handler (and IInteractionCreateHandler) should fire.
		await DispatchAsync(DispatchFrames.InteractionCreate(type: 3, channelId: null));

		await commandHandler.DidNotReceive().HandleAsync(Arg.Any<ICommandContext>(), Arg.Any<IServiceProvider>());
		await modalHandler.DidNotReceive().HandleAsync(Arg.Any<IModalContext>(), Arg.Any<IServiceProvider>());
		await autoHandler.DidNotReceive().HandleAsync(Arg.Any<IAutocompleteContext>(), Arg.Any<IServiceProvider>());
		await componentHandler.Received(1).HandleAsync(Arg.Any<IInteractionContext>(), Arg.Any<IServiceProvider>());
	}
}
