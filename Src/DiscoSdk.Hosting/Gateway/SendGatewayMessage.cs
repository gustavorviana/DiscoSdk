namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Represents a message to be sent to the Discord Gateway.
/// </summary>
/// <param name="OpCode">The operation code for the message.</param>
/// <param name="Data">The message data payload, or null.</param>
internal record SendGatewayMessage(OpCodes OpCode, object? Data);
