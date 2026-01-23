namespace DiscoSdk.Contexts.Channels;

public interface ITypingContext : IWithTextChannel
{
    DateTimeOffset StartedAt { get; }
}