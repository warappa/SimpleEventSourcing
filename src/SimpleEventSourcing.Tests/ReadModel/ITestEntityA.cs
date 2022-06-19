namespace SimpleEventSourcing.ReadModel.Tests
{
    public interface ITestEntityA : IStreamReadModel
    {
        string Value { get; set; }
    }
}
