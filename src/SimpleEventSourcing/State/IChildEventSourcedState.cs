namespace SimpleEventSourcing.State
{
    public interface IChildEventSourcedState : IStreamState, ISynchronousProjector
    {
        object AggregateRootId { get; }
        object Id { get; }
    }
}
