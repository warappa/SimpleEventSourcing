using System;

namespace SimpleEventSourcing.ReadModel.Tests
{
    public interface ICompoundKeyTestEntity : IStreamReadModel
    {
        Guid Key1 { get; set; }
        Guid Key2 { get; set; }
        string Value { get; set; }
    }
}
