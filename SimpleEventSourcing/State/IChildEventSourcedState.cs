namespace SimpleEventSourcing.State
{
    public interface IChildEventSourcedState : IStreamState, IState
    {
        object AggregateRootId { get; }
        object Id { get; }
    }
}
