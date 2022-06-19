namespace SimpleEventSourcing.ReadModel
{
    public interface IStreamReadModel : IReadModelBase
    {
        string Streamname { get; set; }
    }
}
