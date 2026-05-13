using DiscoSdk.Hosting.Gateway;

namespace DiscoSdk.Hosting.Tests.Gateway.Common;

/// <summary>
/// Helpers that build raw JSON gateway frames matching what Discord would send, then parse them
/// into <see cref="ReceivedGatewayMessage"/> so a test can push them into <see cref="FakeGatewaySocket"/>.
/// </summary>
internal static class TestFrames
{
	public static ReceivedGatewayMessage Hello(int heartbeatIntervalMs = 41250) =>
		ReceivedGatewayMessage.Parse($"{{\"op\": 10, \"d\": {{\"heartbeat_interval\": {heartbeatIntervalMs}}}}}");

	public static ReceivedGatewayMessage HeartbeatAck() =>
		ReceivedGatewayMessage.Parse("""{"op": 11, "d": null}""");

	public static ReceivedGatewayMessage Ready(string sessionId = "sess-1", string resumeGatewayUrl = "wss://resume.test/")
	{
		var json = "{" +
			"\"op\": 0," +
			"\"t\": \"READY\"," +
			"\"s\": 1," +
			"\"d\": {" +
				"\"v\": 10," +
				"\"user\": { \"id\": \"1\", \"username\": \"bot\", \"discriminator\": \"0000\" }," +
				"\"guilds\": []," +
				$"\"session_id\": \"{sessionId}\"," +
				$"\"resume_gateway_url\": \"{resumeGatewayUrl}\"," +
				"\"application\": { \"id\": \"1\", \"flags\": 0 }" +
			"}" +
		"}";
		return ReceivedGatewayMessage.Parse(json);
	}

	public static ReceivedGatewayMessage Reconnect() =>
		ReceivedGatewayMessage.Parse("""{"op": 7, "d": null}""");

	public static ReceivedGatewayMessage InvalidSession(bool resumable = false) =>
		ReceivedGatewayMessage.Parse($"{{\"op\": 9, \"d\": {(resumable ? "true" : "false")}}}");
}
