using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Lists the users that reacted to a message with a given emoji. Paginate forward with
/// <see cref="After"/> and cap the page size with <see cref="SetLimit"/> (1–100).
/// </summary>
public interface IGetReactionsAction : IRestAction<IReadOnlyList<IUser>>
{
    /// <summary>Returns reactions whose user IDs are greater than this value.</summary>
    IGetReactionsAction After(Snowflake userId);

    /// <summary>Caps the page size (1–100). Discord defaults to 25.</summary>
    IGetReactionsAction SetLimit(int limit);
}
