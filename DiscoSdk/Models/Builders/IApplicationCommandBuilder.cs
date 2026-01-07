using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Builders
{
    public interface IApplicationCommandBuilder
    {
        IApplicationCommandBuilder AddAttachmentOption(string name, string description, bool required = false);
        IApplicationCommandBuilder AddBooleanOption(string name, string description, bool required = false);
        IApplicationCommandBuilder AddChannelOption(string name, string description, bool required = false, params ChannelType[] channelTypes);
        IApplicationCommandBuilder AddIntegerOption(string name, string description, bool required = false, object? minValue = null, object? maxValue = null, bool? autocomplete = null, params ApplicationCommandOptionChoice[] choices);
        IApplicationCommandBuilder AddMentionableOption(string name, string description, bool required = false);
        IApplicationCommandBuilder AddNumberOption(string name, string description, bool required = false, object? minValue = null, object? maxValue = null, bool? autocomplete = null, params ApplicationCommandOptionChoice[] choices);
        IApplicationCommandBuilder AddRoleOption(string name, string description, bool required = false);
        IApplicationCommandBuilder AddStringOption(string name, string description, bool required = false, int? minLength = null, int? maxLength = null, bool? autocomplete = null, params ApplicationCommandOptionChoice[] choices);
        IApplicationCommandBuilder AddSubCommandGroupOption(string name, string description, params ApplicationCommandOption[] options);
        IApplicationCommandBuilder AddSubCommandOption(string name, string description, params ApplicationCommandOption[] options);
        IApplicationCommandBuilder AddUserOption(string name, string description, bool required = false);
        IApplicationCommandBuilder WithDefaultMemberPermissions(string permissions);
        IApplicationCommandBuilder WithDescription(string description);
        IApplicationCommandBuilder WithDescriptionLocalizations(Dictionary<string, string> localizations);
        IApplicationCommandBuilder WithDmPermission(bool dmPermission);
        IApplicationCommandBuilder WithName(string name);
        IApplicationCommandBuilder WithNameLocalizations(Dictionary<string, string> localizations);
        IApplicationCommandBuilder WithNsfw(bool nsfw);
        IApplicationCommandBuilder WithType(ApplicationCommandType type);
        IApplicationCommandBuilder WithVersion(string version);

        ApplicationCommand Build();
    }
}