namespace SimpleEventSourcing.Messaging
{
    public interface IEventSourcedEntityCommand : ICommand
    {
        string Id { get; }
    }
}
