namespace DiscoSdk.Models.Enums;


/// <summary>
/// Represents the type of a Discord interaction.
/// </summary>
public enum InteractionType
{
    Ping = 1,
    ApplicationCommand = 2,
    MessageComponent = 3,
    ApplicationCommandAutocomplete = 4,
    ModalSubmit = 5
}
