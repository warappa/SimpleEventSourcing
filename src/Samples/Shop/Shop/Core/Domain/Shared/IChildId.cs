namespace Shop.Core.Domain.Shared
{
    public interface IChildId<TAggregateRootId> : IId
        where TAggregateRootId : BaseId<TAggregateRootId>
    {
        TAggregateRootId AggregateRootId { get; }
    }
}
