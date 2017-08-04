namespace SimpleEventSourcing.Messaging
{
    public interface IMessage<out TPayload> : IMessage
    {
        new TPayload Body { get; }
    }
}
