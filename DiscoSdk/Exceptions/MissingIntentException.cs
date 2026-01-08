namespace DiscoSdk.Exceptions;

/// <summary>
/// Exception thrown when an operation requires a gateway intent that is not enabled.
/// </summary>
/// <remarks>
/// Some Discord operations require specific gateway intents to be enabled in the bot's configuration.
/// This exception is thrown when attempting to use a feature that requires an intent that is not currently enabled.
/// </remarks>
public sealed class MissingIntentException : InvalidOperationException
{
	/// <summary>
	/// Gets the required intent that is missing.
	/// </summary>
	public DiscordIntent RequiredIntent { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MissingIntentException"/> class.
	/// </summary>
	/// <param name="requiredIntent">The intent that is required for the operation.</param>
	/// <param name="operation">The operation that was attempted.</param>
	public MissingIntentException(DiscordIntent requiredIntent, string operation)
		: base($"Cannot {operation}. The {GetIntentName(requiredIntent)} intent is required but not enabled in the bot's configuration.")
	{
		RequiredIntent = requiredIntent;
	}

	private static string GetIntentName(DiscordIntent intent)
	{
		return intent switch
		{
			DiscordIntent.MessageContent => "MessageContent",
			DiscordIntent.GuildMessages => "GuildMessages",
			DiscordIntent.DirectMessages => "DirectMessages",
			DiscordIntent.GuildMessageReactions => "GuildMessageReactions",
			DiscordIntent.DirectMessageReactions => "DirectMessageReactions",
			_ => intent.ToString()
		};
	}
}

