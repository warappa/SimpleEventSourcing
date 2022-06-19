namespace SimpleEventSourcing.ReadModel
{
    public interface IReadModel<TKey> : IReadModelBase
    {
        new TKey Id { get; set; }
    }
}
