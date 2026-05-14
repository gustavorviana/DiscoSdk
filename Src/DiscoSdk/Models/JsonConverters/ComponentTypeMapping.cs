using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using System.Text.Json;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// Single source of truth for mapping a <see cref="ComponentType"/> to the concrete class used
/// for deserialization. Shared by <see cref="InteractionComponentConverter"/> and
/// <see cref="MessageComponentPolymorphicConverter"/>.
/// </summary>
internal static class ComponentTypeMapping
{
    /// <summary>
    /// Deserializes a JSON component object into its concrete type. V2 component types
    /// (<c>9</c>–<c>17</c>) route to their dedicated classes; V1 / unknown types fall back to
    /// the <see cref="MessageComponent"/> god-class which already carries every V1 field.
    /// </summary>
    public static IInteractionComponent Deserialize(ComponentType type, string rawJson, JsonSerializerOptions options) => type switch
    {
        // ActionRow goes to the *message*-flavored class here — InteractionComponentConverter is
        // only applied to message fields (Message.Components, MessageCreateRequest.Components,
        // ContainerComponent.Components, etc.). The modal-flavored ActionRowComponent is reached
        // via the separate ModalComponentConverter path (ModalData.Components).
        ComponentType.ActionRow => Deserialize<MessageActionRowComponent>(rawJson, options),

        // V1 leaves — only ever appear nested inside a MessageActionRowComponent.Components or
        // SectionComponent.Accessory. Routing them here keeps receive symmetric with send: the
        // builders produce ButtonComponent/StringSelectComponent/etc, and reading those same
        // payloads back gives the same concrete types instead of the MessageComponent god-class.
        ComponentType.Button => Deserialize<ButtonComponent>(rawJson, options),
        ComponentType.StringSelect => Deserialize<StringSelectComponent>(rawJson, options),
        ComponentType.UserSelect => Deserialize<UserSelectComponent>(rawJson, options),
        ComponentType.RoleSelect => Deserialize<RoleSelectComponent>(rawJson, options),
        ComponentType.MentionableSelect => Deserialize<MentionableSelectComponent>(rawJson, options),
        ComponentType.ChannelSelect => Deserialize<ChannelSelectComponent>(rawJson, options),

        // Components V2.
        ComponentType.Label => Deserialize<LabelComponent>(rawJson, options),
        ComponentType.Section => Deserialize<SectionComponent>(rawJson, options),
        ComponentType.TextDisplay => Deserialize<TextDisplayComponent>(rawJson, options),
        ComponentType.Thumbnail => Deserialize<ThumbnailComponent>(rawJson, options),
        ComponentType.MediaGallery => Deserialize<MediaGalleryComponent>(rawJson, options),
        ComponentType.File => Deserialize<FileComponent>(rawJson, options),
        ComponentType.Separator => Deserialize<SeparatorComponent>(rawJson, options),
        ComponentType.Container => Deserialize<ContainerComponent>(rawJson, options),

        // Modal-exclusive single-input types — also reachable when the SDK reads back submission
        // data through the polymorphic converter (modal submit interactions).
        ComponentType.TextInput => Deserialize<TextInputComponent>(rawJson, options),
        ComponentType.FileUpload => Deserialize<FileUploadComponent>(rawJson, options),
        ComponentType.RadioGroup => Deserialize<RadioGroupComponent>(rawJson, options),
        ComponentType.CheckboxGroup => Deserialize<CheckboxGroupComponent>(rawJson, options),
        ComponentType.Checkbox => Deserialize<CheckboxComponent>(rawJson, options),

        // Unknown / future types — fall back to the god-class so the SDK still reads them.
        _ => Deserialize<MessageComponent>(rawJson, options),
    };

    private static T Deserialize<T>(string raw, JsonSerializerOptions options) where T : IInteractionComponent
        => JsonSerializer.Deserialize<T>(raw, options)
           ?? throw new JsonException($"Failed to deserialize {typeof(T).Name}.");
}
