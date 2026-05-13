namespace DiscoSdk.Models;

/// <summary>
/// A request to change the position (and optionally parent / lock-permissions flag) of a single guild channel.
/// </summary>
/// <param name="Id">The channel to move.</param>
/// <param name="Position">The new sort position, or <c>null</c> to leave unchanged.</param>
/// <param name="LockPermissions">If <c>true</c>, sync the channel's permissions with its new parent.</param>
/// <param name="ParentId">If set, re-parents the channel under this category (or <see cref="Snowflake"/> default to detach).</param>
public readonly record struct ChannelPosition(Snowflake Id, int? Position = null, bool? LockPermissions = null, Snowflake? ParentId = null);