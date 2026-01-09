namespace DiscoSdk.Models.Channels
{
    public interface IChannelUnion : IChannel
    {
        IDmChannel? AsDmChannel();

        /// <summary>
        /// Attempts to cast this channel to a guild text channel.
        /// </summary>
        /// <returns>The channel as a guild text channel, or null if this channel is not a guild text channel.</returns>
        IGuildTextChannel? AsTextChannel();

        /// <summary>
        /// Attempts to cast this channel to a guild news channel.
        /// </summary>
        /// <returns>The channel as a guild news channel, or null if this channel is not a guild news channel.</returns>
        IGuildNewsChannel? AsNewsChannel();

        /// <summary>
        /// Attempts to cast this channel to a guild thread channel.
        /// </summary>
        /// <returns>The channel as a guild thread channel, or null if this channel is not a guild thread channel.</returns>
        IGuildThreadChannel? AsThreadChannel();

        /// <summary>
        /// Attempts to cast this channel to a guild voice channel.
        /// </summary>
        /// <returns>The channel as a guild voice channel, or null if this channel is not a guild voice channel.</returns>
        IGuildVoiceChannel? AsVoiceChannel();

        /// <summary>
        /// Attempts to cast this channel to a guild stage channel.
        /// </summary>
        /// <returns>The channel as a guild stage channel, or null if this channel is not a guild stage channel.</returns>
        IGuildStageChannel? AsStageChannel();

        /// <summary>
        /// Attempts to cast this channel to a guild category.
        /// </summary>
        /// <returns>The channel as a guild category, or null if this channel is not a guild category.</returns>
        IGuildCategory? AsCategory();

        /// <summary>
        /// Attempts to cast this channel to a guild forum channel.
        /// </summary>
        /// <returns>The channel as a guild forum channel, or null if this channel is not a guild forum channel.</returns>
        IGuildForumChannel? AsForumChannel();

        /// <summary>
        /// Attempts to cast this channel to a guild media channel.
        /// </summary>
        /// <returns>The channel as a guild media channel, or null if this channel is not a guild media channel.</returns>
        IGuildMediaChannel? AsMediaChannel();

        /// <summary>
        /// Attempts to cast this channel to a guild message channel.
        /// </summary>
        /// <returns>The channel as a guild message channel, or null if this channel is not a guild message channel.</returns>
        IGuildMessageChannel? AsGuildMessageChannel();

        /// <summary>
        /// Attempts to cast this channel to a guild audio channel.
        /// </summary>
        /// <returns>The channel as a guild audio channel, or null if this channel is not a guild audio channel.</returns>
        IGuildAudioChannel? AsAudioChannel();

        /// <summary>
        /// Attempts to cast this channel to a thread container.
        /// </summary>
        /// <returns>The channel as a thread container, or null if this channel is not a thread container.</returns>
        IThreadContainer? AsThreadContainer();

        /// <summary>
        /// Attempts to cast this channel to a standard guild channel.
        /// </summary>
        /// <returns>The channel as a standard guild channel, or null if this channel is not a standard guild channel.</returns>
        IStandardGuildChannel? AsStandardGuildChannel();

        /// <summary>
        /// Attempts to cast this channel to a standard guild message channel.
        /// </summary>
        /// <returns>The channel as a standard guild message channel, or null if this channel is not a standard guild message channel.</returns>
        IStandardGuildMessageChannel? AsStandardGuildMessageChannel();
    }
}
