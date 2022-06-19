namespace SimpleEventSourcing.Domain
{
    public interface IProcessManagerHandles<TEvent> : IProcessManagerHandles
    {
        void Handle(TEvent @event);
    }
}
