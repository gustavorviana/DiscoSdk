using DiscoSdk.Models.Commands;

namespace DiscoSdk.Commands;

/// <summary>
/// Implemented by every node that can hold a flat list of <see cref="SlashCommandOption"/>: the
/// root <see cref="SlashCommandBuilder"/> and <see cref="SlashCommandSubCommandBuilder"/>.
/// Focused option builders carry a reference to their container and call
/// <see cref="AddOption(SlashCommandOption)"/> when adding siblings via <c>ThenXxxOption</c>,
/// so the validation rules (Discord's 25-option limit, nested-option shape, etc.) live in one
/// implementation per container kind instead of being passed around as callbacks.
/// </summary>
internal interface ISlashCommandOptionContainer
{
    /// <summary>Validates <paramref name="option"/> and appends it to this container.</summary>
    void AddOption(SlashCommandOption option);
}
