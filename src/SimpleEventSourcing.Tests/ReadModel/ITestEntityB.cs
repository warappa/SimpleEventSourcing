using SimpleEventSourcing.ReadModel;

namespace SimpleEventSourcing.Tests.ReadModel
{
    public interface ITestEntityB : IStreamReadModel
    {
        string Value { get; set; }
    }
}
