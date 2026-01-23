using DiscoSdk.Models;
using DiscoSdk.Models.Messages;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a pagination action for retrieving messages.
/// </summary>
public interface IMessagePaginationAction : IPaginationAction<IMessage, IMessagePaginationAction>
{
	/// <summary>
	/// Gets history around a specific message.
	/// </summary>
	/// <param name="messageId">The message ID to get history around.</param>
	/// <returns>The current <see cref="IMessagePaginationAction"/> instance.</returns>
	IMessagePaginationAction Around(Snowflake messageId);

	/// <summary>
	/// Gets history after a specific message.
	/// </summary>
	/// <param name="messageId">The message ID to get history after.</param>
	/// <returns>The current <see cref="IMessagePaginationAction"/> instance.</returns>
	IMessagePaginationAction After(Snowflake messageId);

	/// <summary>
	/// Gets history before a specific message.
	/// </summary>
	/// <param name="messageId">The message ID to get history before.</param>
	/// <returns>The current <see cref="IMessagePaginationAction"/> instance.</returns>
	IMessagePaginationAction Before(Snowflake messageId);

	/// <summary>
	/// Gets history from the beginning of the channel.
	/// </summary>
	/// <returns>The current <see cref="IMessagePaginationAction"/> instance.</returns>
	IMessagePaginationAction FromBeginning();
}