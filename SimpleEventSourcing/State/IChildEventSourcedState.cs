namespace SimpleEventSourcing.State
{
    public interface IChildEventSourcedState : IStreamState, ISynchronousState
    {
        object AggregateRootId { get; }
        object Id { get; }
    }
}
