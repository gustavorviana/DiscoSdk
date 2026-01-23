using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

public class InventorySettings
{
    /// <summary>
    /// Whether the user's inventory features are enabled.
    /// </summary>
    [JsonPropertyName("enable_inventory")]
    public bool EnableInventory { get; set; }
}
