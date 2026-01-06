using System.Text.Json.Serialization;

namespace DiscoSdk
{
    /// <summary>
    /// Represents device information used for Discord Gateway identification.
    /// </summary>
    public class DeviceInfo
    {
        public const string SdkName = "DiscoSdk";

        [JsonPropertyName("os")]
        public required string Os { get; set; }

        [JsonPropertyName("browser")]
        public required string Browser { get; set; }

        [JsonPropertyName("device")]
        public required string Device { get; set; }

        /// <summary>
        /// Creates a default device info instance using the current environment.
        /// </summary>
        /// <returns>A new <see cref="DeviceInfo"/> instance with default values.</returns>
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
