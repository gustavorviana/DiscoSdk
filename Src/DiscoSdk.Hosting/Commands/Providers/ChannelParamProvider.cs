using DiscoSdk.Commands;
using DiscoSdk.Contexts;
using DiscoSdk.Contexts.Channels;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Commands.Providers;

internal class ChannelParamProvider(ISdkContextProvider context) :
    IParamProvider<IChannel>,
    IParamProvider<IDmChannel>,
    IParamProvider<IGroupDmChannel>,
    IParamProvider<IGuildChannel>,
    IParamProvider<IGuildChannelBase>,
    IParamProvider<IGuildChannelUnion>,
    IParamProvider<IGuildMessageChannel>,
    IParamProvider<IGuildTextChannel>,
    IParamProvider<IGuildNewsChannel>,
    IParamProvider<IGuildThreadChannel>,
    IParamProvider<IGuildVoiceChannel>,
    IParamProvider<IGuildStageChannel>,
    IParamProvider<IGuildForumChannel>,
    IParamProvider<IGuildMediaChannel>,
    IParamProvider<IGuildCategoryChannel>,
    IParamProvider<IGuildTextChannelBase>,
    IParamProvider<IStandardGuildChannel>,
    IParamProvider<IStandardGuildMessageChannel>,
    IParamProvider<ITextBasedChannel>
{
    private IChannel GetChannel()
    {
        return ContextGuard.Require<IChannelContext>(context)
            .Channel
            ?? throw new InvalidOperationException("The channel context is available, but the Channel property is null.");
    }

    async Task<IChannel?> IParamProvider<IChannel>.GetValueAsync()
    {
        await Task.CompletedTask;
        return GetChannel();
    }

    async Task<IDmChannel?> IParamProvider<IDmChannel>.GetValueAsync() => GetChannel() as IDmChannel;

    async Task<IGroupDmChannel?> IParamProvider<IGroupDmChannel>.GetValueAsync() => GetChannel() as IGroupDmChannel;

    async Task<IGuildChannel?> IParamProvider<IGuildChannel>.GetValueAsync() => GetChannel() as IGuildChannel;

    async Task<IGuildChannelUnion?> IParamProvider<IGuildChannelUnion>.GetValueAsync() => GetChannel() as IGuildChannelUnion;

    private IChannel? GetExpectedChannel()
    {
        var channel = GetChannel();
        if (channel is GuildChannelUnionWrapper wrapper)
            return wrapper.ToExpectedChannel();

        return channel;
    }

    async Task<IGuildChannelBase?> IParamProvider<IGuildChannelBase>.GetValueAsync() => GetExpectedChannel() as IGuildChannelBase;

    async Task<IGuildMessageChannel?> IParamProvider<IGuildMessageChannel>.GetValueAsync() => GetExpectedChannel() as IGuildMessageChannel;

    async Task<IGuildTextChannel?> IParamProvider<IGuildTextChannel>.GetValueAsync() => GetExpectedChannel() as IGuildTextChannel;

    async Task<IGuildNewsChannel?> IParamProvider<IGuildNewsChannel>.GetValueAsync() => GetExpectedChannel() as IGuildNewsChannel;

    async Task<IGuildThreadChannel?> IParamProvider<IGuildThreadChannel>.GetValueAsync() => GetExpectedChannel() as IGuildThreadChannel;

    async Task<IGuildVoiceChannel?> IParamProvider<IGuildVoiceChannel>.GetValueAsync() => GetExpectedChannel() as IGuildVoiceChannel;

    async Task<IGuildStageChannel?> IParamProvider<IGuildStageChannel>.GetValueAsync() => GetExpectedChannel() as IGuildStageChannel;

    async Task<IGuildForumChannel?> IParamProvider<IGuildForumChannel>.GetValueAsync() => GetExpectedChannel() as IGuildForumChannel;

    async Task<IGuildMediaChannel?> IParamProvider<IGuildMediaChannel>.GetValueAsync() => GetExpectedChannel() as IGuildMediaChannel;

    async Task<IGuildCategoryChannel?> IParamProvider<IGuildCategoryChannel>.GetValueAsync() => GetExpectedChannel() as IGuildCategoryChannel;

    async Task<IGuildTextChannelBase?> IParamProvider<IGuildTextChannelBase>.GetValueAsync() => GetExpectedChannel() as IGuildTextChannelBase;

    async Task<IStandardGuildChannel?> IParamProvider<IStandardGuildChannel>.GetValueAsync() => GetExpectedChannel() as IStandardGuildChannel;

    async Task<IStandardGuildMessageChannel?> IParamProvider<IStandardGuildMessageChannel>.GetValueAsync() => GetExpectedChannel() as IStandardGuildMessageChannel;

    async Task<ITextBasedChannel?> IParamProvider<ITextBasedChannel>.GetValueAsync() => GetExpectedChannel() as ITextBasedChannel;
}