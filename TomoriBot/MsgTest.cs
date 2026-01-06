using DiscoSdk.Events;

namespace TomoriBot
{
    internal class MsgTest : IMessageCreateHandler
    {
        public Task HandleAsync(MessageCreateEvent eventData)
        {
            var authorName = eventData.Message.Author.GlobalName ?? eventData.Message.Author.Username;
            Console.WriteLine($"[MESSAGE] {authorName}: {eventData.Message.Content}");

            return Task.CompletedTask;
        }
    }
}
