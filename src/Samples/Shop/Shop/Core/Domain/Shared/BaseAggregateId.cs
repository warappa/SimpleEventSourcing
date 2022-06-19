namespace Shop.Core.Domain.Shared
{
    public class BaseAggregateId<T> : BaseId<T>, IAggregateId
            where T : BaseAggregateId<T>
    {
        public BaseAggregateId() { }

        public BaseAggregateId(string value) : base(value) { }
    }
}
