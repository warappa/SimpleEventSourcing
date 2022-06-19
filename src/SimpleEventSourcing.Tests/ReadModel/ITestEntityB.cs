namespace SimpleEventSourcing.ReadModel.Tests
{
    public interface ITestEntityB : IStreamReadModel
    {
        string Value { get; set; }
    }
}
