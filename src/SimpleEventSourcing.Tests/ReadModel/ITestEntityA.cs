namespace SimpleEventSourcing.ReadModel.Tests
{
    public interface ITestEntityA : IStreamReadModel
    {
        string Value { get; set; }
        ITestEntityASubEntity SubEntity { get; }
    }
    public interface ITestEntityASubEntity : IReadModelBase
    {
        string SubValue { get; set; }
    }
}
