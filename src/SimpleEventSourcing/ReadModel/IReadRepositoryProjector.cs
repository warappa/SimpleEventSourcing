using SimpleEventSourcing.State;

namespace SimpleEventSourcing.ReadModel
{
    public interface IReadRepositoryProjector<TProjector> : IEventSourcedState<TProjector>
        where TProjector : class, IProjector, new()
    {

    }
}
