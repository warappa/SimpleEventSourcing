using SimpleEventSourcing.ReadModel;

namespace SimpleEventSourcing.Tests.ReadModel
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
    public interface ITestEntityASubItem : IReadModelBase
    {
        string SubItemValue { get; set; }
    }
}
