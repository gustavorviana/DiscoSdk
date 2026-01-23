using DiscoSdk.Contexts.Interactions;

namespace DiscoSdk.Hosting.Contexts.Models;

internal class ModalOption(string customId, string value) : IModalOption
{
    public string CustomId => customId;

    public string Value => value;
}