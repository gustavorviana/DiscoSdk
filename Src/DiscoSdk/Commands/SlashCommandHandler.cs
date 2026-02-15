using DiscoSdk.Contexts.Interactions;

namespace DiscoSdk.Commands;

public abstract class SlashCommandHandler
{
    internal Task ExecuteAsync(ICommandContext context)
    {
        return OnExecuteAsync(context);
    }

    protected abstract Task OnExecuteAsync(ICommandContext context);
}