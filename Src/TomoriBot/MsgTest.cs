using DiscoSdk.Contexts.Messages;
using DiscoSdk.Events;

namespace TomoriBot
{
    internal class MsgTest : IMessageCreateHandler
    {
        public Task HandleAsync(IMessageCreateContext eventData, IServiceProvider services)
        {
            var authorName = eventData.Author.GlobalName ?? eventData.Author.Username;
            Console.WriteLine($"[MESSAGE] {authorName}: {eventData.Message.Content}");

            return Task.CompletedTask;
        }
    }
}
