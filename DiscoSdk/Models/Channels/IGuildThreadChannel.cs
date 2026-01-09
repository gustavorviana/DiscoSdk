using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a thread channel in a Discord guild.
/// </summary>
public interface IGuildThreadChannel : ITextBasedChannel
{
    /// <summary>
    /// Gets the parent channel for this thread.
    /// </summary>
    IRestAction<IChannel?> GetParentChannelId();

    /// <summary>
    /// Gets the number of messages in this thread.
    /// </summary>
    int? MessageCount { get; }

    /// <summary>
    /// Gets the approximate member count of this thread.
    /// </summary>
    int? MemberCount { get; }

    /// <summary>
    /// Gets the total number of messages sent in this thread.
    /// </summary>
    int? TotalMessageSent { get; }

    /// <summary>
    /// Gets the default auto-archive duration for this thread.
    /// </summary>
    ThreadAutoArchiveDuration? AutoArchiveDuration { get; }

    /// <summary>
    /// Gets the ID of the user who started the thread.
    /// </summary>
    DiscordId? OwnerId { get; }

    /// <summary>
    /// Gets the thread metadata containing thread-specific information.
    /// </summary>
    ThreadMetadata? Metadata { get; }

    /// <summary>
    /// Gets the IDs of the tags applied to this thread (for threads in forum/media channels).
    /// </summary>
    DiscordId[]? AppliedTags { get; }

    /// <summary>
    /// Joins this thread.
    /// </summary>
    /// <returns>A REST action that can be executed to join the thread.</returns>
    IRestAction<IGuildThreadChannel> JoinThread();

    /// <summary>
    /// Leaves this thread.
    /// </summary>
    /// <returns>A REST action that can be executed to leave the thread.</returns>
    IRestAction<IGuildThreadChannel> LeaveThread();

    /// <summary>
    /// Adds a member to this thread.
    /// </summary>
    /// <param name="userId">The ID of the user to add to the thread.</param>
    /// <returns>A REST action that can be executed to add the member to the thread.</returns>
    IRestAction<IGuildThreadChannel> AddThreadMember(DiscordId userId);

    /// <summary>
    /// Removes a member from this thread.
    /// </summary>
    /// <param name="userId">The ID of the user to remove from the thread.</param>
    /// <returns>A REST action that can be executed to remove the member from the thread.</returns>
    IRestAction<IGuildThreadChannel> RemoveThreadMember(DiscordId userId);

    /// <summary>
    /// Archives this thread.
    /// </summary>
    /// <returns>A REST action that can be executed to archive the thread.</returns>
    IRestAction<IGuildThreadChannel> ArchiveThread();

    /// <summary>
    /// Unarchives this thread.
    /// </summary>
    /// <returns>A REST action that can be executed to unarchive the thread.</returns>
    IRestAction<IGuildThreadChannel> UnarchiveThread();

    /// <summary>
    /// Locks this thread.
    /// </summary>
    /// <returns>A REST action that can be executed to lock the thread.</returns>
    IRestAction<IGuildThreadChannel> LockThread();

    /// <summary>
    /// Unlocks this thread.
    /// </summary>
    /// <returns>A REST action that can be executed to unlock the thread.</returns>
    IRestAction<IGuildThreadChannel> UnlockThread();
}

