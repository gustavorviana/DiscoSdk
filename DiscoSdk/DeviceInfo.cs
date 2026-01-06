using System.Text.Json.Serialization;

namespace DiscoSdk
{
    public class DeviceInfo
    {
        public const string SdkName = "DiscoSdk";

        [JsonPropertyName("os")]
        public required string Os { get; set; }

        [JsonPropertyName("browser")]
        public required string Browser { get; set; }

        [JsonPropertyName("device")]
        public required string Device { get; set; }

        public static DeviceInfo CreateDefault()
        {
            return new DeviceInfo
            {
                Os = Environment.OSVersion.Platform.ToString(),
                Browser = SdkName,
                Device = SdkName
            };
        }
    }
}
